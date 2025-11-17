using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;

namespace zaaerIntegration.Controllers.Zaaer
{
    [ApiController]
    [Route("api/zaaer/[controller]")]
    public class ZaaerReservationUnitSwitchController : ControllerBase
    {
        private readonly IReservationUnitSwitchService _service;
        private readonly IPartnerQueueService _queue;
        private readonly IQueueSettingsProvider _queueSettings;

        public ZaaerReservationUnitSwitchController(IReservationUnitSwitchService service, IPartnerQueueService queue, IQueueSettingsProvider queueSettings)
        { _service = service; _queue = queue; _queueSettings = queueSettings; }

        // POST: api/zaaer/ZaaerReservationUnitSwitch
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ZaaerCreateReservationUnitSwitchDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var q = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = "/api/zaaer/ZaaerReservationUnitSwitch",
                    OperationKey = "Zaaer.ReservationUnitSwitch.Create",
                    PayloadType = nameof(ZaaerCreateReservationUnitSwitchDto),
                    PayloadJson = JsonSerializer.Serialize(dto)
                };
                await _queue.EnqueueAsync(q);
                return Accepted(new { queued = true, requestRef = q.RequestRef });
            }
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        // GET: api/zaaer/ZaaerReservationUnitSwitch/reservation/{reservationId}
        [HttpGet("reservation/{reservationId:int}")]
        public async Task<IActionResult> GetByReservation([FromRoute] int reservationId)
        {
            var list = await _service.GetByReservationAsync(reservationId);
            return Ok(list);
        }
    }
}


