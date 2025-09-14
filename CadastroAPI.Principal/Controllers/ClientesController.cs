using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BackEnd.Controllers
{
    [ApiController]    
    [Route("[controller]")]
    public partial class ClientesController(ILogger<ClientesController> logger, IRabbitMQService rabbitMQService, IRedisService redisService, IClienteRepository clienteRepository) : ControllerBase
    {
        // Removed duplicate logger field declaration since it's already defined elsewhere in the class

        private readonly ILogger<ClientesController> _logger = logger;
        private readonly IRabbitMQService _rabbitMQService = rabbitMQService;
        private readonly IRedisService _redisService = redisService;
        private readonly IClienteRepository _clienteRepository = clienteRepository;

        [HttpPost]
        [ProducesResponseType(typeof(Cliente), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateCliente([FromBody] Cliente cliente)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Cliente recebido com sucesso: {Nome}", cliente.Nome);
                
                // Salvar o cliente no PostgreSQL
                await _clienteRepository.AddClienteAsync(cliente);

                // Salvar o cliente no Redis com expiração de 5 minutos
                //await _redisService.SetAsync($"cliente:{cliente.Id}", cliente, TimeSpan.FromMinutes(5));

                // Publicar o cliente na fila do RabbitMQ
                //bool publicacaoSucesso = _rabbitMQService.PublicarMensagem("cliente_criado", JsonSerializer.Serialize(cliente));
                
                //if (!publicacaoSucesso)
                //{
                    //_logger.LogWarning("Falha ao publicar cliente {Nome} na fila RabbitMQ", cliente.Nome);
                    //return StatusCode(500, new { Message = "Cliente recebido, mas houve um erro ao publicá-lo na fila.", Cliente = cliente });
                //}
                
                return CreatedAtAction(nameof(GetClienteById), new { id = cliente.Id }, cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar requisição POST de cliente.");
                return StatusCode(500, new { Message = "Ocorreu um erro interno no servidor.", Error = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Cliente>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes([FromQuery] string? nome, [FromQuery] string? email, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var clientes = await _clienteRepository.GetAllClientesAsync(nome, email, pageNumber, pageSize);
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar clientes.");
                return StatusCode(500, new { Message = "Ocorreu um erro interno no servidor.", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Cliente), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Cliente>> GetClienteById(string id)
        {
            try
            {
                // Tentar buscar do Redis primeiro
                var cliente = await _redisService.GetAsync<Cliente>($"cliente:{id}");
                if (cliente != null)
                {
                    return Ok(cliente);
                }

                // Se não estiver no Redis, buscar do PostgreSQL
                cliente = await _clienteRepository.GetClienteByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound();
                }

                // Armazenar no Redis para futuras requisições
                await _redisService.SetAsync($"cliente:{cliente.Id}", cliente, TimeSpan.FromMinutes(5));
                return Ok(cliente);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar cliente por ID.");
                return StatusCode(500, new { Message = "Ocorreu um erro interno no servidor.", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateCliente(string id, [FromBody] Cliente cliente)
        {
            try
            {
                if (id != cliente.Id)
                {
                    return BadRequest();
                }

                // Verificar se o cliente existe no PostgreSQL
                var existingCliente = await _clienteRepository.GetClienteByIdAsync(id);
                if (existingCliente == null)
                {
                    return NotFound();
                }

                // Atualizar no PostgreSQL
                await _clienteRepository.UpdateClienteAsync(cliente);

                // Atualizar no Redis
                await _redisService.SetAsync($"cliente:{id}", cliente, TimeSpan.FromMinutes(5));
                _rabbitMQService.PublicarMensagem("cliente_atualizado", JsonSerializer.Serialize(cliente));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar cliente.");
                return StatusCode(500, new { Message = "Ocorreu um erro interno no servidor.", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteCliente(string id)
        {
            try
            {
                // Verificar se o cliente existe no PostgreSQL
                var existingCliente = await _clienteRepository.GetClienteByIdAsync(id);
                if (existingCliente == null)
                {
                    return NotFound();
                }

                // Remover do PostgreSQL
                await _clienteRepository.DeleteClienteAsync(id);

                // Remover do Redis
                await _redisService.DeleteAsync($"cliente:{id}");
                _rabbitMQService.PublicarMensagem("cliente_deletado", JsonSerializer.Serialize(existingCliente));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao deletar cliente.");
                return StatusCode(500, new { Message = "Ocorreu um erro interno no servidor.", Error = ex.Message });
            }
        }
    }
}