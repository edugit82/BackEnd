using BackEnd.Data;
using BackEnd.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly ApplicationDbContext _context;

        public ClienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cliente>> GetAllClientesAsync(string? nome = null, string? email = null, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(c => c.Nome.Contains(nome));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(c => c.Email.Contains(email));
            }

            return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<Cliente?> GetClienteByIdAsync(string id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task AddClienteAsync(Cliente cliente)
        {
            if (cliente.DataNascimento.Kind == DateTimeKind.Unspecified)
            {
                cliente.DataNascimento = DateTime.SpecifyKind(cliente.DataNascimento, DateTimeKind.Utc);
            }
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClienteAsync(Cliente cliente)
        {
            var existingCliente = await _context.Clientes.FindAsync(cliente.Id);
            if (existingCliente == null)
            {
                throw new KeyNotFoundException($"Cliente com ID {cliente.Id} não encontrado.");
            }
            if (cliente.DataNascimento.Kind == DateTimeKind.Unspecified)
            {
                cliente.DataNascimento = DateTime.SpecifyKind(cliente.DataNascimento, DateTimeKind.Utc);
            }
            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClienteAsync(string id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Cliente com ID {id} não encontrado.");
            }
        }
    }
}