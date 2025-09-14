using System;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    /// <summary>
    /// Interface para processamento de mensagens
    /// </summary>
    /// <typeparam name="T">Tipo de mensagem a ser processada</typeparam>
    public interface IMessageProcessor<T>
    {
        /// <summary>
        /// Processa uma mensagem de forma assíncrona
        /// </summary>
        /// <param name="message">A mensagem a ser processada</param>
        /// <param name="messageId">Identificador único da mensagem</param>
        /// <param name="timestamp">Timestamp de quando a mensagem foi publicada</param>
        /// <returns>Task que representa a operação assíncrona, com resultado indicando se o processamento foi bem-sucedido</returns>
        Task<bool> ProcessMessageAsync(T message, string messageId, DateTime timestamp);
    }
}