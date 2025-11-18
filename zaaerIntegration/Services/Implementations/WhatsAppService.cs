using System.Text;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service for sending WhatsApp messages via UltraMsg API
    /// </summary>
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Constructor for WhatsAppService
        /// </summary>
        public WhatsAppService(
            IConfiguration configuration,
            ILogger<WhatsAppService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Send a WhatsApp message to a phone number
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> SendMessageAsync(string phoneNumber, string message)
        {
            try
            {
                // Get credentials from configuration
                var apiUrl = _configuration["WhatsApp:ApiUrl"];
                var token = _configuration["WhatsApp:Token"];

                if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(token))
                {
                    _logger.LogError("❌ WhatsApp API credentials not configured");
                    return (false, "WhatsApp API credentials not configured");
                }

                // Validate phone number format (should be XXXXXXXXX - without 966 prefix)
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    return (false, "Phone number is required");
                }
                
                // Ensure phone number doesn't have 966 prefix (should be removed by controller)
                if (phoneNumber.StartsWith("966"))
                {
                    phoneNumber = phoneNumber.Substring(3);
                }

                // Build form data
                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("token", token),
                    new KeyValuePair<string, string>("to", phoneNumber),
                    new KeyValuePair<string, string>("body", message)
                };

                var content = new FormUrlEncodedContent(formData);

                _logger.LogInformation("📤 Sending WhatsApp message to: {PhoneNumber}", phoneNumber);

                // Create HttpClient from factory
                using var httpClient = _httpClientFactory.CreateClient();
                
                // Send request
                var response = await httpClient.PostAsync(apiUrl, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Try to parse response
                    try
                    {
                        var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseText);
                        
                        // Check if message was sent successfully
                        if (result != null && 
                            ((result.ContainsKey("sent") && result["sent"]?.ToString() == "True") ||
                            result.ContainsKey("id") ||
                            (result.ContainsKey("message") && !result.ContainsKey("error"))))
                        {
                            _logger.LogInformation("✅ WhatsApp message sent successfully to: {PhoneNumber}", phoneNumber);
                            return (true, null);
                        }
                        else
                        {
                            var errorMsg = result?.ContainsKey("error") == true 
                                ? result["error"].ToString() 
                                : "Unknown error from WhatsApp API";
                            _logger.LogWarning("⚠️ WhatsApp API returned error: {Error}", errorMsg);
                            return (false, errorMsg);
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogWarning("⚠️ Failed to parse WhatsApp API response: {Error}", parseEx.Message);
                        // If response is successful but can't parse, assume it worked
                        return (true, null);
                    }
                }
                else
                {
                    _logger.LogError("❌ WhatsApp API request failed: {StatusCode} - {Response}", 
                        response.StatusCode, responseText);
                    return (false, $"API request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending WhatsApp message: {Message}", ex.Message);
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}

