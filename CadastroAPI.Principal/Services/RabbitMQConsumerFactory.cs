using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackEnd.Services
{
    /// <summary>
    /// Fábrica para criar e gerenciar múltiplos consumidores RabbitMQ
    /// </summary>
    public class RabbitMQConsumerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQConsumerFactory> _logger;
        private readonly RabbitMQOptions _rabbitOptions;
        private readonly RabbitMQConsumerOptions _consumerOptions;
        private readonly Dictionary<string, List<IMessageConsumer>> _consumers = new Dictionary<string, List<IMessageConsumer>>();

        /// <summary>
        /// Construtor da fábrica de consumidores
        /// </summary>
        /// <param name="serviceProvider">Provedor de serviços para criar instâncias de consumidores</param>
        /// <param name="logger">Logger para registro de eventos</param>
        /// <param name="rabbitOptions">Opções de configuração do RabbitMQ</param>
        /// <param name="consumerOptions">Opções de configuração dos consumidores</param>
        public RabbitMQConsumerFactory(
            IServiceProvider serviceProvider,
            ILogger<RabbitMQConsumerFactory> logger,
            IOptions<RabbitMQOptions> rabbitOptions,
            IOptions<RabbitMQConsumerOptions> consumerOptions)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitOptions = rabbitOptions?.Value ?? throw new ArgumentNullException(nameof(rabbitOptions));
            _consumerOptions = consumerOptions?.Value ?? throw new ArgumentNullException(nameof(consumerOptions));
        }

        /// <summary>
        /// Cria e inicia consumidores para uma fila específica
        /// </summary>
        /// <typeparam name="T">Tipo de mensagem a ser consumida</typeparam>
        /// <typeparam name="TProcessor">Tipo do processador de mensagens</typeparam>
        /// <param name="queueName">Nome da fila</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        public async Task CreateAndStartConsumersAsync<T, TProcessor>(string queueName)
            where TProcessor : class, IMessageProcessor<T>
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("O nome da fila não pode ser nulo ou vazio", nameof(queueName));
            }

            // Determinar o número de consumidores para esta fila
            int consumerCount = _consumerOptions.ConsumerCount;
            if (_consumerOptions.QueueOptions.TryGetValue(queueName, out var queueOptions) && queueOptions.ConsumerCount.HasValue)
            {
                consumerCount = queueOptions.ConsumerCount.Value;
            }

            _logger.LogInformation("Criando {ConsumerCount} consumidores para a fila {QueueName}", consumerCount, queueName);

            var consumers = new List<IMessageConsumer>();

            // Criar e iniciar os consumidores
            for (int i = 0; i < consumerCount; i++)
            {
                // Criar o processador de mensagens usando o container de DI
                var processor = _serviceProvider.GetRequiredService<TProcessor>();

                // Criar o consumidor
                var consumer = ActivatorUtilities.CreateInstance<RabbitMQConsumerService<T>>(
                    _serviceProvider,
                    _rabbitOptions,
                    _consumerOptions,
                    processor,
                    queueName);

                // Iniciar o consumidor
                await consumer.StartAsync();
                consumers.Add(consumer);

                _logger.LogInformation("Consumidor {ConsumerNumber} iniciado para a fila {QueueName}", i + 1, queueName);
            }

            // Armazenar os consumidores para gerenciamento posterior
            _consumers[queueName] = consumers;
        }

        /// <summary>
        /// Para todos os consumidores de uma fila específica
        /// </summary>
        /// <param name="queueName">Nome da fila</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        public async Task StopConsumersAsync(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("O nome da fila não pode ser nulo ou vazio", nameof(queueName));
            }

            if (_consumers.TryGetValue(queueName, out var consumers))
            {
                _logger.LogInformation("Parando {ConsumerCount} consumidores para a fila {QueueName}", consumers.Count, queueName);

                foreach (var consumer in consumers)
                {
                    await consumer.StopAsync();
                }

                _consumers.Remove(queueName);
            }
        }

        /// <summary>
        /// Para todos os consumidores de todas as filas
        /// </summary>
        /// <returns>Task que representa a operação assíncrona</returns>
        public async Task StopAllConsumersAsync()
        {
            foreach (var queueName in new List<string>(_consumers.Keys))
            {
                await StopConsumersAsync(queueName);
            }
        }
    }
}