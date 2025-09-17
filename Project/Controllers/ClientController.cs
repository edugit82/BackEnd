using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System;
using System.ComponentModel.DataAnnotations;
using Project.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Project.Messaging;
using System.Text.Json;
using Project.Services;
using Microsoft.Extensions.Logging; // Adicionar este using

namespace Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;        
        private readonly IMessageProducer _messageProducer; // Adicionar este campo
        private readonly IRedisCacheService _redisCache; // Adicionar este campo
        private readonly ILogger<ClientController> _logger; // Adicionar este campo
        private readonly IClientService _clientService; // Adicionar este campo

        public ClientController(ApplicationDbContext context, IMessageProducer messageProducer, IRedisCacheService redisCache, ILogger<ClientController> logger, IClientService clientService)
        {
            _context = context;            
            _messageProducer = messageProducer; // Inicializar
            _redisCache = redisCache; // Inicializar
            _logger = logger; // Inicializar
            _clientService = clientService; // Inicializar
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            try
            {
                var clients = await _context.Clients.ToListAsync();
                return Ok(clients);
            }
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
                var clientJson = JsonSerializer.Serialize(client);
                await _clientService.AddClientAsync(clientJson);

                return Ok("Dados do cliente recebidos com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro interno ao processar sua solicitação.");
                return StatusCode(500, "Ocorreu um erro interno ao processar sua solicitação.");
            }
        }
    }
}