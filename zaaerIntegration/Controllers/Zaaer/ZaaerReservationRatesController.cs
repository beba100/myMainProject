using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;

namespace zaaerIntegration.Controllers.Zaaer
{
    [ApiController]
    [Route("api/zaaer/[controller]")]
    public class ZaaerReservationRatesController : ControllerBase
    {
        private readonly IReservationRatesService _service;
        private readonly IPartnerQueueService _queue;
        private readonly IQueueSettingsProvider _queueSettings;
        public ZaaerReservationRatesController(IReservationRatesService service, IPartnerQueueService queue, IQueueSettingsProvider queueSettings)
        { _service = service; _queue = queue; _queueSettings = queueSettings; }

        // GET: api/zaaer/ZaaerReservationRates/{reservationId}
        [HttpGet("{reservationId:int}")]
        public async Task<IActionResult> Get([FromRoute] int reservationId)
        {
            var list = await _service.GetByReservationAsync(reservationId);
            return Ok(list);
        }

        // PUT: api/zaaer/ZaaerReservationRates/{reservationId}
        [HttpPut("{reservationId:int}")]
        public async Task<IActionResult> Upsert([FromRoute] int reservationId, [FromBody] ZaaerReservationRatesUpsertDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var q = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = $"/api/zaaer/ZaaerReservationRates/{reservationId}",
                    OperationKey = "Zaaer.ReservationRates.Upsert",
                    TargetId = reservationId,
                    PayloadType = nameof(ZaaerReservationRatesUpsertDto),
                    PayloadJson = JsonSerializer.Serialize(dto)
                };
                await _queue.EnqueueAsync(q);
                return Accepted(new { queued = true, requestRef = q.RequestRef });
            }
            await _service.UpsertRatesAsync(reservationId, dto.Items, dto.EwaPercent, dto.VatPercent);
            return NoContent();
        }

        // PUT: api/zaaer/ZaaerReservationRates/{reservationId}/apply-to-all
        [HttpPut("{reservationId:int}/apply-to-all")]
        public async Task<IActionResult> ApplyToAll([FromRoute] int reservationId, [FromBody] ZaaerApplySameAmountDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var q = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = $"/api/zaaer/ZaaerReservationRates/{reservationId}/apply-to-all",
                    OperationKey = "Zaaer.ReservationRates.ApplyAll",
                    TargetId = reservationId,
                    PayloadType = nameof(ZaaerApplySameAmountDto),
                    PayloadJson = JsonSerializer.Serialize(dto)
                };
                await _queue.EnqueueAsync(q);
                return Accepted(new { queued = true, requestRef = q.RequestRef });
            }
            await _service.ApplySameAmountAsync(reservationId, dto.Amount, dto.UnitId, dto.DateFrom, dto.DateTo, dto.EwaPercent, dto.VatPercent);
            return NoContent();
        }
    }
}


