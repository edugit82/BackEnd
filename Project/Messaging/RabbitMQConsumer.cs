using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Project.Messaging
{
    public class RabbitMQConsumer : IMessageConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionFactory _factory;
        private IConnection _connection = null!;
        private IModel _channel = null!;
        private readonly string _queueName = string.Empty;
        private AsyncEventingBasicConsumer _consumer = null!;
        private int _retryCount;
        private const int MaxRetries = 5;
        private readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
        private readonly HashSet<string> _processedMessageIds = new HashSet<string>();

        private readonly ILogger<RabbitMQConsumer> _logger;

        public RabbitMQConsumer(IConfiguration configuration, ILogger<RabbitMQConsumer> logger, Func<IModel, AsyncEventingBasicConsumer>? consumerFactory = null)
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
            if (int.TryParse(_configuration["RabbitMQ:RetryCount"], out int retryCount))
            {
                _retryCount = retryCount;
            }
            else
            {
                _retryCount = 5; // Valor padrão
            }
            _consumerFactory = consumerFactory ?? (model => new AsyncEventingBasicConsumer(model));
        }

        private readonly Func<IModel, AsyncEventingBasicConsumer> _consumerFactory;

        private void ConnectToRabbitMQ()
        {
            try
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
                _channel.CallbackException += (sender, ea) =>
                {
                    _logger.LogError(ea.Exception, $"RabbitMQ CallbackException: {ea.Exception.Message}");
                };
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
                    _logger.LogError("Max reconnection attempts reached. Giving up on consumer connection.");
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

        public void StartConsuming()
        {
            ConnectToRabbitMQ();

            if (_channel != null && _channel.IsOpen)
            {
                _consumer = _consumerFactory(_channel);
                _consumer.Received += async (model, ea) =>
                {
                    await Task.Run(() =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var messageId = Guid.NewGuid().ToString(); // Generate a unique ID for the message

                        if (_processedMessageIds.Contains(messageId))
                        {
                            _logger.LogInformation($" [x] Duplicate message received and ignored: {messageId}");
                            _channel.BasicAck(ea.DeliveryTag, false);
                            return;
                        }

                        _processedMessageIds.Add(messageId);

                        _logger.LogInformation($" [x] Received {message}");

                        // Process the message here
                        // For example, deserialize and handle based on routing key or message content
                        try
                        {
                            // Example: Log the message content
                            _logger.LogInformation($"Processing message: {message}");
                            // Acknowledge the message after successful processing
                            _channel.BasicAck(ea.DeliveryTag, false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing message: {ex.Message}");
                            // Requeue the message if processing fails
                            _channel.BasicNack(ea.DeliveryTag, false, true);
                        }
                    });
                };

                _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: _consumer);
                _logger.LogInformation("RabbitMQ Consumer started.");
            }
            else
            {
                _logger.LogError("Failed to start consumer: RabbitMQ channel is not open after reconnection attempts.");
            }
        }

        public void StopConsuming()
        {
            _connection?.Close();
            _logger.LogInformation("RabbitMQ Consumer stopped.");
        }
    }
}