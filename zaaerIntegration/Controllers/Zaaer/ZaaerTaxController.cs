using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
	[Route("api/zaaer/Tax")]
	[ApiController]
	public class ZaaerTaxController : ControllerBase
	{
		private readonly IZaaerTaxService _taxService;
		private readonly IPartnerQueueService _queueService;
		private readonly ILogger<ZaaerTaxController> _logger;
		private readonly IQueueSettingsProvider _queueSettings;

		public ZaaerTaxController(
			IZaaerTaxService taxService,
			IPartnerQueueService queueService,
			ILogger<ZaaerTaxController> logger,
			IQueueSettingsProvider queueSettings)
		{
			_taxService = taxService;
			_queueService = queueService;
			_logger = logger;
			_queueSettings = queueSettings;
		}

		/// <summary>
		/// Create a new tax record
		/// </summary>
		/// <param name="createTaxDto">Tax data</param>
		/// <returns>A newly created tax record</returns>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ZaaerTaxResponseDto>> CreateTax([FromBody] ZaaerCreateTaxDto createTaxDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				if (createTaxDto == null)
				{
					return BadRequest("Tax payload cannot be null.");
				}

				var queueSettings = _queueSettings.GetSettings();
				if (queueSettings.EnableQueueMode)
				{
					var dtoQ = new EnqueuePartnerRequestDto
					{
						Partner = queueSettings.DefaultPartner,
						Operation = "/api/zaaer/Tax",
						OperationKey = "Zaaer.Tax.Create",
						PayloadType = nameof(ZaaerCreateTaxDto),
						PayloadJson = JsonSerializer.Serialize(createTaxDto),
						HotelId = createTaxDto.HotelId
					};
					await _queueService.EnqueueAsync(dtoQ);
					return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
				}

				var tax = await _taxService.CreateTaxAsync(createTaxDto);
				return CreatedAtAction(nameof(CreateTax), new { id = tax.Id }, tax);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating tax record");
				return StatusCode(500, "An error occurred while creating the tax record.");
			}
		}

		/// <summary>
		/// Update an existing tax record by Zaaer ID
		/// </summary>
		/// <param name="zaaerId">Zaaer ID from URL path</param>
		/// <param name="updateTaxDto">Tax update data</param>
		/// <returns>Updated tax record</returns>
		[HttpPut("{zaaerId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ZaaerTaxResponseDto>> UpdateTax(int zaaerId, [FromBody] ZaaerUpdateTaxDto updateTaxDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				if (updateTaxDto == null)
				{
					return BadRequest("Tax payload cannot be null.");
				}

				// Set ZaaerId from URL if not provided in body
				if (!updateTaxDto.ZaaerId.HasValue)
				{
					updateTaxDto.ZaaerId = zaaerId;
				}
				else if (updateTaxDto.ZaaerId.Value != zaaerId)
				{
					return BadRequest("ZaaerId in URL path must match ZaaerId in request body.");
				}

				var queueSettings = _queueSettings.GetSettings();
				if (queueSettings.EnableQueueMode)
				{
					var dtoQ = new EnqueuePartnerRequestDto
					{
						Partner = queueSettings.DefaultPartner,
						Operation = $"/api/zaaer/Tax/{zaaerId}",
						OperationKey = "Zaaer.Tax.Update",
						PayloadType = nameof(ZaaerUpdateTaxDto),
						PayloadJson = JsonSerializer.Serialize(updateTaxDto),
						HotelId = updateTaxDto.HotelId ?? 0
					};
					await _queueService.EnqueueAsync(dtoQ);
					return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
				}

				var tax = await _taxService.UpdateTaxAsync(zaaerId, updateTaxDto);
				if (tax == null)
				{
					return NotFound($"Tax record with ZaaerId {zaaerId} not found.");
				}

				return Ok(tax);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating tax record with ZaaerId {ZaaerId}. Error: {ErrorMessage}", zaaerId, ex.Message);
				return StatusCode(500, new { error = "An error occurred while updating the tax record.", message = ex.Message });
			}
		}

		/// <summary>
		/// Delete a tax record by Zaaer ID (soft delete - updates status to "inactive")
		/// </summary>
		/// <param name="zaaerId">Zaaer ID</param>
		/// <returns>Success status</returns>
		[HttpDelete("{zaaerId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteTax(int zaaerId)
		{
			try
			{
				var queueSettings = _queueSettings.GetSettings();
				if (queueSettings.EnableQueueMode)
				{
					var dtoQ = new EnqueuePartnerRequestDto
					{
						Partner = queueSettings.DefaultPartner,
						Operation = $"/api/zaaer/Tax/{zaaerId}",
						OperationKey = "Zaaer.Tax.Delete",
						PayloadType = "int",
						PayloadJson = JsonSerializer.Serialize(zaaerId)
					};
					await _queueService.EnqueueAsync(dtoQ);
					return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
				}

				var result = await _taxService.DeleteTaxAsync(zaaerId);
				if (!result)
				{
					return NotFound($"Tax record with ZaaerId {zaaerId} not found.");
				}

				return Ok(new { message = "Tax record disabled successfully (enabled set to false)." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting tax record with ZaaerId {ZaaerId}. Error: {ErrorMessage}", zaaerId, ex.Message);
				return StatusCode(500, new { error = "An error occurred while deleting the tax record.", message = ex.Message });
			}
		}
	}
}

