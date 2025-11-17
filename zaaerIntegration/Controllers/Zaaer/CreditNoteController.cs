using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    /// <summary>
    /// Controller for Zaaer Credit Note integration endpoints
    /// </summary>
    [ApiController]
    [Route("api/zaaer/[controller]")]
    public class CreditNoteController : ControllerBase
    {
        private readonly IZaaerCreditNoteService _creditNoteService;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public CreditNoteController(IZaaerCreditNoteService creditNoteService, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _creditNoteService = creditNoteService;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Create a new credit note
        /// </summary>
        /// <param name="createCreditNoteDto">Credit note creation data</param>
        /// <returns>Created credit note</returns>
        [HttpPost]
        public async Task<ActionResult<ZaaerCreditNoteResponseDto>> CreateCreditNote([FromBody] ZaaerCreateCreditNoteDto createCreditNoteDto)
        {
            try
            {
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = "/api/zaaer/CreditNote",
                        OperationKey = "Zaaer.CreditNote.Create",
                        PayloadType = nameof(ZaaerCreateCreditNoteDto),
                        PayloadJson = JsonSerializer.Serialize(createCreditNoteDto),
                        HotelId = createCreditNoteDto.HotelId
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var creditNote = await _creditNoteService.CreateCreditNoteAsync(createCreditNoteDto);
                return CreatedAtAction(nameof(CreateCreditNote), new { id = creditNote.CreditNoteId }, creditNote);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all credit notes for a specific hotel
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of credit notes</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<IEnumerable<ZaaerCreditNoteResponseDto>>> GetCreditNotesByHotelId(int hotelId)
        {
            try
            {
                var creditNotes = await _creditNoteService.GetCreditNotesByHotelIdAsync(hotelId);
                return Ok(creditNotes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
