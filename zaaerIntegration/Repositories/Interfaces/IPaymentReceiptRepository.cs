using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for PaymentReceipt repository operations
    /// </summary>
    public interface IPaymentReceiptRepository : IGenericRepository<PaymentReceipt>
    {
        /// <summary>
        /// Get payment receipts with pagination and search
        /// </summary>
        Task<(IEnumerable<PaymentReceipt> PaymentReceipts, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<PaymentReceipt, bool>>? filter = null);

        /// <summary>
        /// Get payment receipt by receipt number
        /// </summary>
        Task<PaymentReceipt?> GetByReceiptNoAsync(string receiptNo);

        /// <summary>
        /// Get payment receipts by customer ID
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get payment receipts by hotel ID
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get payment receipts by reservation ID
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get payment receipts by invoice ID
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByInvoiceIdAsync(int invoiceId);

        /// <summary>
        /// Get payment receipts by receipt type
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByReceiptTypeAsync(string receiptType);

        /// <summary>
        /// Get payment receipts by payment method
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByPaymentMethodAsync(string paymentMethod);

        /// <summary>
        /// Get payment receipts by date range
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get payment receipts by amount range
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount);

        /// <summary>
        /// Get payment receipts by customer name (search)
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByCustomerNameAsync(string customerName);

        /// <summary>
        /// Get payment receipts by hotel name (search)
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByHotelNameAsync(string hotelName);

        /// <summary>
        /// Get payment receipts by receipt number (search)
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByReceiptNoSearchAsync(string receiptNo);

        /// <summary>
        /// Get payment receipts by transaction number (search)
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByTransactionNoAsync(string transactionNo);

        /// <summary>
        /// Check if receipt number exists
        /// </summary>
        Task<bool> ReceiptNoExistsAsync(string receiptNo, int? excludeId = null);

        /// <summary>
        /// Get payment receipt statistics
        /// </summary>
        Task<object> GetStatisticsAsync();

        /// <summary>
        /// Get payment receipts with full details (includes all navigation properties)
        /// </summary>
        Task<PaymentReceipt?> GetWithDetailsAsync(int id);

        /// <summary>
        /// Get payment receipts with full details by receipt number
        /// </summary>
        Task<PaymentReceipt?> GetWithDetailsByReceiptNoAsync(string receiptNo);

        /// <summary>
        /// Get payment receipts by bank ID
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByBankIdAsync(int bankId);

        /// <summary>
        /// Get payment receipts by payment method ID
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByPaymentMethodIdAsync(int paymentMethodId);

        /// <summary>
        /// Get payment receipts by created by user
        /// </summary>
        Task<IEnumerable<PaymentReceipt>> GetByCreatedByAsync(int createdBy);


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
        Task<IEnumerable<PaymentReceipt>> GetByPeriodRangeAsync(DateTime startDate, DateTime endDate);
    }
}
