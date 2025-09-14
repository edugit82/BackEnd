using System;
using System.Threading.Tasks;
using BackEnd.Models;
using Microsoft.Extensions.Logging;

namespace BackEnd.Services
{
    /// <summary>
    /// Processador de mensagens de Cliente
    /// </summary>
    public class ClienteMessageProcessor : MessageProcessorBase<Cliente>
    {
        /// <summary>
        /// Construtor do processador de mensagens de Cliente
        /// </summary>
        /// <param name="logger">Logger para registro de eventos</param>
        public ClienteMessageProcessor(ILogger<ClienteMessageProcessor> logger) : base(logger)
        {
        }

        /// <summary>
        /// Valida a mensagem de Cliente antes de processá-la
        /// </summary>
        /// <param name="message">A mensagem a ser validada</param>
        /// <param name="messageId">Identificador único da mensagem</param>
        /// <returns>True se a mensagem é válida, False caso contrário</returns>
        protected override bool ValidateMessage(Cliente message, string messageId)
        {
            if (!base.ValidateMessage(message, messageId))
            {
                return false;
            }

            // Validações específicas para Cliente
            if (string.IsNullOrWhiteSpace(message.Nome))
            {
                _logger.LogWarning("Cliente {MessageId} com nome inválido", messageId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(message.Email))
            {
                _logger.LogWarning("Cliente {MessageId} com email inválido", messageId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(message.Cpf))
            {
                _logger.LogWarning("Cliente {MessageId} com CPF inválido", messageId);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Implementação do processamento da mensagem de Cliente
        /// </summary>
        /// <param name="message">A mensagem a ser processada</param>
        /// <param name="messageId">Identificador único da mensagem</param>
        /// <param name="timestamp">Timestamp de quando a mensagem foi publicada</param>
        /// <returns>Task que representa a operação assíncrona, com resultado indicando se o processamento foi bem-sucedido</returns>
        protected override async Task<bool> ProcessInternalAsync(Cliente message, string messageId, DateTime timestamp)
        {
            try
            {
                // Aqui seria implementada a lógica de processamento do cliente
                // Por exemplo, salvar no banco de dados, enviar notificações, etc.
                _logger.LogInformation("Processando cliente: {Nome}, Email: {Email}, CPF: {CPF}", 
                    message.Nome, message.Email, message.Cpf);

                // Simulação de processamento assíncrono
                await Task.Delay(100);

                // Registrar o sucesso do processamento
                _logger.LogInformation("Cliente {Nome} processado com sucesso. MessageId: {MessageId}, Timestamp: {Timestamp}", 
                    message.Nome, messageId, timestamp);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar cliente {Nome}. MessageId: {MessageId}", 
                    message.Nome, messageId);
                return false;
            }
        }
    }
}