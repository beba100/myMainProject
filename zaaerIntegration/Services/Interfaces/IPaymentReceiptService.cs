using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for PaymentReceipt service operations
    /// </summary>
    public interface IPaymentReceiptService
    {
        /// <summary>
        /// Get all payment receipts with pagination and search
        /// </summary>
        Task<(IEnumerable<PaymentReceiptResponseDto> PaymentReceipts, int TotalCount)> GetAllPaymentReceiptsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null);

        /// <summary>
        /// Get payment receipt by ID
        /// </summary>
        Task<PaymentReceiptResponseDto?> GetPaymentReceiptByIdAsync(int id);

        /// <summary>
        /// Get payment receipt by receipt number
        /// </summary>
        Task<PaymentReceiptResponseDto?> GetPaymentReceiptByNoAsync(string receiptNo);

        /// <summary>
        /// Create new payment receipt
        /// </summary>
        Task<PaymentReceiptResponseDto> CreatePaymentReceiptAsync(CreatePaymentReceiptDto createPaymentReceiptDto);

        /// <summary>
        /// Update existing payment receipt
        /// </summary>
        Task<PaymentReceiptResponseDto?> UpdatePaymentReceiptAsync(int id, UpdatePaymentReceiptDto updatePaymentReceiptDto);

        /// <summary>
        /// Delete payment receipt
        /// </summary>
        Task<bool> DeletePaymentReceiptAsync(int id);

        /// <summary>
        /// Get payment receipts by customer ID
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get payment receipts by hotel ID
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get payment receipts by reservation ID
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get payment receipts by invoice ID
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByInvoiceIdAsync(int invoiceId);

        /// <summary>
        /// Get payment receipts by receipt type
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByTypeAsync(string receiptType);

        /// <summary>
        /// Get payment receipts by payment method
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByPaymentMethodAsync(string paymentMethod);

        /// <summary>
        /// Get payment receipts by date range
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get payment receipts by amount range
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByAmountRangeAsync(decimal minAmount, decimal maxAmount);

        /// <summary>
        /// Search payment receipts by customer name
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> SearchPaymentReceiptsByCustomerNameAsync(string customerName);

        /// <summary>
        /// Search payment receipts by hotel name
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> SearchPaymentReceiptsByHotelNameAsync(string hotelName);

        /// <summary>
        /// Search payment receipts by receipt number
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> SearchPaymentReceiptsByNoAsync(string receiptNo);

        /// <summary>
        /// Search payment receipts by transaction number
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> SearchPaymentReceiptsByTransactionNoAsync(string transactionNo);

        /// <summary>
        /// Get payment receipt statistics
        /// </summary>
        Task<object> GetPaymentReceiptStatisticsAsync();

        /// <summary>
        /// Check if receipt number exists
        /// </summary>
        Task<bool> ReceiptNoExistsAsync(string receiptNo, int? excludeId = null);

        /// <summary>
        /// Get payment receipts by bank ID
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByBankIdAsync(int bankId);

        /// <summary>
        /// Get payment receipts by payment method ID
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByPaymentMethodIdAsync(int paymentMethodId);

        /// <summary>
        /// Get payment receipts by created by user
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByCreatedByAsync(int createdBy);


        /// <summary>
        /// Get total amount paid by customer
        /// </summary>
        Task<decimal> GetTotalAmountByCustomerAsync(int customerId);

        /// <summary>
        /// Get total amount paid by hotel
        /// </summary>
        Task<decimal> GetTotalAmountByHotelAsync(int hotelId);

        /// <summary>
        /// Get payment receipts by period range
        /// </summary>
        Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByPeriodRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Update payment receipt amount
        /// </summary>
        Task<bool> UpdatePaymentAmountAsync(int id, decimal amountPaid);

        /// <summary>
        /// Update payment receipt transaction number
        /// </summary>
        Task<bool> UpdateTransactionNumberAsync(int id, string transactionNo);

        /// <summary>
        /// Update payment receipt notes
        /// </summary>
        Task<bool> UpdateNotesAsync(int id, string notes);
    }
}
