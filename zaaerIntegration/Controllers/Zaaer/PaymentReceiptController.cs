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
    public class PaymentReceiptController : ControllerBase
    {
        private readonly IZaaerPaymentReceiptService _zaaerPaymentReceiptService;
        private readonly IMapper _mapper;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public PaymentReceiptController(IZaaerPaymentReceiptService zaaerPaymentReceiptService, IMapper mapper, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _zaaerPaymentReceiptService = zaaerPaymentReceiptService;
            _mapper = mapper;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Creates a new payment receipt for Zaaer integration.
        /// </summary>
        /// <param name="createPaymentReceiptDto">Payment receipt data</param>
        /// <returns>A newly created payment receipt</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ZaaerPaymentReceiptResponseDto>> CreatePaymentReceipt([FromBody] ZaaerCreatePaymentReceiptDto createPaymentReceiptDto)
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
                    Operation = "/api/zaaer/PaymentReceipt",
                    OperationKey = "Zaaer.PaymentReceipt.Create",
                    PayloadType = nameof(ZaaerCreatePaymentReceiptDto),
                    PayloadJson = JsonSerializer.Serialize(createPaymentReceiptDto),
                    HotelId = createPaymentReceiptDto.HotelId
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }
            var paymentReceiptResponse = await _zaaerPaymentReceiptService.CreatePaymentReceiptAsync(createPaymentReceiptDto);
            return CreatedAtAction(nameof(GetPaymentReceiptById), new { receiptId = paymentReceiptResponse.ReceiptId }, paymentReceiptResponse);
        }

        /// <summary>
        /// Updates an existing payment receipt by ID or ZaaerId for Zaaer integration.
        /// First tries to find by ZaaerId, if not found then tries by ReceiptId.
        /// </summary>
        /// <param name="receiptId">The ID (ReceiptId or ZaaerId) of the payment receipt to update</param>
        /// <param name="updatePaymentReceiptDto">Updated payment receipt data</param>
        /// <returns>The updated payment receipt</returns>
        [HttpPut("{receiptId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerPaymentReceiptResponseDto>> UpdatePaymentReceipt(int receiptId, [FromBody] ZaaerUpdatePaymentReceiptDto updatePaymentReceiptDto)
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
                    Operation = $"/api/zaaer/PaymentReceipt/{receiptId}",
                    OperationKey = "Zaaer.PaymentReceipt.UpdateById",
                    TargetId = receiptId,
                    PayloadType = nameof(ZaaerUpdatePaymentReceiptDto),
                    PayloadJson = JsonSerializer.Serialize(updatePaymentReceiptDto)
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }

            // First try to find by ZaaerId (since Zaaer sends zaaerId in URL)
            var paymentReceiptByZaaerId = await _zaaerPaymentReceiptService.UpdatePaymentReceiptByZaaerIdAsync(receiptId, updatePaymentReceiptDto);
            if (paymentReceiptByZaaerId != null)
            {
                return Ok(paymentReceiptByZaaerId);
            }

            // If not found by ZaaerId, try by ReceiptId (backward compatibility)
            var paymentReceiptResponse = await _zaaerPaymentReceiptService.UpdatePaymentReceiptAsync(receiptId, updatePaymentReceiptDto);
            if (paymentReceiptResponse == null)
            {
                return NotFound($"Payment receipt with ID or ZaaerId {receiptId} not found.");
            }
            return Ok(paymentReceiptResponse);
        }

        /// <summary>
        /// Updates an existing payment receipt by Zaaer external id.
        /// </summary>
        /// <param name="zaaerId">External Zaaer id stored on payment_receipts table</param>
        /// <param name="updatePaymentReceiptDto">Updated payment receipt data</param>
        /// <returns>The updated payment receipt</returns>
        [HttpPut("zaaer/{zaaerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerPaymentReceiptResponseDto>> UpdatePaymentReceiptByZaaerId(int zaaerId, [FromBody] ZaaerUpdatePaymentReceiptDto updatePaymentReceiptDto)
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
                    Operation = $"/api/zaaer/PaymentReceipt/zaaer/{zaaerId}",
                    OperationKey = "Zaaer.PaymentReceipt.UpdateByZaaerId",
                    PayloadType = nameof(ZaaerUpdatePaymentReceiptDto),
                    PayloadJson = JsonSerializer.Serialize(updatePaymentReceiptDto)
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }

            var paymentReceiptResponse = await _zaaerPaymentReceiptService.UpdatePaymentReceiptByZaaerIdAsync(zaaerId, updatePaymentReceiptDto);
            if (paymentReceiptResponse == null)
            {
                return NotFound($"Payment receipt with zaaerId '{zaaerId}' not found.");
            }
            return Ok(paymentReceiptResponse);
        }

        /// <summary>
        /// Updates an existing payment receipt by receipt number for Zaaer integration.
        /// </summary>
        /// <param name="receiptNo">The receipt number of the payment receipt to update</param>
        /// <param name="updatePaymentReceiptDto">Updated payment receipt data</param>
        /// <returns>The updated payment receipt</returns>
        [HttpPut("receipt-no/{receiptNo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerPaymentReceiptResponseDto>> UpdatePaymentReceiptByReceiptNo(string receiptNo, [FromBody] ZaaerUpdatePaymentReceiptDto updatePaymentReceiptDto)
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
                    Operation = $"/api/zaaer/PaymentReceipt/receipt-no/{receiptNo}",
                    OperationKey = "Zaaer.PaymentReceipt.UpdateByNumber",
                    PayloadType = receiptNo,
                    PayloadJson = JsonSerializer.Serialize(updatePaymentReceiptDto)
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }
            var paymentReceiptResponse = await _zaaerPaymentReceiptService.UpdatePaymentReceiptByReceiptNoAsync(receiptNo, updatePaymentReceiptDto);
            if (paymentReceiptResponse == null)
            {
                return NotFound($"Payment receipt with receipt number '{receiptNo}' not found.");
            }
            return Ok(paymentReceiptResponse);
        }

        /// <summary>
        /// Gets a payment receipt by ID for Zaaer integration.
        /// </summary>
        /// <param name="receiptId">The ID of the payment receipt</param>
        /// <returns>The payment receipt data</returns>
        [HttpGet("{receiptId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerPaymentReceiptResponseDto>> GetPaymentReceiptById(int receiptId)
        {
            var paymentReceipt = await _zaaerPaymentReceiptService.GetPaymentReceiptByIdAsync(receiptId);
            if (paymentReceipt == null)
            {
                return NotFound($"Payment receipt with ID {receiptId} not found.");
            }
            return Ok(paymentReceipt);
        }

        /// <summary>
        /// Gets a payment receipt by receipt number for Zaaer integration.
        /// </summary>
        /// <param name="receiptNo">The receipt number of the payment receipt</param>
        /// <returns>The payment receipt data</returns>
        [HttpGet("receipt-no/{receiptNo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerPaymentReceiptResponseDto>> GetPaymentReceiptByReceiptNo(string receiptNo)
        {
            var paymentReceipt = await _zaaerPaymentReceiptService.GetPaymentReceiptByReceiptNoAsync(receiptNo);
            if (paymentReceipt == null)
            {
                return NotFound($"Payment receipt with receipt number '{receiptNo}' not found.");
            }
            return Ok(paymentReceipt);
        }

        /// <summary>
        /// Gets all payment receipts for a specific hotel for Zaaer integration.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel</param>
        /// <returns>A list of payment receipts</returns>
        [HttpGet("hotel/{hotelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ZaaerPaymentReceiptResponseDto>>> GetPaymentReceiptsByHotelId(int hotelId)
        {
            var paymentReceipts = await _zaaerPaymentReceiptService.GetPaymentReceiptsByHotelIdAsync(hotelId);
            return Ok(paymentReceipts);
        }

        /// <summary>
        /// Cancels a payment receipt by ID for Zaaer integration (soft delete).
        /// </summary>
        /// <param name="receiptId">The ID of the payment receipt to delete</param>
        /// <returns>No content</returns>
        [HttpDelete("{receiptId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePaymentReceipt(int receiptId)
        {
            var deleted = await _zaaerPaymentReceiptService.DeletePaymentReceiptAsync(receiptId);
            if (!deleted)
            {
                return NotFound($"Payment receipt with ID {receiptId} not found.");
            }
            return NoContent();
        }

        /// <summary>
        /// Cancels a payment receipt by ZaaerId for Zaaer integration (soft delete).
        /// </summary>
        /// <param name="zaaerId">Zaaer external id stored on payment_receipts</param>
        /// <returns>No content</returns>
        [HttpDelete("zaaer/{zaaerId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePaymentReceiptByZaaerId(int zaaerId)
        {
            var deleted = await _zaaerPaymentReceiptService.DeletePaymentReceiptByZaaerIdAsync(zaaerId);
            if (!deleted)
            {
                return NotFound($"Payment receipt with zaaerId '{zaaerId}' not found.");
            }
            return NoContent();
        }

        /// <summary>
        /// Deletes a payment receipt by receipt number for Zaaer integration.
        /// </summary>
        /// <param name="receiptNo">The receipt number of the payment receipt to delete</param>
        /// <returns>No content</returns>
        [HttpDelete("receipt-no/{receiptNo}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePaymentReceiptByReceiptNo(string receiptNo)
        {
            var deleted = await _zaaerPaymentReceiptService.DeletePaymentReceiptByReceiptNoAsync(receiptNo);
            if (!deleted)
            {
                return NotFound($"Payment receipt with receipt number '{receiptNo}' not found.");
            }
            return NoContent();
        }
    }
}
