using MadrasaSiddeeqiyahAPI.Data;
using MadrasaSiddeeqiyahAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MadrasaSiddeeqiyahAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHostEnvironment _env;

        public HealthController(AppDbContext context, IHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Check API and Database health status
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<HealthCheckResponseDto>> GetHealth()
        {
            var dbHealthy = false;
            try
            {
                dbHealthy = await _context.Database.CanConnectAsync();
            }
            catch
            {
                dbHealthy = false;
            }

            var response = new HealthCheckResponseDto
            {
                Status = dbHealthy ? "Healthy" : "Degraded",
                Database = dbHealthy ? "Connected" : "Disconnected",
                ServerTimeUtc = DateTime.UtcNow,
                Environment = _env.EnvironmentName
            };

            if (!dbHealthy)
            {
                return StatusCode(503, response);
            }

            return Ok(response);
        }
    }
}
