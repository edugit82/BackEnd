using Moq;
using RabbitMQ.Client;
using System.Text;
using Xunit;
using Project.Messaging;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using System;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Project.Configurations;

namespace Tests
{
    public class RabbitMQMessageServiceTests
    {        
        private readonly Mock<IConnectionFactory> _mockConnectionFactory;
        private readonly Mock<IConnection> _mockConnection;
        private readonly Mock<IModel> _mockChannel;
        private readonly Mock<IMessageProducer> _mockMessageProducer;
        private readonly Mock<IMessageConsumer> _mockMessageConsumer;
        private readonly IConfiguration _configuration;

        public RabbitMQMessageServiceTests()
        {
            _mockConnectionFactory = new Mock<IConnectionFactory>();
            _mockConnection = new Mock<IConnection>();
            _mockChannel = new Mock<IModel>();
            _mockMessageProducer = new Mock<IMessageProducer>();
            _mockMessageConsumer = new Mock<IMessageConsumer>();

            _configuration = new ConfigurationBuilder()
                .AddJsonFile("d:/Repositorio/BackEnd/Project/appsettings.json")
                .Build();

            _mockConnectionFactory.Setup(f => f.CreateConnection())
                .Returns(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateModel())
                .Returns(_mockChannel.Object);
        }
        
        [Fact]
        public void PublishMessage_ShouldCallProducerPublishMessage()
        {
            // Arrange
            var message = new { Id = 1, Name = "TestMessage" };
            var routingKey = "test_routing_key";

            // Act
            _mockMessageProducer.Object.PublishMessage(message, routingKey);

            Func<object, bool> message_object = (a) =>
            {
                if(a == null)
                    return false;

                if(a.GetType()?.GetProperty("Id") == null || a.GetType()?.GetProperty("Name") == null)
                    return false;

                var id = a.GetType()?.GetProperty("Id")?.GetValue(a);
                var name = a.GetType()?.GetProperty("Name")?.GetValue(a);
                return id is int && (int)id == message.Id && name is string && (string)name == message.Name;
            };

            // Assert
            _mockMessageProducer.Verify(p => p.PublishMessage(
                It.Is<object>(m => message_object(m)),
                It.Is<string>(r => r == routingKey)),
                Times.Once);
        }

        [Fact]
        public void Publish_ShouldCallProducerPublishWithKeyValueAndRoutingKey()
        {
            // Arrange
            var key = "testKey";
            var value = "testValue";
            var routingKey = "testRoutingKey";

            // Act
            _mockMessageProducer.Object.Publish(key, value, routingKey);

            // Assert
            _mockMessageProducer.Verify(p => p.Publish(
                It.Is<string>(k => k == key),
                It.Is<string>(v => v == value),
                It.Is<string>(r => r == routingKey)),
                Times.Once);
        }
        
        [Fact]
        public void StartConsuming_ShouldCallConsumerStartConsuming()
        {
            // Arrange
            _mockMessageConsumer.Setup(c => c.StartConsuming()).Verifiable();

            // Act
            _mockMessageConsumer.Object.StartConsuming();

            // Assert
            _mockMessageConsumer.Verify(c => c.StartConsuming(), Times.Once);
        }
        
        [Fact]
        public void StopConsuming_ShouldCallConsumerStopConsuming()
        {
            // Arrange
            _mockMessageConsumer.Setup(c => c.StopConsuming()).Verifiable();

            // Act
            _mockMessageConsumer.Object.StopConsuming();

            // Assert
            _mockMessageConsumer.Verify(c => c.StopConsuming(), Times.Once);
        }
    }
}