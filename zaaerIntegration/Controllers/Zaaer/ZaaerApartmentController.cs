using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    /// <summary>
    /// Controller for Zaaer Apartment operations
    /// </summary>
    [ApiController]
    [Route("api/zaaer/Apartment")]
    public class ZaaerApartmentController : ControllerBase
    {
        private readonly IZaaerApartmentService _apartmentService;
        private readonly ILogger<ZaaerApartmentController> _logger;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        /// <summary>
        /// Initializes a new instance of the ZaaerApartmentController class
        /// </summary>
        /// <param name="apartmentService">Apartment service</param>
        /// <param name="logger">Logger</param>
        public ZaaerApartmentController(IZaaerApartmentService apartmentService, ILogger<ZaaerApartmentController> logger, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _apartmentService = apartmentService;
            _logger = logger;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Create a single apartment
        /// </summary>
        /// <param name="createApartmentDto">Apartment creation data</param>
        /// <returns>Created apartment</returns>
        [HttpPost]
        public async Task<IActionResult> CreateApartment([FromBody] ZaaerCreateApartmentDto createApartmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (createApartmentDto == null)
                {
                    return BadRequest("Apartment payload cannot be null.");
                }

                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = "/api/zaaer/Apartment",
                        OperationKey = "Zaaer.Apartment.Create",
                        PayloadType = nameof(ZaaerCreateApartmentDto),
                        PayloadJson = JsonSerializer.Serialize(createApartmentDto),
                        HotelId = createApartmentDto.HotelId
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var apartment = await _apartmentService.CreateApartmentAsync(createApartmentDto);
                return Ok(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating apartment");
                return StatusCode(500, "An error occurred while creating the apartment.");
            }
        }

        /// <summary>
        /// Update an existing apartment by Zaaer ID
        /// </summary>
        /// <param name="zaaerId">Zaaer ID</param>
        /// <param name="updateApartmentDto">Apartment update data</param>
        /// <returns>Updated apartment</returns>
        [HttpPut("{zaaerId}")]
        public async Task<IActionResult> UpdateApartment(int zaaerId, [FromBody] ZaaerUpdateApartmentDto updateApartmentDto)
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
                        Operation = $"/api/zaaer/Apartment/{zaaerId}",
                        OperationKey = "Zaaer.Apartment.UpdateByZaaerId",
                        TargetId = zaaerId,
                        PayloadType = nameof(ZaaerUpdateApartmentDto),
                        PayloadJson = JsonSerializer.Serialize(updateApartmentDto)
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var apartment = await _apartmentService.UpdateApartmentByZaaerIdAsync(zaaerId, updateApartmentDto);
                if (apartment == null)
                {
                    return NotFound($"Apartment with Zaaer ID {zaaerId} not found.");
                }

                return Ok(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating apartment with Zaaer ID {ZaaerId}", zaaerId);
                return StatusCode(500, "An error occurred while updating the apartment.");
            }
        }

        /// <summary>
        /// Get all apartments for a specific hotel
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of apartments for the specified hotel</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetApartmentsByHotelId(int hotelId)
        {
            try
            {
                var apartments = await _apartmentService.GetApartmentsByHotelIdAsync(hotelId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving apartments for the hotel.");
            }
        }

        /// <summary>
        /// Get a specific apartment by ID
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <returns>Apartment details</returns>
        [HttpGet("{apartmentId}")]
        public async Task<IActionResult> GetApartmentById(int apartmentId)
        {
            try
            {
                var apartment = await _apartmentService.GetApartmentByIdAsync(apartmentId);
                if (apartment == null)
                {
                    return NotFound($"Apartment with ID {apartmentId} not found.");
                }

                return Ok(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartment with ID {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while retrieving the apartment.");
            }
        }

        /// <summary>
        /// Update an apartment by apartment code
        /// </summary>
        /// <param name="apartmentCode">Apartment Code</param>
        /// <param name="updateApartmentDto">Apartment update data</param>
        /// <returns>Updated apartment</returns>
        [HttpPut("code/{apartmentCode}")]
        public async Task<IActionResult> UpdateApartmentByCode(string apartmentCode, [FromBody] ZaaerUpdateApartmentDto updateApartmentDto)
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
                        Operation = $"/api/zaaer/Apartment/code/{apartmentCode}",
                        OperationKey = "Zaaer.Apartment.UpdateByCode",
                        PayloadType = apartmentCode,
                        PayloadJson = JsonSerializer.Serialize(updateApartmentDto)
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var apartment = await _apartmentService.UpdateApartmentByCodeAsync(apartmentCode, updateApartmentDto);
                if (apartment == null)
                {
                    return NotFound($"Apartment with code {apartmentCode} not found.");
                }

                return Ok(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating apartment with code {ApartmentCode}", apartmentCode);
                return StatusCode(500, "An error occurred while updating the apartment.");
            }
        }

        /// <summary>
        /// Delete an apartment by ID
        /// </summary>
        /// <param name="apartmentId">Apartment ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{apartmentId}")]
        public async Task<IActionResult> DeleteApartment(int apartmentId)
        {
            try
            {
                var result = await _apartmentService.DeleteApartmentAsync(apartmentId);
                if (!result)
                {
                    return NotFound($"Apartment with ID {apartmentId} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting apartment with ID {ApartmentId}", apartmentId);
                return StatusCode(500, "An error occurred while deleting the apartment.");
            }
        }
    }
}
