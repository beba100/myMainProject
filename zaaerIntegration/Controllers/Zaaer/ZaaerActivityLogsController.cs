using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;

namespace zaaerIntegration.Controllers.Zaaer
{
    [ApiController]
    [Route("api/zaaer/[controller]")]
    public class ZaaerActivityLogsController : ControllerBase
    {
        private readonly IActivityLogService _service;
        private readonly IPartnerQueueService _queue;
        private readonly IQueueSettingsProvider _queueSettings;
        public ZaaerActivityLogsController(IActivityLogService service, IPartnerQueueService queue, IQueueSettingsProvider queueSettings) { _service = service; _queue = queue; _queueSettings = queueSettings; }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ZaaerCreateActivityLogDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var q = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = "/api/zaaer/ZaaerActivityLogs",
                    OperationKey = "Zaaer.ActivityLog.Create",
                    PayloadType = nameof(ZaaerCreateActivityLogDto),
                    PayloadJson = JsonSerializer.Serialize(dto),
                    HotelId = dto.HotelId
                };
                await _queue.EnqueueAsync(q);
                return Accepted(new { queued = true, requestRef = q.RequestRef });
            }
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] ZaaerActivityLogQuery query)
        {
            var list = await _service.SearchAsync(query);
            return Ok(list);
        }
    }
}


