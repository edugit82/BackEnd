using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System;
using System.ComponentModel.DataAnnotations;
using Project.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            try
            {
                var clients = await _context.Clients.ToListAsync();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Ocorreu um erro interno ao buscar os clientes.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            try
            {
                var client = await _context.Clients.FindAsync(id);

                if (client == null)
                {
                    return NotFound("Cliente não encontrado.");
                }

                return Ok(client);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Ocorreu um erro interno ao buscar o cliente.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient(int id, [FromBody] Client client)
        {
            if (id != client.Id)
            {
                return BadRequest("ID do cliente não corresponde.");
            }

            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clients.Any(e => e.Id == id))
                {
                    return NotFound("Cliente não encontrado.");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Ocorreu um erro interno ao atualizar o cliente.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            try
            {
                var client = await _context.Clients.FindAsync(id);
                if (client == null)
                {
                    return NotFound("Cliente não encontrado.");
                }

                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Ocorreu um erro interno ao excluir o cliente.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Client client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                return Ok("Dados do cliente recebidos com sucesso!");
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logger)
                return StatusCode(500, "Ocorreu um erro interno ao processar sua solicitação.");
            }
        }
    }
}