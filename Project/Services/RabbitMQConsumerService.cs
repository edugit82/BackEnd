using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Project.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Project.Services
{
    public class RabbitMQConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IMessageConsumer _consumer = null!;

        public RabbitMQConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            using (var scope = _serviceProvider.CreateScope())
            {
                _consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();
                _consumer.StartConsuming();
            }

            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
            _consumer?.StopConsuming();
        }
    }
}