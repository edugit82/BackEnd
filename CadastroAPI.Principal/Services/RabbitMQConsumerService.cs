using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BackEnd.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BackEnd.Services
{
    /// <summary>
    /// Serviço para consumo de mensagens do RabbitMQ
    /// </summary>
    /// <typeparam name="T">Tipo de mensagem a ser consumida</typeparam>
    public class RabbitMQConsumerService<T> : IMessageConsumer, IDisposable
    {
        private readonly ILogger<RabbitMQConsumerService<T>> _logger;
        private readonly RabbitMQOptions _rabbitOptions;
        private readonly RabbitMQConsumerOptions _consumerOptions;
        private readonly IMessageProcessor<T> _messageProcessor;
        private RabbitMQ.Client.IConnection? _connection;
        private RabbitMQ.Client.IModel? _channel;
        private string? _consumerTag;
        private CancellationTokenSource? _connectionMonitorCts;
        private Task? _connectionMonitorTask;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _processingLock = new SemaphoreSlim(1, 1);
        private int _reconnectAttempts;
        private bool _isDisposed;

        /// <summary>
        /// Indica se o consumidor está em execução
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Nome da fila que está sendo consumida
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Construtor do serviço de consumo
        /// </summary>
        /// <param name="logger">Logger para registro de eventos</param>
        /// <param name="rabbitOptions">Opções de configuração do RabbitMQ</param>
        /// <param name="consumerOptions">Opções de configuração dos consumidores</param>
        /// <param name="messageProcessor">Processador de mensagens</param>
        /// <param name="queueName">Nome da fila a ser consumida</param>
        public RabbitMQConsumerService(
            ILogger<RabbitMQConsumerService<T>> logger,
            RabbitMQOptions rabbitOptions,
            RabbitMQConsumerOptions consumerOptions,
            IMessageProcessor<T> messageProcessor,
            string queueName)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitOptions = rabbitOptions ?? throw new ArgumentNullException(nameof(rabbitOptions));
            _consumerOptions = consumerOptions ?? throw new ArgumentNullException(nameof(consumerOptions));
            _messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
            QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        }

        /// <summary>
        /// Inicia o consumo de mensagens de forma assíncrona
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (IsRunning)
            {
                return;
            }

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (IsRunning)
                {
                    return;
                }

                await EstabelecerConexaoAsync(cancellationToken);
                
                // Iniciar monitoramento de conexão
                _connectionMonitorCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _connectionMonitorTask = MonitorConnectionAsync(_connectionMonitorCts.Token);
                
                IsRunning = true;
                _logger.ConsumerStarted(QueueName);
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Para o consumo de mensagens de forma assíncrona
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!IsRunning)
            {
                return;
            }

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (!IsRunning)
                {
                    return;
                }

                // Cancelar o consumidor
                if (_channel != null && _channel.IsOpen && !string.IsNullOrEmpty(_consumerTag))
                {
                    _channel.BasicCancel(_consumerTag);
                    _consumerTag = null;
                }

                // Parar o monitoramento de conexão
                if (_connectionMonitorCts != null && _connectionMonitorTask != null)
                {
                    _connectionMonitorCts.Cancel();
                    await Task.WhenAny(_connectionMonitorTask, Task.Delay(5000, cancellationToken)); // Esperar até 5 segundos
                    _connectionMonitorCts.Dispose();
                    _connectionMonitorCts = null;
                }

                // Fechar canal e conexão
                _channel?.Close();
                _channel?.Dispose();
                _channel = null;

                _connection?.Close();
                _connection?.Dispose();
                _connection = null;

                IsRunning = false;
                _reconnectAttempts = 0;
                _logger.ConsumerStopped(QueueName);
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Estabelece conexão com o servidor RabbitMQ
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        private Task EstabelecerConexaoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var factory = new RabbitMQ.Client.ConnectionFactory
                {
                    HostName = _rabbitOptions.HostName,
                    Port = _rabbitOptions.Port,
                    UserName = _rabbitOptions.UserName,
                    Password = _rabbitOptions.Password,
                    VirtualHost = _rabbitOptions.VirtualHost,
                    DispatchConsumersAsync = true, // Habilitar consumo assíncrono
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedHeartbeat = TimeSpan.FromSeconds(60)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Configurar QoS para controlar o número de mensagens não confirmadas por consumidor
                // Isso ajuda na escalabilidade horizontal, garantindo distribuição equilibrada
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

                // Declarar a fila como durável para garantir que as mensagens não sejam perdidas
                _channel.QueueDeclare(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Configurar o consumidor assíncrono
                var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(_channel);
                consumer.Received += HandleMessageReceivedAsync;

                // Iniciar o consumo
                _consumerTag = _channel.BasicConsume(
                    queue: QueueName,
                    autoAck: false, // Desabilitar confirmação automática para garantir processamento
                    consumerTag: Guid.NewGuid().ToString(),
                    noLocal: false,
                    exclusive: false,
                    arguments: null,
                    consumer: consumer);

                _reconnectAttempts = 0;
                _logger.ConnectionEstablished(_rabbitOptions.HostName, _rabbitOptions.Port.ToString());
            }
            catch (Exception ex)
            {
                _logger.ConnectionFailed(_rabbitOptions.HostName, _rabbitOptions.Port.ToString(), ex);
                throw;
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Monitora a conexão e tenta reconectar em caso de falha
        /// </summary>
        private async Task MonitorConnectionAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(5000, cancellationToken); // Verificar a cada 5 segundos

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Verificar se a conexão está aberta
                    if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
                    {
                        await _connectionLock.WaitAsync(cancellationToken);
                        try
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            // Verificar novamente após obter o lock
                            if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
                            {
                                _reconnectAttempts++;
                                _logger.Reconnecting(QueueName, _reconnectAttempts);

                                // Fechar recursos existentes
                                _channel?.Dispose();
                                _channel = null;

                                _connection?.Dispose();
                                _connection = null;

                                // Tentar reconectar com backoff exponencial
                                int delayMs = Math.Min(30000, 1000 * (int)Math.Pow(2, Math.Min(10, _reconnectAttempts)));
                                await Task.Delay(delayMs, cancellationToken);

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                // Tentar estabelecer conexão novamente
                                await EstabelecerConexaoAsync(cancellationToken);
                            }
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            _logger.LogError(ex, "Erro ao tentar reconectar ao RabbitMQ. Tentativa {AttemptNumber}", _reconnectAttempts);
                        }
                        finally
                        {
                            _connectionLock.Release();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no monitoramento de conexão do RabbitMQ");
                }
            }
        }

        /// <summary>
        /// Manipula mensagens recebidas do RabbitMQ
        /// </summary>
        private async Task HandleMessageReceivedAsync(object sender, RabbitMQ.Client.Events.BasicDeliverEventArgs ea)
        {
            string messageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString();
            DateTime timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)ea.BasicProperties.Timestamp.UnixTime).DateTime;
            
            _logger.MessageReceived(messageId, QueueName);

            // Usar semáforo para limitar o processamento paralelo e evitar sobrecarga
            await _processingLock.WaitAsync();
            try
            {
                try
                {
                    // Deserializar a mensagem
                    string message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    T? typedMessage = JsonSerializer.Deserialize<T>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    // Verificar se a mensagem foi deserializada corretamente
                    if (typedMessage == null)
                    {
                        _logger.LogError("Falha ao deserializar a mensagem {MessageId}", messageId);
                        _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                        return;
                    }

                    // Processar a mensagem com timeout
                    var processingTask = _messageProcessor.ProcessMessageAsync(typedMessage, messageId, timestamp);
                    
                    // Aplicar timeout no processamento
                    var timeoutTask = Task.Delay(_consumerOptions.ProcessingTimeoutMs);
                    var completedTask = await Task.WhenAny(processingTask, timeoutTask);
                    
                    bool success;
                    if (completedTask == timeoutTask)
                    {
                        // Timeout no processamento
                        _logger.LogWarning("Timeout ao processar a mensagem {MessageId} após {Timeout}ms", messageId, _consumerOptions.ProcessingTimeoutMs);
                        success = false;
                    }
                    else
                    {
                        // Processamento concluído dentro do timeout
                        success = await processingTask;
                    }

                    if (success)
                    {
                        // Confirmar processamento bem-sucedido
                        _channel?.BasicAck(ea.DeliveryTag, multiple: false);
                        _logger.MessageProcessed(messageId, QueueName);
                    }
                    else
                    {
                        // Verificar se a mensagem tem informações de retry
                        int retryCount = 0;
                        if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var retryObj) && retryObj is byte[] retryBytes)
                        {
                            retryCount = BitConverter.ToInt32(retryBytes, 0);
                        }
                        
                        // Verificar se atingiu o número máximo de retentativas
                        QueueConsumerOptions? queueOptions = null;
                        if (_consumerOptions.QueueOptions.TryGetValue(QueueName, out var options))
                        {
                            queueOptions = options;
                        }
                        int maxRetryCount = queueOptions?.MaxRetryCount ?? 3; // Valor padrão se não configurado
                        bool requeueOnFailure = queueOptions?.RequeueOnFailure ?? true;
                        
                        if (retryCount >= maxRetryCount || !requeueOnFailure)
                        {
                            // Enviar para dead letter queue se configurado ou rejeitar permanentemente
                            _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                            _logger.LogWarning("Processamento da mensagem {MessageId} falhou após {RetryCount} tentativas. Rejeitando permanentemente.", messageId, retryCount);
                        }
                        else
                        {
                            // Calcular delay com backoff exponencial
                            int reconnectInterval = _rabbitOptions.ReconnectInterval;
                            double backoffMultiplier = _rabbitOptions.ReconnectBackoffMultiplier;
                            int delayMs = (int)(reconnectInterval * Math.Pow(backoffMultiplier, retryCount));
                            
                            // Publicar a mensagem novamente com delay
                            if (_channel == null)
                            {
                                _logger.LogError("Canal RabbitMQ não está disponível para republicar a mensagem. Mensagem será rejeitada.");
                                _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                                return;
                            }
                            var properties = _channel.CreateBasicProperties();
                            properties.Persistent = true;
                            properties.Headers = new Dictionary<string, object>
                            {
                                { "x-retry-count", BitConverter.GetBytes(retryCount + 1) },
                                { "x-original-message-id", messageId }
                            };
                            
                            // Confirmar recebimento da mensagem original
                            _channel?.BasicAck(ea.DeliveryTag, multiple: false);
                            
                            // Aguardar o delay calculado
                            await Task.Delay(delayMs);
                            
                            // Republicar a mensagem
                            _channel?.BasicPublish(
                                exchange: "",
                                routingKey: QueueName,
                                mandatory: true,
                                basicProperties: properties,
                                body: ea.Body);
                            
                            _logger.LogWarning("Processamento da mensagem {MessageId} falhou. Tentativa {RetryCount}/{MaxRetryCount} agendada após {DelayMs}ms.", 
                                messageId, retryCount + 1, maxRetryCount, delayMs);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    // Erro de deserialização - rejeitar a mensagem sem recolocá-la na fila
                    _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    _logger.MessageProcessingFailed(messageId, QueueName, ex);
                }
                catch (Exception ex)
                {
                    // Outros erros - recolocar na fila para tentar novamente
                    _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                    _logger.MessageProcessingFailed(messageId, QueueName, ex);
                }
            }
            finally
            {
                _processingLock.Release();
            }
        }

        /// <summary>
        /// Libera recursos não gerenciados
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera recursos não gerenciados e opcionalmente recursos gerenciados
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Parar o consumidor de forma síncrona
                if (IsRunning)
                {
                    StopAsync().GetAwaiter().GetResult();
                }

                // Liberar recursos gerenciados
                _connectionLock?.Dispose();
                _processingLock?.Dispose();
                _connectionMonitorCts?.Dispose();
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Destrutor
        /// </summary>
        ~RabbitMQConsumerService()
        {
            Dispose(false);
        }
    }
}