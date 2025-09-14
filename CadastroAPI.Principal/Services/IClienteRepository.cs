using BackEnd.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface IClienteRepository
    {
        Task<IEnumerable<Cliente>> GetAllClientesAsync(string? nome = null, string? email = null, int pageNumber = 1, int pageSize = 10);
        Task<Cliente?> GetClienteByIdAsync(string id);
        Task AddClienteAsync(Cliente cliente);
        Task UpdateClienteAsync(Cliente cliente);
        Task DeleteClienteAsync(string id);
    }
}