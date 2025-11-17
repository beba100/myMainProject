using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    [Route("api/zaaer/[controller]")]
    [ApiController]
    public class ZaaerExpenseController : ControllerBase
    {
        private readonly IZaaerExpenseService _service;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public ZaaerExpenseController(IZaaerExpenseService service, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _service = service;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        [HttpPost]
        public async Task<ActionResult<ZaaerExpenseResponseDto>> Create([FromBody] ZaaerCreateExpenseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var q = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = "/api/zaaer/ZaaerExpense",
                    OperationKey = "Zaaer.Expense.Create",
                    PayloadType = nameof(ZaaerCreateExpenseDto),
                    PayloadJson = JsonSerializer.Serialize(dto),
                    HotelId = dto.HotelId
                };
                await _queueService.EnqueueAsync(q);
                return Accepted(new { queued = true, requestRef = q.RequestRef });
            }
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { id = result.ExpenseId }, result);
        }

        [HttpPut("{expenseId}")]
        public async Task<ActionResult<ZaaerExpenseResponseDto>> Update(int expenseId, [FromBody] ZaaerUpdateExpenseDto dto)
        {
            var queueSettings = _queueSettings.GetSettings();
            if (queueSettings.EnableQueueMode)
            {
                var q = new EnqueuePartnerRequestDto
                {
                    Partner = queueSettings.DefaultPartner,
                    Operation = $"/api/zaaer/ZaaerExpense/{expenseId}",
                    OperationKey = "Zaaer.Expense.UpdateById",
                    TargetId = expenseId,
                    PayloadType = nameof(ZaaerUpdateExpenseDto),
                    PayloadJson = JsonSerializer.Serialize(dto)
                };
                await _queueService.EnqueueAsync(q);
                return Accepted(new { queued = true, requestRef = q.RequestRef });
            }
            var result = await _service.UpdateAsync(expenseId, dto);
            if (result == null) return NotFound($"Expense with id {expenseId} not found");
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ZaaerExpenseResponseDto>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }
    }
}


