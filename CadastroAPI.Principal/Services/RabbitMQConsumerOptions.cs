using System.Collections.Generic;

namespace BackEnd.Services
{
    /// <summary>
    /// Opções de configuração para consumidores RabbitMQ
    /// </summary>
    public class RabbitMQConsumerOptions
    {
        /// <summary>
        /// Número de instâncias de consumidores por fila
        /// </summary>
        /// <remarks>
        /// Aumentar este valor permite processar mais mensagens em paralelo,
        /// facilitando a escalabilidade horizontal.
        /// </remarks>
        public int ConsumerCount { get; set; } = 1;

        /// <summary>
        /// Número máximo de mensagens que um consumidor pode processar simultaneamente
        /// </summary>
        /// <remarks>
        /// Este valor controla o prefetch count do RabbitMQ e afeta diretamente
        /// a distribuição de carga entre os consumidores.
        /// </remarks>
        public int PrefetchCount { get; set; } = 10;

        /// <summary>
        /// Tempo máximo em milissegundos para processar uma mensagem
        /// </summary>
        /// <remarks>
        /// Se o processamento exceder este tempo, a mensagem será considerada como falha
        /// e poderá ser recolocada na fila dependendo da configuração.
        /// </remarks>
        public int ProcessingTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Configurações específicas para cada fila
        /// </summary>
        public Dictionary<string, QueueConsumerOptions> QueueOptions { get; set; } = new Dictionary<string, QueueConsumerOptions>();
    }

    /// <summary>
    /// Opções de configuração para uma fila específica
    /// </summary>
    public class QueueConsumerOptions
    {
        /// <summary>
        /// Nome da fila
        /// </summary>
        public string QueueName { get; set; } = string.Empty;

        /// <summary>
        /// Número de instâncias de consumidores para esta fila específica
        /// </summary>
        /// <remarks>
        /// Se definido, substitui o valor global ConsumerCount
        /// </remarks>
        public int? ConsumerCount { get; set; }

        /// <summary>
        /// Número máximo de mensagens que um consumidor pode processar simultaneamente para esta fila
        /// </summary>
        /// <remarks>
        /// Se definido, substitui o valor global PrefetchCount
        /// </remarks>
        public int? PrefetchCount { get; set; }

        /// <summary>
        /// Indica se as mensagens devem ser recolocadas na fila em caso de falha no processamento
        /// </summary>
        public bool RequeueOnFailure { get; set; } = true;

        /// <summary>
        /// Número máximo de tentativas de processamento antes de mover a mensagem para uma fila de mensagens mortas
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// Nome da fila de mensagens mortas (DLQ) para onde as mensagens serão enviadas após exceder o número máximo de tentativas
        /// </summary>
        public string DeadLetterQueue { get; set; } = string.Empty;
    }
}