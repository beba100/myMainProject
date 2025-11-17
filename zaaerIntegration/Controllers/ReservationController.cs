using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Services.Interfaces;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for Reservation operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationController> _logger;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public ReservationController(IReservationService reservationService, ILogger<ReservationController> logger, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _reservationService = reservationService;
            _logger = logger;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Get all reservations with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for reservation number, external ref, or notes</param>
        /// <returns>List of reservations with total count</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllReservations(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (reservations, totalCount) = await _reservationService.GetAllReservationsAsync(pageNumber, pageSize, searchTerm);
                
                return Ok(new
                {
                    Reservations = reservations,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations");
                return StatusCode(500, "An error occurred while retrieving reservations.");
            }
        }

        /// <summary>
        /// Get reservation by ID
        /// </summary>
        /// <param name="id">Reservation ID</param>
        /// <returns>Reservation details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            try
            {
                var reservation = await _reservationService.GetReservationByIdAsync(id);
                if (reservation == null)
                {
                    return NotFound($"Reservation with ID {id} not found.");
                }

                return Ok(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation with ID {ReservationId}", id);
                return StatusCode(500, "An error occurred while retrieving the reservation.");
            }
        }

        /// <summary>
        /// Get reservation by reservation number
        /// </summary>
        /// <param name="reservationNo">Reservation number</param>
        /// <returns>Reservation details</returns>
        [HttpGet("number/{reservationNo}")]
        public async Task<IActionResult> GetReservationByNo(string reservationNo)
        {
            try
            {
                var reservation = await _reservationService.GetReservationByNoAsync(reservationNo);
                if (reservation == null)
                {
                    return NotFound($"Reservation with number '{reservationNo}' not found.");
                }

                return Ok(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation with number {ReservationNo}", reservationNo);
                return StatusCode(500, "An error occurred while retrieving the reservation.");
            }
        }

        /// <summary>
        /// Create new reservation
        /// </summary>
        /// <param name="createReservationDto">Reservation creation data</param>
        /// <returns>Created reservation</returns>
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto createReservationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = "/api/Reservation",
                        OperationKey = "App.Reservation.Create",
                        PayloadType = nameof(CreateReservationDto),
                        PayloadJson = JsonSerializer.Serialize(createReservationDto)
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var reservation = await _reservationService.CreateReservationAsync(createReservationDto);
                return CreatedAtAction(nameof(GetReservationById), new { id = reservation.ReservationId }, reservation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                return StatusCode(500, "An error occurred while creating the reservation.");
            }
        }

        /// <summary>
        /// Update existing reservation
        /// </summary>
        /// <param name="id">Reservation ID</param>
        /// <param name="updateReservationDto">Reservation update data</param>
        /// <returns>Updated reservation</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto updateReservationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = $"/api/Reservation/{id}",
                        OperationKey = "App.Reservation.UpdateById",
                        TargetId = id,
                        PayloadType = nameof(UpdateReservationDto),
                        PayloadJson = JsonSerializer.Serialize(updateReservationDto)
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var reservation = await _reservationService.UpdateReservationAsync(id, updateReservationDto);
                if (reservation == null)
                {
                    return NotFound($"Reservation with ID {id} not found.");
                }

                return Ok(reservation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation with ID {ReservationId}", id);
                return StatusCode(500, "An error occurred while updating the reservation.");
            }
        }

        /// <summary>
        /// Delete reservation
        /// </summary>
        /// <param name="id">Reservation ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            try
            {
                var result = await _reservationService.DeleteReservationAsync(id);
                if (!result)
                {
                    return NotFound($"Reservation with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reservation with ID {ReservationId}", id);
                return StatusCode(500, "An error occurred while deleting the reservation.");
            }
        }

        /// <summary>
        /// Get reservations by customer ID
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of customer reservations</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetReservationsByCustomerId(int customerId)
        {
            try
            {
                var reservations = await _reservationService.GetReservationsByCustomerIdAsync(customerId);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while retrieving customer reservations.");
            }
        }

        /// <summary>
        /// Get reservations by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of hotel reservations</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetReservationsByHotelId(int hotelId)
        {
            try
            {
                var reservations = await _reservationService.GetReservationsByHotelIdAsync(hotelId);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving hotel reservations.");
            }
        }

        /// <summary>
        /// Get reservations by status
        /// </summary>
        /// <param name="status">Reservation status</param>
        /// <returns>List of reservations with specified status</returns>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetReservationsByStatus(string status)
        {
            try
            {
                var reservations = await _reservationService.GetReservationsByStatusAsync(status);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations with status {Status}", status);
                return StatusCode(500, "An error occurred while retrieving reservations by status.");
            }
        }

        /// <summary>
        /// Get reservations by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of reservations in date range</returns>
        [HttpGet("date-range")]
        public async Task<IActionResult> GetReservationsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var reservations = await _reservationService.GetReservationsByDateRangeAsync(startDate, endDate);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations by date range");
                return StatusCode(500, "An error occurred while retrieving reservations by date range.");
            }
        }


        /// <summary>
        /// Search reservations by customer name
        /// </summary>
        /// <param name="customerName">Customer name to search for</param>
        /// <returns>List of matching reservations</returns>
        [HttpGet("search/customer")]
        public async Task<IActionResult> SearchReservationsByCustomerName([FromQuery] string customerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerName))
                {
                    return BadRequest("Customer name cannot be empty.");
                }

                var reservations = await _reservationService.SearchReservationsByCustomerNameAsync(customerName);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservations by customer name {CustomerName}", customerName);
                return StatusCode(500, "An error occurred while searching reservations by customer name.");
            }
        }

        /// <summary>
        /// Search reservations by hotel name
        /// </summary>
        /// <param name="hotelName">Hotel name to search for</param>
        /// <returns>List of matching reservations</returns>
        [HttpGet("search/hotel")]
        public async Task<IActionResult> SearchReservationsByHotelName([FromQuery] string hotelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hotelName))
                {
                    return BadRequest("Hotel name cannot be empty.");
                }

                var reservations = await _reservationService.SearchReservationsByHotelNameAsync(hotelName);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservations by hotel name {HotelName}", hotelName);
                return StatusCode(500, "An error occurred while searching reservations by hotel name.");
            }
        }

        /// <summary>
        /// Search reservations by reservation number
        /// </summary>
        /// <param name="reservationNo">Reservation number to search for</param>
        /// <returns>List of matching reservations</returns>
        [HttpGet("search/number")]
        public async Task<IActionResult> SearchReservationsByNo([FromQuery] string reservationNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reservationNo))
                {
                    return BadRequest("Reservation number cannot be empty.");
                }

                var reservations = await _reservationService.SearchReservationsByNoAsync(reservationNo);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservations by number {ReservationNo}", reservationNo);
                return StatusCode(500, "An error occurred while searching reservations by number.");
            }
        }

        /// <summary>
        /// Get reservation statistics
        /// </summary>
        /// <returns>Reservation statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetReservationStatistics()
        {
            try
            {
                var statistics = await _reservationService.GetReservationStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation statistics");
                return StatusCode(500, "An error occurred while retrieving reservation statistics.");
            }
        }

        /// <summary>
        /// Check if reservation number exists
        /// </summary>
        /// <param name="reservationNo">Reservation number to check</param>
        /// <param name="excludeId">Reservation ID to exclude from check (for updates)</param>
        /// <returns>True if number exists, false otherwise</returns>
        [HttpGet("check-number")]
        public async Task<IActionResult> CheckReservationNumber([FromQuery] string reservationNo, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reservationNo))
                {
                    return BadRequest("Reservation number cannot be empty.");
                }

                var exists = await _reservationService.ReservationNoExistsAsync(reservationNo, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking reservation number {ReservationNo}", reservationNo);
                return StatusCode(500, "An error occurred while checking reservation number.");
            }
        }


        /// <summary>
        /// Update reservation status
        /// </summary>
        /// <param name="id">Reservation ID</param>
        /// <param name="status">New status</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateReservationStatus(int id, [FromBody] string status)
        {
            try
            {
                var result = await _reservationService.UpdateReservationStatusAsync(id, status);
                if (!result)
                {
                    return NotFound($"Reservation with ID {id} not found.");
                }

                return Ok(new { Message = "Reservation status updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation status for ID {ReservationId}", id);
                return StatusCode(500, "An error occurred while updating reservation status.");
            }
        }


        /// <summary>
        /// Cancel reservation
        /// </summary>
        /// <param name="id">Reservation ID</param>
        /// <param name="cancellationReason">Reason for cancellation</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(int id, [FromBody] string cancellationReason)
        {
            try
            {
                var result = await _reservationService.CancelReservationAsync(id, cancellationReason);
                if (!result)
                {
                    return NotFound($"Reservation with ID {id} not found.");
                }

                return Ok(new { Message = "Reservation cancelled successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation with ID {ReservationId}", id);
                return StatusCode(500, "An error occurred while cancelling the reservation.");
            }
        }

        /// <summary>
        /// Complete reservation
        /// </summary>
        /// <param name="id">Reservation ID</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> CompleteReservation(int id)
        {
            try
            {
                var result = await _reservationService.CompleteReservationAsync(id);
                if (!result)
                {
                    return NotFound($"Reservation with ID {id} not found.");
                }

                return Ok(new { Message = "Reservation completed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing reservation with ID {ReservationId}", id);
                return StatusCode(500, "An error occurred while completing the reservation.");
            }
        }
    }
}
