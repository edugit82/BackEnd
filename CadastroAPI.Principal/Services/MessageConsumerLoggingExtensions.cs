using System;
using Microsoft.Extensions.Logging;

namespace BackEnd.Services
{
    /// <summary>
    /// Extensões para logging do consumidor de mensagens
    /// </summary>
    public static class MessageConsumerLoggingExtensions
    {
        // Definição de eventos de log para o consumidor de mensagens
        private static readonly Action<ILogger, string, string, Exception> _connectionEstablished =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1000, nameof(ConnectionEstablished)),
                "Conexão estabelecida com RabbitMQ em {HostName}:{Port}");

        private static readonly Action<ILogger, string, string, Exception> _connectionFailed =
            LoggerMessage.Define<string, string>(
                LogLevel.Error,
                new EventId(1001, nameof(ConnectionFailed)),
                "Falha ao conectar com RabbitMQ em {HostName}:{Port}");

        private static readonly Action<ILogger, string, Exception> _consumerStarted =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1002, nameof(ConsumerStarted)),
                "Consumidor iniciado para a fila {QueueName}");

        private static readonly Action<ILogger, string, Exception> _consumerStopped =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1003, nameof(ConsumerStopped)),
                "Consumidor parado para a fila {QueueName}");

        private static readonly Action<ILogger, string, string, Exception> _messageReceived =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(1004, nameof(MessageReceived)),
                "Mensagem {MessageId} recebida na fila {QueueName}");

        private static readonly Action<ILogger, string, string, Exception> _messageProcessed =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1005, nameof(MessageProcessed)),
                "Mensagem {MessageId} processada com sucesso na fila {QueueName}");

        private static readonly Action<ILogger, string, string, Exception> _messageProcessingFailed =
            LoggerMessage.Define<string, string>(
                LogLevel.Error,
                new EventId(1006, nameof(MessageProcessingFailed)),
                "Falha ao processar mensagem {MessageId} na fila {QueueName}");

        private static readonly Action<ILogger, string, int, Exception> _reconnecting =
            LoggerMessage.Define<string, int>(
                LogLevel.Warning,
                new EventId(1007, nameof(Reconnecting)),
                "Tentando reconectar ao RabbitMQ para a fila {QueueName}. Tentativa {AttemptNumber}");

        /// <summary>
        /// Registra que uma conexão foi estabelecida com o RabbitMQ
        /// </summary>
        public static void ConnectionEstablished(this ILogger logger, string hostName, string port) =>
            _connectionEstablished(logger, hostName, port, null!);

        /// <summary>
        /// Registra que houve falha ao conectar com o RabbitMQ
        /// </summary>
        public static void ConnectionFailed(this ILogger logger, string hostName, string port, Exception ex) =>
            _connectionFailed(logger, hostName, port, ex);

        /// <summary>
        /// Registra que o consumidor foi iniciado
        /// </summary>
        public static void ConsumerStarted(this ILogger logger, string queueName) =>
            _consumerStarted(logger, queueName, null!);

        /// <summary>
        /// Registra que o consumidor foi parado
        /// </summary>
        public static void ConsumerStopped(this ILogger logger, string queueName) =>
            _consumerStopped(logger, queueName, null!);

        /// <summary>
        /// Registra que uma mensagem foi recebida
        /// </summary>
        public static void MessageReceived(this ILogger logger, string messageId, string queueName) =>
            _messageReceived(logger, messageId, queueName, null!);

        /// <summary>
        /// Registra que uma mensagem foi processada com sucesso
        /// </summary>
        public static void MessageProcessed(this ILogger logger, string messageId, string queueName) =>
            _messageProcessed(logger, messageId, queueName, null!);

        /// <summary>
        /// Registra que houve falha ao processar uma mensagem
        /// </summary>
        public static void MessageProcessingFailed(this ILogger logger, string messageId, string queueName, Exception ex) =>
            _messageProcessingFailed(logger, messageId, queueName, ex);

        /// <summary>
        /// Registra que está tentando reconectar ao RabbitMQ
        /// </summary>
        public static void Reconnecting(this ILogger logger, string queueName, int attemptNumber) =>
            _reconnecting(logger, queueName, attemptNumber, null!);
    }
}