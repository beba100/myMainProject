using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for CreditNote data access
    /// </summary>
    public interface ICreditNoteRepository : IGenericRepository<CreditNote>
    {
        /// <summary>
        /// Get credit note with full details by ID
        /// </summary>
        Task<CreditNote?> GetCreditNoteWithDetailsAsync(int id);

        /// <summary>
        /// Get credit notes by hotel ID
        /// </summary>
        Task<IEnumerable<CreditNote>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get credit notes by reservation ID
        /// </summary>
        Task<IEnumerable<CreditNote>> GetByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get credit notes by customer ID
        /// </summary>
        Task<IEnumerable<CreditNote>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get credit notes by date range
        /// </summary>
        Task<IEnumerable<CreditNote>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get credit notes by amount range
        /// </summary>
        Task<IEnumerable<CreditNote>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount);

        /// <summary>
        /// Get credit notes by credit note number
        /// </summary>
        Task<IEnumerable<CreditNote>> GetByCreditNoteNumberAsync(string creditNoteNumber);

        /// <summary>
        /// Get credit notes by created date
        /// </summary>
        Task<IEnumerable<CreditNote>> GetByCreatedDateAsync(DateTime createdDate);

        /// <summary>
        /// Get credit notes by created date range
        /// </summary>
        Task<IEnumerable<CreditNote>> GetByCreatedDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get credit notes by created by user
        /// </summary>
        Task<IEnumerable<CreditNote>> GetByCreatedByAsync(string createdBy);

        /// <summary>
        /// Get credit note statistics
        /// </summary>
        Task<CreditNoteStatisticsDto> GetCreditNoteStatisticsAsync();

        /// <summary>
        /// Search credit notes by credit note number
        /// </summary>
        Task<IEnumerable<CreditNote>> SearchByCreditNoteNumberAsync(string creditNoteNumber);

        /// <summary>
        /// Search credit notes by customer name
        /// </summary>
        Task<IEnumerable<CreditNote>> SearchByCustomerNameAsync(string customerName);

        /// <summary>
        /// Search credit notes by hotel name
        /// </summary>
        Task<IEnumerable<CreditNote>> SearchByHotelNameAsync(string hotelName);

        /// <summary>
        /// Get total credit note amount by hotel ID
        /// </summary>
        Task<decimal> GetTotalCreditNoteAmountByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get total credit note amount by date range
        /// </summary>
        Task<decimal> GetTotalCreditNoteAmountByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get total credit note amount by customer ID
        /// </summary>
        Task<decimal> GetTotalCreditNoteAmountByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get total credit note amount by reservation ID
        /// </summary>
        Task<decimal> GetTotalCreditNoteAmountByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get average credit note amount by hotel ID
        /// </summary>
        Task<decimal> GetAverageCreditNoteAmountByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get average credit note amount by date range
        /// </summary>
        Task<decimal> GetAverageCreditNoteAmountByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get credit notes by multiple criteria
        /// </summary>
        Task<IEnumerable<CreditNote>> GetCreditNotesByCriteriaAsync(
            int? hotelId = null,
            int? customerId = null,
            int? reservationId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            string? creditNoteNumber = null,
            string? createdBy = null);
    }
}
