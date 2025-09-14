# Sistema de Mensageria RabbitMQ

## Visão Geral

Este sistema implementa uma solução robusta de mensageria utilizando RabbitMQ para processamento assíncrono de mensagens. A implementação segue boas práticas de design e padrões de projeto para garantir confiabilidade, escalabilidade e manutenibilidade.

## Componentes Principais

### Classes de Configuração

- **RabbitMQOptions**: Configurações globais para conexão com o RabbitMQ
  - Configurações de conexão (HostName, Port, UserName, Password, VirtualHost)
  - Configurações de segurança (UseSsl)
  - Configurações de reconexão (MaxReconnectAttempts, ReconnectInterval, ReconnectBackoffMultiplier)

- **RabbitMQConsumerOptions**: Configurações específicas para consumidores
  - Configurações globais (ConsumerCount, PrefetchCount, ProcessingTimeoutMs)
  - Configurações por fila (QueueOptions)
    - Nome da fila
    - Configurações de retry (RequeueOnFailure, MaxRetryCount)
    - Configurações de dead letter queue

### Serviços

- **RabbitMQConsumerService**: Implementa a lógica de consumo de mensagens
  - Gerenciamento de conexão e reconexão automática
  - Processamento de mensagens com timeout
  - Mecanismo de retry com backoff exponencial
  - Tratamento de erros e logging abrangente

- **RabbitMQConsumerFactory**: Fábrica para criação de consumidores
  - Criação e inicialização de múltiplos consumidores para uma fila
  - Injeção de dependências para processadores de mensagens

- **MessageProcessorBase**: Classe base para processadores de mensagens
  - Implementação comum de validação e processamento
  - Logging padronizado

## Melhorias Implementadas

### Segurança

- Remoção de credenciais hardcoded
- Configurações padrão seguras
- Suporte a SSL para comunicação segura

### Robustez

- Validação de timeout no processamento de mensagens
- Mecanismo de retry com backoff exponencial para falhas de processamento
- Tratamento adequado de erros e exceções
- Monitoramento de conexão e reconexão automática

### Extensibilidade

- Design modular com interfaces bem definidas
- Configurações flexíveis por fila
- Suporte a diferentes tipos de mensagens e processadores

## Uso

### Configuração

```csharp
// Em Program.cs ou Startup.cs
services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));
services.Configure<RabbitMQConsumerOptions>(configuration.GetSection("RabbitMQConsumer"));

// Registrar serviços
services.AddSingleton<RabbitMQConsumerFactory>();
services.AddHostedService<RabbitMQConsumerHostedService>();

// Registrar processadores de mensagens
services.AddTransient<IMessageProcessor<Cliente>, ClienteMessageProcessor>();
```

### Exemplo de configuração no appsettings.json

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "UseSsl": false,
    "ConnectionTimeout": 30000,
    "MaxReconnectAttempts": 5,
    "ReconnectInterval": 5000,
    "ReconnectBackoffMultiplier": 2.0
  },
  "RabbitMQConsumer": {
    "ConsumerCount": 1,
    "PrefetchCount": 10,
    "ProcessingTimeoutMs": 30000,
    "QueueOptions": [
      {
        "QueueName": "clientes",
        "ConsumerCount": 2,
        "PrefetchCount": 5,
        "RequeueOnFailure": true,
        "MaxRetryCount": 3,
        "DeadLetterQueue": "clientes.dlq"
      }
    ]
  }
}
```

## Boas Práticas

1. **Configuração**: Nunca hardcode credenciais ou configurações sensíveis
2. **Logging**: Utilize as extensões de logging para rastreabilidade completa
3. **Tratamento de Erros**: Implemente validações adequadas e tratamento de exceções
4. **Retry**: Configure adequadamente os parâmetros de retry para cada tipo de mensagem
5. **Monitoramento**: Utilize os logs para monitorar o funcionamento do sistema