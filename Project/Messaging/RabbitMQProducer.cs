using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Project.Messaging
{
    public class RabbitMQProducer : IMessageProducer
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionFactory _factory;
        private IConnection _connection = null!;
        private IModel _channel = null!;
        private readonly string _queueName = string.Empty;
        private int _retryCount;
        private const int MaxRetries = 5;
        private readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

        private readonly ILogger<RabbitMQProducer> _logger;

        public RabbitMQProducer(IConfiguration configuration, ILogger<RabbitMQProducer> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _queueName = _configuration["RabbitMQ:QueueName"] ?? "default_queue";

            _factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.TryParse(_configuration["RabbitMQ:Port"], out int port) ? port : 5672, // Default RabbitMQ port
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            ConnectToRabbitMQ();
        }

        private void ConnectToRabbitMQ()
        {
            try
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
                _retryCount = 0; // Reset retry count on successful connection
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not connect to RabbitMQ: {ex.Message}");
                if (_retryCount < MaxRetries)
                {
                    _logger.LogWarning($"Attempting to reconnect to RabbitMQ in {RetryDelay.TotalSeconds} seconds... (Attempt {_retryCount}/{MaxRetries})");
                    Thread.Sleep(RetryDelay);
                    ConnectToRabbitMQ();
                }
                else
                {
                    _logger.LogError("Max reconnection attempts reached. Giving up.");
                }
            }
        }

        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("RabbitMQ Connection Shutdown");
            if (_connection.IsOpen)
            {
                _connection.Close();
            }
            ConnectToRabbitMQ(); // Attempt to reconnect
        }

        public void PublishMessage<T>(T message, string routingKey)
        {
            if (_channel == null || !_channel.IsOpen)
            {
                _logger.LogWarning("Channel is not open. Attempting to reconnect...");
                ConnectToRabbitMQ(); // Attempt to reconnect if channel is closed
            }

            if (_channel != null && _channel.IsOpen)
            {
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(exchange: "", routingKey: routingKey, basicProperties: null, body: body);
                _logger.LogInformation($"Message published: {json}");
            }
            else
            {
                _logger.LogError("Failed to publish message: RabbitMQ channel is not open after reconnection attempts.");
            }
        }

        public void Publish(string key, string value, string routingKey)
        {
            if (_channel == null || !_channel.IsOpen)
            {
                _logger.LogWarning("Channel is not open. Attempting to reconnect...");
                ConnectToRabbitMQ();
            }

            if (_channel != null && _channel.IsOpen)
            {
                var message = new { Key = key, Value = value };
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(exchange: "", routingKey: routingKey, basicProperties: null, body: body);
                _logger.LogInformation($"Key-Value message published: {json}");
            }
            else
            {
                _logger.LogError("Failed to publish key-value message: RabbitMQ channel is not open after reconnection attempts.");
            }
        }
    }
}