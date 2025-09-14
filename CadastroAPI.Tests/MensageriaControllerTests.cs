using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BackEnd.Controllers;
using BackEnd.Services;
using BackEnd.Models;
using System.Threading.Tasks;

namespace CadastroAPI.Tests
{
    public class MensageriaControllerTests
    {
        private readonly Mock<IRabbitMQService> _mockRabbitMQService;
        private readonly Mock<ILogger<MensageriaController>> _mockLogger;
        private readonly MensageriaController _controller;

        public MensageriaControllerTests()
        {
            _mockRabbitMQService = new Mock<IRabbitMQService>();
            _mockLogger = new Mock<ILogger<MensageriaController>>();
            _controller = new MensageriaController(_mockRabbitMQService.Object, _mockLogger.Object);
        }

        [Fact]
        public void PublicarCliente_ReturnsBadRequest_WhenClienteIsNull()
        {
            // Arrange
            Cliente? nullCliente = null;

            // Act
            var result = _controller.PublicarCliente(nullCliente);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Cliente não pode ser nulo", badRequestResult.Value);
        }

        [Fact]
        public void PublicarCliente_ReturnsOk_WhenPublicationIsSuccessful()
        {
            // Arrange
            var cliente = new Cliente { Id = "1", Nome = "Test Client" };
            _mockRabbitMQService.Setup(s => s.PublicarCliente(It.IsAny<Cliente>())).Returns(true);

            // Act
            var result = _controller.PublicarCliente(cliente);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockRabbitMQService.Verify(s => s.PublicarCliente(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public void PublicarCliente_ReturnsInternalServerError_WhenPublicationFails()
        {
            // Arrange
            var cliente = new Cliente { Id = "1", Nome = "Test Client" };
            _mockRabbitMQService.Setup(s => s.PublicarCliente(It.IsAny<Cliente>())).Returns(false);

            // Act
            var result = _controller.PublicarCliente(cliente);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _mockRabbitMQService.Verify(s => s.PublicarCliente(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public void PublicarCliente_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var cliente = new Cliente { Id = "1", Nome = "Test Client" };
            _mockRabbitMQService.Setup(s => s.PublicarCliente(It.IsAny<Cliente>())).Throws(new Exception("Test Exception"));

            // Act
            var result = _controller.PublicarCliente(cliente);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PublicarMensagem_ReturnsBadRequest_WhenMessageIsNull()
        {
            // Arrange
            object? nullMessage = null;
            string fila = "test_queue";

            // Act
            var result = await _controller.PublicarMensagem(nullMessage, fila);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Mensagem não pode ser nula", badRequestResult.Value);
        }

        [Fact]
        public async Task PublicarMensagem_ReturnsBadRequest_WhenFilaIsNullOrEmpty()
        {
            // Arrange
            var message = new { Data = "Test" };
            string emptyFila = "";

            // Act
            var result = await _controller.PublicarMensagem(message, emptyFila);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Nome da fila não pode ser nulo ou vazio", badRequestResult.Value);
        }

        [Fact]
        public async Task PublicarMensagem_ReturnsOk_WhenPublicationIsSuccessful()
        {
            // Arrange
            var message = new { Data = "Test" };
            string fila = "test_queue";
            _mockRabbitMQService.Setup(s => s.PublicarMensagemAsync(It.IsAny<object>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _controller.PublicarMensagem(message, fila);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockRabbitMQService.Verify(s => s.PublicarMensagemAsync(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task PublicarMensagem_ReturnsInternalServerError_WhenPublicationFails()
        {
            // Arrange
            var message = new { Data = "Test" };
            string fila = "test_queue";
            _mockRabbitMQService.Setup(s => s.PublicarMensagemAsync(It.IsAny<object>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await _controller.PublicarMensagem(message, fila);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _mockRabbitMQService.Verify(s => s.PublicarMensagemAsync(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task PublicarMensagem_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var message = new { Data = "Test" };
            string fila = "test_queue";
            _mockRabbitMQService.Setup(s => s.PublicarMensagemAsync(It.IsAny<object>(), It.IsAny<string>())).ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.PublicarMensagem(message, fila);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PublicarMensagemTeste_ReturnsBadRequest_WhenFilaIsNullOrEmpty()
        {
            // Arrange
            string emptyFila = "";

            // Act
            var result = await _controller.PublicarMensagemTeste(emptyFila);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Nome da fila não pode ser nulo ou vazio", badRequestResult.Value);
        }

        [Fact]
        public async Task PublicarMensagemTeste_ReturnsOk_WhenPublicationIsSuccessful()
        {
            // Arrange
            string fila = "test_queue";
            _mockRabbitMQService.Setup(s => s.PublicarMensagemAsync(It.IsAny<object>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _controller.PublicarMensagemTeste(fila);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockRabbitMQService.Verify(s => s.PublicarMensagemAsync(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task PublicarMensagemTeste_ReturnsInternalServerError_WhenPublicationFails()
        {
            // Arrange
            string fila = "test_queue";
            _mockRabbitMQService.Setup(s => s.PublicarMensagemAsync(It.IsAny<object>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await _controller.PublicarMensagemTeste(fila);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _mockRabbitMQService.Verify(s => s.PublicarMensagemAsync(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task PublicarMensagemTeste_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            string fila = "test_queue";
            _mockRabbitMQService.Setup(s => s.PublicarMensagemAsync(It.IsAny<object>(), It.IsAny<string>())).ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.PublicarMensagemTeste(fila);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}