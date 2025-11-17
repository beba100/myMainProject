using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;

namespace zaaerIntegration.Controllers.Zaaer
{
	[ApiController]
	[Route("api/zaaer/[controller]")]
	public class ZaaerSeasonalRateController : ControllerBase
	{
		private readonly IZaaerSeasonalRateService _service;
		private readonly IPartnerQueueService _queue;
		private readonly IQueueSettingsProvider _queueSettings;

		public ZaaerSeasonalRateController(IZaaerSeasonalRateService service, IPartnerQueueService queue, IQueueSettingsProvider queueSettings)
		{
			_service = service;
			_queue = queue;
			_queueSettings = queueSettings;
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] ZaaerCreateSeasonalRateDto dto)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			var queueSettings = _queueSettings.GetSettings();
			if (queueSettings.EnableQueueMode)
			{
				var q = new EnqueuePartnerRequestDto
				{
					Partner = queueSettings.DefaultPartner,
					Operation = "/api/zaaer/ZaaerSeasonalRate",
					OperationKey = "Zaaer.SeasonalRate.Create",
					PayloadType = nameof(ZaaerCreateSeasonalRateDto),
					PayloadJson = JsonSerializer.Serialize(dto),
					HotelId = dto.HotelId
				};
				await _queue.EnqueueAsync(q);
				return Accepted(new { queued = true, requestRef = q.RequestRef });
			}
			var result = await _service.CreateAsync(dto);
			return Ok(result);
		}

		[HttpPut("{seasonId:int}")]
		public async Task<IActionResult> Update([FromRoute] int seasonId, [FromBody] ZaaerUpdateSeasonalRateDto dto)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			var queueSettings = _queueSettings.GetSettings();
			if (queueSettings.EnableQueueMode)
			{
				var q = new EnqueuePartnerRequestDto
				{
					Partner = queueSettings.DefaultPartner,
					Operation = $"/api/zaaer/ZaaerSeasonalRate/{seasonId}",
					OperationKey = "Zaaer.SeasonalRate.UpdateById",
					TargetId = seasonId,
					PayloadType = nameof(ZaaerUpdateSeasonalRateDto),
					PayloadJson = JsonSerializer.Serialize(dto),
					HotelId = dto.HotelId
				};
				await _queue.EnqueueAsync(q);
				return Accepted(new { queued = true, requestRef = q.RequestRef });
			}
			var result = await _service.UpdateAsync(seasonId, dto);
			if (result == null) return NotFound();
			return Ok(result);
		}

		[HttpGet("hotel/{hotelId:int}")]
		public async Task<IActionResult> GetByHotel([FromRoute] int hotelId)
		{
			var list = await _service.GetAllByHotelIdAsync(hotelId);
			return Ok(list);
		}
	}
}


