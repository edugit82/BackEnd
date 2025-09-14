using System;

namespace BackEnd.Services
{
    /// <summary>
    /// Opções de configuração para conexão com o RabbitMQ
    /// </summary>
    public class RabbitMQOptions
    {
        /// <summary>
        /// Endereço do servidor RabbitMQ
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// Porta do servidor RabbitMQ
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// Nome de usuário para autenticação no RabbitMQ
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// Senha para autenticação no RabbitMQ
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Virtual host no RabbitMQ
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// Tempo limite para conexão em milissegundos
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30000;

        /// <summary>
        /// Indica se deve usar SSL/TLS para conexão
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// Número máximo de tentativas de reconexão
        /// </summary>
        public int MaxReconnectAttempts { get; set; } = 5;

        /// <summary>
        /// Intervalo inicial entre tentativas de reconexão em milissegundos
        /// </summary>
        public int ReconnectInterval { get; set; } = 5000;

        /// <summary>
        /// Fator de multiplicação para backoff exponencial entre tentativas
        /// </summary>
        public double ReconnectBackoffMultiplier { get; set; } = 2.0;
    }
}