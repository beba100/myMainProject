using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for Invoice repository operations
    /// </summary>
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        /// <summary>
        /// Get invoices with pagination and search
        /// </summary>
        Task<(IEnumerable<Invoice> Invoices, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<Invoice, bool>>? filter = null);

        /// <summary>
        /// Get invoice by invoice number
        /// </summary>
        Task<Invoice?> GetByInvoiceNoAsync(string invoiceNo);

        /// <summary>
        /// Get invoices by customer ID
        /// </summary>
        Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get invoices by hotel ID
        /// </summary>
        Task<IEnumerable<Invoice>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get invoices by reservation ID
        /// </summary>
        Task<IEnumerable<Invoice>> GetByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get invoices by payment status
        /// </summary>
        Task<IEnumerable<Invoice>> GetByPaymentStatusAsync(string paymentStatus);

        /// <summary>
        /// Get invoices by date range
        /// </summary>
        Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get invoices by invoice type
        /// </summary>
        Task<IEnumerable<Invoice>> GetByInvoiceTypeAsync(string invoiceType);

        /// <summary>
        /// Get invoices by customer name (search)
        /// </summary>
        Task<IEnumerable<Invoice>> GetByCustomerNameAsync(string customerName);

        /// <summary>
        /// Get invoices by hotel name (search)
        /// </summary>
        Task<IEnumerable<Invoice>> GetByHotelNameAsync(string hotelName);

        /// <summary>
        /// Get invoices by invoice number (search)
        /// </summary>
        Task<IEnumerable<Invoice>> GetByInvoiceNoSearchAsync(string invoiceNo);

        /// <summary>
        /// Check if invoice number exists
        /// </summary>
        Task<bool> InvoiceNoExistsAsync(string invoiceNo, int? excludeId = null);

        /// <summary>
        /// Get invoice statistics
        /// </summary>
        Task<object> GetStatisticsAsync();

        /// <summary>
        /// Get invoices with full details (includes all navigation properties)
        /// </summary>
        Task<Invoice?> GetWithDetailsAsync(int id);

        /// <summary>
        /// Get invoices with full details by invoice number
        /// </summary>
        Task<Invoice?> GetWithDetailsByInvoiceNoAsync(string invoiceNo);

        /// <summary>
        /// Get unpaid invoices
        /// </summary>
        Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync();

        /// <summary>
        /// Get overdue invoices
        /// </summary>
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();

        /// <summary>
        /// Get invoices by ZATCA status
        /// </summary>
        Task<IEnumerable<Invoice>> GetByZatcaStatusAsync(bool isSentZatca);

        /// <summary>
        /// Get invoices by period range
        /// </summary>
        Task<IEnumerable<Invoice>> GetByPeriodRangeAsync(DateTime startDate, DateTime endDate);
    }
}
