using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    /// <summary>
    /// Controller for Zaaer Floor integration endpoints
    /// </summary>
    [ApiController]
    [Route("api/zaaer/[controller]")]
    public class FloorController : ControllerBase
    {
        private readonly IZaaerFloorService _floorService;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public FloorController(IZaaerFloorService floorService, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _floorService = floorService;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Create a new floor
        /// </summary>
        /// <param name="createFloorDto">Floor creation data</param>
        /// <returns>Created floor</returns>
        [HttpPost]
        public async Task<ActionResult<ZaaerFloorResponseDto>> CreateFloor([FromBody] ZaaerCreateFloorDto createFloorDto)
        {
            try
            {
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = "/api/zaaer/Floor",
                        OperationKey = "Zaaer.Floor.Create",
                        PayloadType = nameof(ZaaerCreateFloorDto),
                        PayloadJson = JsonSerializer.Serialize(createFloorDto),
                        HotelId = createFloorDto.HotelId
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var result = await _floorService.CreateFloorAsync(createFloorDto);
                return CreatedAtAction(nameof(GetFloorById), new { floorId = result.FloorId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing floor
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="updateFloorDto">Floor update data</param>
        /// <returns>Updated floor</returns>
        [HttpPut("{floorId}")]
        public async Task<ActionResult<ZaaerFloorResponseDto>> UpdateFloor(int floorId, [FromBody] ZaaerUpdateFloorDto updateFloorDto)
        {
            try
            {
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = $"/api/zaaer/Floor/{floorId}",
                        OperationKey = "Zaaer.Floor.UpdateById",
                        TargetId = floorId,
                        PayloadType = nameof(ZaaerUpdateFloorDto),
                        PayloadJson = JsonSerializer.Serialize(updateFloorDto)
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var result = await _floorService.UpdateFloorAsync(floorId, updateFloorDto);
                if (result == null)
                {
                    return NotFound(new { message = "Floor not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get floor by ID
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <returns>Floor details</returns>
        [HttpGet("{floorId}")]
        public async Task<ActionResult<ZaaerFloorResponseDto>> GetFloorById(int floorId)
        {
            try
            {
                var result = await _floorService.GetFloorByIdAsync(floorId);
                if (result == null)
                {
                    return NotFound(new { message = "Floor not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all floors by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of floors</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<IEnumerable<ZaaerFloorResponseDto>>> GetFloorsByHotelId(int hotelId)
        {
            try
            {
                var result = await _floorService.GetFloorsByHotelIdAsync(hotelId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a floor
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{floorId}")]
        public async Task<ActionResult> DeleteFloor(int floorId)
        {
            try
            {
                var result = await _floorService.DeleteFloorAsync(floorId);
                if (!result)
                {
                    return NotFound(new { message = "Floor not found" });
                }
                return Ok(new { message = "Floor deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
