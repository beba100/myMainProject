using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for CreditNote business logic
    /// </summary>
    public interface ICreditNoteService
    {
        /// <summary>
        /// Get all credit notes with pagination and search
        /// </summary>
        Task<(IEnumerable<CreditNoteResponseDto> creditNotes, int totalCount)> GetAllCreditNotesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);

        /// <summary>
        /// Get credit note by ID
        /// </summary>
        Task<CreditNoteResponseDto?> GetCreditNoteByIdAsync(int id);

        /// <summary>
        /// Create new credit note
        /// </summary>
        Task<CreditNoteResponseDto> CreateCreditNoteAsync(CreateCreditNoteDto createCreditNoteDto);

        /// <summary>
        /// Update existing credit note
        /// </summary>
        Task<CreditNoteResponseDto?> UpdateCreditNoteAsync(int id, UpdateCreditNoteDto updateCreditNoteDto);

        /// <summary>
        /// Delete credit note
        /// </summary>
        Task<bool> DeleteCreditNoteAsync(int id);

        /// <summary>
        /// Get credit notes by hotel ID
        /// </summary>
        Task<IEnumerable<CreditNoteResponseDto>> GetCreditNotesByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get credit notes by reservation ID
        /// </summary>
        Task<IEnumerable<CreditNoteResponseDto>> GetCreditNotesByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get credit notes by customer ID
        /// </summary>
        Task<IEnumerable<CreditNoteResponseDto>> GetCreditNotesByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get credit notes by date range
        /// </summary>
        Task<IEnumerable<CreditNoteResponseDto>> GetCreditNotesByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get credit notes by amount range
        /// </summary>
        Task<IEnumerable<CreditNoteResponseDto>> GetCreditNotesByAmountRangeAsync(decimal minAmount, decimal maxAmount);

        /// <summary>
        /// Get credit note statistics
        /// </summary>
        Task<CreditNoteStatisticsDto> GetCreditNoteStatisticsAsync();

        /// <summary>
        /// Search credit notes by credit note number
        /// </summary>
        Task<IEnumerable<CreditNoteResponseDto>> SearchCreditNotesByCreditNoteNumberAsync(string creditNoteNumber);

        /// <summary>
        /// Search credit notes by customer name
        /// </summary>
        Task<IEnumerable<CreditNoteResponseDto>> SearchCreditNotesByCustomerNameAsync(string customerName);

        /// <summary>
        /// Get total credit note amount by hotel ID
        /// </summary>
        Task<decimal> GetTotalCreditNoteAmountByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get total credit note amount by date range
        /// </summary>
        Task<decimal> GetTotalCreditNoteAmountByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
