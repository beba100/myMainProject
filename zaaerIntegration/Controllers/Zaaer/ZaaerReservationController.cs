using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    [Route("api/zaaer/ZaaerReservation")]
    [ApiController]
    public class ZaaerReservationController : ControllerBase
    {
        private readonly IZaaerReservationService _zaaerReservationService;
        private readonly IMapper _mapper;
        private readonly IPartnerQueueService _queueService;
        private readonly IConfiguration _configuration;
        private readonly IQueueSettingsProvider _queueSettings;

        public ZaaerReservationController(IZaaerReservationService zaaerReservationService, IMapper mapper, IPartnerQueueService queueService, IConfiguration configuration, IQueueSettingsProvider queueSettings)
        {
            _zaaerReservationService = zaaerReservationService;
            _mapper = mapper;
            _queueService = queueService;
            _configuration = configuration;
            _queueSettings = queueSettings;
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

            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var dto = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = "/api/zaaer/ZaaerReservation",
                    OperationKey = "Zaaer.Reservation.Create",
                    PayloadType = nameof(ZaaerCreateReservationDto),
                    PayloadJson = JsonSerializer.Serialize(createReservationDto),
                    HotelId = createReservationDto.HotelId
                };
                await _queueService.EnqueueAsync(dto);
                return Accepted(new { queued = true, requestRef = dto.RequestRef });
            }

            try
            {
            var reservationResponse = await _zaaerReservationService.CreateReservationAsync(createReservationDto);
            return CreatedAtAction(nameof(GetReservationById), new { reservationId = reservationResponse.ReservationId }, reservationResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner: {ex.InnerException.Message}";
                }
                return StatusCode(500, $"Error creating reservation: {errorMessage}");
            }
        }

        /// <summary>
        /// Creates a new reservation (Tools endpoint) - same logic but different route to avoid conflicts with the existing UI.
        /// </summary>
        /// <param name="createReservationDto">Reservation data</param>
        /// <returns>A newly created reservation</returns>
        [HttpPost("tool")] // api/zaaer/ZaaerReservation/tool
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ZaaerReservationResponseDto>> CreateReservationTool([FromBody] ZaaerCreateReservationToolDto createReservationDto)
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
                    Operation = "/api/zaaer/ZaaerReservation/tool",
                    OperationKey = "Zaaer.Reservation.Create",
                    PayloadType = nameof(ZaaerCreateReservationToolDto),
                    PayloadJson = JsonSerializer.Serialize(createReservationDto),
                    HotelId = createReservationDto.HotelId
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }

            // Map minimal Tool DTO to the service DTO used internally
            var mapped = new ZaaerCreateReservationDto
            {
                ReservationNo = createReservationDto.ReservationNo,
                HotelId = createReservationDto.HotelId,
                CustomerId = createReservationDto.CustomerId,
                ReservationDate = createReservationDto.ReservationDate,
                RentalType = createReservationDto.RentalType,
                NumberOfMonths = createReservationDto.NumberOfMonths,
                TotalPenalties = createReservationDto.TotalPenalties,
                TotalDiscounts = createReservationDto.TotalDiscounts,
                TotalExtra = createReservationDto.TotalExtra,
                CorporateId = createReservationDto.CorporateId,
                CheckInDate = createReservationDto.CheckInDate,
                CheckOutDate = createReservationDto.CheckOutDate,
                DepartureDate = createReservationDto.DepartureDate,
                ReservationUnits = createReservationDto.ReservationUnits.Select(u => new ZaaerReservationUnitDto
                {
                    ReservationId = u.ReservationId,
                    ApartmentId = u.ApartmentId,
                    CheckInDate = u.CheckInDate,
                    CheckOutDate = u.CheckOutDate,
                    DepartureDate = u.DepartureDate,
                    RentAmount = u.RentAmount
                }).ToList()
            };

            var reservationResponse = await _zaaerReservationService.CreateReservationAsync(mapped);
            return CreatedAtAction(nameof(GetReservationById), new { reservationId = reservationResponse.ReservationId }, reservationResponse);
        }

        /// <summary>
        /// Updates an existing reservation by ID or ZaaerId for Zaaer integration.
        /// First tries to find by ZaaerId, if not found then tries by ReservationId.
        /// </summary>
        /// <param name="reservationId">The ID (ReservationId or ZaaerId) of the reservation to update</param>
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

            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var dto = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = $"/api/zaaer/ZaaerReservation/{reservationId}",
                    OperationKey = "Zaaer.Reservation.UpdateById",
                    TargetId = reservationId,
                    PayloadType = nameof(ZaaerUpdateReservationDto),
                    PayloadJson = JsonSerializer.Serialize(updateReservationDto),
                    HotelId = updateReservationDto.HotelId
                };
                await _queueService.EnqueueAsync(dto);
                return Accepted(new { queued = true, requestRef = dto.RequestRef });
            }

            // First try to find by ZaaerId (since Zaaer sends zaaerId in URL)
            var reservationByZaaerId = await _zaaerReservationService.UpdateReservationByZaaerIdAsync(reservationId, updateReservationDto);
            if (reservationByZaaerId != null)
            {
                return Ok(reservationByZaaerId);
            }

            // If not found by ZaaerId, try by ReservationId (backward compatibility)
            var reservationResponse = await _zaaerReservationService.UpdateReservationAsync(reservationId, updateReservationDto);
            if (reservationResponse == null)
            {
                return NotFound($"Reservation with ID or ZaaerId {reservationId} not found");
            }

            return Ok(reservationResponse);
        }

        /// <summary>
        /// Updates an existing reservation by reservation number for Zaaer integration.
        /// </summary>
        /// <param name="reservationNo">The reservation number to update</param>
        /// <param name="updateReservationDto">Updated reservation data</param>
        /// <returns>The updated reservation</returns>
        [HttpPut("number/{reservationNo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerReservationResponseDto>> UpdateReservationByNumber(string reservationNo, [FromBody] ZaaerUpdateReservationDto updateReservationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var dto = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = $"/api/zaaer/ZaaerReservation/number/{reservationNo}",
                    OperationKey = "Zaaer.Reservation.UpdateByNumber",
                    PayloadType = reservationNo, // carry number temporarily
                    PayloadJson = JsonSerializer.Serialize(updateReservationDto),
                    HotelId = updateReservationDto.HotelId
                };
                await _queueService.EnqueueAsync(dto);
                return Accepted(new { queued = true, requestRef = dto.RequestRef });
            }

            var reservationResponse = await _zaaerReservationService.UpdateReservationByNumberAsync(reservationNo, updateReservationDto);
            if (reservationResponse == null)
            {
                return NotFound($"Reservation with number '{reservationNo}' not found");
            }

            return Ok(reservationResponse);
        }

        /// <summary>
        /// Updates an existing reservation by Zaaer external id.
        /// </summary>
        /// <param name="zaaerId">External Zaaer id stored on reservations table</param>
        /// <param name="updateReservationDto">Updated reservation data</param>
        /// <returns>The updated reservation</returns>
        [HttpPut("zaaer/{zaaerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerReservationResponseDto>> UpdateReservationByZaaerId(int zaaerId, [FromBody] ZaaerUpdateReservationDto updateReservationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var dto = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = $"/api/zaaer/ZaaerReservation/zaaer/{zaaerId}",
                    OperationKey = "Zaaer.Reservation.UpdateByZaaerId",
                    PayloadType = nameof(ZaaerUpdateReservationDto),
                    PayloadJson = JsonSerializer.Serialize(updateReservationDto),
                    HotelId = updateReservationDto.HotelId
                };
                await _queueService.EnqueueAsync(dto);
                return Accepted(new { queued = true, requestRef = dto.RequestRef });
            }

            var reservationResponse = await _zaaerReservationService.UpdateReservationByZaaerIdAsync(zaaerId, updateReservationDto);
            if (reservationResponse == null)
            {
                return NotFound($"Reservation with zaaerId '{zaaerId}' not found");
            }

            return Ok(reservationResponse);
        }

        /// <summary>
        /// Gets all reservations for a specific hotel.
        /// </summary>
        /// <param name="hotelId">The hotel ID</param>
        /// <returns>List of reservations</returns>
        [HttpGet("hotel/{hotelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ZaaerReservationResponseDto>>> GetReservationsByHotelId(int hotelId)
        {
            var reservations = await _zaaerReservationService.GetReservationsByHotelIdAsync(hotelId);
            return Ok(reservations);
        }

        /// <summary>
        /// Gets a specific reservation by ID.
        /// </summary>
        /// <param name="reservationId">The reservation ID</param>
        /// <returns>The reservation</returns>
        [HttpGet("{reservationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerReservationResponseDto>> GetReservationById(int reservationId)
        {
            var reservation = await _zaaerReservationService.GetReservationByIdAsync(reservationId);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {reservationId} not found");
            }

            return Ok(reservation);
        }

        /// <summary>
        /// Gets a specific reservation by reservation number.
        /// </summary>
        /// <param name="reservationNo">The reservation number</param>
        /// <returns>The reservation</returns>
        [HttpGet("number/{reservationNo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerReservationResponseDto>> GetReservationByNumber(string reservationNo)
        {
            var reservation = await _zaaerReservationService.GetReservationByNumberAsync(reservationNo);
            if (reservation == null)
            {
                return NotFound($"Reservation with number '{reservationNo}' not found");
            }

            return Ok(reservation);
        }
    }
}
