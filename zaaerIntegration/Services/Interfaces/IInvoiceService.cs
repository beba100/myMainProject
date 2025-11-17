using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for Invoice service operations
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Get all invoices with pagination and search
        /// </summary>
        Task<(IEnumerable<InvoiceResponseDto> Invoices, int TotalCount)> GetAllInvoicesAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null);

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int id);

        /// <summary>
        /// Get invoice by invoice number
        /// </summary>
        Task<InvoiceResponseDto?> GetInvoiceByNoAsync(string invoiceNo);

        /// <summary>
        /// Create new invoice
        /// </summary>
        Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto);

        /// <summary>
        /// Update existing invoice
        /// </summary>
        Task<InvoiceResponseDto?> UpdateInvoiceAsync(int id, UpdateInvoiceDto updateInvoiceDto);

        /// <summary>
        /// Delete invoice
        /// </summary>
        Task<bool> DeleteInvoiceAsync(int id);

        /// <summary>
        /// Get invoices by customer ID
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get invoices by hotel ID
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get invoices by reservation ID
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get invoices by payment status
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByPaymentStatusAsync(string paymentStatus);

        /// <summary>
        /// Get invoices by date range
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get invoices by invoice type
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByTypeAsync(string invoiceType);

        /// <summary>
        /// Search invoices by customer name
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> SearchInvoicesByCustomerNameAsync(string customerName);

        /// <summary>
        /// Search invoices by hotel name
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> SearchInvoicesByHotelNameAsync(string hotelName);

        /// <summary>
        /// Search invoices by invoice number
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> SearchInvoicesByNoAsync(string invoiceNo);

        /// <summary>
        /// Get invoice statistics
        /// </summary>
        Task<object> GetInvoiceStatisticsAsync();

        /// <summary>
        /// Check if invoice number exists
        /// </summary>
        Task<bool> InvoiceNoExistsAsync(string invoiceNo, int? excludeId = null);

        /// <summary>
        /// Get unpaid invoices
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> GetUnpaidInvoicesAsync();

        /// <summary>
        /// Get overdue invoices
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> GetOverdueInvoicesAsync();

        /// <summary>
        /// Get invoices by ZATCA status
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByZatcaStatusAsync(bool isSentZatca);

        /// <summary>
        /// Get invoices by period range
        /// </summary>
        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByPeriodRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Update payment status
        /// </summary>
        Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus);

        /// <summary>
        /// Update payment amount
        /// </summary>
        Task<bool> UpdatePaymentAmountAsync(int id, decimal amountPaid);

        /// <summary>
        /// Mark invoice as sent to ZATCA
        /// </summary>
        Task<bool> MarkAsSentToZatcaAsync(int id, string zatcaUuid);

        /// <summary>
        /// Calculate invoice totals
        /// </summary>
        Task<bool> CalculateInvoiceTotalsAsync(int id);
    }
}
