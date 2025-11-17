using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    /// <summary>
    /// Controller for Zaaer Room Type Rate operations
    /// </summary>
    [Route("api/zaaer/[controller]")]
    [ApiController]
    public class ZaaerRoomTypeRateController : ControllerBase
    {
        private readonly IZaaerRoomTypeRateService _service;
        private readonly IMapper _mapper;
        private readonly ILogger<ZaaerRoomTypeRateController> _logger;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        /// <summary>
        /// Initializes controller and injects queue service for optional enqueue mode
        /// </summary>
        public ZaaerRoomTypeRateController(
            IZaaerRoomTypeRateService service, 
            IMapper mapper,
			ILogger<ZaaerRoomTypeRateController> logger,
            IPartnerQueueService queueService,
            IQueueSettingsProvider queueSettings)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Creates a new room type rate for Zaaer integration.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ZaaerRoomTypeRateResponseDto>> CreateRoomTypeRate([FromBody] ZaaerCreateRoomTypeRateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ��� ��� ������� ����� ����� �� ������� ������ 202
            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var dto = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = "/api/zaaer/ZaaerRoomTypeRate",
                    OperationKey = "Zaaer.RoomTypeRate.Create",
                    PayloadType = nameof(ZaaerCreateRoomTypeRateDto),
                    PayloadJson = JsonSerializer.Serialize(createDto),
                    HotelId = createDto.HotelId
                };
                await _queueService.EnqueueAsync(dto);
                return Accepted(new { queued = true, requestRef = dto.RequestRef });
            }

            try
            {
                var response = await _service.CreateRoomTypeRateAsync(createDto);
                return CreatedAtAction(nameof(GetRoomTypeRateById), new { rateId = response.RateId }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing room type rate for Zaaer integration by ZaaerId from body.
        /// This endpoint matches Zaaer's expected format: PUT /api/zaaer/ZaaerRoomTypeRate/ with zaaerId in body.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerRoomTypeRateResponseDto>> UpdateRoomTypeRate([FromBody] ZaaerUpdateRoomTypeRateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (updateDto == null || !updateDto.ZaaerId.HasValue)
            {
                return BadRequest("ZaaerId is required in the request body.");
            }

            var zaaerId = updateDto.ZaaerId.Value;

            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var dto = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = "/api/zaaer/ZaaerRoomTypeRate",
                    OperationKey = "Zaaer.RoomTypeRate.UpdateByZaaerId",
                    TargetId = zaaerId,
                    PayloadType = nameof(ZaaerUpdateRoomTypeRateDto),
                    PayloadJson = JsonSerializer.Serialize(updateDto),
                    HotelId = updateDto.HotelId
                };
                await _queueService.EnqueueAsync(dto);
                return Accepted(new { queued = true, requestRef = dto.RequestRef });
            }

            var response = await _service.UpdateRoomTypeRateByZaaerIdAsync(zaaerId, updateDto);
            if (response == null)
            {
                return NotFound($"Room type rate with ZaaerId {zaaerId} not found");
            }

            return Ok(response);
        }

        /// <summary>
        /// Updates an existing room type rate for Zaaer integration by ZaaerId from route parameter.
        /// Alternative endpoint: PUT /api/zaaer/ZaaerRoomTypeRate/{zaaerId}
        /// </summary>
        [HttpPut("{zaaerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerRoomTypeRateResponseDto>> UpdateRoomTypeRateByRoute(int zaaerId, [FromBody] ZaaerUpdateRoomTypeRateDto updateDto)
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
                    Operation = $"/api/zaaer/ZaaerRoomTypeRate/{zaaerId}",
                    OperationKey = "Zaaer.RoomTypeRate.UpdateByZaaerId",
                    TargetId = zaaerId,
                    PayloadType = nameof(ZaaerUpdateRoomTypeRateDto),
                    PayloadJson = JsonSerializer.Serialize(updateDto),
                    HotelId = updateDto.HotelId
                };
                await _queueService.EnqueueAsync(dto);
                return Accepted(new { queued = true, requestRef = dto.RequestRef });
            }

            var response = await _service.UpdateRoomTypeRateByZaaerIdAsync(zaaerId, updateDto);
            if (response == null)
            {
                return NotFound($"Room type rate with ZaaerId {zaaerId} not found");
            }

            return Ok(response);
        }

        /// <summary>
        /// Gets all room type rates for a specific hotel.
        /// </summary>
        [HttpGet("hotel/{hotelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ZaaerRoomTypeRateResponseDto>>> GetRoomTypeRatesByHotelId(int hotelId)
        {
            var rates = await _service.GetRoomTypeRatesByHotelIdAsync(hotelId);
            return Ok(rates);
        }

        /// <summary>
        /// Gets all room type rates for a specific room type.
        /// </summary>
        [HttpGet("roomtype/{roomTypeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ZaaerRoomTypeRateResponseDto>>> GetRoomTypeRatesByRoomTypeId(int roomTypeId)
        {
            var rates = await _service.GetRoomTypeRatesByRoomTypeIdAsync(roomTypeId);
            return Ok(rates);
        }

        /// <summary>
        /// Gets a specific room type rate by ID.
        /// </summary>
        [HttpGet("{rateId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerRoomTypeRateResponseDto>> GetRoomTypeRateById(int rateId)
        {
            var rate = await _service.GetRoomTypeRateByIdAsync(rateId);
            if (rate == null)
            {
                return NotFound($"Room type rate with ID {rateId} not found");
            }

            return Ok(rate);
        }

        /// <summary>
        /// Deletes a room type rate by ID.
        /// </summary>
        [HttpDelete("{rateId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRoomTypeRate(int rateId)
        {
            var result = await _service.DeleteRoomTypeRateAsync(rateId);
            if (!result)
            {
                return NotFound($"Room type rate with ID {rateId} not found");
            }

            return NoContent();
        }
    }
}

