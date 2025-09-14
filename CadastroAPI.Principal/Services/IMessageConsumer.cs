using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    /// <summary>
    /// Interface para consumo de mensagens de filas
    /// </summary>
    public interface IMessageConsumer : IDisposable
    {
        /// <summary>
        /// Inicia o consumo de mensagens de forma assíncrona
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Para o consumo de mensagens de forma assíncrona
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica se o consumidor está em execução
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Nome da fila que está sendo consumida
        /// </summary>
        string QueueName { get; }
    }
}