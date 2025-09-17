using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Project.Messaging;

namespace Project.Services
{
    public class RabbitMQConsumerService : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IMessageConsumer _consumer;

        public RabbitMQConsumerService(ILogger<RabbitMQConsumerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQ Consumer Service running.");

            using (var scope = _serviceProvider.CreateScope())
            {
                _consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();
                _consumer.StartConsuming();
            }

            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
            _consumer?.StopConsuming();
        }
    }
}