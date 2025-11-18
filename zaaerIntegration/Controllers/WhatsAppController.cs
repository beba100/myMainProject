using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for sending WhatsApp messages
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<WhatsAppController> _logger;

        public WhatsAppController(
            IWhatsAppService whatsAppService,
            ILogger<WhatsAppController> logger)
        {
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        /// <summary>
        /// Send a WhatsApp message
        /// </summary>
        /// <param name="request">Request containing phone number and message</param>
        /// <returns>Result of the send operation</returns>
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> SendMessage([FromBody] SendWhatsAppMessageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return BadRequest(new { error = "Phone number is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Message is required" });
                }

                // Validate phone number format (should be 966XXXXXXXXX)
                var phoneRegex = new System.Text.RegularExpressions.Regex(@"^966\d{9}$");
                if (!phoneRegex.IsMatch(request.PhoneNumber))
                {
                    return BadRequest(new { error = "Invalid phone number format. Must be: 966XXXXXXXXX" });
                }

                // Remove 966 prefix before sending to UltraMsg API
                var phoneNumberWithoutPrefix = request.PhoneNumber;
                if (phoneNumberWithoutPrefix.StartsWith("966"))
                {
                    phoneNumberWithoutPrefix = phoneNumberWithoutPrefix.Substring(3);
                }

                _logger.LogInformation("📤 Sending WhatsApp message to: {PhoneNumber} (without prefix: {PhoneWithoutPrefix})", 
                    request.PhoneNumber, phoneNumberWithoutPrefix);

                var (success, errorMessage) = await _whatsAppService.SendMessageAsync(phoneNumberWithoutPrefix, request.Message);

                if (success)
                {
                    _logger.LogInformation("✅ WhatsApp message sent successfully to: {PhoneNumber}", request.PhoneNumber);
                    return Ok(new { success = true, message = "Message sent successfully" });
                }
                else
                {
                    _logger.LogWarning("⚠️ Failed to send WhatsApp message to: {PhoneNumber}, Error: {Error}", 
                        request.PhoneNumber, errorMessage);
                    return StatusCode(500, new { success = false, error = errorMessage ?? "Failed to send message" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending WhatsApp message: {Message}", ex.Message);
                return StatusCode(500, new { success = false, error = "An error occurred while sending the message" });
            }
        }
    }

    /// <summary>
    /// Request DTO for sending WhatsApp message
    /// </summary>
    public class SendWhatsAppMessageRequest
    {
        /// <summary>
        /// Phone number with country code (e.g., 966541997799)
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Message text to send
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}

