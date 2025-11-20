using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        /// Get available hotels/tenants based on user role and permissions
        /// - Admin/Manager/Accountant: All hotels
        /// - Supervisor/Staff/ReadOnly: Hotels from UserTenants table only
        /// </summary>
        /// <returns>List of hotels the user has access to</returns>
        [HttpGet("hotels")]
        [Authorize] // ✅ Requires JWT Token
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAllHotels()
		{
			try
			{
				// ✅ استخراج معلومات المستخدم من JWT Token
				var userIdClaim = User.FindFirst("userId")?.Value ?? HttpContext.Items["UserId"]?.ToString();
				var rolesClaim = User.FindFirst("roles")?.Value ?? HttpContext.Items["Roles"]?.ToString();
				var usernameClaim = User.FindFirst("username")?.Value ?? HttpContext.Items["Username"]?.ToString();

				if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
				{
					_logger.LogWarning("⚠️ [GetAllHotels] UserId not found in JWT token");
					return Unauthorized(new { error = "User information not found in token" });
				}

				var roles = rolesClaim?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) 
					?? Array.Empty<string>();
				var username = usernameClaim ?? "Unknown";

				_logger.LogInformation("📋 [GetAllHotels] User: {Username} (Id: {UserId}), Roles: [{Roles}]", 
					username, userId, string.Join(", ", roles));

				// ✅ تحديد الأدوار التي ترى كل الفنادق
				var fullAccessRoles = new[] { "Admin", "Manager", "Accountant" };
				var hasFullAccess = roles.Any(r => fullAccessRoles.Contains(r, StringComparer.OrdinalIgnoreCase));

				var hotels = new List<object>();

				if (hasFullAccess)
				{
					// ✅ Admin/Manager/Accountant: كل الفنادق
					_logger.LogInformation("🔓 [GetAllHotels] User has full access role - returning all hotels");
					
					var allHotels = await _masterDbContext.Tenants
						.AsNoTracking()
						.Select(t => new
						{
							t.Id,
							t.Code,
							t.Name,
							t.BaseUrl
						})
						.OrderBy(t => t.Id)
						.ToListAsync();

					hotels = allHotels.Cast<object>().ToList();

					_logger.LogInformation("✅ [GetAllHotels] User {Username} (Full Access) - Retrieved {Count} hotels", 
						username, hotels.Count);
				}
				else
				{
					// ✅ Supervisor/Staff/ReadOnly: الفنادق من UserTenants فقط
					_logger.LogInformation("🔒 [GetAllHotels] User has limited access - fetching hotels from UserTenants");

					var userHotels = await (from ut in _masterDbContext.UserTenants
											join t in _masterDbContext.Tenants on ut.TenantId equals t.Id
											where ut.UserId == userId
											select new
											{
												t.Id,
												t.Code,
												t.Name,
												t.BaseUrl
											})
											.OrderBy(t => t.Id)
											.ToListAsync();

					hotels = userHotels.Cast<object>().ToList();
					var hotelCodes = userHotels.Select(h => h.Code).ToList();

					_logger.LogInformation("✅ [GetAllHotels] User {Username} (Limited Access) - Retrieved {Count} hotels from UserTenants: [{HotelCodes}]", 
						username, hotels.Count, string.Join(", ", hotelCodes));

					// ✅ تحذير إذا لم يكن لدى المستخدم أي فنادق
					if (hotels.Count == 0)
					{
						_logger.LogWarning("⚠️ [GetAllHotels] User {Username} (Id: {UserId}) has no hotels assigned in UserTenants table", 
							username, userId);
					}
				}

				return Ok(hotels);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ [GetAllHotels] Error fetching hotels: {Message}", ex.Message);
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

