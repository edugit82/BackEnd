using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BackEnd.Services
{
    /// <summary>
    /// Classe base para processadores de mensagens
    /// </summary>
    /// <typeparam name="T">Tipo de mensagem a ser processada</typeparam>
    public abstract class MessageProcessorBase<T> : IMessageProcessor<T>
    {
        protected readonly ILogger _logger;
        
        /// <summary>
        /// Construtor da classe base
        /// </summary>
        /// <param name="logger">Logger para registro de eventos</param>
        protected MessageProcessorBase(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Processa uma mensagem de forma assíncrona
        /// </summary>
        /// <param name="message">A mensagem a ser processada</param>
        /// <param name="messageId">Identificador único da mensagem</param>
        /// <param name="timestamp">Timestamp de quando a mensagem foi publicada</param>
        /// <returns>Task que representa a operação assíncrona, com resultado indicando se o processamento foi bem-sucedido</returns>
        public async Task<bool> ProcessMessageAsync(T message, string messageId, DateTime timestamp)
        {
            try
            {
                _logger.LogInformation("Iniciando processamento da mensagem {MessageId} recebida em {Timestamp}", messageId, timestamp);
                
                // Validar a mensagem antes de processá-la
                if (!ValidateMessage(message, messageId))
                {
                    _logger.LogWarning("Mensagem {MessageId} inválida. Processamento abortado.", messageId);
                    return false;
                }

                // Processar a mensagem
                bool result = await ProcessInternalAsync(message, messageId, timestamp);
                
                if (result)
                {
                    _logger.LogInformation("Mensagem {MessageId} processada com sucesso", messageId);
                }
                else
                {
                    _logger.LogWarning("Falha ao processar mensagem {MessageId}", messageId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem {MessageId}", messageId);
                return false;
            }
        }

        /// <summary>
        /// Valida a mensagem antes de processá-la
        /// </summary>
        /// <param name="message">A mensagem a ser validada</param>
        /// <param name="messageId">Identificador único da mensagem</param>
        /// <returns>True se a mensagem é válida, False caso contrário</returns>
        protected virtual bool ValidateMessage(T message, string messageId)
        {
            // Validação básica: verificar se a mensagem não é nula
            if (message == null)
            {
                _logger.LogWarning("Mensagem {MessageId} é nula", messageId);
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Implementação interna do processamento da mensagem
        /// </summary>
        /// <param name="message">A mensagem a ser processada</param>
        /// <param name="messageId">Identificador único da mensagem</param>
        /// <param name="timestamp">Timestamp de quando a mensagem foi publicada</param>
        /// <returns>Task que representa a operação assíncrona, com resultado indicando se o processamento foi bem-sucedido</returns>
        protected abstract Task<bool> ProcessInternalAsync(T message, string messageId, DateTime timestamp);
    }
}