using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackEnd.Services
{
    /// <summary>
    /// Serviço hospedado para iniciar e parar consumidores RabbitMQ automaticamente
    /// </summary>
    public class RabbitMQConsumerHostedService : IHostedService
    {
        private readonly RabbitMQConsumerFactory _consumerFactory;
        private readonly ILogger<RabbitMQConsumerHostedService> _logger;

        /// <summary>
        /// Construtor do serviço hospedado
        /// </summary>
        /// <param name="consumerFactory">Fábrica de consumidores RabbitMQ</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public RabbitMQConsumerHostedService(
            RabbitMQConsumerFactory consumerFactory,
            ILogger<RabbitMQConsumerHostedService> logger)
        {
            _consumerFactory = consumerFactory ?? throw new ArgumentNullException(nameof(consumerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Inicia os consumidores quando a aplicação é iniciada
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando consumidores RabbitMQ");

            try
            {
                // Iniciar consumidores para a fila de clientes
                await _consumerFactory.CreateAndStartConsumersAsync<BackEnd.Models.Cliente, ClienteMessageProcessor>("clientes");

                // Aqui podem ser adicionados mais consumidores para outras filas conforme necessário

                _logger.LogInformation("Consumidores RabbitMQ iniciados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar consumidores RabbitMQ");
                throw;
            }
        }

        /// <summary>
        /// Para os consumidores quando a aplicação é encerrada
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Parando consumidores RabbitMQ");

            try
            {
                await _consumerFactory.StopAllConsumersAsync();
                _logger.LogInformation("Consumidores RabbitMQ parados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao parar consumidores RabbitMQ");
            }
        }
    }
}