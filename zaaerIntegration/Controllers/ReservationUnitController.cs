using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for ReservationUnit operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationUnitController : ControllerBase
    {
        private readonly IReservationUnitService _reservationUnitService;
        private readonly ILogger<ReservationUnitController> _logger;

        /// <summary>
        /// Initializes a new instance of the ReservationUnitController class
        /// </summary>
        /// <param name="reservationUnitService">ReservationUnit service</param>
        /// <param name="logger">Logger</param>
        public ReservationUnitController(IReservationUnitService reservationUnitService, ILogger<ReservationUnitController> logger)
        {
            _reservationUnitService = reservationUnitService;
            _logger = logger;
        }

        /// <summary>
        /// Get all reservation units with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for apartment name, code, status, or reservation number</param>
        /// <returns>List of reservation units with total count</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllReservationUnits(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (reservationUnits, totalCount) = await _reservationUnitService.GetAllReservationUnitsAsync(pageNumber, pageSize, searchTerm);
                
                return Ok(new
                {
                    ReservationUnits = reservationUnits,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units");
                return StatusCode(500, "An error occurred while retrieving reservation units.");
            }
        }

        /// <summary>
        /// Get reservation unit by ID
        /// </summary>
        /// <param name="id">Reservation unit ID</param>
        /// <returns>Reservation unit details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationUnitById(int id)
        {
            try
            {
                var reservationUnit = await _reservationUnitService.GetReservationUnitByIdAsync(id);
                if (reservationUnit == null)
                {
                    return NotFound($"Reservation unit with ID {id} not found.");
                }

                return Ok(reservationUnit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation unit with ID {ReservationUnitId}", id);
                return StatusCode(500, "An error occurred while retrieving the reservation unit.");
            }
        }

        /// <summary>
        /// Create new reservation unit
        /// </summary>
        /// <param name="createReservationUnitDto">Reservation unit creation data</param>
        /// <returns>Created reservation unit</returns>
        [HttpPost]
        public async Task<IActionResult> CreateReservationUnit([FromBody] CreateReservationUnitDto createReservationUnitDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var reservationUnit = await _reservationUnitService.CreateReservationUnitAsync(createReservationUnitDto);
                return CreatedAtAction(nameof(GetReservationUnitById), new { id = reservationUnit.UnitId }, reservationUnit);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation unit");
                return StatusCode(500, "An error occurred while creating the reservation unit.");
            }
        }

        /// <summary>
        /// Update existing reservation unit
        /// </summary>
        /// <param name="id">Reservation unit ID</param>
        /// <param name="updateReservationUnitDto">Reservation unit update data</param>
        /// <returns>Updated reservation unit</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservationUnit(int id, [FromBody] UpdateReservationUnitDto updateReservationUnitDto)
        {
            try
            {
                if (id != updateReservationUnitDto.UnitId)
                {
                    return BadRequest("Reservation unit ID in URL does not match ID in body.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var reservationUnit = await _reservationUnitService.UpdateReservationUnitAsync(id, updateReservationUnitDto);
                if (reservationUnit == null)
                {
                    return NotFound($"Reservation unit with ID {id} not found.");
                }

                return Ok(reservationUnit);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation unit with ID {ReservationUnitId}", id);
                return StatusCode(500, "An error occurred while updating the reservation unit.");
            }
        }

        /// <summary>
        /// Delete reservation unit
        /// </summary>
        /// <param name="id">Reservation unit ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservationUnit(int id)
        {
            try
            {
                var result = await _reservationUnitService.DeleteReservationUnitAsync(id);
                if (!result)
                {
                    return NotFound($"Reservation unit with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reservation unit with ID {ReservationUnitId}", id);
                return StatusCode(500, "An error occurred while deleting the reservation unit.");
            }
        }

        /// <summary>
        /// Get reservation units by reservation ID
        /// </summary>
        /// <param name="reservationId">Reservation ID</param>
        /// <returns>List of reservation units for the specified reservation</returns>
        [HttpGet("reservation/{reservationId}")]
        public async Task<IActionResult> GetReservationUnitsByReservationId(int reservationId)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByReservationIdAsync(reservationId);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units for reservation {ReservationId}", reservationId);
                return StatusCode(500, "An error occurred while retrieving reservation units for the reservation.");
            }
        }

        /// <summary>
        /// Get reservation units by apartment ID
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <returns>List of reservation units for the specified apartment</returns>
        [HttpGet("apartment/{apartmentId}")]
        public async Task<IActionResult> GetReservationUnitsByApartmentId(int apartmentId)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByApartmentIdAsync(apartmentId);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units for apartment {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while retrieving reservation units for the apartment.");
            }
        }

        /// <summary>
        /// Get reservation units by status
        /// </summary>
        /// <param name="status">Reservation unit status</param>
        /// <returns>List of reservation units with the specified status</returns>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetReservationUnitsByStatus(string status)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByStatusAsync(status);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by status {Status}", status);
                return StatusCode(500, "An error occurred while retrieving reservation units by status.");
            }
        }

        /// <summary>
        /// Get reservation units by check-in date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of reservation units with check-in dates in the specified range</returns>
        [HttpGet("check-in-range")]
        public async Task<IActionResult> GetReservationUnitsByCheckInDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByCheckInDateRangeAsync(startDate, endDate);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by check-in date range");
                return StatusCode(500, "An error occurred while retrieving reservation units by check-in date range.");
            }
        }

        /// <summary>
        /// Get reservation units by check-out date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of reservation units with check-out dates in the specified range</returns>
        [HttpGet("check-out-range")]
        public async Task<IActionResult> GetReservationUnitsByCheckOutDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByCheckOutDateRangeAsync(startDate, endDate);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by check-out date range");
                return StatusCode(500, "An error occurred while retrieving reservation units by check-out date range.");
            }
        }

        /// <summary>
        /// Get reservation units by date range (overlapping dates)
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of reservation units with overlapping dates</returns>
        [HttpGet("date-range")]
        public async Task<IActionResult> GetReservationUnitsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByDateRangeAsync(startDate, endDate);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by date range");
                return StatusCode(500, "An error occurred while retrieving reservation units by date range.");
            }
        }

        /// <summary>
        /// Get reservation units by rent amount range
        /// </summary>
        /// <param name="minAmount">Minimum rent amount</param>
        /// <param name="maxAmount">Maximum rent amount</param>
        /// <returns>List of reservation units with rent amounts in the specified range</returns>
        [HttpGet("rent-amount-range")]
        public async Task<IActionResult> GetReservationUnitsByRentAmountRange([FromQuery] decimal minAmount, [FromQuery] decimal maxAmount)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByRentAmountRangeAsync(minAmount, maxAmount);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by rent amount range");
                return StatusCode(500, "An error occurred while retrieving reservation units by rent amount range.");
            }
        }

        /// <summary>
        /// Get reservation units by total amount range
        /// </summary>
        /// <param name="minAmount">Minimum total amount</param>
        /// <param name="maxAmount">Maximum total amount</param>
        /// <returns>List of reservation units with total amounts in the specified range</returns>
        [HttpGet("total-amount-range")]
        public async Task<IActionResult> GetReservationUnitsByTotalAmountRange([FromQuery] decimal minAmount, [FromQuery] decimal maxAmount)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByTotalAmountRangeAsync(minAmount, maxAmount);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by total amount range");
                return StatusCode(500, "An error occurred while retrieving reservation units by total amount range.");
            }
        }

        /// <summary>
        /// Get reservation units by number of nights range
        /// </summary>
        /// <param name="minNights">Minimum number of nights</param>
        /// <param name="maxNights">Maximum number of nights</param>
        /// <returns>List of reservation units with number of nights in the specified range</returns>
        [HttpGet("nights-range")]
        public async Task<IActionResult> GetReservationUnitsByNumberOfNightsRange([FromQuery] int minNights, [FromQuery] int maxNights)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByNumberOfNightsRangeAsync(minNights, maxNights);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by number of nights range");
                return StatusCode(500, "An error occurred while retrieving reservation units by number of nights range.");
            }
        }

        /// <summary>
        /// Get reservation units by VAT rate range
        /// </summary>
        /// <param name="minRate">Minimum VAT rate</param>
        /// <param name="maxRate">Maximum VAT rate</param>
        /// <returns>List of reservation units with VAT rates in the specified range</returns>
        [HttpGet("vat-rate-range")]
        public async Task<IActionResult> GetReservationUnitsByVatRateRange([FromQuery] decimal minRate, [FromQuery] decimal maxRate)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByVatRateRangeAsync(minRate, maxRate);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by VAT rate range");
                return StatusCode(500, "An error occurred while retrieving reservation units by VAT rate range.");
            }
        }

        /// <summary>
        /// Get reservation units by lodging tax rate range
        /// </summary>
        /// <param name="minRate">Minimum lodging tax rate</param>
        /// <param name="maxRate">Maximum lodging tax rate</param>
        /// <returns>List of reservation units with lodging tax rates in the specified range</returns>
        [HttpGet("lodging-tax-rate-range")]
        public async Task<IActionResult> GetReservationUnitsByLodgingTaxRateRange([FromQuery] decimal minRate, [FromQuery] decimal maxRate)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByLodgingTaxRateRangeAsync(minRate, maxRate);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by lodging tax rate range");
                return StatusCode(500, "An error occurred while retrieving reservation units by lodging tax rate range.");
            }
        }

        /// <summary>
        /// Get reservation unit statistics
        /// </summary>
        /// <returns>Reservation unit statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetReservationUnitStatistics()
        {
            try
            {
                var statistics = await _reservationUnitService.GetReservationUnitStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation unit statistics");
                return StatusCode(500, "An error occurred while retrieving reservation unit statistics.");
            }
        }

        /// <summary>
        /// Get reservation units by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of reservation units for the specified hotel</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetReservationUnitsByHotelId(int hotelId)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByHotelIdAsync(hotelId);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving reservation units for the hotel.");
            }
        }

        /// <summary>
        /// Get reservation units by building ID
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns>List of reservation units for the specified building</returns>
        [HttpGet("building/{buildingId}")]
        public async Task<IActionResult> GetReservationUnitsByBuildingId(int buildingId)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByBuildingIdAsync(buildingId);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while retrieving reservation units for the building.");
            }
        }

        /// <summary>
        /// Get reservation units by floor ID
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <returns>List of reservation units for the specified floor</returns>
        [HttpGet("floor/{floorId}")]
        public async Task<IActionResult> GetReservationUnitsByFloorId(int floorId)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByFloorIdAsync(floorId);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units for floor {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving reservation units for the floor.");
            }
        }

        /// <summary>
        /// Get reservation units by room type ID
        /// </summary>
        /// <param name="roomTypeId">Room type ID</param>
        /// <returns>List of reservation units for the specified room type</returns>
        [HttpGet("room-type/{roomTypeId}")]
        public async Task<IActionResult> GetReservationUnitsByRoomTypeId(int roomTypeId)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByRoomTypeIdAsync(roomTypeId);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, "An error occurred while retrieving reservation units for the room type.");
            }
        }

        /// <summary>
        /// Get reservation units by customer ID
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of reservation units for the specified customer</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetReservationUnitsByCustomerId(int customerId)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByCustomerIdAsync(customerId);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while retrieving reservation units for the customer.");
            }
        }

        /// <summary>
        /// Get reservation units by corporate customer ID
        /// </summary>
        /// <param name="corporateCustomerId">Corporate customer ID</param>
        /// <returns>List of reservation units for the specified corporate customer</returns>
        [HttpGet("corporate-customer/{corporateCustomerId}")]
        public async Task<IActionResult> GetReservationUnitsByCorporateCustomerId(int corporateCustomerId)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByCorporateCustomerIdAsync(corporateCustomerId);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units for corporate customer {CorporateCustomerId}", corporateCustomerId);
                return StatusCode(500, "An error occurred while retrieving reservation units for the corporate customer.");
            }
        }

        /// <summary>
        /// Get reservation units by created date
        /// </summary>
        /// <param name="createdDate">Created date</param>
        /// <returns>List of reservation units created on the specified date</returns>
        [HttpGet("created-date")]
        public async Task<IActionResult> GetReservationUnitsByCreatedDate([FromQuery] DateTime createdDate)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByCreatedDateAsync(createdDate);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by created date");
                return StatusCode(500, "An error occurred while retrieving reservation units by created date.");
            }
        }

        /// <summary>
        /// Get reservation units by created date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of reservation units created in the specified date range</returns>
        [HttpGet("created-date-range")]
        public async Task<IActionResult> GetReservationUnitsByCreatedDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetReservationUnitsByCreatedDateRangeAsync(startDate, endDate);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation units by created date range");
                return StatusCode(500, "An error occurred while retrieving reservation units by created date range.");
            }
        }

        /// <summary>
        /// Get active reservation units
        /// </summary>
        /// <returns>List of active reservation units</returns>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveReservationUnits()
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetActiveReservationUnitsAsync();
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active reservation units");
                return StatusCode(500, "An error occurred while retrieving active reservation units.");
            }
        }

        /// <summary>
        /// Get cancelled reservation units
        /// </summary>
        /// <returns>List of cancelled reservation units</returns>
        [HttpGet("cancelled")]
        public async Task<IActionResult> GetCancelledReservationUnits()
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetCancelledReservationUnitsAsync();
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cancelled reservation units");
                return StatusCode(500, "An error occurred while retrieving cancelled reservation units.");
            }
        }

        /// <summary>
        /// Get completed reservation units
        /// </summary>
        /// <returns>List of completed reservation units</returns>
        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedReservationUnits()
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetCompletedReservationUnitsAsync();
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed reservation units");
                return StatusCode(500, "An error occurred while retrieving completed reservation units.");
            }
        }

        /// <summary>
        /// Search reservation units by apartment name
        /// </summary>
        /// <param name="apartmentName">Apartment name to search for</param>
        /// <returns>List of matching reservation units</returns>
        [HttpGet("search/apartment")]
        public async Task<IActionResult> SearchReservationUnitsByApartmentName([FromQuery] string apartmentName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apartmentName))
                {
                    return BadRequest("Apartment name cannot be empty.");
                }

                var reservationUnits = await _reservationUnitService.SearchReservationUnitsByApartmentNameAsync(apartmentName);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservation units by apartment name {ApartmentName}", apartmentName);
                return StatusCode(500, "An error occurred while searching reservation units by apartment name.");
            }
        }

        /// <summary>
        /// Search reservation units by building name
        /// </summary>
        /// <param name="buildingName">Building name to search for</param>
        /// <returns>List of matching reservation units</returns>
        [HttpGet("search/building")]
        public async Task<IActionResult> SearchReservationUnitsByBuildingName([FromQuery] string buildingName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(buildingName))
                {
                    return BadRequest("Building name cannot be empty.");
                }

                var reservationUnits = await _reservationUnitService.SearchReservationUnitsByBuildingNameAsync(buildingName);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservation units by building name {BuildingName}", buildingName);
                return StatusCode(500, "An error occurred while searching reservation units by building name.");
            }
        }

        /// <summary>
        /// Search reservation units by room type name
        /// </summary>
        /// <param name="roomTypeName">Room type name to search for</param>
        /// <returns>List of matching reservation units</returns>
        [HttpGet("search/room-type")]
        public async Task<IActionResult> SearchReservationUnitsByRoomTypeName([FromQuery] string roomTypeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomTypeName))
                {
                    return BadRequest("Room type name cannot be empty.");
                }

                var reservationUnits = await _reservationUnitService.SearchReservationUnitsByRoomTypeNameAsync(roomTypeName);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservation units by room type name {RoomTypeName}", roomTypeName);
                return StatusCode(500, "An error occurred while searching reservation units by room type name.");
            }
        }

        /// <summary>
        /// Search reservation units by customer name
        /// </summary>
        /// <param name="customerName">Customer name to search for</param>
        /// <returns>List of matching reservation units</returns>
        [HttpGet("search/customer")]
        public async Task<IActionResult> SearchReservationUnitsByCustomerName([FromQuery] string customerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerName))
                {
                    return BadRequest("Customer name cannot be empty.");
                }

                var reservationUnits = await _reservationUnitService.SearchReservationUnitsByCustomerNameAsync(customerName);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservation units by customer name {CustomerName}", customerName);
                return StatusCode(500, "An error occurred while searching reservation units by customer name.");
            }
        }

        /// <summary>
        /// Search reservation units by corporate customer name
        /// </summary>
        /// <param name="corporateCustomerName">Corporate customer name to search for</param>
        /// <returns>List of matching reservation units</returns>
        [HttpGet("search/corporate-customer")]
        public async Task<IActionResult> SearchReservationUnitsByCorporateCustomerName([FromQuery] string corporateCustomerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(corporateCustomerName))
                {
                    return BadRequest("Corporate customer name cannot be empty.");
                }

                var reservationUnits = await _reservationUnitService.SearchReservationUnitsByCorporateCustomerNameAsync(corporateCustomerName);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservation units by corporate customer name {CorporateCustomerName}", corporateCustomerName);
                return StatusCode(500, "An error occurred while searching reservation units by corporate customer name.");
            }
        }

        /// <summary>
        /// Search reservation units by hotel name
        /// </summary>
        /// <param name="hotelName">Hotel name to search for</param>
        /// <returns>List of matching reservation units</returns>
        [HttpGet("search/hotel")]
        public async Task<IActionResult> SearchReservationUnitsByHotelName([FromQuery] string hotelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hotelName))
                {
                    return BadRequest("Hotel name cannot be empty.");
                }

                var reservationUnits = await _reservationUnitService.SearchReservationUnitsByHotelNameAsync(hotelName);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservation units by hotel name {HotelName}", hotelName);
                return StatusCode(500, "An error occurred while searching reservation units by hotel name.");
            }
        }

        /// <summary>
        /// Get total revenue by reservation ID
        /// </summary>
        /// <param name="reservationId">Reservation ID</param>
        /// <returns>Total revenue for the specified reservation</returns>
        [HttpGet("revenue/reservation/{reservationId}")]
        public async Task<IActionResult> GetTotalRevenueByReservationId(int reservationId)
        {
            try
            {
                var totalRevenue = await _reservationUnitService.GetTotalRevenueByReservationIdAsync(reservationId);
                return Ok(new { ReservationId = reservationId, TotalRevenue = totalRevenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total revenue for reservation {ReservationId}", reservationId);
                return StatusCode(500, "An error occurred while retrieving total revenue for the reservation.");
            }
        }

        /// <summary>
        /// Get total revenue by apartment ID
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <returns>Total revenue for the specified apartment</returns>
        [HttpGet("revenue/apartment/{apartmentId}")]
        public async Task<IActionResult> GetTotalRevenueByApartmentId(int apartmentId)
        {
            try
            {
                var totalRevenue = await _reservationUnitService.GetTotalRevenueByApartmentIdAsync(apartmentId);
                return Ok(new { ApartmentId = apartmentId, TotalRevenue = totalRevenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total revenue for apartment {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while retrieving total revenue for the apartment.");
            }
        }

        /// <summary>
        /// Get total revenue by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>Total revenue for the specified hotel</returns>
        [HttpGet("revenue/hotel/{hotelId}")]
        public async Task<IActionResult> GetTotalRevenueByHotelId(int hotelId)
        {
            try
            {
                var totalRevenue = await _reservationUnitService.GetTotalRevenueByHotelIdAsync(hotelId);
                return Ok(new { HotelId = hotelId, TotalRevenue = totalRevenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total revenue for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving total revenue for the hotel.");
            }
        }

        /// <summary>
        /// Get total revenue by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Total revenue for the specified date range</returns>
        [HttpGet("revenue/date-range")]
        public async Task<IActionResult> GetTotalRevenueByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var totalRevenue = await _reservationUnitService.GetTotalRevenueByDateRangeAsync(startDate, endDate);
                return Ok(new { StartDate = startDate, EndDate = endDate, TotalRevenue = totalRevenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total revenue by date range");
                return StatusCode(500, "An error occurred while retrieving total revenue by date range.");
            }
        }

        /// <summary>
        /// Get average rent amount by apartment ID
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <returns>Average rent amount for the specified apartment</returns>
        [HttpGet("average-rent/apartment/{apartmentId}")]
        public async Task<IActionResult> GetAverageRentAmountByApartmentId(int apartmentId)
        {
            try
            {
                var averageRentAmount = await _reservationUnitService.GetAverageRentAmountByApartmentIdAsync(apartmentId);
                return Ok(new { ApartmentId = apartmentId, AverageRentAmount = averageRentAmount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving average rent amount for apartment {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while retrieving average rent amount for the apartment.");
            }
        }

        /// <summary>
        /// Get average total amount by apartment ID
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <returns>Average total amount for the specified apartment</returns>
        [HttpGet("average-total/apartment/{apartmentId}")]
        public async Task<IActionResult> GetAverageTotalAmountByApartmentId(int apartmentId)
        {
            try
            {
                var averageTotalAmount = await _reservationUnitService.GetAverageTotalAmountByApartmentIdAsync(apartmentId);
                return Ok(new { ApartmentId = apartmentId, AverageTotalAmount = averageTotalAmount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving average total amount for apartment {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while retrieving average total amount for the apartment.");
            }
        }

        /// <summary>
        /// Get top apartments by revenue
        /// </summary>
        /// <param name="topCount">Number of top apartments to return (default: 10)</param>
        /// <returns>List of top apartments by revenue</returns>
        [HttpGet("top-apartments")]
        public async Task<IActionResult> GetTopApartmentsByRevenue([FromQuery] int topCount = 10)
        {
            try
            {
                var topApartments = await _reservationUnitService.GetTopApartmentsByRevenueAsync(topCount);
                return Ok(topApartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top apartments by revenue");
                return StatusCode(500, "An error occurred while retrieving top apartments by revenue.");
            }
        }

        /// <summary>
        /// Get top hotels by revenue
        /// </summary>
        /// <param name="topCount">Number of top hotels to return (default: 10)</param>
        /// <returns>List of top hotels by revenue</returns>
        [HttpGet("top-hotels")]
        public async Task<IActionResult> GetTopHotelsByRevenue([FromQuery] int topCount = 10)
        {
            try
            {
                var topHotels = await _reservationUnitService.GetTopHotelsByRevenueAsync(topCount);
                return Ok(topHotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top hotels by revenue");
                return StatusCode(500, "An error occurred while retrieving top hotels by revenue.");
            }
        }

        /// <summary>
        /// Get reservation units by overlapping dates (for availability checking)
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <param name="checkInDate">Check-in date</param>
        /// <param name="checkOutDate">Check-out date</param>
        /// <param name="excludeUnitId">Reservation unit ID to exclude from check (for updates)</param>
        /// <returns>List of reservation units with overlapping dates</returns>
        [HttpGet("overlapping-dates")]
        public async Task<IActionResult> GetOverlappingDates([FromQuery] int apartmentId, [FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate, [FromQuery] int? excludeUnitId = null)
        {
            try
            {
                var reservationUnits = await _reservationUnitService.GetOverlappingDatesAsync(apartmentId, checkInDate, checkOutDate, excludeUnitId);
                return Ok(reservationUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overlapping dates");
                return StatusCode(500, "An error occurred while retrieving overlapping dates.");
            }
        }

        /// <summary>
        /// Update reservation unit status
        /// </summary>
        /// <param name="id">Reservation unit ID</param>
        /// <param name="status">New status</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateReservationUnitStatus(int id, [FromBody] string status)
        {
            try
            {
                var result = await _reservationUnitService.UpdateReservationUnitStatusAsync(id, status);
                if (!result)
                {
                    return NotFound($"Reservation unit with ID {id} not found.");
                }

                return Ok(new { Message = "Reservation unit status updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation unit status for ID {ReservationUnitId}", id);
                return StatusCode(500, "An error occurred while updating reservation unit status.");
            }
        }

        /// <summary>
        /// Update reservation unit amounts
        /// </summary>
        /// <param name="id">Reservation unit ID</param>
        /// <param name="rentAmount">New rent amount</param>
        /// <param name="vatAmount">New VAT amount</param>
        /// <param name="lodgingTaxAmount">New lodging tax amount</param>
        /// <param name="totalAmount">New total amount</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/amounts")]
        public async Task<IActionResult> UpdateReservationUnitAmounts(int id, [FromQuery] decimal rentAmount, [FromQuery] decimal? vatAmount, [FromQuery] decimal? lodgingTaxAmount, [FromQuery] decimal totalAmount)
        {
            try
            {
                var result = await _reservationUnitService.UpdateReservationUnitAmountsAsync(id, rentAmount, vatAmount, lodgingTaxAmount, totalAmount);
                if (!result)
                {
                    return NotFound($"Reservation unit with ID {id} not found.");
                }

                return Ok(new { Message = "Reservation unit amounts updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation unit amounts for ID {ReservationUnitId}", id);
                return StatusCode(500, "An error occurred while updating reservation unit amounts.");
            }
        }

        /// <summary>
        /// Update reservation unit dates
        /// </summary>
        /// <param name="id">Reservation unit ID</param>
        /// <param name="checkInDate">New check-in date</param>
        /// <param name="checkOutDate">New check-out date</param>
        /// <param name="numberOfNights">New number of nights</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/dates")]
        public async Task<IActionResult> UpdateReservationUnitDates(int id, [FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate, [FromQuery] int? numberOfNights)
        {
            try
            {
                var result = await _reservationUnitService.UpdateReservationUnitDatesAsync(id, checkInDate, checkOutDate, numberOfNights);
                if (!result)
                {
                    return NotFound($"Reservation unit with ID {id} not found.");
                }

                return Ok(new { Message = "Reservation unit dates updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation unit dates for ID {ReservationUnitId}", id);
                return StatusCode(500, "An error occurred while updating reservation unit dates.");
            }
        }

        /// <summary>
        /// Check apartment availability
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <param name="checkInDate">Check-in date</param>
        /// <param name="checkOutDate">Check-out date</param>
        /// <param name="excludeUnitId">Reservation unit ID to exclude from check (for updates)</param>
        /// <returns>True if apartment is available, false otherwise</returns>
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckApartmentAvailability([FromQuery] int apartmentId, [FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate, [FromQuery] int? excludeUnitId = null)
        {
            try
            {
                var isAvailable = await _reservationUnitService.CheckApartmentAvailabilityAsync(apartmentId, checkInDate, checkOutDate, excludeUnitId);
                return Ok(new { ApartmentId = apartmentId, CheckInDate = checkInDate, CheckOutDate = checkOutDate, IsAvailable = isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking apartment availability");
                return StatusCode(500, "An error occurred while checking apartment availability.");
            }
        }
    }
}
