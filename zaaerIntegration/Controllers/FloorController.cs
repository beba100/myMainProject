using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for Floor operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FloorController : ControllerBase
    {
        private readonly IFloorService _floorService;
        private readonly ILogger<FloorController> _logger;

        /// <summary>
        /// Initializes a new instance of the FloorController class
        /// </summary>
        /// <param name="floorService">Floor service</param>
        /// <param name="logger">Logger</param>
        public FloorController(IFloorService floorService, ILogger<FloorController> logger)
        {
            _floorService = floorService;
            _logger = logger;
        }

        /// <summary>
        /// Get all floors with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for floor name, number, building name, or hotel name</param>
        /// <returns>List of floors with total count</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllFloors(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (floors, totalCount) = await _floorService.GetAllFloorsAsync(pageNumber, pageSize, searchTerm);
                
                return Ok(new
                {
                    Floors = floors,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors");
                return StatusCode(500, "An error occurred while retrieving floors.");
            }
        }

        /// <summary>
        /// Get floor by ID
        /// </summary>
        /// <param name="id">Floor ID</param>
        /// <returns>Floor details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFloorById(int id)
        {
            try
            {
                var floor = await _floorService.GetFloorByIdAsync(id);
                if (floor == null)
                {
                    return NotFound($"Floor with ID {id} not found.");
                }

                return Ok(floor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor with ID {FloorId}", id);
                return StatusCode(500, "An error occurred while retrieving the floor.");
            }
        }

        /// <summary>
        /// Get floor by floor number in building
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="floorNumber">Floor number</param>
        /// <returns>Floor details</returns>
        [HttpGet("building/{buildingId}/number/{floorNumber}")]
        public async Task<IActionResult> GetFloorByNumber(int buildingId, int floorNumber)
        {
            try
            {
                var floor = await _floorService.GetFloorByNumberAsync(buildingId, floorNumber);
                if (floor == null)
                {
                    return NotFound($"Floor with number {floorNumber} not found in building {buildingId}.");
                }

                return Ok(floor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor with number {FloorNumber} in building {BuildingId}", floorNumber, buildingId);
                return StatusCode(500, "An error occurred while retrieving the floor.");
            }
        }

        /// <summary>
        /// Create new floor
        /// </summary>
        /// <param name="createFloorDto">Floor creation data</param>
        /// <returns>Created floor</returns>
        [HttpPost]
        public async Task<IActionResult> CreateFloor([FromBody] CreateFloorDto createFloorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var floor = await _floorService.CreateFloorAsync(createFloorDto);
                return CreatedAtAction(nameof(GetFloorById), new { id = floor.FloorId }, floor);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating floor");
                return StatusCode(500, "An error occurred while creating the floor.");
            }
        }

        /// <summary>
        /// Update existing floor
        /// </summary>
        /// <param name="id">Floor ID</param>
        /// <param name="updateFloorDto">Floor update data</param>
        /// <returns>Updated floor</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFloor(int id, [FromBody] UpdateFloorDto updateFloorDto)
        {
            try
            {
                if (id != updateFloorDto.FloorId)
                {
                    return BadRequest("Floor ID in URL does not match ID in body.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var floor = await _floorService.UpdateFloorAsync(id, updateFloorDto);
                if (floor == null)
                {
                    return NotFound($"Floor with ID {id} not found.");
                }

                return Ok(floor);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating floor with ID {FloorId}", id);
                return StatusCode(500, "An error occurred while updating the floor.");
            }
        }

        /// <summary>
        /// Delete floor
        /// </summary>
        /// <param name="id">Floor ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFloor(int id)
        {
            try
            {
                var result = await _floorService.DeleteFloorAsync(id);
                if (!result)
                {
                    return NotFound($"Floor with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting floor with ID {FloorId}", id);
                return StatusCode(500, "An error occurred while deleting the floor.");
            }
        }

        /// <summary>
        /// Get floors by building ID
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns>List of floors for the specified building</returns>
        [HttpGet("building/{buildingId}")]
        public async Task<IActionResult> GetFloorsByBuildingId(int buildingId)
        {
            try
            {
                var floors = await _floorService.GetFloorsByBuildingIdAsync(buildingId);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving floors for the building.");
            }
        }

        /// <summary>
        /// Get floors by floor name
        /// </summary>
        /// <param name="floorName">Floor name</param>
        /// <returns>List of floors with the specified name</returns>
        [HttpGet("name/{floorName}")]
        public async Task<IActionResult> GetFloorsByFloorName(string floorName)
        {
            try
            {
                var floors = await _floorService.GetFloorsByFloorNameAsync(floorName);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors by name {FloorName}", floorName);
                return StatusCode(500, "An error occurred while retrieving floors by name.");
            }
        }

        /// <summary>
        /// Get floor statistics
        /// </summary>
        /// <returns>Floor statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetFloorStatistics()
        {
            try
            {
                var statistics = await _floorService.GetFloorStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor statistics");
                return StatusCode(500, "An error occurred while retrieving floor statistics.");
            }
        }

        /// <summary>
        /// Search floors by name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>List of matching floors</returns>
        [HttpGet("search/name")]
        public async Task<IActionResult> SearchFloorsByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Name cannot be empty.");
                }

                var floors = await _floorService.SearchFloorsByNameAsync(name);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching floors by name {Name}", name);
                return StatusCode(500, "An error occurred while searching floors by name.");
            }
        }

        /// <summary>
        /// Search floors by number
        /// </summary>
        /// <param name="floorNumber">Floor number to search for</param>
        /// <returns>List of matching floors</returns>
        [HttpGet("search/number")]
        public async Task<IActionResult> SearchFloorsByNumber([FromQuery] int floorNumber)
        {
            try
            {
                var floors = await _floorService.SearchFloorsByNumberAsync(floorNumber);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching floors by number {FloorNumber}", floorNumber);
                return StatusCode(500, "An error occurred while searching floors by number.");
            }
        }

        /// <summary>
        /// Search floors by building name
        /// </summary>
        /// <param name="buildingName">Building name to search for</param>
        /// <returns>List of matching floors</returns>
        [HttpGet("search/building")]
        public async Task<IActionResult> SearchFloorsByBuildingName([FromQuery] string buildingName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(buildingName))
                {
                    return BadRequest("Building name cannot be empty.");
                }

                var floors = await _floorService.SearchFloorsByBuildingNameAsync(buildingName);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching floors by building name {BuildingName}", buildingName);
                return StatusCode(500, "An error occurred while searching floors by building name.");
            }
        }

        /// <summary>
        /// Search floors by hotel name
        /// </summary>
        /// <param name="hotelName">Hotel name to search for</param>
        /// <returns>List of matching floors</returns>
        [HttpGet("search/hotel")]
        public async Task<IActionResult> SearchFloorsByHotelName([FromQuery] string hotelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hotelName))
                {
                    return BadRequest("Hotel name cannot be empty.");
                }

                var floors = await _floorService.SearchFloorsByHotelNameAsync(hotelName);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching floors by hotel name {HotelName}", hotelName);
                return StatusCode(500, "An error occurred while searching floors by hotel name.");
            }
        }

        /// <summary>
        /// Check if floor number exists in building
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="floorNumber">Floor number to check</param>
        /// <param name="excludeId">Floor ID to exclude from check (for updates)</param>
        /// <returns>True if number exists, false otherwise</returns>
        [HttpGet("check-number")]
        public async Task<IActionResult> CheckFloorNumber([FromQuery] int buildingId, [FromQuery] int floorNumber, [FromQuery] int? excludeId = null)
        {
            try
            {
                var exists = await _floorService.FloorNumberExistsAsync(buildingId, floorNumber, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking floor number {FloorNumber} in building {BuildingId}", floorNumber, buildingId);
                return StatusCode(500, "An error occurred while checking floor number.");
            }
        }

        /// <summary>
        /// Get floors with apartments
        /// </summary>
        /// <returns>List of floors that have apartments</returns>
        [HttpGet("with-apartments")]
        public async Task<IActionResult> GetFloorsWithApartments()
        {
            try
            {
                var floors = await _floorService.GetFloorsWithApartmentsAsync();
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors with apartments");
                return StatusCode(500, "An error occurred while retrieving floors with apartments.");
            }
        }

        /// <summary>
        /// Get floors without apartments
        /// </summary>
        /// <returns>List of floors that have no apartments</returns>
        [HttpGet("without-apartments")]
        public async Task<IActionResult> GetFloorsWithoutApartments()
        {
            try
            {
                var floors = await _floorService.GetFloorsWithoutApartmentsAsync();
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors without apartments");
                return StatusCode(500, "An error occurred while retrieving floors without apartments.");
            }
        }

        /// <summary>
        /// Get floors by apartment count range
        /// </summary>
        /// <param name="minCount">Minimum apartment count</param>
        /// <param name="maxCount">Maximum apartment count</param>
        /// <returns>List of floors with apartment count in the specified range</returns>
        [HttpGet("apartment-count-range")]
        public async Task<IActionResult> GetFloorsByApartmentCountRange([FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var floors = await _floorService.GetFloorsByApartmentCountRangeAsync(minCount, maxCount);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors by apartment count range");
                return StatusCode(500, "An error occurred while retrieving floors by apartment count range.");
            }
        }

        /// <summary>
        /// Get top floors by apartment count
        /// </summary>
        /// <param name="topCount">Number of top floors to return (default: 10)</param>
        /// <returns>List of top floors by apartment count</returns>
        [HttpGet("top-by-apartments")]
        public async Task<IActionResult> GetTopFloorsByApartmentCount([FromQuery] int topCount = 10)
        {
            try
            {
                var floors = await _floorService.GetTopFloorsByApartmentCountAsync(topCount);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top floors by apartment count");
                return StatusCode(500, "An error occurred while retrieving top floors by apartment count.");
            }
        }

        /// <summary>
        /// Get floors by revenue range
        /// </summary>
        /// <param name="minRevenue">Minimum revenue</param>
        /// <param name="maxRevenue">Maximum revenue</param>
        /// <returns>List of floors with revenue in the specified range</returns>
        [HttpGet("revenue-range")]
        public async Task<IActionResult> GetFloorsByRevenueRange([FromQuery] decimal minRevenue, [FromQuery] decimal maxRevenue)
        {
            try
            {
                var floors = await _floorService.GetFloorsByRevenueRangeAsync(minRevenue, maxRevenue);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors by revenue range");
                return StatusCode(500, "An error occurred while retrieving floors by revenue range.");
            }
        }

        /// <summary>
        /// Get top floors by revenue
        /// </summary>
        /// <param name="topCount">Number of top floors to return (default: 10)</param>
        /// <returns>List of top floors by revenue</returns>
        [HttpGet("top-by-revenue")]
        public async Task<IActionResult> GetTopFloorsByRevenue([FromQuery] int topCount = 10)
        {
            try
            {
                var floors = await _floorService.GetTopFloorsByRevenueAsync(topCount);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top floors by revenue");
                return StatusCode(500, "An error occurred while retrieving top floors by revenue.");
            }
        }

        /// <summary>
        /// Get floors by reservation count range
        /// </summary>
        /// <param name="minCount">Minimum reservation count</param>
        /// <param name="maxCount">Maximum reservation count</param>
        /// <returns>List of floors with reservation count in the specified range</returns>
        [HttpGet("reservation-count-range")]
        public async Task<IActionResult> GetFloorsByReservationCountRange([FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var floors = await _floorService.GetFloorsByReservationCountRangeAsync(minCount, maxCount);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors by reservation count range");
                return StatusCode(500, "An error occurred while retrieving floors by reservation count range.");
            }
        }

        /// <summary>
        /// Get top floors by reservation count
        /// </summary>
        /// <param name="topCount">Number of top floors to return (default: 10)</param>
        /// <returns>List of top floors by reservation count</returns>
        [HttpGet("top-by-reservations")]
        public async Task<IActionResult> GetTopFloorsByReservationCount([FromQuery] int topCount = 10)
        {
            try
            {
                var floors = await _floorService.GetTopFloorsByReservationCountAsync(topCount);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top floors by reservation count");
                return StatusCode(500, "An error occurred while retrieving top floors by reservation count.");
            }
        }

        /// <summary>
        /// Get floor occupancy rate
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Occupancy rate for the specified floor and date range</returns>
        [HttpGet("{floorId}/occupancy-rate")]
        public async Task<IActionResult> GetFloorOccupancyRate(int floorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var occupancyRate = await _floorService.GetFloorOccupancyRateAsync(floorId, startDate, endDate);
                return Ok(new { FloorId = floorId, StartDate = startDate, EndDate = endDate, OccupancyRate = occupancyRate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor occupancy rate for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving floor occupancy rate.");
            }
        }

        /// <summary>
        /// Get floor revenue
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Revenue for the specified floor and date range</returns>
        [HttpGet("{floorId}/revenue")]
        public async Task<IActionResult> GetFloorRevenue(int floorId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var revenue = await _floorService.GetFloorRevenueAsync(floorId, startDate, endDate);
                return Ok(new { FloorId = floorId, StartDate = startDate, EndDate = endDate, Revenue = revenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor revenue for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving floor revenue.");
            }
        }

        /// <summary>
        /// Get floor reservation count
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Reservation count for the specified floor and date range</returns>
        [HttpGet("{floorId}/reservation-count")]
        public async Task<IActionResult> GetFloorReservationCount(int floorId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var count = await _floorService.GetFloorReservationCountAsync(floorId, startDate, endDate);
                return Ok(new { FloorId = floorId, StartDate = startDate, EndDate = endDate, Count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor reservation count for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving floor reservation count.");
            }
        }

        /// <summary>
        /// Get floor average stay duration
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Average stay duration for the specified floor and date range</returns>
        [HttpGet("{floorId}/average-stay-duration")]
        public async Task<IActionResult> GetFloorAverageStayDuration(int floorId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var averageDuration = await _floorService.GetFloorAverageStayDurationAsync(floorId, startDate, endDate);
                return Ok(new { FloorId = floorId, StartDate = startDate, EndDate = endDate, AverageStayDuration = averageDuration });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor average stay duration for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving floor average stay duration.");
            }
        }

        /// <summary>
        /// Get floor utilization statistics
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Utilization statistics for the specified floor and date range</returns>
        [HttpGet("{floorId}/utilization-statistics")]
        public async Task<IActionResult> GetFloorUtilizationStatistics(int floorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _floorService.GetFloorUtilizationStatisticsAsync(floorId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor utilization statistics for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving floor utilization statistics.");
            }
        }

        /// <summary>
        /// Get floors by multiple criteria
        /// </summary>
        /// <param name="buildingId">Building ID (optional)</param>
        /// <param name="floorNumber">Floor number (optional)</param>
        /// <param name="floorName">Floor name (optional)</param>
        /// <returns>List of floors matching the specified criteria</returns>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFloorsByMultipleCriteria(
            [FromQuery] int? buildingId = null, 
            [FromQuery] int? floorNumber = null, 
            [FromQuery] string? floorName = null)
        {
            try
            {
                var floors = await _floorService.GetFloorsByMultipleCriteriaAsync(buildingId, floorNumber, floorName);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors by multiple criteria");
                return StatusCode(500, "An error occurred while retrieving floors by multiple criteria.");
            }
        }

        /// <summary>
        /// Get floor apartment statistics
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <returns>Apartment statistics for the specified floor</returns>
        [HttpGet("{floorId}/apartment-statistics")]
        public async Task<IActionResult> GetFloorApartmentStatistics(int floorId)
        {
            try
            {
                var statistics = await _floorService.GetFloorApartmentStatisticsAsync(floorId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor apartment statistics for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving floor apartment statistics.");
            }
        }

        /// <summary>
        /// Get floor reservation statistics
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Reservation statistics for the specified floor and date range</returns>
        [HttpGet("{floorId}/reservation-statistics")]
        public async Task<IActionResult> GetFloorReservationStatistics(int floorId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _floorService.GetFloorReservationStatisticsAsync(floorId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor reservation statistics for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving floor reservation statistics.");
            }
        }

        /// <summary>
        /// Get floor revenue statistics
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Revenue statistics for the specified floor and date range</returns>
        [HttpGet("{floorId}/revenue-statistics")]
        public async Task<IActionResult> GetFloorRevenueStatistics(int floorId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _floorService.GetFloorRevenueStatisticsAsync(floorId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor revenue statistics for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving floor revenue statistics.");
            }
        }

        /// <summary>
        /// Get floor occupancy statistics
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Occupancy statistics for the specified floor and date range</returns>
        [HttpGet("{floorId}/occupancy-statistics")]
        public async Task<IActionResult> GetFloorOccupancyStatistics(int floorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _floorService.GetFloorOccupancyStatisticsAsync(floorId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor occupancy statistics for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving floor occupancy statistics.");
            }
        }

        /// <summary>
        /// Get floor performance metrics
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Performance metrics for the specified floor and date range</returns>
        [HttpGet("{floorId}/performance-metrics")]
        public async Task<IActionResult> GetFloorPerformanceMetrics(int floorId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var metrics = await _floorService.GetFloorPerformanceMetricsAsync(floorId, startDate, endDate);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor performance metrics for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving floor performance metrics.");
            }
        }

        // Bulk Operations
        /// <summary>
        /// Bulk create multiple floors for a building
        /// </summary>
        /// <param name="bulkCreateFloorDto">Bulk floor creation data</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("bulk-create")]
        public async Task<IActionResult> BulkCreateFloors([FromBody] BulkCreateFloorDto bulkCreateFloorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _floorService.BulkCreateFloorsAsync(bulkCreateFloorDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk creating floors");
                return StatusCode(500, "An error occurred while bulk creating floors.");
            }
        }

        /// <summary>
        /// Bulk update multiple floors
        /// </summary>
        /// <param name="bulkUpdateFloorDto">Bulk floor update data</param>
        /// <returns>Bulk operation result</returns>
        [HttpPut("bulk-update")]
        public async Task<IActionResult> BulkUpdateFloors([FromBody] BulkUpdateFloorDto bulkUpdateFloorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _floorService.BulkUpdateFloorsAsync(bulkUpdateFloorDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating floors");
                return StatusCode(500, "An error occurred while bulk updating floors.");
            }
        }

        /// <summary>
        /// Bulk delete multiple floors
        /// </summary>
        /// <param name="floorIds">List of floor IDs to delete</param>
        /// <returns>Bulk deletion result</returns>
        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> BulkDeleteFloors([FromBody] IEnumerable<int> floorIds)
        {
            try
            {
                var (deletedCount, errors) = await _floorService.BulkDeleteFloorsAsync(floorIds);
                return Ok(new { DeletedCount = deletedCount, Errors = errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting floors");
                return StatusCode(500, "An error occurred while bulk deleting floors.");
            }
        }

        /// <summary>
        /// Get floors by building with floor numbers
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="floorNumbers">List of floor numbers</param>
        /// <returns>List of floors with the specified numbers in the building</returns>
        [HttpGet("building/{buildingId}/floor-numbers")]
        public async Task<IActionResult> GetFloorsByBuildingWithFloorNumbers(int buildingId, [FromQuery] IEnumerable<int> floorNumbers)
        {
            try
            {
                var floors = await _floorService.GetFloorsByBuildingWithFloorNumbersAsync(buildingId, floorNumbers);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors by building with floor numbers");
                return StatusCode(500, "An error occurred while retrieving floors by building with floor numbers.");
            }
        }

        /// <summary>
        /// Get next available floor number for building
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns>Next available floor number</returns>
        [HttpGet("building/{buildingId}/next-floor-number")]
        public async Task<IActionResult> GetNextAvailableFloorNumber(int buildingId)
        {
            try
            {
                var nextFloorNumber = await _floorService.GetNextAvailableFloorNumberAsync(buildingId);
                return Ok(new { BuildingId = buildingId, NextFloorNumber = nextFloorNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next available floor number for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while getting next available floor number.");
            }
        }

        /// <summary>
        /// Get floor numbers in building
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns>List of floor numbers in the building</returns>
        [HttpGet("building/{buildingId}/floor-numbers-list")]
        public async Task<IActionResult> GetFloorNumbersInBuilding(int buildingId)
        {
            try
            {
                var floorNumbers = await _floorService.GetFloorNumbersInBuildingAsync(buildingId);
                return Ok(new { BuildingId = buildingId, FloorNumbers = floorNumbers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting floor numbers in building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while getting floor numbers in building.");
            }
        }

        /// <summary>
        /// Check if floor numbers exist in building
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="floorNumbers">List of floor numbers to check</param>
        /// <returns>Dictionary of floor numbers and their existence status</returns>
        [HttpGet("building/{buildingId}/check-floor-numbers")]
        public async Task<IActionResult> CheckFloorNumbersExist(int buildingId, [FromQuery] IEnumerable<int> floorNumbers)
        {
            try
            {
                var exists = await _floorService.CheckFloorNumbersExistAsync(buildingId, floorNumbers);
                return Ok(new { BuildingId = buildingId, FloorNumbers = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking floor numbers existence in building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while checking floor numbers existence.");
            }
        }

        /// <summary>
        /// Get floors by building and floor number range
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="minFloorNumber">Minimum floor number</param>
        /// <param name="maxFloorNumber">Maximum floor number</param>
        /// <returns>List of floors in the specified range</returns>
        [HttpGet("building/{buildingId}/floor-number-range")]
        public async Task<IActionResult> GetFloorsByBuildingAndFloorNumberRange(int buildingId, [FromQuery] int minFloorNumber, [FromQuery] int maxFloorNumber)
        {
            try
            {
                var floors = await _floorService.GetFloorsByBuildingAndFloorNumberRangeAsync(buildingId, minFloorNumber, maxFloorNumber);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors by building and floor number range");
                return StatusCode(500, "An error occurred while retrieving floors by building and floor number range.");
            }
        }

        /// <summary>
        /// Get floors by building and apartment count range
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="minCount">Minimum apartment count</param>
        /// <param name="maxCount">Maximum apartment count</param>
        /// <returns>List of floors with apartment count in the specified range</returns>
        [HttpGet("building/{buildingId}/apartment-count-range")]
        public async Task<IActionResult> GetFloorsByBuildingAndApartmentCountRange(int buildingId, [FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var floors = await _floorService.GetFloorsByBuildingAndApartmentCountRangeAsync(buildingId, minCount, maxCount);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors by building and apartment count range");
                return StatusCode(500, "An error occurred while retrieving floors by building and apartment count range.");
            }
        }

        /// <summary>
        /// Get floors by building and revenue range
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="minRevenue">Minimum revenue</param>
        /// <param name="maxRevenue">Maximum revenue</param>
        /// <returns>List of floors with revenue in the specified range</returns>
        [HttpGet("building/{buildingId}/revenue-range")]
        public async Task<IActionResult> GetFloorsByBuildingAndRevenueRange(int buildingId, [FromQuery] decimal minRevenue, [FromQuery] decimal maxRevenue)
        {
            try
            {
                var floors = await _floorService.GetFloorsByBuildingAndRevenueRangeAsync(buildingId, minRevenue, maxRevenue);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors by building and revenue range");
                return StatusCode(500, "An error occurred while retrieving floors by building and revenue range.");
            }
        }

        /// <summary>
        /// Get floors by building and reservation count range
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="minCount">Minimum reservation count</param>
        /// <param name="maxCount">Maximum reservation count</param>
        /// <returns>List of floors with reservation count in the specified range</returns>
        [HttpGet("building/{buildingId}/reservation-count-range")]
        public async Task<IActionResult> GetFloorsByBuildingAndReservationCountRange(int buildingId, [FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var floors = await _floorService.GetFloorsByBuildingAndReservationCountRangeAsync(buildingId, minCount, maxCount);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors by building and reservation count range");
                return StatusCode(500, "An error occurred while retrieving floors by building and reservation count range.");
            }
        }

        /// <summary>
        /// Get building floor statistics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns>Floor statistics for the specified building</returns>
        [HttpGet("building/{buildingId}/floor-statistics")]
        public async Task<IActionResult> GetBuildingFloorStatistics(int buildingId)
        {
            try
            {
                var statistics = await _floorService.GetBuildingFloorStatisticsAsync(buildingId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building floor statistics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building floor statistics.");
            }
        }

        /// <summary>
        /// Get building floor occupancy statistics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Occupancy statistics for all floors in the building</returns>
        [HttpGet("building/{buildingId}/occupancy-statistics")]
        public async Task<IActionResult> GetBuildingFloorOccupancyStatistics(int buildingId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _floorService.GetBuildingFloorOccupancyStatisticsAsync(buildingId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building floor occupancy statistics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building floor occupancy statistics.");
            }
        }

        /// <summary>
        /// Get building floor revenue statistics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Revenue statistics for all floors in the building</returns>
        [HttpGet("building/{buildingId}/revenue-statistics")]
        public async Task<IActionResult> GetBuildingFloorRevenueStatistics(int buildingId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _floorService.GetBuildingFloorRevenueStatisticsAsync(buildingId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building floor revenue statistics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building floor revenue statistics.");
            }
        }

        /// <summary>
        /// Get building floor performance metrics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Performance metrics for all floors in the building</returns>
        [HttpGet("building/{buildingId}/performance-metrics")]
        public async Task<IActionResult> GetBuildingFloorPerformanceMetrics(int buildingId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var metrics = await _floorService.GetBuildingFloorPerformanceMetricsAsync(buildingId, startDate, endDate);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building floor performance metrics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building floor performance metrics.");
            }
        }
    }
}
