using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for RoomType operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RoomTypeController : ControllerBase
    {
        private readonly IRoomTypeService _roomTypeService;
        private readonly ILogger<RoomTypeController> _logger;

        /// <summary>
        /// Initializes a new instance of the RoomTypeController class
        /// </summary>
        /// <param name="roomTypeService">RoomType service</param>
        /// <param name="logger">Logger</param>
        public RoomTypeController(IRoomTypeService roomTypeService, ILogger<RoomTypeController> logger)
        {
            _roomTypeService = roomTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all room types with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for room type name, description, or hotel name</param>
        /// <returns>List of room types with total count</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllRoomTypes(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (roomTypes, totalCount) = await _roomTypeService.GetAllRoomTypesAsync(pageNumber, pageSize, searchTerm);
                
                return Ok(new
                {
                    RoomTypes = roomTypes,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types");
                return StatusCode(500, "An error occurred while retrieving room types.");
            }
        }

        /// <summary>
        /// Get room type by ID
        /// </summary>
        /// <param name="id">Room type ID</param>
        /// <returns>Room type details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomTypeById(int id)
        {
            try
            {
                var roomType = await _roomTypeService.GetRoomTypeByIdAsync(id);
                if (roomType == null)
                {
                    return NotFound($"Room type with ID {id} not found.");
                }

                return Ok(roomType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type with ID {RoomTypeId}", id);
                return StatusCode(500, "An error occurred while retrieving the room type.");
            }
        }

        /// <summary>
        /// Get room type by name in hotel
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="roomTypeName">Room type name</param>
        /// <returns>Room type details</returns>
        [HttpGet("hotel/{hotelId}/name/{roomTypeName}")]
        public async Task<IActionResult> GetRoomTypeByName(int hotelId, string roomTypeName)
        {
            try
            {
                var roomType = await _roomTypeService.GetRoomTypeByNameAsync(hotelId, roomTypeName);
                if (roomType == null)
                {
                    return NotFound($"Room type with name '{roomTypeName}' not found in hotel {hotelId}.");
                }

                return Ok(roomType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type with name {RoomTypeName} in hotel {HotelId}", roomTypeName, hotelId);
                return StatusCode(500, "An error occurred while retrieving the room type.");
            }
        }

        /// <summary>
        /// Create new room type
        /// </summary>
        /// <param name="createRoomTypeDto">Room type creation data</param>
        /// <returns>Created room type</returns>
        [HttpPost]
        public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeDto createRoomTypeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var roomType = await _roomTypeService.CreateRoomTypeAsync(createRoomTypeDto);
                return CreatedAtAction(nameof(GetRoomTypeById), new { id = roomType.RoomTypeId }, roomType);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room type");
                return StatusCode(500, "An error occurred while creating the room type.");
            }
        }

        /// <summary>
        /// Update existing room type
        /// </summary>
        /// <param name="id">Room type ID</param>
        /// <param name="updateRoomTypeDto">Room type update data</param>
        /// <returns>Updated room type</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoomType(int id, [FromBody] UpdateRoomTypeDto updateRoomTypeDto)
        {
            try
            {
                if (id != updateRoomTypeDto.RoomTypeId)
                {
                    return BadRequest("Room type ID in URL does not match ID in body.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var roomType = await _roomTypeService.UpdateRoomTypeAsync(id, updateRoomTypeDto);
                if (roomType == null)
                {
                    return NotFound($"Room type with ID {id} not found.");
                }

                return Ok(roomType);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room type with ID {RoomTypeId}", id);
                return StatusCode(500, "An error occurred while updating the room type.");
            }
        }

        /// <summary>
        /// Delete room type
        /// </summary>
        /// <param name="id">Room type ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoomType(int id)
        {
            try
            {
                var result = await _roomTypeService.DeleteRoomTypeAsync(id);
                if (!result)
                {
                    return NotFound($"Room type with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room type with ID {RoomTypeId}", id);
                return StatusCode(500, "An error occurred while deleting the room type.");
            }
        }

        /// <summary>
        /// Get room types by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of room types for the specified hotel</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetRoomTypesByHotelId(int hotelId)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByHotelIdAsync(hotelId);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving room types for the hotel.");
            }
        }

        /// <summary>
        /// Get room types by room type name
        /// </summary>
        /// <param name="roomTypeName">Room type name</param>
        /// <returns>List of room types with the specified name</returns>
        [HttpGet("name/{roomTypeName}")]
        public async Task<IActionResult> GetRoomTypesByName(string roomTypeName)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByNameAsync(roomTypeName);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by name {RoomTypeName}", roomTypeName);
                return StatusCode(500, "An error occurred while retrieving room types by name.");
            }
        }

        /// <summary>
        /// Get room type statistics
        /// </summary>
        /// <returns>Room type statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetRoomTypeStatistics()
        {
            try
            {
                var statistics = await _roomTypeService.GetRoomTypeStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type statistics");
                return StatusCode(500, "An error occurred while retrieving room type statistics.");
            }
        }

        /// <summary>
        /// Search room types by name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>List of matching room types</returns>
        [HttpGet("search/name")]
        public async Task<IActionResult> SearchRoomTypesByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Name cannot be empty.");
                }

                var roomTypes = await _roomTypeService.SearchRoomTypesByNameAsync(name);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching room types by name {Name}", name);
                return StatusCode(500, "An error occurred while searching room types by name.");
            }
        }

        /// <summary>
        /// Search room types by description
        /// </summary>
        /// <param name="description">Description to search for</param>
        /// <returns>List of matching room types</returns>
        [HttpGet("search/description")]
        public async Task<IActionResult> SearchRoomTypesByDescription([FromQuery] string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    return BadRequest("Description cannot be empty.");
                }

                var roomTypes = await _roomTypeService.SearchRoomTypesByDescriptionAsync(description);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching room types by description {Description}", description);
                return StatusCode(500, "An error occurred while searching room types by description.");
            }
        }

        /// <summary>
        /// Search room types by hotel name
        /// </summary>
        /// <param name="hotelName">Hotel name to search for</param>
        /// <returns>List of matching room types</returns>
        [HttpGet("search/hotel")]
        public async Task<IActionResult> SearchRoomTypesByHotelName([FromQuery] string hotelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hotelName))
                {
                    return BadRequest("Hotel name cannot be empty.");
                }

                var roomTypes = await _roomTypeService.SearchRoomTypesByHotelNameAsync(hotelName);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching room types by hotel name {HotelName}", hotelName);
                return StatusCode(500, "An error occurred while searching room types by hotel name.");
            }
        }

        /// <summary>
        /// Check if room type name exists in hotel
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="roomTypeName">Room type name to check</param>
        /// <param name="excludeId">Room type ID to exclude from check (for updates)</param>
        /// <returns>True if name exists, false otherwise</returns>
        [HttpGet("check-name")]
        public async Task<IActionResult> CheckRoomTypeName([FromQuery] int hotelId, [FromQuery] string roomTypeName, [FromQuery] int? excludeId = null)
        {
            try
            {
                var exists = await _roomTypeService.RoomTypeNameExistsAsync(hotelId, roomTypeName, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room type name {RoomTypeName} in hotel {HotelId}", roomTypeName, hotelId);
                return StatusCode(500, "An error occurred while checking room type name.");
            }
        }

        /// <summary>
        /// Get room types with apartments
        /// </summary>
        /// <returns>List of room types that have apartments</returns>
        [HttpGet("with-apartments")]
        public async Task<IActionResult> GetRoomTypesWithApartments()
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesWithApartmentsAsync();
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types with apartments");
                return StatusCode(500, "An error occurred while retrieving room types with apartments.");
            }
        }

        /// <summary>
        /// Get room types without apartments
        /// </summary>
        /// <returns>List of room types that have no apartments</returns>
        [HttpGet("without-apartments")]
        public async Task<IActionResult> GetRoomTypesWithoutApartments()
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesWithoutApartmentsAsync();
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types without apartments");
                return StatusCode(500, "An error occurred while retrieving room types without apartments.");
            }
        }

        /// <summary>
        /// Get room types by apartment count range
        /// </summary>
        /// <param name="minCount">Minimum apartment count</param>
        /// <param name="maxCount">Maximum apartment count</param>
        /// <returns>List of room types with apartment count in the specified range</returns>
        [HttpGet("apartment-count-range")]
        public async Task<IActionResult> GetRoomTypesByApartmentCountRange([FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByApartmentCountRangeAsync(minCount, maxCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by apartment count range");
                return StatusCode(500, "An error occurred while retrieving room types by apartment count range.");
            }
        }

        /// <summary>
        /// Get top room types by apartment count
        /// </summary>
        /// <param name="topCount">Number of top room types to return (default: 10)</param>
        /// <returns>List of top room types by apartment count</returns>
        [HttpGet("top-by-apartments")]
        public async Task<IActionResult> GetTopRoomTypesByApartmentCount([FromQuery] int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetTopRoomTypesByApartmentCountAsync(topCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top room types by apartment count");
                return StatusCode(500, "An error occurred while retrieving top room types by apartment count.");
            }
        }

        /// <summary>
        /// Get room types by base rate range
        /// </summary>
        /// <param name="minRate">Minimum base rate</param>
        /// <param name="maxRate">Maximum base rate</param>
        /// <returns>List of room types with base rate in the specified range</returns>
        [HttpGet("base-rate-range")]
        public async Task<IActionResult> GetRoomTypesByBaseRateRange([FromQuery] decimal minRate, [FromQuery] decimal maxRate)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByBaseRateRangeAsync(minRate, maxRate);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by base rate range");
                return StatusCode(500, "An error occurred while retrieving room types by base rate range.");
            }
        }

        /// <summary>
        /// Get top room types by base rate
        /// </summary>
        /// <param name="topCount">Number of top room types to return (default: 10)</param>
        /// <returns>List of top room types by base rate</returns>
        [HttpGet("top-by-base-rate")]
        public async Task<IActionResult> GetTopRoomTypesByBaseRate([FromQuery] int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetTopRoomTypesByBaseRateAsync(topCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top room types by base rate");
                return StatusCode(500, "An error occurred while retrieving top room types by base rate.");
            }
        }

        /// <summary>
        /// Get room types by revenue range
        /// </summary>
        /// <param name="minRevenue">Minimum revenue</param>
        /// <param name="maxRevenue">Maximum revenue</param>
        /// <returns>List of room types with revenue in the specified range</returns>
        [HttpGet("revenue-range")]
        public async Task<IActionResult> GetRoomTypesByRevenueRange([FromQuery] decimal minRevenue, [FromQuery] decimal maxRevenue)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByRevenueRangeAsync(minRevenue, maxRevenue);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by revenue range");
                return StatusCode(500, "An error occurred while retrieving room types by revenue range.");
            }
        }

        /// <summary>
        /// Get top room types by revenue
        /// </summary>
        /// <param name="topCount">Number of top room types to return (default: 10)</param>
        /// <returns>List of top room types by revenue</returns>
        [HttpGet("top-by-revenue")]
        public async Task<IActionResult> GetTopRoomTypesByRevenue([FromQuery] int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetTopRoomTypesByRevenueAsync(topCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top room types by revenue");
                return StatusCode(500, "An error occurred while retrieving top room types by revenue.");
            }
        }

        /// <summary>
        /// Get room types by reservation count range
        /// </summary>
        /// <param name="minCount">Minimum reservation count</param>
        /// <param name="maxCount">Maximum reservation count</param>
        /// <returns>List of room types with reservation count in the specified range</returns>
        [HttpGet("reservation-count-range")]
        public async Task<IActionResult> GetRoomTypesByReservationCountRange([FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByReservationCountRangeAsync(minCount, maxCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by reservation count range");
                return StatusCode(500, "An error occurred while retrieving room types by reservation count range.");
            }
        }

        /// <summary>
        /// Get top room types by reservation count
        /// </summary>
        /// <param name="topCount">Number of top room types to return (default: 10)</param>
        /// <returns>List of top room types by reservation count</returns>
        [HttpGet("top-by-reservations")]
        public async Task<IActionResult> GetTopRoomTypesByReservationCount([FromQuery] int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetTopRoomTypesByReservationCountAsync(topCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top room types by reservation count");
                return StatusCode(500, "An error occurred while retrieving top room types by reservation count.");
            }
        }

        /// <summary>
        /// Get room type occupancy rate
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Occupancy rate for the specified room type and date range</returns>
        [HttpGet("{roomTypeId}/occupancy-rate")]
        public async Task<IActionResult> GetRoomTypeOccupancyRate(int roomTypeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var occupancyRate = await _roomTypeService.GetRoomTypeOccupancyRateAsync(roomTypeId, startDate, endDate);
                return Ok(new { RoomTypeId = roomTypeId, StartDate = startDate, EndDate = endDate, OccupancyRate = occupancyRate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type occupancy rate for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving room type occupancy rate.");
            }
        }

        /// <summary>
        /// Get room type revenue
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Revenue for the specified room type and date range</returns>
        [HttpGet("{roomTypeId}/revenue")]
        public async Task<IActionResult> GetRoomTypeRevenue(int roomTypeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var revenue = await _roomTypeService.GetRoomTypeRevenueAsync(roomTypeId, startDate, endDate);
                return Ok(new { RoomTypeId = roomTypeId, StartDate = startDate, EndDate = endDate, Revenue = revenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type revenue for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving room type revenue.");
            }
        }

        /// <summary>
        /// Get room type reservation count
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Reservation count for the specified room type and date range</returns>
        [HttpGet("{roomTypeId}/reservation-count")]
        public async Task<IActionResult> GetRoomTypeReservationCount(int roomTypeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var count = await _roomTypeService.GetRoomTypeReservationCountAsync(roomTypeId, startDate, endDate);
                return Ok(new { RoomTypeId = roomTypeId, StartDate = startDate, EndDate = endDate, Count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type reservation count for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving room type reservation count.");
            }
        }

        /// <summary>
        /// Get room type average stay duration
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Average stay duration for the specified room type and date range</returns>
        [HttpGet("{roomTypeId}/average-stay-duration")]
        public async Task<IActionResult> GetRoomTypeAverageStayDuration(int roomTypeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var averageDuration = await _roomTypeService.GetRoomTypeAverageStayDurationAsync(roomTypeId, startDate, endDate);
                return Ok(new { RoomTypeId = roomTypeId, StartDate = startDate, EndDate = endDate, AverageStayDuration = averageDuration });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type average stay duration for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving room type average stay duration.");
            }
        }

        /// <summary>
        /// Get room type utilization statistics
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Utilization statistics for the specified room type and date range</returns>
        [HttpGet("{roomTypeId}/utilization-statistics")]
        public async Task<IActionResult> GetRoomTypeUtilizationStatistics(int roomTypeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _roomTypeService.GetRoomTypeUtilizationStatisticsAsync(roomTypeId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type utilization statistics for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving room type utilization statistics.");
            }
        }

        /// <summary>
        /// Get room types by multiple criteria
        /// </summary>
        /// <param name="hotelId">Hotel ID (optional)</param>
        /// <param name="roomTypeName">Room type name (optional)</param>
        /// <param name="minBaseRate">Minimum base rate (optional)</param>
        /// <param name="maxBaseRate">Maximum base rate (optional)</param>
        /// <returns>List of room types matching the specified criteria</returns>
        [HttpGet("filter")]
        public async Task<IActionResult> GetRoomTypesByMultipleCriteria(
            [FromQuery] int? hotelId = null, 
            [FromQuery] string? roomTypeName = null, 
            [FromQuery] decimal? minBaseRate = null, 
            [FromQuery] decimal? maxBaseRate = null)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByMultipleCriteriaAsync(hotelId, roomTypeName, minBaseRate, maxBaseRate);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by multiple criteria");
                return StatusCode(500, "An error occurred while retrieving room types by multiple criteria.");
            }
        }

        /// <summary>
        /// Get room type apartment statistics
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <returns>Apartment statistics for the specified room type</returns>
        [HttpGet("{roomTypeId}/apartment-statistics")]
        public async Task<IActionResult> GetRoomTypeApartmentStatistics(int roomTypeId)
        {
            try
            {
                var statistics = await _roomTypeService.GetRoomTypeApartmentStatisticsAsync(roomTypeId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type apartment statistics for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving room type apartment statistics.");
            }
        }

        /// <summary>
        /// Get room type reservation statistics
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Reservation statistics for the specified room type and date range</returns>
        [HttpGet("{roomTypeId}/reservation-statistics")]
        public async Task<IActionResult> GetRoomTypeReservationStatistics(int roomTypeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _roomTypeService.GetRoomTypeReservationStatisticsAsync(roomTypeId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type reservation statistics for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving room type reservation statistics.");
            }
        }

        /// <summary>
        /// Get room type revenue statistics
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Revenue statistics for the specified room type and date range</returns>
        [HttpGet("{roomTypeId}/revenue-statistics")]
        public async Task<IActionResult> GetRoomTypeRevenueStatistics(int roomTypeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _roomTypeService.GetRoomTypeRevenueStatisticsAsync(roomTypeId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type revenue statistics for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving room type revenue statistics.");
            }
        }

        /// <summary>
        /// Get room type occupancy statistics
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Occupancy statistics for the specified room type and date range</returns>
        [HttpGet("{roomTypeId}/occupancy-statistics")]
        public async Task<IActionResult> GetRoomTypeOccupancyStatistics(int roomTypeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _roomTypeService.GetRoomTypeOccupancyStatisticsAsync(roomTypeId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type occupancy statistics for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving room type occupancy statistics.");
            }
        }

        /// <summary>
        /// Get room type performance metrics
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Performance metrics for the specified room type and date range</returns>
        [HttpGet("{roomTypeId}/performance-metrics")]
        public async Task<IActionResult> GetRoomTypePerformanceMetrics(int roomTypeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var metrics = await _roomTypeService.GetRoomTypePerformanceMetricsAsync(roomTypeId, startDate, endDate);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room type performance metrics for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving room type performance metrics.");
            }
        }

        /// <summary>
        /// Get room types by hotel and base rate range
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="minRate">Minimum base rate</param>
        /// <param name="maxRate">Maximum base rate</param>
        /// <returns>List of room types with base rate in the specified range for the hotel</returns>
        [HttpGet("hotel/{hotelId}/base-rate-range")]
        public async Task<IActionResult> GetRoomTypesByHotelAndBaseRateRange(int hotelId, [FromQuery] decimal minRate, [FromQuery] decimal maxRate)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByHotelAndBaseRateRangeAsync(hotelId, minRate, maxRate);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by hotel and base rate range");
                return StatusCode(500, "An error occurred while retrieving room types by hotel and base rate range.");
            }
        }

        /// <summary>
        /// Get room types by hotel and apartment count range
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="minCount">Minimum apartment count</param>
        /// <param name="maxCount">Maximum apartment count</param>
        /// <returns>List of room types with apartment count in the specified range for the hotel</returns>
        [HttpGet("hotel/{hotelId}/apartment-count-range")]
        public async Task<IActionResult> GetRoomTypesByHotelAndApartmentCountRange(int hotelId, [FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByHotelAndApartmentCountRangeAsync(hotelId, minCount, maxCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by hotel and apartment count range");
                return StatusCode(500, "An error occurred while retrieving room types by hotel and apartment count range.");
            }
        }

        /// <summary>
        /// Get room types by hotel and revenue range
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="minRevenue">Minimum revenue</param>
        /// <param name="maxRevenue">Maximum revenue</param>
        /// <returns>List of room types with revenue in the specified range for the hotel</returns>
        [HttpGet("hotel/{hotelId}/revenue-range")]
        public async Task<IActionResult> GetRoomTypesByHotelAndRevenueRange(int hotelId, [FromQuery] decimal minRevenue, [FromQuery] decimal maxRevenue)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByHotelAndRevenueRangeAsync(hotelId, minRevenue, maxRevenue);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by hotel and revenue range");
                return StatusCode(500, "An error occurred while retrieving room types by hotel and revenue range.");
            }
        }

        /// <summary>
        /// Get room types by hotel and reservation count range
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="minCount">Minimum reservation count</param>
        /// <param name="maxCount">Maximum reservation count</param>
        /// <returns>List of room types with reservation count in the specified range for the hotel</returns>
        [HttpGet("hotel/{hotelId}/reservation-count-range")]
        public async Task<IActionResult> GetRoomTypesByHotelAndReservationCountRange(int hotelId, [FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByHotelAndReservationCountRangeAsync(hotelId, minCount, maxCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by hotel and reservation count range");
                return StatusCode(500, "An error occurred while retrieving room types by hotel and reservation count range.");
            }
        }

        /// <summary>
        /// Get hotel room type statistics
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>Room type statistics for the specified hotel</returns>
        [HttpGet("hotel/{hotelId}/room-type-statistics")]
        public async Task<IActionResult> GetHotelRoomTypeStatistics(int hotelId)
        {
            try
            {
                var statistics = await _roomTypeService.GetHotelRoomTypeStatisticsAsync(hotelId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotel room type statistics for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving hotel room type statistics.");
            }
        }

        /// <summary>
        /// Get hotel room type occupancy statistics
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Occupancy statistics for all room types in the hotel</returns>
        [HttpGet("hotel/{hotelId}/occupancy-statistics")]
        public async Task<IActionResult> GetHotelRoomTypeOccupancyStatistics(int hotelId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _roomTypeService.GetHotelRoomTypeOccupancyStatisticsAsync(hotelId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotel room type occupancy statistics for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving hotel room type occupancy statistics.");
            }
        }

        /// <summary>
        /// Get hotel room type revenue statistics
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Revenue statistics for all room types in the hotel</returns>
        [HttpGet("hotel/{hotelId}/revenue-statistics")]
        public async Task<IActionResult> GetHotelRoomTypeRevenueStatistics(int hotelId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _roomTypeService.GetHotelRoomTypeRevenueStatisticsAsync(hotelId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotel room type revenue statistics for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving hotel room type revenue statistics.");
            }
        }

        /// <summary>
        /// Get hotel room type performance metrics
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Performance metrics for all room types in the hotel</returns>
        [HttpGet("hotel/{hotelId}/performance-metrics")]
        public async Task<IActionResult> GetHotelRoomTypePerformanceMetrics(int hotelId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var metrics = await _roomTypeService.GetHotelRoomTypePerformanceMetricsAsync(hotelId, startDate, endDate);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotel room type performance metrics for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving hotel room type performance metrics.");
            }
        }

        /// <summary>
        /// Get room types by average revenue range
        /// </summary>
        /// <param name="minRevenue">Minimum average revenue</param>
        /// <param name="maxRevenue">Maximum average revenue</param>
        /// <returns>List of room types with average revenue in the specified range</returns>
        [HttpGet("average-revenue-range")]
        public async Task<IActionResult> GetRoomTypesByAverageRevenueRange([FromQuery] decimal minRevenue, [FromQuery] decimal maxRevenue)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByAverageRevenueRangeAsync(minRevenue, maxRevenue);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by average revenue range");
                return StatusCode(500, "An error occurred while retrieving room types by average revenue range.");
            }
        }

        /// <summary>
        /// Get top room types by average revenue
        /// </summary>
        /// <param name="topCount">Number of top room types to return (default: 10)</param>
        /// <returns>List of top room types by average revenue</returns>
        [HttpGet("top-by-average-revenue")]
        public async Task<IActionResult> GetTopRoomTypesByAverageRevenue([FromQuery] int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetTopRoomTypesByAverageRevenueAsync(topCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top room types by average revenue");
                return StatusCode(500, "An error occurred while retrieving top room types by average revenue.");
            }
        }

        /// <summary>
        /// Get room types by occupancy rate range
        /// </summary>
        /// <param name="minRate">Minimum occupancy rate</param>
        /// <param name="maxRate">Maximum occupancy rate</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of room types with occupancy rate in the specified range</returns>
        [HttpGet("occupancy-rate-range")]
        public async Task<IActionResult> GetRoomTypesByOccupancyRateRange([FromQuery] decimal minRate, [FromQuery] decimal maxRate, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByOccupancyRateRangeAsync(minRate, maxRate, startDate, endDate);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by occupancy rate range");
                return StatusCode(500, "An error occurred while retrieving room types by occupancy rate range.");
            }
        }

        /// <summary>
        /// Get top room types by occupancy rate
        /// </summary>
        /// <param name="topCount">Number of top room types to return (default: 10)</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of top room types by occupancy rate</returns>
        [HttpGet("top-by-occupancy-rate")]
        public async Task<IActionResult> GetTopRoomTypesByOccupancyRate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetTopRoomTypesByOccupancyRateAsync(startDate, endDate, topCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top room types by occupancy rate");
                return StatusCode(500, "An error occurred while retrieving top room types by occupancy rate.");
            }
        }

        /// <summary>
        /// Get room types by average stay duration range
        /// </summary>
        /// <param name="minDuration">Minimum average stay duration</param>
        /// <param name="maxDuration">Maximum average stay duration</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>List of room types with average stay duration in the specified range</returns>
        [HttpGet("average-stay-duration-range")]
        public async Task<IActionResult> GetRoomTypesByAverageStayDurationRange([FromQuery] decimal minDuration, [FromQuery] decimal maxDuration, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByAverageStayDurationRangeAsync(minDuration, maxDuration, startDate, endDate);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by average stay duration range");
                return StatusCode(500, "An error occurred while retrieving room types by average stay duration range.");
            }
        }

        /// <summary>
        /// Get top room types by average stay duration
        /// </summary>
        /// <param name="topCount">Number of top room types to return (default: 10)</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>List of top room types by average stay duration</returns>
        [HttpGet("top-by-average-stay-duration")]
        public async Task<IActionResult> GetTopRoomTypesByAverageStayDuration([FromQuery] int topCount = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetTopRoomTypesByAverageStayDurationAsync(topCount, startDate, endDate);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top room types by average stay duration");
                return StatusCode(500, "An error occurred while retrieving top room types by average stay duration.");
            }
        }

        /// <summary>
        /// Get room types by profitability
        /// </summary>
        /// <param name="minProfitability">Minimum profitability ratio</param>
        /// <returns>List of room types with profitability above the specified threshold</returns>
        [HttpGet("profitability")]
        public async Task<IActionResult> GetRoomTypesByProfitability([FromQuery] decimal minProfitability)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByProfitabilityAsync(minProfitability);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by profitability");
                return StatusCode(500, "An error occurred while retrieving room types by profitability.");
            }
        }

        /// <summary>
        /// Get top room types by profitability
        /// </summary>
        /// <param name="topCount">Number of top room types to return (default: 10)</param>
        /// <returns>List of top room types by profitability</returns>
        [HttpGet("top-by-profitability")]
        public async Task<IActionResult> GetTopRoomTypesByProfitability([FromQuery] int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetTopRoomTypesByProfitabilityAsync(topCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top room types by profitability");
                return StatusCode(500, "An error occurred while retrieving top room types by profitability.");
            }
        }

        /// <summary>
        /// Get room types by utilization efficiency
        /// </summary>
        /// <param name="minEfficiency">Minimum utilization efficiency</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of room types with utilization efficiency above the specified threshold</returns>
        [HttpGet("utilization-efficiency")]
        public async Task<IActionResult> GetRoomTypesByUtilizationEfficiency([FromQuery] decimal minEfficiency, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByUtilizationEfficiencyAsync(minEfficiency, startDate, endDate);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room types by utilization efficiency");
                return StatusCode(500, "An error occurred while retrieving room types by utilization efficiency.");
            }
        }

        /// <summary>
        /// Get top room types by utilization efficiency
        /// </summary>
        /// <param name="topCount">Number of top room types to return (default: 10)</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of top room types by utilization efficiency</returns>
        [HttpGet("top-by-utilization-efficiency")]
        public async Task<IActionResult> GetTopRoomTypesByUtilizationEfficiency([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetTopRoomTypesByUtilizationEfficiencyAsync(startDate, endDate, topCount);
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top room types by utilization efficiency");
                return StatusCode(500, "An error occurred while retrieving top room types by utilization efficiency.");
            }
        }
    }
}
