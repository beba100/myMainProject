using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for Apartment operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ApartmentController : ControllerBase
    {
        private readonly IApartmentService _apartmentService;
        private readonly ILogger<ApartmentController> _logger;

        /// <summary>
        /// Initializes a new instance of the ApartmentController class
        /// </summary>
        /// <param name="apartmentService">Apartment service</param>
        /// <param name="logger">Logger</param>
        public ApartmentController(IApartmentService apartmentService, ILogger<ApartmentController> logger)
        {
            _apartmentService = apartmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all apartments with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for apartment name, code, status, or related entity names</param>
        /// <returns>List of apartments with total count</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllApartments(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                // ✅ Log request details including headers
                var hotelCode = Request.Headers.ContainsKey("X-Hotel-Code") 
                    ? Request.Headers["X-Hotel-Code"].ToString() 
                    : "Not provided";
                
                _logger.LogInformation("📋 [GetAllApartments] Request received: PageNumber={PageNumber}, PageSize={PageSize}, SearchTerm={SearchTerm}, X-Hotel-Code={HotelCode}", 
                    pageNumber, pageSize, searchTerm ?? "null", hotelCode);

                var (apartments, totalCount) = await _apartmentService.GetAllApartmentsAsync(pageNumber, pageSize, searchTerm);
                
                _logger.LogInformation("✅ [GetAllApartments] Successfully retrieved {Count} apartments (Total: {TotalCount}) for HotelCode: {HotelCode}", 
                    apartments.Count(), totalCount, hotelCode);

                // ✅ Log if no apartments found
                if (totalCount == 0)
                {
                    _logger.LogWarning("⚠️ [GetAllApartments] No apartments found for HotelCode: {HotelCode}. This might indicate:", 
                        hotelCode);
                    _logger.LogWarning("   1. No apartments exist in the tenant database for this hotel");
                    _logger.LogWarning("   2. HotelSettings not found or HotelId mismatch");
                    _logger.LogWarning("   3. Tenant.Code ({HotelCode}) does not match any HotelSettings.HotelCode in tenant DB", hotelCode);
                }

                return Ok(new
                {
                    apartments = apartments, // ✅ Changed to lowercase 'apartments' to match frontend expectation
                    totalCount = totalCount,
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "⚠️ [GetAllApartments] Unauthorized: {Message}", ex.Message);
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "❌ [GetAllApartments] Invalid operation: {Message}", ex.Message);
                return StatusCode(500, new { error = "Invalid Operation", message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "❌ [GetAllApartments] Not found: {Message}", ex.Message);
                return NotFound(new { error = "Not Found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetAllApartments] Unexpected error: {Message}", ex.Message);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, details = ex.ToString() });
            }
        }

        /// <summary>
        /// Get apartment by ID
        /// </summary>
        /// <param name="id">Apartment ID</param>
        /// <returns>Apartment details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApartmentById(int id)
        {
            try
            {
                var apartment = await _apartmentService.GetApartmentByIdAsync(id);
                if (apartment == null)
                {
                    return NotFound($"Apartment with ID {id} not found.");
                }

                return Ok(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartment with ID {ApartmentId}", id);
                return StatusCode(500, "An error occurred while retrieving the apartment.");
            }
        }

        /// <summary>
        /// Get apartment by apartment code
        /// </summary>
        /// <param name="apartmentCode">Apartment code</param>
        /// <returns>Apartment details</returns>
        [HttpGet("code/{apartmentCode}")]
        public async Task<IActionResult> GetApartmentByCode(string apartmentCode)
        {
            try
            {
                var apartment = await _apartmentService.GetApartmentByCodeAsync(apartmentCode);
                if (apartment == null)
                {
                    return NotFound($"Apartment with code '{apartmentCode}' not found.");
                }

                return Ok(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartment with code {ApartmentCode}", apartmentCode);
                return StatusCode(500, "An error occurred while retrieving the apartment.");
            }
        }

        /// <summary>
        /// Create new apartment
        /// </summary>
        /// <param name="createApartmentDto">Apartment creation data</param>
        /// <returns>Created apartment</returns>
        [HttpPost]
        public async Task<IActionResult> CreateApartment([FromBody] CreateApartmentDto createApartmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var apartment = await _apartmentService.CreateApartmentAsync(createApartmentDto);
                return CreatedAtAction(nameof(GetApartmentById), new { id = apartment.ApartmentId }, apartment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating apartment");
                return StatusCode(500, "An error occurred while creating the apartment.");
            }
        }

        /// <summary>
        /// Update existing apartment
        /// </summary>
        /// <param name="id">Apartment ID</param>
        /// <param name="updateApartmentDto">Apartment update data</param>
        /// <returns>Updated apartment</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApartment(int id, [FromBody] UpdateApartmentDto updateApartmentDto)
        {
            try
            {
                if (id != updateApartmentDto.ApartmentId)
                {
                    return BadRequest("Apartment ID in URL does not match ID in body.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var apartment = await _apartmentService.UpdateApartmentAsync(id, updateApartmentDto);
                if (apartment == null)
                {
                    return NotFound($"Apartment with ID {id} not found.");
                }

                return Ok(apartment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating apartment with ID {ApartmentId}", id);
                return StatusCode(500, "An error occurred while updating the apartment.");
            }
        }

        /// <summary>
        /// Delete apartment
        /// </summary>
        /// <param name="id">Apartment ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApartment(int id)
        {
            try
            {
                var result = await _apartmentService.DeleteApartmentAsync(id);
                if (!result)
                {
                    return NotFound($"Apartment with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting apartment with ID {ApartmentId}", id);
                return StatusCode(500, "An error occurred while deleting the apartment.");
            }
        }

        /// <summary>
        /// Get apartments by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of apartments for the specified hotel</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetApartmentsByHotelId(int hotelId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByHotelIdAsync(hotelId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving apartments for the hotel.");
            }
        }

        /// <summary>
        /// Get apartments by building ID
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns>List of apartments for the specified building</returns>
        [HttpGet("building/{buildingId}")]
        public async Task<IActionResult> GetApartmentsByBuildingId(int buildingId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByBuildingIdAsync(buildingId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving apartments for the building.");
            }
        }

        /// <summary>
        /// Get apartments by floor ID
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <returns>List of apartments for the specified floor</returns>
        [HttpGet("floor/{floorId}")]
        public async Task<IActionResult> GetApartmentsByFloorId(int floorId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByFloorIdAsync(floorId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving apartments for the floor.");
            }
        }

        /// <summary>
        /// Get apartments by room type ID
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <returns>List of apartments for the specified room type</returns>
        [HttpGet("room-type/{roomTypeId}")]
        public async Task<IActionResult> GetApartmentsByRoomTypeId(int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByRoomTypeIdAsync(roomTypeId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving apartments for the room type.");
            }
        }

        /// <summary>
        /// Get apartments by status
        /// </summary>
        /// <param name="status">Apartment status</param>
        /// <returns>List of apartments with the specified status</returns>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetApartmentsByStatus(string status)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByStatusAsync(status);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments by status {Status}", status);
                return StatusCode(500, "An error occurred while retrieving apartments by status.");
            }
        }

        /// <summary>
        /// Get available apartments
        /// </summary>
        /// <returns>List of available apartments</returns>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableApartments()
        {
            try
            {
                var apartments = await _apartmentService.GetAvailableApartmentsAsync();
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available apartments");
                return StatusCode(500, "An error occurred while retrieving available apartments.");
            }
        }

        /// <summary>
        /// Get occupied apartments
        /// </summary>
        /// <returns>List of occupied apartments</returns>
        [HttpGet("occupied")]
        public async Task<IActionResult> GetOccupiedApartments()
        {
            try
            {
                var apartments = await _apartmentService.GetOccupiedApartmentsAsync();
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving occupied apartments");
                return StatusCode(500, "An error occurred while retrieving occupied apartments.");
            }
        }

        /// <summary>
        /// Get maintenance apartments
        /// </summary>
        /// <returns>List of maintenance apartments</returns>
        [HttpGet("maintenance")]
        public async Task<IActionResult> GetMaintenanceApartments()
        {
            try
            {
                var apartments = await _apartmentService.GetMaintenanceApartmentsAsync();
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance apartments");
                return StatusCode(500, "An error occurred while retrieving maintenance apartments.");
            }
        }

        /// <summary>
        /// Get apartment statistics
        /// </summary>
        /// <returns>Apartment statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetApartmentStatistics()
        {
            try
            {
                var statistics = await _apartmentService.GetApartmentStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartment statistics");
                return StatusCode(500, "An error occurred while retrieving apartment statistics.");
            }
        }

        /// <summary>
        /// Search apartments by name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>List of matching apartments</returns>
        [HttpGet("search/name")]
        public async Task<IActionResult> SearchApartmentsByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Name cannot be empty.");
                }

                var apartments = await _apartmentService.SearchApartmentsByNameAsync(name);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching apartments by name {Name}", name);
                return StatusCode(500, "An error occurred while searching apartments by name.");
            }
        }

        /// <summary>
        /// Search apartments by code
        /// </summary>
        /// <param name="code">Code to search for</param>
        /// <returns>List of matching apartments</returns>
        [HttpGet("search/code")]
        public async Task<IActionResult> SearchApartmentsByCode([FromQuery] string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return BadRequest("Code cannot be empty.");
                }

                var apartments = await _apartmentService.SearchApartmentsByCodeAsync(code);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching apartments by code {Code}", code);
                return StatusCode(500, "An error occurred while searching apartments by code.");
            }
        }

        /// <summary>
        /// Search apartments by hotel name
        /// </summary>
        /// <param name="hotelName">Hotel name to search for</param>
        /// <returns>List of matching apartments</returns>
        [HttpGet("search/hotel")]
        public async Task<IActionResult> SearchApartmentsByHotelName([FromQuery] string hotelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hotelName))
                {
                    return BadRequest("Hotel name cannot be empty.");
                }

                var apartments = await _apartmentService.SearchApartmentsByHotelNameAsync(hotelName);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching apartments by hotel name {HotelName}", hotelName);
                return StatusCode(500, "An error occurred while searching apartments by hotel name.");
            }
        }

        /// <summary>
        /// Search apartments by building name
        /// </summary>
        /// <param name="buildingName">Building name to search for</param>
        /// <returns>List of matching apartments</returns>
        [HttpGet("search/building")]
        public async Task<IActionResult> SearchApartmentsByBuildingName([FromQuery] string buildingName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(buildingName))
                {
                    return BadRequest("Building name cannot be empty.");
                }

                var apartments = await _apartmentService.SearchApartmentsByBuildingNameAsync(buildingName);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching apartments by building name {BuildingName}", buildingName);
                return StatusCode(500, "An error occurred while searching apartments by building name.");
            }
        }

        /// <summary>
        /// Search apartments by floor name
        /// </summary>
        /// <param name="floorName">Floor name to search for</param>
        /// <returns>List of matching apartments</returns>
        [HttpGet("search/floor")]
        public async Task<IActionResult> SearchApartmentsByFloorName([FromQuery] string floorName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(floorName))
                {
                    return BadRequest("Floor name cannot be empty.");
                }

                var apartments = await _apartmentService.SearchApartmentsByFloorNameAsync(floorName);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching apartments by floor name {FloorName}", floorName);
                return StatusCode(500, "An error occurred while searching apartments by floor name.");
            }
        }

        /// <summary>
        /// Search apartments by room type name
        /// </summary>
        /// <param name="roomTypeName">Room type name to search for</param>
        /// <returns>List of matching apartments</returns>
        [HttpGet("search/room-type")]
        public async Task<IActionResult> SearchApartmentsByRoomTypeName([FromQuery] string roomTypeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomTypeName))
                {
                    return BadRequest("Room type name cannot be empty.");
                }

                var apartments = await _apartmentService.SearchApartmentsByRoomTypeNameAsync(roomTypeName);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching apartments by room type name {RoomTypeName}", roomTypeName);
                return StatusCode(500, "An error occurred while searching apartments by room type name.");
            }
        }

        /// <summary>
        /// Check if apartment code exists
        /// </summary>
        /// <param name="apartmentCode">Apartment code to check</param>
        /// <param name="excludeId">Apartment ID to exclude from check (for updates)</param>
        /// <returns>True if code exists, false otherwise</returns>
        [HttpGet("check-code")]
        public async Task<IActionResult> CheckApartmentCode([FromQuery] string apartmentCode, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apartmentCode))
                {
                    return BadRequest("Apartment code cannot be empty.");
                }

                var exists = await _apartmentService.ApartmentCodeExistsAsync(apartmentCode, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking apartment code {ApartmentCode}", apartmentCode);
                return StatusCode(500, "An error occurred while checking apartment code.");
            }
        }

        /// <summary>
        /// Get apartments with reservations
        /// </summary>
        /// <returns>List of apartments that have reservations</returns>
        [HttpGet("with-reservations")]
        public async Task<IActionResult> GetApartmentsWithReservations()
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsWithReservationsAsync();
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments with reservations");
                return StatusCode(500, "An error occurred while retrieving apartments with reservations.");
            }
        }

        /// <summary>
        /// Get apartments without reservations
        /// </summary>
        /// <returns>List of apartments that have no reservations</returns>
        [HttpGet("without-reservations")]
        public async Task<IActionResult> GetApartmentsWithoutReservations()
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsWithoutReservationsAsync();
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments without reservations");
                return StatusCode(500, "An error occurred while retrieving apartments without reservations.");
            }
        }

        /// <summary>
        /// Get apartments by reservation count range
        /// </summary>
        /// <param name="minCount">Minimum reservation count</param>
        /// <param name="maxCount">Maximum reservation count</param>
        /// <returns>List of apartments with reservation count in the specified range</returns>
        [HttpGet("reservation-count-range")]
        public async Task<IActionResult> GetApartmentsByReservationCountRange([FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByReservationCountRangeAsync(minCount, maxCount);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments by reservation count range");
                return StatusCode(500, "An error occurred while retrieving apartments by reservation count range.");
            }
        }

        /// <summary>
        /// Get top apartments by reservation count
        /// </summary>
        /// <param name="topCount">Number of top apartments to return (default: 10)</param>
        /// <returns>List of top apartments by reservation count</returns>
        [HttpGet("top-by-reservations")]
        public async Task<IActionResult> GetTopApartmentsByReservationCount([FromQuery] int topCount = 10)
        {
            try
            {
                var apartments = await _apartmentService.GetTopApartmentsByReservationCountAsync(topCount);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top apartments by reservation count");
                return StatusCode(500, "An error occurred while retrieving top apartments by reservation count.");
            }
        }

        /// <summary>
        /// Get apartments by revenue range
        /// </summary>
        /// <param name="minRevenue">Minimum revenue</param>
        /// <param name="maxRevenue">Maximum revenue</param>
        /// <returns>List of apartments with revenue in the specified range</returns>
        [HttpGet("revenue-range")]
        public async Task<IActionResult> GetApartmentsByRevenueRange([FromQuery] decimal minRevenue, [FromQuery] decimal maxRevenue)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByRevenueRangeAsync(minRevenue, maxRevenue);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments by revenue range");
                return StatusCode(500, "An error occurred while retrieving apartments by revenue range.");
            }
        }

        /// <summary>
        /// Get top apartments by revenue
        /// </summary>
        /// <param name="topCount">Number of top apartments to return (default: 10)</param>
        /// <returns>List of top apartments by revenue</returns>
        [HttpGet("top-by-revenue")]
        public async Task<IActionResult> GetTopApartmentsByRevenue([FromQuery] int topCount = 10)
        {
            try
            {
                var apartments = await _apartmentService.GetTopApartmentsByRevenueAsync(topCount);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top apartments by revenue");
                return StatusCode(500, "An error occurred while retrieving top apartments by revenue.");
            }
        }

        /// <summary>
        /// Get available apartments for date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of apartments available for the specified date range</returns>
        [HttpGet("available-for-date-range")]
        public async Task<IActionResult> GetAvailableApartmentsForDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var apartments = await _apartmentService.GetAvailableApartmentsForDateRangeAsync(startDate, endDate);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available apartments for date range");
                return StatusCode(500, "An error occurred while retrieving available apartments for date range.");
            }
        }

        /// <summary>
        /// Get apartments with overlapping reservations
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of apartments with overlapping reservations</returns>
        [HttpGet("with-overlapping-reservations")]
        public async Task<IActionResult> GetApartmentsWithOverlappingReservations([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsWithOverlappingReservationsAsync(startDate, endDate);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments with overlapping reservations");
                return StatusCode(500, "An error occurred while retrieving apartments with overlapping reservations.");
            }
        }

        /// <summary>
        /// Get apartments by hotel and building
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="buildingId">Building ID</param>
        /// <returns>List of apartments for the specified hotel and building</returns>
        [HttpGet("hotel/{hotelId}/building/{buildingId}")]
        public async Task<IActionResult> GetApartmentsByHotelAndBuilding(int hotelId, int buildingId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByHotelAndBuildingAsync(hotelId, buildingId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments by hotel and building");
                return StatusCode(500, "An error occurred while retrieving apartments by hotel and building.");
            }
        }

        /// <summary>
        /// Get apartments by hotel and floor
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="floorId">Floor ID</param>
        /// <returns>List of apartments for the specified hotel and floor</returns>
        [HttpGet("hotel/{hotelId}/floor/{floorId}")]
        public async Task<IActionResult> GetApartmentsByHotelAndFloor(int hotelId, int floorId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByHotelAndFloorAsync(hotelId, floorId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments by hotel and floor");
                return StatusCode(500, "An error occurred while retrieving apartments by hotel and floor.");
            }
        }

        /// <summary>
        /// Get apartments by hotel and room type
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="roomTypeId">Room type ID</param>
        /// <returns>List of apartments for the specified hotel and room type</returns>
        [HttpGet("hotel/{hotelId}/room-type/{roomTypeId}")]
        public async Task<IActionResult> GetApartmentsByHotelAndRoomType(int hotelId, int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByHotelAndRoomTypeAsync(hotelId, roomTypeId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments by hotel and room type");
                return StatusCode(500, "An error occurred while retrieving apartments by hotel and room type.");
            }
        }

        /// <summary>
        /// Get apartments by building and floor
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="floorId">Floor ID</param>
        /// <returns>List of apartments for the specified building and floor</returns>
        [HttpGet("building/{buildingId}/floor/{floorId}")]
        public async Task<IActionResult> GetApartmentsByBuildingAndFloor(int buildingId, int floorId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByBuildingAndFloorAsync(buildingId, floorId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments by building and floor");
                return StatusCode(500, "An error occurred while retrieving apartments by building and floor.");
            }
        }

        /// <summary>
        /// Get apartments by building and room type
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="roomTypeId">Room type ID</param>
        /// <returns>List of apartments for the specified building and room type</returns>
        [HttpGet("building/{buildingId}/room-type/{roomTypeId}")]
        public async Task<IActionResult> GetApartmentsByBuildingAndRoomType(int buildingId, int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByBuildingAndRoomTypeAsync(buildingId, roomTypeId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments by building and room type");
                return StatusCode(500, "An error occurred while retrieving apartments by building and room type.");
            }
        }

        /// <summary>
        /// Get apartments by floor and room type
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="roomTypeId">Room type ID</param>
        /// <returns>List of apartments for the specified floor and room type</returns>
        [HttpGet("floor/{floorId}/room-type/{roomTypeId}")]
        public async Task<IActionResult> GetApartmentsByFloorAndRoomType(int floorId, int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByFloorAndRoomTypeAsync(floorId, roomTypeId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments by floor and room type");
                return StatusCode(500, "An error occurred while retrieving apartments by floor and room type.");
            }
        }

        /// <summary>
        /// Get apartments by multiple criteria
        /// </summary>
        /// <param name="hotelId">Hotel ID (optional)</param>
        /// <param name="buildingId">Building ID (optional)</param>
        /// <param name="floorId">Floor ID (optional)</param>
        /// <param name="roomTypeId">Room type ID (optional)</param>
        /// <param name="status">Status (optional)</param>
        /// <returns>List of apartments matching the specified criteria</returns>
        [HttpGet("filter")]
        public async Task<IActionResult> GetApartmentsByMultipleCriteria(
            [FromQuery] int? hotelId = null, 
            [FromQuery] int? buildingId = null, 
            [FromQuery] int? floorId = null, 
            [FromQuery] int? roomTypeId = null, 
            [FromQuery] string? status = null)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByMultipleCriteriaAsync(hotelId, buildingId, floorId, roomTypeId, status);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments by multiple criteria");
                return StatusCode(500, "An error occurred while retrieving apartments by multiple criteria.");
            }
        }

        /// <summary>
        /// Get apartment occupancy rate
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Occupancy rate for the specified apartment and date range</returns>
        [HttpGet("{apartmentId}/occupancy-rate")]
        public async Task<IActionResult> GetApartmentOccupancyRate(int apartmentId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var occupancyRate = await _apartmentService.GetApartmentOccupancyRateAsync(apartmentId, startDate, endDate);
                return Ok(new { ApartmentId = apartmentId, StartDate = startDate, EndDate = endDate, OccupancyRate = occupancyRate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartment occupancy rate for apartment {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while retrieving apartment occupancy rate.");
            }
        }

        /// <summary>
        /// Get apartment revenue
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Revenue for the specified apartment and date range</returns>
        [HttpGet("{apartmentId}/revenue")]
        public async Task<IActionResult> GetApartmentRevenue(int apartmentId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var revenue = await _apartmentService.GetApartmentRevenueAsync(apartmentId, startDate, endDate);
                return Ok(new { ApartmentId = apartmentId, StartDate = startDate, EndDate = endDate, Revenue = revenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartment revenue for apartment {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while retrieving apartment revenue.");
            }
        }

        /// <summary>
        /// Get apartment reservation count
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Reservation count for the specified apartment and date range</returns>
        [HttpGet("{apartmentId}/reservation-count")]
        public async Task<IActionResult> GetApartmentReservationCount(int apartmentId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var count = await _apartmentService.GetApartmentReservationCountAsync(apartmentId, startDate, endDate);
                return Ok(new { ApartmentId = apartmentId, StartDate = startDate, EndDate = endDate, Count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartment reservation count for apartment {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while retrieving apartment reservation count.");
            }
        }

        /// <summary>
        /// Get apartment average stay duration
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Average stay duration for the specified apartment and date range</returns>
        [HttpGet("{apartmentId}/average-stay-duration")]
        public async Task<IActionResult> GetApartmentAverageStayDuration(int apartmentId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var averageDuration = await _apartmentService.GetApartmentAverageStayDurationAsync(apartmentId, startDate, endDate);
                return Ok(new { ApartmentId = apartmentId, StartDate = startDate, EndDate = endDate, AverageStayDuration = averageDuration });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartment average stay duration for apartment {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while retrieving apartment average stay duration.");
            }
        }

        /// <summary>
        /// Get apartment utilization statistics
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Utilization statistics for the specified apartment and date range</returns>
        [HttpGet("{apartmentId}/utilization-statistics")]
        public async Task<IActionResult> GetApartmentUtilizationStatistics(int apartmentId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _apartmentService.GetApartmentUtilizationStatisticsAsync(apartmentId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartment utilization statistics for apartment {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while retrieving apartment utilization statistics.");
            }
        }

        /// <summary>
        /// Update apartment status
        /// </summary>
        /// <param name="id">Apartment ID</param>
        /// <param name="status">New status</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateApartmentStatus(int id, [FromBody] string status)
        {
            try
            {
                var result = await _apartmentService.UpdateApartmentStatusAsync(id, status);
                if (!result)
                {
                    return NotFound($"Apartment with ID {id} not found.");
                }

                return Ok(new { Message = "Apartment status updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating apartment status for ID {ApartmentId}", id);
                return StatusCode(500, "An error occurred while updating apartment status.");
            }
        }

        /// <summary>
        /// Check apartment availability for date range
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>True if apartment is available, false otherwise</returns>
        [HttpGet("{apartmentId}/check-availability")]
        public async Task<IActionResult> CheckApartmentAvailability(int apartmentId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var isAvailable = await _apartmentService.CheckApartmentAvailabilityAsync(apartmentId, startDate, endDate);
                return Ok(new { ApartmentId = apartmentId, StartDate = startDate, EndDate = endDate, IsAvailable = isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking apartment availability for apartment {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while checking apartment availability.");
            }
        }
    }
}
