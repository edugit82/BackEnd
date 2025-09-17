using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Project.Services;

namespace Project.Messaging
{
    public class RabbitMQConsumer : IMessageConsumer
    {
        private IModel _channel;
        private IConnection _connection;
        private AsyncEventingBasicConsumer _consumer;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _queueName;
        private int _retryCount;
        private const int _maxRetries = 5;

        public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _queueName = _configuration["RabbitMQ:QueueName"];
            _retryCount = 0;
            ConnectToRabbitMQ();
        }

        private void ConnectToRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMQ:HostName"],
                    Port = int.Parse(_configuration["RabbitMQ:Port"]),
                    UserName = _configuration["RabbitMQ:UserName"],
                    Password = _configuration["RabbitMQ:Password"]
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(queue: _queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
                _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso.");
                _retryCount = 0; // Reset retry count on successful connection
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao conectar ao RabbitMQ.");
                if (_retryCount < _maxRetries)
                {
                    _retryCount++;
                    _logger.LogInformation($"Tentando reconectar ao RabbitMQ (Tentativa {_retryCount}/{_maxRetries})...");
                    Thread.Sleep(5000); // Wait 5 seconds before retrying
                    ConnectToRabbitMQ();
                }
                else
                {
                    _logger.LogError("Número máximo de tentativas de reconexão atingido. Não foi possível conectar ao RabbitMQ.");
                    throw; // Re-throw the exception if max retries reached
                }
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning($"Conexão com RabbitMQ encerrada: {e.Cause}");
            _consumer = null; // Invalidate the consumer
            if (_connection.IsOpen)
            {
                _connection.Close();
            }
            _connection.Dispose();
            _channel.Dispose();
            ConnectToRabbitMQ(); // Attempt to reconnect
        }

        public void StartConsuming()
        {
            if (_consumer == null)
            {
                _consumer = new AsyncEventingBasicConsumer(_channel);
                _consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Mensagem recebida: {message}");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var clientService = scope.ServiceProvider.GetRequiredService<ClientService>();
                        await clientService.AddClientAsync(message);
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                };

                _channel.BasicConsume(queue: _queueName,
                                     autoAck: false,
                                     consumer: _consumer);
                _logger.LogInformation("Consumidor RabbitMQ iniciado.");
            }
            else
            {
                _logger.LogInformation("Consumidor RabbitMQ já está ativo.");
            }
        }

        public void StopConsuming()
        {
            _connection?.Close();
            _logger.LogInformation("RabbitMQ Consumer stopped.");
        }
    }
}