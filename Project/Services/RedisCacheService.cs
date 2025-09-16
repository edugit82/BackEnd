using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Project.Configurations;
using StackExchange.Redis;

namespace Project.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly RedisCacheSettings _settings;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly string _instanceName;

        public RedisCacheService(
            IOptions<RedisCacheSettings> settings,
            ILogger<RedisCacheService> logger,
            IConnectionMultiplexer connectionMultiplexer,
            IDatabase database)
        {
            _settings = settings.Value;
            _logger = logger;
            _instanceName = "ProjectCache:"; // Default namespace

            _redis = connectionMultiplexer;
            _database = database;

            _redis.ConnectionFailed += OnConnectionFailed;
            _redis.HashSlotMoved += OnHashSlotMoved;
            _redis.InternalError += OnInternalError;
        }

        private void OnConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            _logger.LogError(e.Exception, "Redis connection failed: {FailureType}", e.FailureType);
        }

        private void OnHashSlotMoved(object? sender, StackExchange.Redis.HashSlotMovedEventArgs e)
        {
            _logger.LogInformation("Redis hash slot moved: {OldEndPoint} to {NewEndPoint}", e.OldEndPoint, e.NewEndPoint);
        }

        private void OnInternalError(object? sender, StackExchange.Redis.InternalErrorEventArgs e)
        {
            _logger.LogError(e.Exception, "Redis internal error: {Origin}", e.Origin);
        }

        private string GetKeyWithNamespace(string key)
        {
            return _instanceName + key;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(GetKeyWithNamespace(key));
                if (value.IsNullOrEmpty)
                {
                    return default!;
                }
                return JsonSerializer.Deserialize<T?>(value.ToString()) ?? default!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting key {Key} from Redis.", key);
                return default!;
            }
        }

        public async Task<T?> GetCacheValueAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(GetKeyWithNamespace(key));
            if (value.IsNullOrEmpty)
            {
                return default!;
            }
            return JsonSerializer.Deserialize<T?>(value.ToString()) ?? default!;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var expiry = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);
                var serializedValue = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(GetKeyWithNamespace(key), serializedValue, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting key {Key} in Redis.", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(GetKeyWithNamespace(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing key {Key} from Redis.", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(GetKeyWithNamespace(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of key {Key} in Redis.", key);
                return false;
            }
        }
    }
}