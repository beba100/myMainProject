namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Service for sending WhatsApp messages via UltraMsg API
    /// </summary>
    public interface IWhatsAppService
    {
        /// <summary>
        /// Send a WhatsApp message to a phone number
        /// </summary>
        /// <param name="phoneNumber">Phone number with country code (e.g., 966541997799)</param>
        /// <param name="message">Message text to send</param>
        /// <returns>True if message was sent successfully, false otherwise</returns>
        Task<(bool Success, string? ErrorMessage)> SendMessageAsync(string phoneNumber, string message);
    }
}

