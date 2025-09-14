using Microsoft.AspNetCore.Mvc;
using BackEnd.Services;
using System.Threading.Tasks;
using System;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheController(ILogger<CacheController> logger, IRedisService redisService) : ControllerBase
    {
        private readonly ILogger<CacheController> _logger = logger;
        private readonly IRedisService _redisService = redisService;

        [HttpPost("set/{key}")]
        public async Task<IActionResult> SetCache(string key, [FromBody] dynamic value, [FromQuery] int? expiryMinutes = null)
        {
            try
            {
                TimeSpan? expiry = expiryMinutes.HasValue ? TimeSpan.FromMinutes(expiryMinutes.Value) : (TimeSpan?)null;
                await _redisService.SetAsync(key, value, expiry);
                _logger.LogInformation("Cache set for key: {Key}", key);
                return Ok(new { Message = "Cache set successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
                return StatusCode(500, new { Message = "Error setting cache.", Error = ex.Message });
            }
        }

        [HttpGet("get/{key}")]
        public async Task<IActionResult> GetCache(string key)
        {
            try
            {
                var value = await _redisService.GetAsync<dynamic>(key);
                if (value == null)
                {
                    _logger.LogInformation("Cache miss for key: {Key}", key);
                    return NotFound(new { Message = "Cache key not found." });
                }
                _logger.LogInformation("Cache hit for key: {Key}", key);
                return Ok(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache for key: {Key}", key);
                return StatusCode(500, new { Message = "Error getting cache.", Error = ex.Message });
            }
        }

        [HttpDelete("delete/{key}")]
        public async Task<IActionResult> DeleteCache(string key)
        {
            try
            {
                bool deleted = await _redisService.DeleteAsync(key);
                if (deleted)
                {
                    _logger.LogInformation("Cache deleted for key: {Key}", key);
                    return Ok(new { Message = "Cache deleted successfully." });
                }
                _logger.LogInformation("Cache key not found for deletion: {Key}", key);
                return NotFound(new { Message = "Cache key not found for deletion." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cache for key: {Key}", key);
                return StatusCode(500, new { Message = "Error deleting cache.", Error = ex.Message });
            }
        }
    }
}