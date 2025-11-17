using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for Refund data access
    /// </summary>
    public interface IRefundRepository : IGenericRepository<Refund>
    {
        /// <summary>
        /// Get refund with full details by ID
        /// </summary>
        Task<Refund?> GetRefundWithDetailsAsync(int id);

        /// <summary>
        /// Get refunds by hotel ID
        /// </summary>
        Task<IEnumerable<Refund>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get refunds by reservation ID
        /// </summary>
        Task<IEnumerable<Refund>> GetByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get refunds by customer ID
        /// </summary>
        Task<IEnumerable<Refund>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get refunds by date range
        /// </summary>
        Task<IEnumerable<Refund>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get refunds by amount range
        /// </summary>
        Task<IEnumerable<Refund>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount);

        /// <summary>
        /// Get refunds by refund number
        /// </summary>
        Task<IEnumerable<Refund>> GetByRefundNumberAsync(string refundNumber);

        /// <summary>
        /// Get refunds by payment method ID
        /// </summary>
        Task<IEnumerable<Refund>> GetByPaymentMethodIdAsync(int paymentMethodId);

        /// <summary>
        /// Get refunds by bank ID
        /// </summary>
        Task<IEnumerable<Refund>> GetByBankIdAsync(int bankId);

        /// <summary>
        /// Get refunds by transaction number
        /// </summary>
        Task<IEnumerable<Refund>> GetByTransactionNumberAsync(string transactionNumber);


        /// <summary>
        /// Get refunds by created date
        /// </summary>
        Task<IEnumerable<Refund>> GetByCreatedDateAsync(DateTime createdDate);

        /// <summary>
        /// Get refunds by created date range
        /// </summary>
        Task<IEnumerable<Refund>> GetByCreatedDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get refunds by created by user
        /// </summary>
        Task<IEnumerable<Refund>> GetByCreatedByAsync(string createdBy);

        /// <summary>
        /// Get refund statistics
        /// </summary>
        Task<RefundStatisticsDto> GetRefundStatisticsAsync();

        /// <summary>
        /// Search refunds by refund number
        /// </summary>
        Task<IEnumerable<Refund>> SearchByRefundNumberAsync(string refundNumber);

        /// <summary>
        /// Search refunds by customer name
        /// </summary>
        Task<IEnumerable<Refund>> SearchByCustomerNameAsync(string customerName);

        /// <summary>
        /// Search refunds by hotel name
        /// </summary>
        Task<IEnumerable<Refund>> SearchByHotelNameAsync(string hotelName);

        /// <summary>
        /// Get total refund amount by hotel ID
        /// </summary>
        Task<decimal> GetTotalRefundAmountByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get total refund amount by date range
        /// </summary>
        Task<decimal> GetTotalRefundAmountByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get total refund amount by customer ID
        /// </summary>
        Task<decimal> GetTotalRefundAmountByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get total refund amount by reservation ID
        /// </summary>
        Task<decimal> GetTotalRefundAmountByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get average refund amount by hotel ID
        /// </summary>
        Task<decimal> GetAverageRefundAmountByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get average refund amount by date range
        /// </summary>
        Task<decimal> GetAverageRefundAmountByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get refunds by multiple criteria
        /// </summary>
        Task<IEnumerable<Refund>> GetRefundsByCriteriaAsync(
            int? hotelId = null,
            int? customerId = null,
            int? reservationId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            string? refundNumber = null,
            string? transactionNumber = null,
            string? createdBy = null);
    }
}
