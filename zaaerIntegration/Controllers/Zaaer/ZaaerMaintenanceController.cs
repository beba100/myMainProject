using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
	[Route("api/zaaer/Maintenance")]
	[ApiController]
	public class ZaaerMaintenanceController : ControllerBase
	{
		private readonly IZaaerMaintenanceService _maintenanceService;
		private readonly IPartnerQueueService _queueService;
		private readonly ILogger<ZaaerMaintenanceController> _logger;
		private readonly IQueueSettingsProvider _queueSettings;

		public ZaaerMaintenanceController(
			IZaaerMaintenanceService maintenanceService,
			IPartnerQueueService queueService,
			ILogger<ZaaerMaintenanceController> logger,
			IQueueSettingsProvider queueSettings)
		{
			_maintenanceService = maintenanceService;
			_queueService = queueService;
			_logger = logger;
			_queueSettings = queueSettings;
		}

		/// <summary>
		/// Create a new maintenance record
		/// </summary>
		/// <param name="createMaintenanceDto">Maintenance data</param>
		/// <returns>A newly created maintenance record</returns>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ZaaerMaintenanceResponseDto>> CreateMaintenance([FromBody] ZaaerCreateMaintenanceDto createMaintenanceDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				if (createMaintenanceDto == null)
				{
					return BadRequest("Maintenance payload cannot be null.");
				}

				var queueSettings = _queueSettings.GetSettings();
				if (queueSettings.EnableQueueMode)
				{
					var dtoQ = new EnqueuePartnerRequestDto
					{
						Partner = queueSettings.DefaultPartner,
						Operation = "/api/zaaer/Maintenance",
						OperationKey = "Zaaer.Maintenance.Create",
						PayloadType = nameof(ZaaerCreateMaintenanceDto),
						PayloadJson = JsonSerializer.Serialize(createMaintenanceDto),
						HotelId = createMaintenanceDto.HotelId
					};
					await _queueService.EnqueueAsync(dtoQ);
					return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
				}

				var maintenance = await _maintenanceService.CreateMaintenanceAsync(createMaintenanceDto);
				return CreatedAtAction(nameof(CreateMaintenance), new { id = maintenance.Id }, maintenance);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating maintenance record");
				return StatusCode(500, "An error occurred while creating the maintenance record.");
			}
		}

		/// <summary>
		/// Update an existing maintenance record by Zaaer ID
		/// </summary>
		/// <param name="updateMaintenanceDto">Maintenance update data</param>
		/// <returns>Updated maintenance record</returns>
		[HttpPut]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ZaaerMaintenanceResponseDto>> UpdateMaintenance([FromBody] ZaaerUpdateMaintenanceDto updateMaintenanceDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				if (updateMaintenanceDto == null || !updateMaintenanceDto.ZaaerId.HasValue)
				{
					return BadRequest("Maintenance payload and ZaaerId are required.");
				}

				var queueSettings = _queueSettings.GetSettings();
				if (queueSettings.EnableQueueMode)
				{
					var dtoQ = new EnqueuePartnerRequestDto
					{
						Partner = queueSettings.DefaultPartner,
						Operation = "/api/zaaer/Maintenance",
						OperationKey = "Zaaer.Maintenance.Update",
						PayloadType = nameof(ZaaerUpdateMaintenanceDto),
						PayloadJson = JsonSerializer.Serialize(updateMaintenanceDto),
						HotelId = updateMaintenanceDto.HotelId
					};
					await _queueService.EnqueueAsync(dtoQ);
					return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
				}

				var maintenance = await _maintenanceService.UpdateMaintenanceAsync(updateMaintenanceDto.ZaaerId.Value, updateMaintenanceDto);
				if (maintenance == null)
				{
					return NotFound($"Maintenance record with ZaaerId {updateMaintenanceDto.ZaaerId} not found.");
				}

				return Ok(maintenance);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating maintenance record");
				return StatusCode(500, "An error occurred while updating the maintenance record.");
			}
		}

		/// <summary>
		/// Delete a maintenance record by Zaaer ID and set apartment status to "vacant"
		/// </summary>
		/// <param name="zaaerId">Zaaer ID</param>
		/// <returns>Success status</returns>
		[HttpDelete("{zaaerId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteMaintenance(int zaaerId)
		{
			try
			{
				var result = await _maintenanceService.DeleteMaintenanceAsync(zaaerId);
				if (!result)
				{
					return NotFound($"Maintenance record with ZaaerId {zaaerId} not found.");
				}

				return Ok(new { message = "Maintenance record deleted successfully and apartment status set to vacant." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting maintenance record with ZaaerId {ZaaerId}", zaaerId);
				return StatusCode(500, "An error occurred while deleting the maintenance record.");
			}
		}
	}
}
