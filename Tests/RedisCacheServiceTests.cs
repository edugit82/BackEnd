using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Project.Configurations;
using Project.Services;
using System.Text.Json;
using Xunit;
using System.Data.Common;

namespace Tests
{
    public class RedisCacheServiceTests
    {
        private readonly Mock<ILogger<RedisCacheService>> _mockLogger;
        private readonly Mock<IOptions<RedisCacheSettings>> _mockOptions;
        private readonly RedisCacheSettings _redisSettings;
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;

        public RedisCacheServiceTests()
        {
            _mockLogger = new Mock<ILogger<RedisCacheService>>();
            _mockOptions = new Mock<IOptions<RedisCacheSettings>>();
            _redisSettings = new RedisCacheSettings
            {
                Host = "68.211.177.39",
                User = "eduardocorrea82_redis",
                Password = "bO7#sL1@",
                Port = 6379,
                DefaultExpirationMinutes = 1
            };
            _mockOptions.Setup(o => o.Value).Returns(_redisSettings);

            _mockDatabase = new Mock<IDatabase>();
            _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
            _mockConnectionMultiplexer.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDatabase.Object);
        }

        private RedisCacheService CreateRedisCacheService()
        {
            return new RedisCacheService(
                _mockOptions.Object,
                _mockLogger.Object,
                _mockConnectionMultiplexer.Object,
                _mockDatabase.Object);
        }

        private class TestClass
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        [Fact]
        public async Task SetAsync_ShouldSerializeAndSetDataInRedis_WithoutExpiry()
        {
            // Arrange
            var service = CreateRedisCacheService();
            var key = "testKey";
            var value = "testValue";
            var serializedValue = JsonSerializer.Serialize(value);
            var expectedExpiry = TimeSpan.FromMinutes(_redisSettings.DefaultExpirationMinutes);
            
            _mockDatabase.Setup(db => db.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                false,                
                When.Always,
                CommandFlags.None))
                .ReturnsAsync(true);

            // Act
            await service.SetAsync(key, value);

            // Assert
            _mockDatabase.Verify(db => db.StringSetAsync(
                It.Is<RedisKey>(k => k.ToString().Contains(key)),                
                It.Is<RedisValue>(v => v.ToString() == serializedValue),
                It.Is<TimeSpan?>(ts => ts == expectedExpiry),
                false,                
                When.Always,
                CommandFlags.None),
                Times.Once);
        }

        [Fact]
        public async Task SetAsync_ShouldSerializeAndSetDataInRedis_WithExpiry()
        {
            // Arrange
            var service = CreateRedisCacheService();
            var key = "testKey";
            var value = new TestClass { Id = 1, Name = "Test" };
            var expiry = TimeSpan.FromMinutes(5);
            var serializedValue = JsonSerializer.Serialize(value);

            _mockDatabase.Setup(db => db.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                false,
                When.Always,
                CommandFlags.None))
                .ReturnsAsync(true);

            // Act
            await service.SetAsync(key, value, expiry);

            // Assert
            _mockDatabase.Verify(db => db.StringSetAsync(
                It.Is<RedisKey>(k => k.ToString().Contains(key)),
                It.Is<RedisValue>(v => v.ToString() == serializedValue),
                It.Is<TimeSpan?>(ts => ts == expiry),
                false,
                When.Always,
                CommandFlags.None),
                Times.Once);
        }
        [Fact]
        public async Task GetAsync_ShouldRetrieveAndDeserializeDataFromRedis()
        {
            // Arrange
            var service = CreateRedisCacheService();
            var key = "testKey";
            var value = "testValue";
            var serializedValue = JsonSerializer.Serialize(value);

            _mockDatabase.Setup(db => db.StringGetAsync(
                It.Is<RedisKey>(k => k.ToString().Contains(key)),
                CommandFlags.None))
                .ReturnsAsync(serializedValue);

            // Act
            var result = await service.GetAsync<string>(key);

            // Assert
            Assert.Equal(expected: "testValue", actual: result);

            _mockDatabase.Verify(db => db.StringGetAsync(
                It.Is<RedisKey>(k => k.ToString().Contains(key)),
                CommandFlags.None),
                Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_ShouldDeleteKeyFromRedis()
        {
            // Arrange
            var service = CreateRedisCacheService();
            var key = "testKey";

            // Act
            await service.RemoveAsync(key);

            // Assert
            _mockDatabase.Verify(db => db.KeyDeleteAsync(
                It.Is<RedisKey>(k => k.ToString().Contains(key)),
                CommandFlags.None),
                Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ShouldCheckKeyExistenceInRedis()
        {
            // Arrange
            var service = CreateRedisCacheService();
            var key = "testKey";
            _mockDatabase.Setup(db => db.KeyExistsAsync(
                It.Is<RedisKey>(k => k.ToString().Contains(key)),
                CommandFlags.None))
                .ReturnsAsync(true);

            // Act
            var result = await service.ExistsAsync(key);

            // Assert
            Assert.True(result);
            _mockDatabase.Verify(db => db.KeyExistsAsync(
                It.Is<RedisKey>(k => k.ToString().Contains(key)),
                CommandFlags.None),
                Times.Once);
        }


    }
}