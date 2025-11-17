using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    /// <summary>
    /// Controller for Zaaer Floor operations
    /// </summary>
    [ApiController]
    [Route("api/zaaer/[controller]")]
    public class ZaaerFloorController : ControllerBase
    {
        private readonly IZaaerFloorService _floorService;
        private readonly ILogger<ZaaerFloorController> _logger;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        /// <summary>
        /// Initializes a new instance of the ZaaerFloorController class
        /// </summary>
        /// <param name="floorService">Floor service</param>
        /// <param name="logger">Logger</param>
        public ZaaerFloorController(IZaaerFloorService floorService, ILogger<ZaaerFloorController> logger, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _floorService = floorService;
            _logger = logger;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Create multiple floors (bulk create)
        /// </summary>
        /// <param name="createFloorDto">List of floor creation data</param>
        /// <returns>Created floors</returns>
        [HttpPost]
        public async Task<IActionResult> CreateFloors([FromBody] List<ZaaerCreateFloorDto> createFloorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (createFloorDto == null || !createFloorDto.Any())
                {
                    return BadRequest("Floor list cannot be empty.");
                }
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = "/api/zaaer/ZaaerFloor",
                        OperationKey = "Zaaer.Floor.CreateBulk",
                        PayloadType = nameof(List<ZaaerCreateFloorDto>),
                        PayloadJson = JsonSerializer.Serialize(createFloorDto)
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var floors = await _floorService.CreateFloorsAsync(createFloorDto);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating floors");
                return StatusCode(500, "An error occurred while creating floors.");
            }
        }

        /// <summary>
        /// Update an existing floor
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <param name="updateFloorDto">Floor update data</param>
        /// <returns>Updated floor</returns>
        [HttpPut("{floorId}")]
        public async Task<IActionResult> UpdateFloor(int floorId, [FromBody] ZaaerUpdateFloorDto updateFloorDto)
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
                        Operation = $"/api/zaaer/ZaaerFloor/{floorId}",
                        OperationKey = "Zaaer.Floor.UpdateById",
                        TargetId = floorId,
                        PayloadType = nameof(ZaaerUpdateFloorDto),
                        PayloadJson = JsonSerializer.Serialize(updateFloorDto)
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var floor = await _floorService.UpdateFloorAsync(floorId, updateFloorDto);
                if (floor == null)
                {
                    return NotFound($"Floor with ID {floorId} not found.");
                }

                return Ok(floor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating floor with ID {FloorId}", floorId);
                return StatusCode(500, "An error occurred while updating the floor.");
            }
        }

        /// <summary>
        /// Get all floors for a specific hotel
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of floors for the specified hotel</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetFloorsByHotelId(int hotelId)
        {
            try
            {
                var floors = await _floorService.GetFloorsByHotelIdAsync(hotelId);
                return Ok(floors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floors for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving floors for the hotel.");
            }
        }

        /// <summary>
        /// Get a specific floor by ID
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <returns>Floor details</returns>
        [HttpGet("{floorId}")]
        public async Task<IActionResult> GetFloorById(int floorId)
        {
            try
            {
                var floor = await _floorService.GetFloorByIdAsync(floorId);
                if (floor == null)
                {
                    return NotFound($"Floor with ID {floorId} not found.");
                }

                return Ok(floor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floor with ID {FloorId}", floorId);
                return StatusCode(500, "An error occurred while retrieving the floor.");
            }
        }

        /// <summary>
        /// Delete a floor by ID
        /// </summary>
        /// <param name="floorId">Floor ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{floorId}")]
        public async Task<IActionResult> DeleteFloor(int floorId)
        {
            try
            {
                var result = await _floorService.DeleteFloorAsync(floorId);
                if (!result)
                {
                    return NotFound($"Floor with ID {floorId} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting floor with ID {FloorId}", floorId);
                return StatusCode(500, "An error occurred while deleting the floor.");
            }
        }
    }
}
