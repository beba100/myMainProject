using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for Refund business logic
    /// </summary>
    public interface IRefundService
    {
        /// <summary>
        /// Get all refunds with pagination and search
        /// </summary>
        Task<(IEnumerable<RefundResponseDto> refunds, int totalCount)> GetAllRefundsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);

        /// <summary>
        /// Get refund by ID
        /// </summary>
        Task<RefundResponseDto?> GetRefundByIdAsync(int id);

        /// <summary>
        /// Create new refund
        /// </summary>
        Task<RefundResponseDto> CreateRefundAsync(CreateRefundDto createRefundDto);

        /// <summary>
        /// Update existing refund
        /// </summary>
        Task<RefundResponseDto?> UpdateRefundAsync(int id, UpdateRefundDto updateRefundDto);

        /// <summary>
        /// Delete refund
        /// </summary>
        Task<bool> DeleteRefundAsync(int id);

        /// <summary>
        /// Get refunds by hotel ID
        /// </summary>
        Task<IEnumerable<RefundResponseDto>> GetRefundsByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get refunds by reservation ID
        /// </summary>
        Task<IEnumerable<RefundResponseDto>> GetRefundsByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get refunds by customer ID
        /// </summary>
        Task<IEnumerable<RefundResponseDto>> GetRefundsByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get refunds by date range
        /// </summary>
        Task<IEnumerable<RefundResponseDto>> GetRefundsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get refunds by amount range
        /// </summary>
        Task<IEnumerable<RefundResponseDto>> GetRefundsByAmountRangeAsync(decimal minAmount, decimal maxAmount);

        /// <summary>
        /// Get refund statistics
        /// </summary>
        Task<RefundStatisticsDto> GetRefundStatisticsAsync();

        /// <summary>
        /// Search refunds by refund number
        /// </summary>
        Task<IEnumerable<RefundResponseDto>> SearchRefundsByRefundNumberAsync(string refundNumber);

        /// <summary>
        /// Search refunds by customer name
        /// </summary>
        Task<IEnumerable<RefundResponseDto>> SearchRefundsByCustomerNameAsync(string customerName);

        /// <summary>
        /// Get total refund amount by hotel ID
        /// </summary>
        Task<decimal> GetTotalRefundAmountByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get total refund amount by date range
        /// </summary>
        Task<decimal> GetTotalRefundAmountByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
