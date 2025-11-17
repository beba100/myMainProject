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
    public class InvoiceController : ControllerBase
    {
        private readonly IZaaerInvoiceService _zaaerInvoiceService;
        private readonly IMapper _mapper;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public InvoiceController(IZaaerInvoiceService zaaerInvoiceService, IMapper mapper, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _zaaerInvoiceService = zaaerInvoiceService;
            _mapper = mapper;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Creates a new invoice for Zaaer integration.
        /// </summary>
        /// <param name="createInvoiceDto">Invoice data</param>
        /// <returns>A newly created invoice</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ZaaerInvoiceResponseDto>> CreateInvoice([FromBody] ZaaerCreateInvoiceDto createInvoiceDto)
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
                    Operation = "/api/zaaer/Invoice",
                    OperationKey = "Zaaer.Invoice.Create",
                    PayloadType = nameof(ZaaerCreateInvoiceDto),
                    PayloadJson = JsonSerializer.Serialize(createInvoiceDto),
                    HotelId = createInvoiceDto.HotelId
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }

            var invoiceResponse = await _zaaerInvoiceService.CreateInvoiceAsync(createInvoiceDto);
            return CreatedAtAction(nameof(GetInvoiceById), new { invoiceId = invoiceResponse.InvoiceId }, invoiceResponse);
        }

        /// <summary>
        /// Updates an existing invoice for Zaaer integration.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice to update</param>
        /// <param name="updateInvoiceDto">Updated invoice data</param>
        /// <returns>The updated invoice</returns>
        [HttpPut("{invoiceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerInvoiceResponseDto>> UpdateInvoice(int invoiceId, [FromBody] ZaaerUpdateInvoiceDto updateInvoiceDto)
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
                    Operation = $"/api/zaaer/Invoice/{invoiceId}",
                    OperationKey = "Zaaer.Invoice.UpdateById",
                    TargetId = invoiceId,
                    PayloadType = nameof(ZaaerUpdateInvoiceDto),
                    PayloadJson = JsonSerializer.Serialize(updateInvoiceDto)
                };
                await _queueService.EnqueueAsync(dtoQ);
                return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
            }

            var invoiceResponse = await _zaaerInvoiceService.UpdateInvoiceAsync(invoiceId, updateInvoiceDto);
            if (invoiceResponse == null)
            {
                return NotFound($"Invoice with ID {invoiceId} not found.");
            }
            return Ok(invoiceResponse);
        }

        /// <summary>
        /// Gets an invoice by ID for Zaaer integration.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice</param>
        /// <returns>The invoice data</returns>
        [HttpGet("{invoiceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZaaerInvoiceResponseDto>> GetInvoiceById(int invoiceId)
        {
            var invoice = await _zaaerInvoiceService.GetInvoiceByIdAsync(invoiceId);
            if (invoice == null)
            {
                return NotFound($"Invoice with ID {invoiceId} not found.");
            }
            return Ok(invoice);
        }

        /// <summary>
        /// Gets all invoices for a specific hotel for Zaaer integration.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel</param>
        /// <returns>A list of invoices</returns>
        [HttpGet("hotel/{hotelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ZaaerInvoiceResponseDto>>> GetInvoicesByHotelId(int hotelId)
        {
            var invoices = await _zaaerInvoiceService.GetInvoicesByHotelIdAsync(hotelId);
            return Ok(invoices);
        }

        /// <summary>
        /// Deletes an invoice by ID for Zaaer integration.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice to delete</param>
        /// <returns>No content</returns>
        [HttpDelete("{invoiceId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteInvoice(int invoiceId)
        {
            var deleted = await _zaaerInvoiceService.DeleteInvoiceAsync(invoiceId);
            if (!deleted)
            {
                return NotFound($"Invoice with ID {invoiceId} not found.");
            }
            return NoContent();
        }
    }
}
