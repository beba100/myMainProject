using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    [Route("api/zaaer/[controller]")]
    [ApiController]
    public class RoomTypeController : ControllerBase
    {
        private readonly IZaaerRoomTypeService _zaaerRoomTypeService;
        private readonly IMapper _mapper;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public RoomTypeController(IZaaerRoomTypeService zaaerRoomTypeService, IMapper mapper, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _zaaerRoomTypeService = zaaerRoomTypeService;
            _mapper = mapper;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Creates a new room type for Zaaer integration.
        /// </summary>
        /// <param name="createRoomTypeDto">Room type data</param>
        /// <returns>A newly created room type</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ZaaerRoomTypeResponseDto>> CreateRoomType([FromBody] ZaaerCreateRoomTypeDto createRoomTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var q = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = "/api/zaaer/RoomType",
                    OperationKey = "Zaaer.RoomType.Create",
                    PayloadType = nameof(ZaaerCreateRoomTypeDto),
                    PayloadJson = JsonSerializer.Serialize(createRoomTypeDto),
                    HotelId = createRoomTypeDto.HotelId
                };
                await _queueService.EnqueueAsync(q);
                return Accepted(new { queued = true, requestRef = q.RequestRef });
            }
            var roomTypeResponse = await _zaaerRoomTypeService.CreateRoomTypeAsync(createRoomTypeDto);
            return CreatedAtAction(nameof(GetRoomTypeById), new { roomTypeId = roomTypeResponse.RoomTypeId }, roomTypeResponse);
        }

        /// <summary>
        /// Updates an existing room type for Zaaer integration.
        /// </summary>
        /// <param name="roomTypeId">The ID of the room type to update</param>
        /// <param name="updateRoomTypeDto">Updated room type data</param>
        /// <returns>The updated room type</returns>
        [HttpPut("{roomTypeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerRoomTypeResponseDto>> UpdateRoomType(int roomTypeId, [FromBody] ZaaerUpdateRoomTypeDto updateRoomTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var q = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = $"/api/zaaer/RoomType/{roomTypeId}",
                    OperationKey = "Zaaer.RoomType.UpdateById",
                    TargetId = roomTypeId,
                    PayloadType = nameof(ZaaerUpdateRoomTypeDto),
                    PayloadJson = JsonSerializer.Serialize(updateRoomTypeDto)
                };
                await _queueService.EnqueueAsync(q);
                return Accepted(new { queued = true, requestRef = q.RequestRef });
            }
            // First try to treat route param as ZaaerId
            var roomTypeResponse = await _zaaerRoomTypeService.UpdateRoomTypeByZaaerIdAsync(roomTypeId, updateRoomTypeDto);
            if (roomTypeResponse == null)
            {
                // Fallback to internal RoomTypeId
                roomTypeResponse = await _zaaerRoomTypeService.UpdateRoomTypeAsync(roomTypeId, updateRoomTypeDto);
                if (roomTypeResponse == null)
                {
                    return NotFound($"Room type with ID or ZaaerId {roomTypeId} not found");
                }
            }

            return Ok(roomTypeResponse);
        }

        /// <summary>
        /// Gets all room types for a specific hotel.
        /// </summary>
        /// <param name="hotelId">The hotel ID</param>
        /// <returns>List of room types</returns>
        [HttpGet("hotel/{hotelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ZaaerRoomTypeResponseDto>>> GetRoomTypesByHotelId(int hotelId)
        {
            var roomTypes = await _zaaerRoomTypeService.GetRoomTypesByHotelIdAsync(hotelId);
            return Ok(roomTypes);
        }

        /// <summary>
        /// Gets a specific room type by ID.
        /// </summary>
        /// <param name="roomTypeId">The room type ID</param>
        /// <returns>The room type</returns>
        [HttpGet("{roomTypeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerRoomTypeResponseDto>> GetRoomTypeById(int roomTypeId)
        {
            var roomType = await _zaaerRoomTypeService.GetRoomTypeByIdAsync(roomTypeId);
            if (roomType == null)
            {
                return NotFound($"Room type with ID {roomTypeId} not found");
            }

            return Ok(roomType);
        }

        /// <summary>
        /// Deletes a room type by ID.
        /// </summary>
        /// <param name="roomTypeId">The room type ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{roomTypeId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRoomType(int roomTypeId)
        {
            var result = await _zaaerRoomTypeService.DeleteRoomTypeAsync(roomTypeId);
            if (!result)
            {
                return NotFound($"Room type with ID {roomTypeId} not found");
            }

            return NoContent();
        }
    }
}
