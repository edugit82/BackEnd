using System;
using System.Threading.Tasks;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MensageriaController : ControllerBase
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<MensageriaController> _logger;

        public MensageriaController(IRabbitMQService rabbitMQService, ILogger<MensageriaController> logger)
        {
            _rabbitMQService = rabbitMQService ?? throw new ArgumentNullException(nameof(rabbitMQService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Publica um cliente na fila do RabbitMQ
        /// </summary>
        /// <param name="cliente">O objeto cliente a ser publicado</param>
        /// <returns>Resultado da operação</returns>
        [HttpPost("cliente")]
        public IActionResult PublicarCliente([FromBody] Cliente? cliente)
        {
            if (cliente == null)
            {
                return BadRequest("Cliente não pode ser nulo");
            }

            try
            {
                bool resultado = _rabbitMQService.PublicarCliente(cliente);

                if (resultado)
                {
                    return Ok(new { Mensagem = $"Cliente {cliente.Nome} publicado com sucesso na fila" });
                }
                else
                {
                    return StatusCode(500, new { Erro = "Não foi possível publicar o cliente na fila" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar cliente {Nome} na fila", cliente.Nome);
                return StatusCode(500, new { Erro = $"Erro ao publicar cliente: {ex.Message}" });
            }
        }

        /// <summary>
        /// Publica uma mensagem genérica na fila especificada
        /// </summary>
        /// <typeparam name="T">Tipo da mensagem</typeparam>
        /// <param name="mensagem">A mensagem a ser publicada</param>
        /// <param name="fila">Nome da fila</param>
        /// <returns>Resultado da operação</returns>
        [HttpPost("publicar")]
        public async Task<IActionResult> PublicarMensagem<T>([FromBody] T mensagem, [FromQuery] string fila)
        {
            if (mensagem == null)
            {
                return BadRequest("Mensagem não pode ser nula");
            }

            if (string.IsNullOrEmpty(fila))
            {
                return BadRequest("Nome da fila não pode ser nulo ou vazio");
            }

            try
            {
                bool resultado = await _rabbitMQService.PublicarMensagemAsync(mensagem, fila);

                if (resultado)
                {
                    return Ok(new { Mensagem = $"Mensagem do tipo {typeof(T).Name} publicada com sucesso na fila {fila}" });
                }
                else
                {
                    return StatusCode(500, new { Erro = $"Não foi possível publicar a mensagem na fila {fila}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar mensagem do tipo {TipoMensagem} na fila {FilaNome}", typeof(T).Name, fila);
                return StatusCode(500, new { Erro = $"Erro ao publicar mensagem: {ex.Message}" });
            }
        }

        /// <summary>
        /// Publica uma mensagem de teste na fila especificada
        /// </summary>
        /// <param name="fila">Nome da fila</param>
        /// <returns>Resultado da operação</returns>
        [HttpPost("teste")]
        public async Task<IActionResult> PublicarMensagemTeste([FromQuery] string fila)
        {
            if (string.IsNullOrEmpty(fila))
            {
                return BadRequest("Nome da fila não pode ser nulo ou vazio");
            }

            try
            {
                var mensagemTeste = new { Id = Guid.NewGuid(), Texto = "Mensagem de teste", Timestamp = DateTime.UtcNow };
                bool resultado = await _rabbitMQService.PublicarMensagemAsync(mensagemTeste, fila);

                if (resultado)
                {
                    return Ok(new { Mensagem = $"Mensagem de teste publicada com sucesso na fila {fila}" });
                }
                else
                {
                    return StatusCode(500, new { Erro = $"Não foi possível publicar a mensagem de teste na fila {fila}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar mensagem de teste na fila {FilaNome}", fila);
                return StatusCode(500, new { Erro = $"Erro ao publicar mensagem de teste: {ex.Message}" });
            }
        }
    }
}