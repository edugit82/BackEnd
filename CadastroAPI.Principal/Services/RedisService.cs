using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _database;

        public RedisService(IConnectionMultiplexer redisConnection)
        {
            _database = redisConnection.GetDatabase();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, serializedValue, expiry);
            }
            catch (Exception ex)
            {
                // Log the exception, but don't rethrow to allow the application to continue
                Console.WriteLine($"Error setting value to Redis: {ex.Message}");
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var serializedValue = await _database.StringGetAsync(key);
                if (serializedValue.IsNullOrEmpty)
                {
                    return default;
                }
                return JsonSerializer.Deserialize<T>(serializedValue!);
            }
            catch (Exception ex)
            {
                // Log the exception, but don't rethrow to allow the application to continue
                Console.WriteLine($"Error getting value from Redis: {ex.Message}");
                return default; // Fallback: return default value on error
            }
        }

        public async Task<bool> DeleteAsync(string key)
        {
            try
            {
                return await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                // Log the exception, but don't rethrow to allow the application to continue
                Console.WriteLine($"Error deleting value from Redis: {ex.Message}");
                return false; // Fallback: return false on error
            }
        }
    }
}