using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.Data;
using Microsoft.EntityFrameworkCore;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for managing tenant/hotel information from Master DB
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly MasterDbContext _masterDbContext;
        private readonly ILogger<TenantController> _logger;

        public TenantController(MasterDbContext masterDbContext, ILogger<TenantController> logger)
        {
            _masterDbContext = masterDbContext;
            _logger = logger;
        }

        /// <summary>
        /// Get all available hotels/tenants from Master DB
        /// </summary>
        /// <returns>List of all hotels with their codes and names</returns>
        [HttpGet("hotels")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAllHotels()
		{
			try
			{
				_logger.LogInformation("📋 Fetching all hotels from Master DB");

				var hotels = await _masterDbContext.Tenants
					.AsNoTracking()
					.Select(t => new
					{
						t.Id,
						t.Code,
						t.Name,
						t.BaseUrl
					})
					.OrderBy(t => t.Id)   // <-- Order by ID here
					.ToListAsync();

				_logger.LogInformation("✅ Successfully retrieved {Count} hotels from Master DB", hotels.Count);

				return Ok(hotels);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Error fetching hotels from Master DB: {Message}", ex.Message);
				return StatusCode(500, new { error = "Failed to fetch hotels", details = ex.Message });
			}
		}


		/// <summary>
		/// Get a specific hotel by code
		/// </summary>
		/// <param name="code">Hotel code (e.g., Dammam1)</param>
		/// <returns>Hotel information</returns>
		[HttpGet("hotels/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotelByCode(string code)
        {
            try
            {
                _logger.LogInformation("🔍 Searching for hotel with code: {Code}", code);

                var hotel = await _masterDbContext.Tenants
                    .AsNoTracking()
                    .Where(t => t.Code == code)
                    .Select(t => new
                    {
                        t.Id,
                        t.Code,
                        t.Name,
                        t.BaseUrl
                    })
                    .FirstOrDefaultAsync();

                if (hotel == null)
                {
                    _logger.LogWarning("⚠️ Hotel not found with code: {Code}", code);
                    return NotFound(new { error = $"Hotel not found with code: {code}" });
                }

                _logger.LogInformation("✅ Hotel found: {Name} ({Code})", hotel.Name, hotel.Code);

                return Ok(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching hotel by code: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch hotel", details = ex.Message });
            }
        }
    }
}

