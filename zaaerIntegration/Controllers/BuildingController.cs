using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for Building operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BuildingController : ControllerBase
    {
        private readonly IBuildingService _buildingService;
        private readonly ILogger<BuildingController> _logger;

        /// <summary>
        /// Initializes a new instance of the BuildingController class
        /// </summary>
        /// <param name="buildingService">Building service</param>
        /// <param name="logger">Logger</param>
        public BuildingController(IBuildingService buildingService, ILogger<BuildingController> logger)
        {
            _buildingService = buildingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all buildings with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for building name, number, address, or hotel name</param>
        /// <returns>List of buildings with total count</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllBuildings(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (buildings, totalCount) = await _buildingService.GetAllBuildingsAsync(pageNumber, pageSize, searchTerm);
                
                return Ok(new
                {
                    Buildings = buildings,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings");
                return StatusCode(500, "An error occurred while retrieving buildings.");
            }
        }

        /// <summary>
        /// Get building by ID
        /// </summary>
        /// <param name="id">Building ID</param>
        /// <returns>Building details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBuildingById(int id)
        {
            try
            {
                var building = await _buildingService.GetBuildingByIdAsync(id);
                if (building == null)
                {
                    return NotFound($"Building with ID {id} not found.");
                }

                return Ok(building);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building with ID {BuildingId}", id);
                return StatusCode(500, "An error occurred while retrieving the building.");
            }
        }

        /// <summary>
        /// Get building by building number
        /// </summary>
        /// <param name="buildingNumber">Building number</param>
        /// <returns>Building details</returns>
        [HttpGet("number/{buildingNumber}")]
        public async Task<IActionResult> GetBuildingByNumber(string buildingNumber)
        {
            try
            {
                var building = await _buildingService.GetBuildingByNumberAsync(buildingNumber);
                if (building == null)
                {
                    return NotFound($"Building with number '{buildingNumber}' not found.");
                }

                return Ok(building);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building with number {BuildingNumber}", buildingNumber);
                return StatusCode(500, "An error occurred while retrieving the building.");
            }
        }

        /// <summary>
        /// Create new building
        /// </summary>
        /// <param name="createBuildingDto">Building creation data</param>
        /// <returns>Created building</returns>
        [HttpPost]
        public async Task<IActionResult> CreateBuilding([FromBody] CreateBuildingDto createBuildingDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var building = await _buildingService.CreateBuildingAsync(createBuildingDto);
                return CreatedAtAction(nameof(GetBuildingById), new { id = building.BuildingId }, building);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating building");
                return StatusCode(500, "An error occurred while creating the building.");
            }
        }

        /// <summary>
        /// Update existing building
        /// </summary>
        /// <param name="id">Building ID</param>
        /// <param name="updateBuildingDto">Building update data</param>
        /// <returns>Updated building</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilding(int id, [FromBody] UpdateBuildingDto updateBuildingDto)
        {
            try
            {
                if (id != updateBuildingDto.BuildingId)
                {
                    return BadRequest("Building ID in URL does not match ID in body.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var building = await _buildingService.UpdateBuildingAsync(id, updateBuildingDto);
                if (building == null)
                {
                    return NotFound($"Building with ID {id} not found.");
                }

                return Ok(building);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating building with ID {BuildingId}", id);
                return StatusCode(500, "An error occurred while updating the building.");
            }
        }

        /// <summary>
        /// Delete building
        /// </summary>
        /// <param name="id">Building ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            try
            {
                var result = await _buildingService.DeleteBuildingAsync(id);
                if (!result)
                {
                    return NotFound($"Building with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting building with ID {BuildingId}", id);
                return StatusCode(500, "An error occurred while deleting the building.");
            }
        }

        /// <summary>
        /// Get buildings by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of buildings for the specified hotel</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetBuildingsByHotelId(int hotelId)
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsByHotelIdAsync(hotelId);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving buildings for the hotel.");
            }
        }

        /// <summary>
        /// Get buildings by building name
        /// </summary>
        /// <param name="buildingName">Building name</param>
        /// <returns>List of buildings with the specified name</returns>
        [HttpGet("name/{buildingName}")]
        public async Task<IActionResult> GetBuildingsByBuildingName(string buildingName)
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsByBuildingNameAsync(buildingName);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings by name {BuildingName}", buildingName);
                return StatusCode(500, "An error occurred while retrieving buildings by name.");
            }
        }

        /// <summary>
        /// Get building statistics
        /// </summary>
        /// <returns>Building statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetBuildingStatistics()
        {
            try
            {
                var statistics = await _buildingService.GetBuildingStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building statistics");
                return StatusCode(500, "An error occurred while retrieving building statistics.");
            }
        }

        /// <summary>
        /// Search buildings by name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>List of matching buildings</returns>
        [HttpGet("search/name")]
        public async Task<IActionResult> SearchBuildingsByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Name cannot be empty.");
                }

                var buildings = await _buildingService.SearchBuildingsByNameAsync(name);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching buildings by name {Name}", name);
                return StatusCode(500, "An error occurred while searching buildings by name.");
            }
        }

        /// <summary>
        /// Search buildings by number
        /// </summary>
        /// <param name="number">Number to search for</param>
        /// <returns>List of matching buildings</returns>
        [HttpGet("search/number")]
        public async Task<IActionResult> SearchBuildingsByNumber([FromQuery] string number)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(number))
                {
                    return BadRequest("Number cannot be empty.");
                }

                var buildings = await _buildingService.SearchBuildingsByNumberAsync(number);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching buildings by number {Number}", number);
                return StatusCode(500, "An error occurred while searching buildings by number.");
            }
        }

        /// <summary>
        /// Search buildings by hotel name
        /// </summary>
        /// <param name="hotelName">Hotel name to search for</param>
        /// <returns>List of matching buildings</returns>
        [HttpGet("search/hotel")]
        public async Task<IActionResult> SearchBuildingsByHotelName([FromQuery] string hotelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hotelName))
                {
                    return BadRequest("Hotel name cannot be empty.");
                }

                var buildings = await _buildingService.SearchBuildingsByHotelNameAsync(hotelName);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching buildings by hotel name {HotelName}", hotelName);
                return StatusCode(500, "An error occurred while searching buildings by hotel name.");
            }
        }

        /// <summary>
        /// Check if building number exists
        /// </summary>
        /// <param name="buildingNumber">Building number to check</param>
        /// <param name="excludeId">Building ID to exclude from check (for updates)</param>
        /// <returns>True if number exists, false otherwise</returns>
        [HttpGet("check-number")]
        public async Task<IActionResult> CheckBuildingNumber([FromQuery] string buildingNumber, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(buildingNumber))
                {
                    return BadRequest("Building number cannot be empty.");
                }

                var exists = await _buildingService.BuildingNumberExistsAsync(buildingNumber, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking building number {BuildingNumber}", buildingNumber);
                return StatusCode(500, "An error occurred while checking building number.");
            }
        }

        /// <summary>
        /// Get buildings with floors
        /// </summary>
        /// <returns>List of buildings that have floors</returns>
        [HttpGet("with-floors")]
        public async Task<IActionResult> GetBuildingsWithFloors()
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsWithFloorsAsync();
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings with floors");
                return StatusCode(500, "An error occurred while retrieving buildings with floors.");
            }
        }

        /// <summary>
        /// Get buildings without floors
        /// </summary>
        /// <returns>List of buildings that have no floors</returns>
        [HttpGet("without-floors")]
        public async Task<IActionResult> GetBuildingsWithoutFloors()
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsWithoutFloorsAsync();
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings without floors");
                return StatusCode(500, "An error occurred while retrieving buildings without floors.");
            }
        }

        /// <summary>
        /// Get buildings with apartments
        /// </summary>
        /// <returns>List of buildings that have apartments</returns>
        [HttpGet("with-apartments")]
        public async Task<IActionResult> GetBuildingsWithApartments()
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsWithApartmentsAsync();
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings with apartments");
                return StatusCode(500, "An error occurred while retrieving buildings with apartments.");
            }
        }

        /// <summary>
        /// Get buildings without apartments
        /// </summary>
        /// <returns>List of buildings that have no apartments</returns>
        [HttpGet("without-apartments")]
        public async Task<IActionResult> GetBuildingsWithoutApartments()
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsWithoutApartmentsAsync();
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings without apartments");
                return StatusCode(500, "An error occurred while retrieving buildings without apartments.");
            }
        }

        /// <summary>
        /// Get buildings by floor count range
        /// </summary>
        /// <param name="minCount">Minimum floor count</param>
        /// <param name="maxCount">Maximum floor count</param>
        /// <returns>List of buildings with floor count in the specified range</returns>
        [HttpGet("floor-count-range")]
        public async Task<IActionResult> GetBuildingsByFloorCountRange([FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsByFloorCountRangeAsync(minCount, maxCount);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings by floor count range");
                return StatusCode(500, "An error occurred while retrieving buildings by floor count range.");
            }
        }

        /// <summary>
        /// Get buildings by apartment count range
        /// </summary>
        /// <param name="minCount">Minimum apartment count</param>
        /// <param name="maxCount">Maximum apartment count</param>
        /// <returns>List of buildings with apartment count in the specified range</returns>
        [HttpGet("apartment-count-range")]
        public async Task<IActionResult> GetBuildingsByApartmentCountRange([FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsByApartmentCountRangeAsync(minCount, maxCount);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings by apartment count range");
                return StatusCode(500, "An error occurred while retrieving buildings by apartment count range.");
            }
        }

        /// <summary>
        /// Get top buildings by floor count
        /// </summary>
        /// <param name="topCount">Number of top buildings to return (default: 10)</param>
        /// <returns>List of top buildings by floor count</returns>
        [HttpGet("top-by-floors")]
        public async Task<IActionResult> GetTopBuildingsByFloorCount([FromQuery] int topCount = 10)
        {
            try
            {
                var buildings = await _buildingService.GetTopBuildingsByFloorCountAsync(topCount);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top buildings by floor count");
                return StatusCode(500, "An error occurred while retrieving top buildings by floor count.");
            }
        }

        /// <summary>
        /// Get top buildings by apartment count
        /// </summary>
        /// <param name="topCount">Number of top buildings to return (default: 10)</param>
        /// <returns>List of top buildings by apartment count</returns>
        [HttpGet("top-by-apartments")]
        public async Task<IActionResult> GetTopBuildingsByApartmentCount([FromQuery] int topCount = 10)
        {
            try
            {
                var buildings = await _buildingService.GetTopBuildingsByApartmentCountAsync(topCount);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top buildings by apartment count");
                return StatusCode(500, "An error occurred while retrieving top buildings by apartment count.");
            }
        }

        /// <summary>
        /// Get buildings by revenue range
        /// </summary>
        /// <param name="minRevenue">Minimum revenue</param>
        /// <param name="maxRevenue">Maximum revenue</param>
        /// <returns>List of buildings with revenue in the specified range</returns>
        [HttpGet("revenue-range")]
        public async Task<IActionResult> GetBuildingsByRevenueRange([FromQuery] decimal minRevenue, [FromQuery] decimal maxRevenue)
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsByRevenueRangeAsync(minRevenue, maxRevenue);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings by revenue range");
                return StatusCode(500, "An error occurred while retrieving buildings by revenue range.");
            }
        }

        /// <summary>
        /// Get top buildings by revenue
        /// </summary>
        /// <param name="topCount">Number of top buildings to return (default: 10)</param>
        /// <returns>List of top buildings by revenue</returns>
        [HttpGet("top-by-revenue")]
        public async Task<IActionResult> GetTopBuildingsByRevenue([FromQuery] int topCount = 10)
        {
            try
            {
                var buildings = await _buildingService.GetTopBuildingsByRevenueAsync(topCount);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top buildings by revenue");
                return StatusCode(500, "An error occurred while retrieving top buildings by revenue.");
            }
        }

        /// <summary>
        /// Get buildings by reservation count range
        /// </summary>
        /// <param name="minCount">Minimum reservation count</param>
        /// <param name="maxCount">Maximum reservation count</param>
        /// <returns>List of buildings with reservation count in the specified range</returns>
        [HttpGet("reservation-count-range")]
        public async Task<IActionResult> GetBuildingsByReservationCountRange([FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsByReservationCountRangeAsync(minCount, maxCount);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings by reservation count range");
                return StatusCode(500, "An error occurred while retrieving buildings by reservation count range.");
            }
        }

        /// <summary>
        /// Get top buildings by reservation count
        /// </summary>
        /// <param name="topCount">Number of top buildings to return (default: 10)</param>
        /// <returns>List of top buildings by reservation count</returns>
        [HttpGet("top-by-reservations")]
        public async Task<IActionResult> GetTopBuildingsByReservationCount([FromQuery] int topCount = 10)
        {
            try
            {
                var buildings = await _buildingService.GetTopBuildingsByReservationCountAsync(topCount);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top buildings by reservation count");
                return StatusCode(500, "An error occurred while retrieving top buildings by reservation count.");
            }
        }

        /// <summary>
        /// Get building occupancy rate
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Occupancy rate for the specified building and date range</returns>
        [HttpGet("{buildingId}/occupancy-rate")]
        public async Task<IActionResult> GetBuildingOccupancyRate(int buildingId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var occupancyRate = await _buildingService.GetBuildingOccupancyRateAsync(buildingId, startDate, endDate);
                return Ok(new { BuildingId = buildingId, StartDate = startDate, EndDate = endDate, OccupancyRate = occupancyRate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building occupancy rate for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building occupancy rate.");
            }
        }

        /// <summary>
        /// Get building revenue
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Revenue for the specified building and date range</returns>
        [HttpGet("{buildingId}/revenue")]
        public async Task<IActionResult> GetBuildingRevenue(int buildingId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var revenue = await _buildingService.GetBuildingRevenueAsync(buildingId, startDate, endDate);
                return Ok(new { BuildingId = buildingId, StartDate = startDate, EndDate = endDate, Revenue = revenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building revenue for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building revenue.");
            }
        }

        /// <summary>
        /// Get building reservation count
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Reservation count for the specified building and date range</returns>
        [HttpGet("{buildingId}/reservation-count")]
        public async Task<IActionResult> GetBuildingReservationCount(int buildingId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var count = await _buildingService.GetBuildingReservationCountAsync(buildingId, startDate, endDate);
                return Ok(new { BuildingId = buildingId, StartDate = startDate, EndDate = endDate, Count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building reservation count for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building reservation count.");
            }
        }

        /// <summary>
        /// Get building average stay duration
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Average stay duration for the specified building and date range</returns>
        [HttpGet("{buildingId}/average-stay-duration")]
        public async Task<IActionResult> GetBuildingAverageStayDuration(int buildingId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var averageDuration = await _buildingService.GetBuildingAverageStayDurationAsync(buildingId, startDate, endDate);
                return Ok(new { BuildingId = buildingId, StartDate = startDate, EndDate = endDate, AverageStayDuration = averageDuration });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building average stay duration for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building average stay duration.");
            }
        }

        /// <summary>
        /// Get building utilization statistics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Utilization statistics for the specified building and date range</returns>
        [HttpGet("{buildingId}/utilization-statistics")]
        public async Task<IActionResult> GetBuildingUtilizationStatistics(int buildingId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _buildingService.GetBuildingUtilizationStatisticsAsync(buildingId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building utilization statistics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building utilization statistics.");
            }
        }

        /// <summary>
        /// Get buildings by address
        /// </summary>
        /// <param name="address">Address to search for</param>
        /// <returns>List of buildings with the specified address</returns>
        [HttpGet("address/{address}")]
        public async Task<IActionResult> GetBuildingsByAddress(string address)
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsByAddressAsync(address);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings by address {Address}", address);
                return StatusCode(500, "An error occurred while retrieving buildings by address.");
            }
        }

        /// <summary>
        /// Search buildings by address
        /// </summary>
        /// <param name="address">Address to search for</param>
        /// <returns>List of matching buildings</returns>
        [HttpGet("search/address")]
        public async Task<IActionResult> SearchBuildingsByAddress([FromQuery] string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    return BadRequest("Address cannot be empty.");
                }

                var buildings = await _buildingService.SearchBuildingsByAddressAsync(address);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching buildings by address {Address}", address);
                return StatusCode(500, "An error occurred while searching buildings by address.");
            }
        }

        /// <summary>
        /// Get buildings by hotel and building number
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="buildingNumber">Building number</param>
        /// <returns>Building details</returns>
        [HttpGet("hotel/{hotelId}/number/{buildingNumber}")]
        public async Task<IActionResult> GetBuildingsByHotelAndBuildingNumber(int hotelId, string buildingNumber)
        {
            try
            {
                var building = await _buildingService.GetBuildingsByHotelAndBuildingNumberAsync(hotelId, buildingNumber);
                if (building == null)
                {
                    return NotFound($"Building with number '{buildingNumber}' not found in hotel {hotelId}.");
                }

                return Ok(building);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building by hotel and number");
                return StatusCode(500, "An error occurred while retrieving building by hotel and number.");
            }
        }

        /// <summary>
        /// Get buildings by hotel and building name
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="buildingName">Building name</param>
        /// <returns>List of buildings for the specified hotel and name</returns>
        [HttpGet("hotel/{hotelId}/name/{buildingName}")]
        public async Task<IActionResult> GetBuildingsByHotelAndBuildingName(int hotelId, string buildingName)
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsByHotelAndBuildingNameAsync(hotelId, buildingName);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings by hotel and name");
                return StatusCode(500, "An error occurred while retrieving buildings by hotel and name.");
            }
        }

        /// <summary>
        /// Get buildings by multiple criteria
        /// </summary>
        /// <param name="hotelId">Hotel ID (optional)</param>
        /// <param name="buildingNumber">Building number (optional)</param>
        /// <param name="buildingName">Building name (optional)</param>
        /// <param name="address">Address (optional)</param>
        /// <returns>List of buildings matching the specified criteria</returns>
        [HttpGet("filter")]
        public async Task<IActionResult> GetBuildingsByMultipleCriteria(
            [FromQuery] int? hotelId = null, 
            [FromQuery] string? buildingNumber = null, 
            [FromQuery] string? buildingName = null, 
            [FromQuery] string? address = null)
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsByMultipleCriteriaAsync(hotelId, buildingNumber, buildingName, address);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings by multiple criteria");
                return StatusCode(500, "An error occurred while retrieving buildings by multiple criteria.");
            }
        }

        /// <summary>
        /// Get building floor statistics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns>Floor statistics for the specified building</returns>
        [HttpGet("{buildingId}/floor-statistics")]
        public async Task<IActionResult> GetBuildingFloorStatistics(int buildingId)
        {
            try
            {
                var statistics = await _buildingService.GetBuildingFloorStatisticsAsync(buildingId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building floor statistics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building floor statistics.");
            }
        }

        /// <summary>
        /// Get building apartment statistics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns>Apartment statistics for the specified building</returns>
        [HttpGet("{buildingId}/apartment-statistics")]
        public async Task<IActionResult> GetBuildingApartmentStatistics(int buildingId)
        {
            try
            {
                var statistics = await _buildingService.GetBuildingApartmentStatisticsAsync(buildingId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building apartment statistics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building apartment statistics.");
            }
        }

        /// <summary>
        /// Get building reservation statistics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Reservation statistics for the specified building and date range</returns>
        [HttpGet("{buildingId}/reservation-statistics")]
        public async Task<IActionResult> GetBuildingReservationStatistics(int buildingId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _buildingService.GetBuildingReservationStatisticsAsync(buildingId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building reservation statistics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building reservation statistics.");
            }
        }

        /// <summary>
        /// Get building revenue statistics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Revenue statistics for the specified building and date range</returns>
        [HttpGet("{buildingId}/revenue-statistics")]
        public async Task<IActionResult> GetBuildingRevenueStatistics(int buildingId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _buildingService.GetBuildingRevenueStatisticsAsync(buildingId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building revenue statistics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building revenue statistics.");
            }
        }

        /// <summary>
        /// Get building occupancy statistics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Occupancy statistics for the specified building and date range</returns>
        [HttpGet("{buildingId}/occupancy-statistics")]
        public async Task<IActionResult> GetBuildingOccupancyStatistics(int buildingId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _buildingService.GetBuildingOccupancyStatisticsAsync(buildingId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building occupancy statistics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building occupancy statistics.");
            }
        }

        /// <summary>
        /// Get building performance metrics
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Performance metrics for the specified building and date range</returns>
        [HttpGet("{buildingId}/performance-metrics")]
        public async Task<IActionResult> GetBuildingPerformanceMetrics(int buildingId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var metrics = await _buildingService.GetBuildingPerformanceMetricsAsync(buildingId, startDate, endDate);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building performance metrics for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving building performance metrics.");
            }
        }
    }
}
