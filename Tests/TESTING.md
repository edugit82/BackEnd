# Documentação de Testes

## 1. Visão Geral

Este documento descreve a estrutura, a escrita e a execução dos testes automatizados para o projeto. Os testes são essenciais para garantir a qualidade, a confiabilidade e o comportamento esperado do código, especialmente em cenários de cache com Redis.

## 2. Estrutura do Projeto de Testes

Os testes estão localizados no diretório `Tests` e são organizados da seguinte forma:

- `Tests.csproj`: O arquivo de projeto C# para os testes, contendo as referências aos pacotes NuGet necessários (Xunit, Moq, etc.).
- `RedisCacheServiceTests.cs`: Contém os testes unitários para a classe `RedisCacheService`, que lida com as operações de cache usando Redis.

## 3. Como Escrever Testes

Para escrever novos testes, siga as diretrizes abaixo:

### 3.1. Convenções de Nomenclatura

- **Classes de Teste**: Devem seguir o padrão `[NomeDaClasseSendoTestada]Tests.cs` (ex: `RedisCacheServiceTests.cs`).
- **Métodos de Teste**: Devem ser descritivos e seguir o padrão `[NomeDoMetodoSendoTestado]_[Cenario]_[ComportamentoEsperado]` (ex: `SetAsync_ShouldSerializeAndSetDataInRedis`).

### 3.2. Estrutura de um Teste Unitário (AAA - Arrange, Act, Assert)

Cada método de teste deve seguir a estrutura AAA:

1.  **Arrange**: Configure o ambiente de teste, inicialize objetos, mocks e dados de entrada.
2.  **Act**: Execute a ação que está sendo testada (chame o método do serviço).
3.  **Assert**: Verifique se o resultado da ação é o esperado e se as interações com os mocks ocorreram conforme o planejado.

### 3.3. Uso de Mocks com Moq

Utilizamos a biblioteca [Moq](https://github.com/moq/moq4) para criar objetos mock (simulados) de dependências, permitindo testar unidades de código isoladamente. 

**Exemplo de Mocking:**

```csharp
// Arrange
var mockDatabase = new Mock<IDatabase>();
var service = new RedisCacheService(mockOptions.Object, mockLogger.Object, mockConnectionMultiplexer.Object, mockDatabase.Object);

// Configurar o mock para um método específico
mockDatabase.Setup(db => db.StringGetAsync(
    It.IsAny<RedisKey>(),
    CommandFlags.None))
    .ReturnsAsync("serializedValue");

// Verificar se um método foi chamado
mockDatabase.Verify(db => db.StringSetAsync(
    It.Is<RedisKey>(k => k.ToString().Contains("testKey")),
    It.Is<RedisValue>(v => v.ToString() == "testValue")),
    Times.Once);
```

## 4. Como Executar Testes

Para executar os testes do projeto, siga os passos abaixo:

1.  **Navegue até o diretório `BackEnd`**:
    ```bash
    cd d:\Repositorio\BackEnd
    ```

2.  **Execute os testes usando o comando `dotnet test`**: 
    Especifique o caminho para o arquivo `.csproj` do projeto de testes.
    ```bash
    dotnet test Tests\Tests.csproj
    ```

    Este comando compilará e executará todos os testes definidos no projeto `Tests`. O resultado mostrará quais testes passaram, falharam ou foram ignorados.