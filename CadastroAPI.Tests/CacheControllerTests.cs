using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BackEnd.Controllers;
using BackEnd.Services;
using System.Threading.Tasks;
using System;

namespace CadastroAPI.Tests
{
    public class CacheControllerTests
    {
        private readonly Mock<IRedisService> _mockRedisService;
        private readonly Mock<ILogger<CacheController>> _mockLogger;
        private readonly CacheController _controller;

        public CacheControllerTests()
        {
            _mockRedisService = new Mock<IRedisService>();
            _mockLogger = new Mock<ILogger<CacheController>>();
            _controller = new CacheController(_mockLogger.Object, _mockRedisService.Object);
        }

        [Fact]
        public async Task SetCache_ReturnsOk_WhenCacheIsSetSuccessfully()
        {
            // Arrange
            string key = "testKey";
            object value = new { Data = "testValue" };
            _mockRedisService.Setup(s => s.SetAsync(key, value, It.IsAny<TimeSpan?>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SetCache(key, value, null);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockRedisService.Verify(s => s.SetAsync(key, value, It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Fact]
        public async Task SetCache_ReturnsOkWithExpiry_WhenCacheIsSetSuccessfully()
        {
            // Arrange
            string key = "testKey";
            object value = new { Data = "testValue" };
            int expiryMinutes = 5;
            _mockRedisService.Setup(s => s.SetAsync(key, value, TimeSpan.FromMinutes(expiryMinutes))).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SetCache(key, value, expiryMinutes);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockRedisService.Verify(s => s.SetAsync(key, value, TimeSpan.FromMinutes(expiryMinutes)), Times.Once);
        }

        [Fact]
        public async Task SetCache_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            string key = "testKey";
            object value = new { Data = "testValue" };
            _mockRedisService.Setup(s => s.SetAsync(key, value, It.IsAny<TimeSpan?>())).Throws(new Exception("Test Exception"));

            // Act
            var result = await _controller.SetCache(key, value, null);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
            _mockRedisService.Verify(s => s.SetAsync(key, value, It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Fact]
        public async Task GetCache_ReturnsOk_WhenCacheKeyIsFound()
        {
            // Arrange
            string key = "testKey";
            object cachedValue = new { Data = "cachedValue" };
            _mockRedisService.Setup(s => s.GetAsync<object>(It.IsAny<string>())).ReturnsAsync(cachedValue);

            // Act
            var result = await _controller.GetCache(key);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(cachedValue, okResult.Value);
            _mockRedisService.Verify(s => s.GetAsync<object>(key), Times.Once);
        }

        [Fact]
        public async Task GetCache_ReturnsNotFound_WhenCacheKeyIsNotFound()
        {
            // Arrange
            string key = "nonExistentKey";
            object? cachedValue = null;
            _mockRedisService.Setup(s => s.GetAsync<object>(It.IsAny<string>())).ReturnsAsync(cachedValue);

            // Act
            var result = await _controller.GetCache(key);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _mockRedisService.Verify(s => s.GetAsync<object>(key), Times.Once);
        }

        [Fact]
        public async Task GetCache_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            string key = "testKey";
            _mockRedisService.Setup(s => s.GetAsync<object>(It.IsAny<string>())).Throws(new Exception("Test Exception"));

            // Act
            var result = await _controller.GetCache(key);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
            _mockRedisService.Verify(s => s.GetAsync<object>(key), Times.Once);
        }

        [Fact]
        public async Task DeleteCache_ReturnsOk_WhenCacheKeyIsDeletedSuccessfully()
        {
            // Arrange
            string key = "testKey";
            _mockRedisService.Setup(s => s.DeleteAsync(key)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCache(key);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockRedisService.Verify(s => s.DeleteAsync(key), Times.Once);
        }

        [Fact]
        public async Task DeleteCache_ReturnsNotFound_WhenCacheKeyIsNotPresentForDeletion()
        {
            // Arrange
            string key = "nonExistentKey";
            _mockRedisService.Setup(s => s.DeleteAsync(key)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCache(key);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _mockRedisService.Verify(s => s.DeleteAsync(key), Times.Once);
        }

        [Fact]
        public async Task DeleteCache_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            string key = "testKey";
            _mockRedisService.Setup(s => s.DeleteAsync(key)).Throws(new Exception("Test Exception"));

            // Act
            var result = await _controller.DeleteCache(key);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
        }
    }
}