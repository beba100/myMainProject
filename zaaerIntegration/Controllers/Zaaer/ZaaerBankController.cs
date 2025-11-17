using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    [Route("api/zaaer/[controller]")]
    [ApiController]
    public class ZaaerBankController : ControllerBase
    {
        private readonly IZaaerBankService _service;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public ZaaerBankController(IZaaerBankService service, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _service = service;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ZaaerBankResponseDto>> Create([FromBody] ZaaerCreateBankDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var q = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = "/api/zaaer/ZaaerBank",
                    OperationKey = "Zaaer.Bank.Create",
                    PayloadType = nameof(ZaaerCreateBankDto),
                    PayloadJson = JsonSerializer.Serialize(dto)
                };
                await _queueService.EnqueueAsync(q);
                return Accepted(new { queued = true, requestRef = q.RequestRef });
            }
            var result = await _service.CreateBankAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { id = result.BankId }, result);
        }

        [HttpPut("{bankId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerBankResponseDto>> Update(int bankId, [FromBody] ZaaerUpdateBankDto dto)
        {
            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var q = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = $"/api/zaaer/ZaaerBank/{bankId}",
                    OperationKey = "Zaaer.Bank.UpdateById",
                    TargetId = bankId,
                    PayloadType = nameof(ZaaerUpdateBankDto),
                    PayloadJson = JsonSerializer.Serialize(dto)
                };
                await _queueService.EnqueueAsync(q);
                return Accepted(new { queued = true, requestRef = q.RequestRef });
            }
            var result = await _service.UpdateBankAsync(bankId, dto);
            if (result == null) return NotFound($"Bank with id {bankId} not found");
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ZaaerBankResponseDto>>> GetAll()
        {
            var list = await _service.GetAllBanksAsync();
            return Ok(list);
        }
    }
}


