using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;

namespace zaaerIntegration.Controllers.Zaaer
{
    [Route("api/zaaer/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IZaaerReservationService _zaaerReservationService;
        private readonly IMapper _mapper;

        public ReservationController(IZaaerReservationService zaaerReservationService, IMapper mapper)
        {
            _zaaerReservationService = zaaerReservationService;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new reservation for Zaaer integration.
        /// </summary>
        /// <param name="createReservationDto">Reservation data</param>
        /// <returns>A newly created reservation</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ZaaerReservationResponseDto>> CreateReservation([FromBody] ZaaerCreateReservationDto createReservationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reservationResponse = await _zaaerReservationService.CreateReservationAsync(createReservationDto);
            return CreatedAtAction(nameof(GetReservationById), new { reservationId = reservationResponse.ReservationId }, reservationResponse);
        }

        /// <summary>
        /// Updates an existing reservation for Zaaer integration.
        /// </summary>
        /// <param name="reservationId">The ID of the reservation to update</param>
        /// <param name="updateReservationDto">Updated reservation data</param>
        /// <returns>The updated reservation</returns>
        [HttpPut("{reservationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerReservationResponseDto>> UpdateReservation(int reservationId, [FromBody] ZaaerUpdateReservationDto updateReservationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reservationResponse = await _zaaerReservationService.UpdateReservationAsync(reservationId, updateReservationDto);
            if (reservationResponse == null)
            {
                return NotFound($"Reservation with ID {reservationId} not found.");
            }
            return Ok(reservationResponse);
        }

        /// <summary>
        /// Gets a reservation by ID for Zaaer integration.
        /// </summary>
        /// <param name="reservationId">The ID of the reservation</param>
        /// <returns>The reservation data</returns>
        [HttpGet("{reservationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerReservationResponseDto>> GetReservationById(int reservationId)
        {
            var reservation = await _zaaerReservationService.GetReservationByIdAsync(reservationId);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {reservationId} not found.");
            }
            return Ok(reservation);
        }

        /// <summary>
        /// Gets all reservations for a specific hotel for Zaaer integration.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel</param>
        /// <returns>A list of reservations</returns>
        [HttpGet("hotel/{hotelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ZaaerReservationResponseDto>>> GetReservationsByHotelId(int hotelId)
        {
            var reservations = await _zaaerReservationService.GetReservationsByHotelIdAsync(hotelId);
            return Ok(reservations);
        }

        /// <summary>
        /// Deletes a reservation by ID for Zaaer integration.
        /// </summary>
        /// <param name="reservationId">The ID of the reservation to delete</param>
        /// <returns>No content</returns>
        [HttpDelete("{reservationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteReservation(int reservationId)
        {
            var deleted = await _zaaerReservationService.DeleteReservationAsync(reservationId);
            if (!deleted)
            {
                return NotFound($"Reservation with ID {reservationId} not found.");
            }
            return NoContent();
        }
    }
}
