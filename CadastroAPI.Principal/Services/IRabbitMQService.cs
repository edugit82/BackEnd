using BackEnd.Models;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface IRabbitMQService
    {
        /// <summary>
        /// Publica um cliente na fila do RabbitMQ
        /// </summary>
        /// <param name="cliente">O objeto cliente a ser publicado</param>
        /// <returns>True se a publicação foi bem-sucedida, False caso contrário</returns>
        bool PublicarCliente(Cliente cliente);

        /// <summary>
        /// Publica uma mensagem na fila especificada
        /// </summary>
        /// <typeparam name="T">Tipo da mensagem</typeparam>
        /// <param name="message">Mensagem a ser publicada</param>
        /// <param name="queueName">Nome da fila</param>
        /// <returns>True se a publicação foi bem-sucedida, False caso contrário</returns>
        bool PublicarMensagem<T>(T message, string queueName);

        /// <summary>
        /// Publica uma mensagem na fila especificada de forma assíncrona
        /// </summary>
        /// <typeparam name="T">Tipo da mensagem</typeparam>
        /// <param name="message">Mensagem a ser publicada</param>
        /// <param name="queueName">Nome da fila</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        Task<bool> PublicarMensagemAsync<T>(T message, string queueName);
    }
}