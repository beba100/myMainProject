using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;

namespace zaaerIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();
                
                return Ok(new
                {
                    Status = "Healthy",
                    Database = canConnect ? "Connected" : "Disconnected",
                    Timestamp = KsaTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Unhealthy",
                    Error = ex.Message,
                    Timestamp = KsaTime.Now
                });
            }
        }
    }
}
