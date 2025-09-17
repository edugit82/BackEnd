using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Project.Data;
using Project.Models;

namespace Project.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientService> _logger;

        public ClientService(ApplicationDbContext context, ILogger<ClientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddClientAsync(string message)
        {
            try
            {
                var client = JsonSerializer.Deserialize<Client>(message);
                if (client != null)
                {
                    // Remover caracteres não numéricos do CPF e Telefone
                    client.CPF = RemoveNonNumeric(client.CPF);
                    client.Telefone = RemoveNonNumeric(client.Telefone);

                    // Truncar CPF para 11 dígitos se for maior
                    if (client.CPF.Length > 11)
                    {
                        client.CPF = client.CPF.Substring(0, 11);
                    }

                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Cliente adicionado ao banco de dados: {client.Nome}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar cliente ao banco de dados.");
            }
        }

        private string RemoveNonNumeric(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            return new string(text.Where(char.IsDigit).ToArray());
        }
    }
}