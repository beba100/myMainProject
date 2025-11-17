using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    /// <summary>
    /// Controller for Zaaer Hotel Settings operations
    /// </summary>
    [ApiController]
    [Route("api/zaaer/HotelSettings")]
    public class ZaaerHotelSettingsController : ControllerBase
    {
        private readonly IZaaerHotelSettingsService _hotelSettingsService;
        private readonly ILogger<ZaaerHotelSettingsController> _logger;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public ZaaerHotelSettingsController(IZaaerHotelSettingsService hotelSettingsService, ILogger<ZaaerHotelSettingsController> logger, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _hotelSettingsService = hotelSettingsService;
            _logger = logger;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Create new hotel settings
        /// </summary>
        /// <param name="createHotelSettingsDto">Hotel settings creation data</param>
        /// <returns>Created hotel settings</returns>
        [HttpPost]
        public async Task<IActionResult> CreateHotelSettings([FromBody] ZaaerCreateHotelSettingsDto createHotelSettingsDto)
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
                        Operation = "/api/zaaer/ZaaerHotelSettings",
                        OperationKey = "Zaaer.HotelSettings.Create",
                        PayloadType = nameof(ZaaerCreateHotelSettingsDto),
                        PayloadJson = JsonSerializer.Serialize(createHotelSettingsDto),
                        HotelId = createHotelSettingsDto.HotelId
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var hotelSettings = await _hotelSettingsService.CreateHotelSettingsAsync(createHotelSettingsDto);
                return CreatedAtAction(nameof(GetHotelSettingsById), new { hotelId = hotelSettings.HotelId }, hotelSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel settings");
                return StatusCode(500, "An error occurred while creating the hotel settings.");
            }
        }

        /// <summary>
        /// Update existing hotel settings by ZaaerId
        /// </summary>
        /// <param name="updateHotelSettingsDto">Hotel settings update data (must include zaaerId)</param>
        /// <returns>Updated hotel settings</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateHotelSettingsByZaaerId([FromBody] ZaaerUpdateHotelSettingsDto updateHotelSettingsDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!updateHotelSettingsDto.ZaaerId.HasValue)
                {
                    return BadRequest("ZaaerId is required in the request body.");
                }

                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = "/api/zaaer/HotelSettings",
                        OperationKey = "Zaaer.HotelSettings.UpdateByZaaerId",
                        PayloadType = nameof(ZaaerUpdateHotelSettingsDto),
                        PayloadJson = JsonSerializer.Serialize(updateHotelSettingsDto)
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }

                var hotelSettings = await _hotelSettingsService.UpdateHotelSettingsByZaaerIdAsync(updateHotelSettingsDto.ZaaerId.Value, updateHotelSettingsDto);
                if (hotelSettings == null)
                {
                    return NotFound($"Hotel settings with Zaaer ID {updateHotelSettingsDto.ZaaerId} not found.");
                }

                return Ok(hotelSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel settings with Zaaer ID {ZaaerId}. Exception: {ExceptionMessage}. StackTrace: {StackTrace}", 
                    updateHotelSettingsDto.ZaaerId, ex.Message, ex.StackTrace);
                return StatusCode(500, new { 
                    error = "An error occurred while updating the hotel settings.", 
                    message = ex.Message,
                    details = ex.InnerException?.Message 
                });
            }
        }

        /// <summary>
        /// Update existing hotel settings by Hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="updateHotelSettingsDto">Hotel settings update data</param>
        /// <returns>Updated hotel settings</returns>
        [HttpPut("{hotelId}")]
        public async Task<IActionResult> UpdateHotelSettings(int hotelId, [FromBody] ZaaerUpdateHotelSettingsDto updateHotelSettingsDto)
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
                        Operation = $"/api/zaaer/ZaaerHotelSettings/{hotelId}",
                        OperationKey = "Zaaer.HotelSettings.UpdateById",
                        TargetId = hotelId,
                        PayloadType = nameof(ZaaerUpdateHotelSettingsDto),
                        PayloadJson = JsonSerializer.Serialize(updateHotelSettingsDto)
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var hotelSettings = await _hotelSettingsService.UpdateHotelSettingsAsync(hotelId, updateHotelSettingsDto);
                if (hotelSettings == null)
                {
                    return NotFound($"Hotel settings with ID {hotelId} not found.");
                }

                return Ok(hotelSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel settings with ID {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while updating the hotel settings.");
            }
        }

        /// <summary>
        /// Get all hotel settings
        /// </summary>
        /// <returns>List of hotel settings</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllHotelSettings()
        {
            try
            {
                var hotelSettings = await _hotelSettingsService.GetAllHotelSettingsAsync();
                return Ok(hotelSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all hotel settings");
                return StatusCode(500, "An error occurred while retrieving the hotel settings.");
            }
        }

        /// <summary>
        /// Get hotel settings by ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>Hotel settings details</returns>
        [HttpGet("{hotelId}")]
        public async Task<IActionResult> GetHotelSettingsById(int hotelId)
        {
            try
            {
                var hotelSettings = await _hotelSettingsService.GetHotelSettingsByIdAsync(hotelId);
                if (hotelSettings == null)
                {
                    return NotFound($"Hotel settings with ID {hotelId} not found.");
                }

                return Ok(hotelSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotel settings with ID {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving the hotel settings.");
            }
        }

        /// <summary>
        /// Delete hotel settings by ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{hotelId}")]
        public async Task<IActionResult> DeleteHotelSettings(int hotelId)
        {
            try
            {
                var result = await _hotelSettingsService.DeleteHotelSettingsAsync(hotelId);
                if (!result)
                {
                    return NotFound($"Hotel settings with ID {hotelId} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hotel settings with ID {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while deleting the hotel settings.");
            }
        }
    }
}
