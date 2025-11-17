using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    [Route("api/zaaer/[controller]")]
    [ApiController]
    public class RefundController : ControllerBase
    {
        private readonly IZaaerRefundService _zaaerRefundService;
        private readonly IMapper _mapper;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public RefundController(IZaaerRefundService zaaerRefundService, IMapper mapper, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _zaaerRefundService = zaaerRefundService;
            _mapper = mapper;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Creates a new refund for Zaaer integration.
        /// </summary>
        /// <param name="createRefundDto">Refund data</param>
        /// <returns>A newly created refund</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ZaaerRefundResponseDto>> CreateRefund([FromBody] ZaaerCreateRefundDto createRefundDto)
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
                    Operation = "/api/zaaer/Refund",
                    OperationKey = "Zaaer.Refund.Create",
                    PayloadType = nameof(ZaaerCreateRefundDto),
                    PayloadJson = JsonSerializer.Serialize(createRefundDto),
                    HotelId = createRefundDto.HotelId
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }
            var refundResponse = await _zaaerRefundService.CreateRefundAsync(createRefundDto);
            return CreatedAtAction(nameof(GetRefundById), new { refundId = refundResponse.RefundId }, refundResponse);
        }

        /// <summary>
        /// Updates an existing refund for Zaaer integration.
        /// </summary>
        /// <param name="refundId">The ID of the refund to update</param>
        /// <param name="updateRefundDto">Updated refund data</param>
        /// <returns>The updated refund</returns>
        [HttpPut("{refundId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerRefundResponseDto>> UpdateRefund(int refundId, [FromBody] ZaaerUpdateRefundDto updateRefundDto)
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
                    Operation = $"/api/zaaer/Refund/{refundId}",
                    OperationKey = "Zaaer.Refund.UpdateById",
                    TargetId = refundId,
                    PayloadType = nameof(ZaaerUpdateRefundDto),
                    PayloadJson = JsonSerializer.Serialize(updateRefundDto)
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }
            var refundResponse = await _zaaerRefundService.UpdateRefundAsync(refundId, updateRefundDto);
            if (refundResponse == null)
            {
                return NotFound($"Refund with ID {refundId} not found.");
            }
            return Ok(refundResponse);
        }

        /// <summary>
        /// Updates an existing refund by refund number for Zaaer integration.
        /// </summary>
        /// <param name="refundNo">The refund number to update</param>
        /// <param name="updateRefundDto">Updated refund data</param>
        /// <returns>The updated refund</returns>
        [HttpPut("refund-no/{refundNo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerRefundResponseDto>> UpdateRefundByRefundNo(string refundNo, [FromBody] ZaaerUpdateRefundDto updateRefundDto)
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
                    Operation = $"/api/zaaer/Refund/refund-no/{refundNo}",
                    OperationKey = "Zaaer.Refund.UpdateByNumber",
                    PayloadType = refundNo,
                    PayloadJson = JsonSerializer.Serialize(updateRefundDto)
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }
            var refundResponse = await _zaaerRefundService.UpdateRefundByRefundNoAsync(refundNo, updateRefundDto);
            if (refundResponse == null)
            {
                return NotFound($"Refund with number {refundNo} not found.");
            }
            return Ok(refundResponse);
        }

        /// <summary>
        /// Updates an existing refund by Zaaer external id for Zaaer integration.
        /// </summary>
        /// <param name="zaaerId">External Zaaer id stored on refunds table</param>
        /// <param name="updateRefundDto">Updated refund data</param>
        /// <returns>The updated refund</returns>
        [HttpPut("zaaer/{zaaerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerRefundResponseDto>> UpdateRefundByZaaerId(int zaaerId, [FromBody] ZaaerUpdateRefundDto updateRefundDto)
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
                    Operation = $"/api/zaaer/Refund/zaaer/{zaaerId}",
                    OperationKey = "Zaaer.Refund.UpdateByZaaerId",
                    PayloadType = nameof(ZaaerUpdateRefundDto),
                    PayloadJson = JsonSerializer.Serialize(updateRefundDto)
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }

            var refundResponse = await _zaaerRefundService.UpdateRefundByZaaerIdAsync(zaaerId, updateRefundDto);
            if (refundResponse == null)
            {
                return NotFound($"Refund with zaaerId '{zaaerId}' not found.");
            }
            return Ok(refundResponse);
        }

        /// <summary>
        /// Gets a refund by ID for Zaaer integration.
        /// </summary>
        /// <param name="refundId">The ID of the refund</param>
        /// <returns>The refund data</returns>
        [HttpGet("{refundId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerRefundResponseDto>> GetRefundById(int refundId)
        {
            var refund = await _zaaerRefundService.GetRefundByIdAsync(refundId);
            if (refund == null)
            {
                return NotFound($"Refund with ID {refundId} not found.");
            }
            return Ok(refund);
        }

        /// <summary>
        /// Gets all refunds for a specific hotel for Zaaer integration.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel</param>
        /// <returns>A list of refunds</returns>
        [HttpGet("hotel/{hotelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ZaaerRefundResponseDto>>> GetRefundsByHotelId(int hotelId)
        {
            var refunds = await _zaaerRefundService.GetRefundsByHotelIdAsync(hotelId);
            return Ok(refunds);
        }

        /// <summary>
        /// Deletes a refund by ID for Zaaer integration.
        /// </summary>
        /// <param name="refundId">The ID of the refund to delete</param>
        /// <returns>No content</returns>
        [HttpDelete("{refundId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteRefund(int refundId)
        {
            var deleted = await _zaaerRefundService.DeleteRefundAsync(refundId);
            if (!deleted)
            {
                return NotFound($"Refund with ID {refundId} not found.");
            }
            return NoContent();
        }
    }
}
