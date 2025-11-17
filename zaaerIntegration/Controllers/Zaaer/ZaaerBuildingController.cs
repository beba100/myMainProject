using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
	/// <summary>
	/// Zaaer controller to manage buildings with their floors
	/// </summary>
	[ApiController]
	[Route("api/zaaer/[controller]")]
	public class ZaaerBuildingController : ControllerBase
	{
		private readonly IZaaerBuildingService _service;
		private readonly ILogger<ZaaerBuildingController> _logger;
		private readonly IPartnerQueueService _queueService;
		private readonly IQueueSettingsProvider _queueSettings;

		public ZaaerBuildingController(IZaaerBuildingService service, ILogger<ZaaerBuildingController> logger, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
		{
			_service = service;
			_logger = logger;
			_queueService = queueService;
			_queueSettings = queueSettings;
		}

		/// <summary>
		/// Create a building and its floors (transactional)
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> CreateBuildingWithFloors([FromBody] ZaaerCreateBuildingDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var queueSettings = _queueSettings.GetSettings();
				if (queueSettings.EnableQueueMode)
				{
					var q = new EnqueuePartnerRequestDto
					{
						Partner = queueSettings.DefaultPartner,
						Operation = "/api/zaaer/ZaaerBuilding",
						OperationKey = "Zaaer.Building.CreateWithFloors",
						PayloadType = nameof(ZaaerCreateBuildingDto),
						PayloadJson = JsonSerializer.Serialize(dto),
						HotelId = dto.HotelId
					};
					await _queueService.EnqueueAsync(q);
					return Accepted(new { queued = true, requestRef = q.RequestRef });
				}
				var result = await _service.CreateBuildingWithFloorsAsync(dto);
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating building with floors");
				return StatusCode(500, "An error occurred while creating building and floors.");
			}
		}

		/// <summary>
		/// Update a building and its floors (transactional)
		/// </summary>
		[HttpPut]
		public async Task<IActionResult> UpdateBuildingWithFloors([FromBody] ZaaerUpdateBuildingDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var queueSettings = _queueSettings.GetSettings();
				if (queueSettings.EnableQueueMode)
				{
					var q = new EnqueuePartnerRequestDto
					{
						Partner = queueSettings.DefaultPartner,
						Operation = "/api/zaaer/ZaaerBuilding",
						OperationKey = "Zaaer.Building.UpdateWithFloors",
						PayloadType = nameof(ZaaerUpdateBuildingDto),
						PayloadJson = JsonSerializer.Serialize(dto),
						HotelId = dto.HotelId
					};
					await _queueService.EnqueueAsync(q);
					return Accepted(new { queued = true, requestRef = q.RequestRef });
				}
				var result = await _service.UpdateBuildingWithFloorsAsync(dto);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				_logger.LogWarning(ex, "Building not found for update");
				return NotFound(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating building with floors");
				return StatusCode(500, "An error occurred while updating building and floors.");
			}
		}

		/// <summary>
		/// Safe update method that only adds/updates floors without deleting existing ones
		/// This prevents foreign key constraint violations with apartments
		/// </summary>
		[HttpPut("safe")]
		public async Task<IActionResult> UpdateBuildingWithFloorsSafe([FromBody] ZaaerUpdateBuildingDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var queueSettings = _queueSettings.GetSettings();
				if (queueSettings.EnableQueueMode)
				{
					var q = new EnqueuePartnerRequestDto
					{
						Partner = queueSettings.DefaultPartner,
						Operation = "/api/zaaer/ZaaerBuilding/safe",
						OperationKey = "Zaaer.Building.UpdateWithFloorsSafe",
						PayloadType = nameof(ZaaerUpdateBuildingDto),
						PayloadJson = JsonSerializer.Serialize(dto),
						HotelId = dto.HotelId
					};
					await _queueService.EnqueueAsync(q);
					return Accepted(new { queued = true, requestRef = q.RequestRef });
				}
				var result = await _service.UpdateBuildingWithFloorsSafeAsync(dto);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				_logger.LogWarning(ex, "Building not found for safe update");
				return NotFound(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in safe update of building with floors");
				return StatusCode(500, "An error occurred while safely updating building and floors.");
			}
		}

		/// <summary>
		/// Get all buildings with their floors for a specific hotel
		/// </summary>
		[HttpGet("hotel/{hotelId}")]
		public async Task<IActionResult> GetAllBuildingsWithFloors(int hotelId)
		{
			try
			{
				var result = await _service.GetAllBuildingsWithFloorsAsync(hotelId);
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving buildings with floors for hotel {HotelId}", hotelId);
				return StatusCode(500, "An error occurred while retrieving buildings and floors.");
			}
		}
	}
}


