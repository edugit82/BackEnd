using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BackEnd.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BackEnd.Services
{
    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private RabbitMQ.Client.IConnection? _connection;
        private RabbitMQ.Client.IModel? _channel;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly RabbitMQOptions _options;
        private const string QueueName = "clientes_queue";

        public RabbitMQService(IOptions<RabbitMQOptions> options, ILogger<RabbitMQService> logger)
        {
            _options = options.Value;
            _logger = logger;

            EstabelecerConexao();
        }

        private bool EstabelecerConexao()
        {
            try
            {
                if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
                {
                    return true; // Conexão já está estabelecida
                }

                // Fechar conexões existentes se houver
                if (_channel != null)
                {
                    _channel.Dispose();
                }
                if (_connection != null)
                {
                    _connection.Dispose();
                }

                var factory = new RabbitMQ.Client.ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost,
                    // Configurações adicionais para melhorar a resiliência
                    RequestedHeartbeat = TimeSpan.FromSeconds(30),
                    AutomaticRecoveryEnabled = true, // Habilita a recuperação automática
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10) // Intervalo para tentar reconectar
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar a fila para garantir que ela existe
                _channel.QueueDeclare(
                    queue: QueueName,
                    durable: true,  // A fila sobreviverá a reinicializações do servidor
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao estabelecer conexão com RabbitMQ");
                return false;
            }
        }

        public bool PublicarCliente(Cliente cliente)
        {
            return PublicarMensagem(cliente, QueueName);
        }

        public bool PublicarMensagem<T>(T message, string queueName)
        {
            // Número máximo de tentativas
            const int maxTentativas = 3;
            int tentativaAtual = 0;
            
            while (tentativaAtual < maxTentativas)
            {
                tentativaAtual++;
                
                try
                {
                    // Verificar se o canal está aberto, tentar reconectar se necessário
                    if (_channel == null || _channel.IsClosed)
                    {
                        _logger.LogWarning("Canal fechado ou nulo. Tentando reconectar... (Tentativa {Tentativa}/{MaxTentativas})", tentativaAtual, maxTentativas);
                        
                        if (!EstabelecerConexao())
                        {
                            if (tentativaAtual < maxTentativas)
                            {
                                // Aguardar antes de tentar novamente
                                Thread.Sleep(1000 * tentativaAtual); // Backoff exponencial
                                continue;
                            }
                            else
                            {
                                _logger.LogError("Falha ao reconectar com RabbitMQ após {Tentativas} tentativas", maxTentativas);
                                return false;
                            }
                        }
                    }
                    
                    // Verificar novamente se o canal está disponível após a tentativa de reconexão
                    if (_channel == null)
                    {
                        _logger.LogError("Canal ainda é nulo após tentativa de reconexão");
                        return false;
                    }

                    // Garantir que a fila existe
                    _channel.QueueDeclare(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    // Serializar o objeto para JSON
                    string mensagem = JsonSerializer.Serialize(message);
                    var body = Encoding.UTF8.GetBytes(mensagem);

                    // Configurar as propriedades da mensagem
                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true; // Mensagem persistente para não perder em caso de reinicialização
                    properties.MessageId = Guid.NewGuid().ToString();
                    properties.Timestamp = new RabbitMQ.Client.AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    properties.ContentType = "application/json";
                    properties.Type = typeof(T).FullName;
                    
                    // Configurar confirmação de publicação
                    _channel.ConfirmSelect();

                    // Publicar a mensagem na fila
                    _channel.BasicPublish(
                        exchange: "",
                        routingKey: queueName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);

                    // Aguardar confirmação de entrega com timeout
                    bool confirmado = _channel.WaitForConfirms(TimeSpan.FromSeconds(5));
                    
                    if (confirmado)
                    {
                        _logger.LogInformation("Mensagem do tipo {TipoMensagem} publicada com sucesso na fila {FilaNome}", typeof(T).Name, queueName);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Não foi possível confirmar a entrega da mensagem do tipo {TipoMensagem} na fila {FilaNome} (Tentativa {Tentativa}/{MaxTentativas})", typeof(T).Name, queueName, tentativaAtual, maxTentativas);
                        
                        if (tentativaAtual < maxTentativas)
                        {
                            // Aguardar antes de tentar novamente
                            Thread.Sleep(1000 * tentativaAtual); // Backoff exponencial
                            continue;
                        }
                    }
                    
                    return false;
                }
                catch (RabbitMQ.Client.Exceptions.AlreadyClosedException ex)
                {
                    _logger.LogError(ex, "Conexão com RabbitMQ já está fechada ao tentar publicar mensagem do tipo {TipoMensagem} na fila {FilaNome} (Tentativa {Tentativa}/{MaxTentativas})", typeof(T).Name, queueName, tentativaAtual, maxTentativas);
                    
                    if (tentativaAtual < maxTentativas && EstabelecerConexao())
                    {
                        // Aguardar antes de tentar novamente
                        Thread.Sleep(1000 * tentativaAtual); // Backoff exponencial
                        continue;
                    }
                }
                catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
                {
                    _logger.LogError(ex, "Operação interrompida ao publicar mensagem do tipo {TipoMensagem} na fila {FilaNome} (Tentativa {Tentativa}/{MaxTentativas})", typeof(T).Name, queueName, tentativaAtual, maxTentativas);
                    
                    if (tentativaAtual < maxTentativas)
                    {
                        // Aguardar antes de tentar novamente
                        Thread.Sleep(1000 * tentativaAtual); // Backoff exponencial
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao publicar mensagem do tipo {TipoMensagem} na fila {FilaNome} (Tentativa {Tentativa}/{MaxTentativas})", typeof(T).Name, queueName, tentativaAtual, maxTentativas);
                    
                    if (tentativaAtual < maxTentativas)
                    {
                        // Aguardar antes de tentar novamente
                        Thread.Sleep(1000 * tentativaAtual); // Backoff exponencial
                        continue;
                    }
                }
            }
            
            return false; // Todas as tentativas falharam
        }

        public async Task<bool> PublicarMensagemAsync<T>(T message, string queueName)
        {
            return await Task.Run(() => PublicarMensagem(message, queueName));
        }

        public void Dispose()
        {
            if (_channel != null)
            {
                if (_channel.IsOpen)
                {
                    _channel.Close();
                }
                _channel.Dispose();
            }
            
            if (_connection != null)
            {
                if (_connection.IsOpen)
                {
                    _connection.Close();
                }
                _connection.Dispose();
            }
        }
    }
}