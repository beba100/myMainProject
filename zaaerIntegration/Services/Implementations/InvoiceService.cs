using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service implementation for Invoice operations
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMapper _mapper;

        public InvoiceService(IInvoiceRepository invoiceRepository, IMapper mapper)
        {
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<InvoiceResponseDto> Invoices, int TotalCount)> GetAllInvoicesAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null)
        {
            try
            {
                System.Linq.Expressions.Expression<Func<FinanceLedgerAPI.Models.Invoice, bool>>? filter = null;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filter = i => i.InvoiceNo.Contains(searchTerm) || 
                                 (i.Notes != null && i.Notes.Contains(searchTerm));
                }

                var (invoices, totalCount) = await _invoiceRepository.GetPagedAsync(
                    pageNumber, 
                    pageSize, 
                    filter);

                var invoiceDtos = _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
                return (invoiceDtos, totalCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoices: {ex.Message}", ex);
            }
        }

        public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int id)
        {
            try
            {
                var invoice = await _invoiceRepository.GetWithDetailsAsync(id);
                return invoice != null ? _mapper.Map<InvoiceResponseDto>(invoice) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoice with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<InvoiceResponseDto?> GetInvoiceByNoAsync(string invoiceNo)
        {
            try
            {
                var invoice = await _invoiceRepository.GetWithDetailsByInvoiceNoAsync(invoiceNo);
                return invoice != null ? _mapper.Map<InvoiceResponseDto>(invoice) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoice with number {invoiceNo}: {ex.Message}", ex);
            }
        }

        public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto)
        {
            try
            {
                // Check if invoice number already exists
                if (await _invoiceRepository.InvoiceNoExistsAsync(createInvoiceDto.InvoiceNo))
                {
                    throw new InvalidOperationException($"Invoice with number '{createInvoiceDto.InvoiceNo}' already exists.");
                }

                var invoice = _mapper.Map<Invoice>(createInvoiceDto);
                invoice.InvoiceDate = createInvoiceDto.InvoiceDate ?? DateTime.Now;
                invoice.CreatedAt = KsaTime.Now;

                // Calculate amount remaining if not provided
                if (invoice.TotalAmount.HasValue && invoice.AmountPaid > 0)
                {
                    invoice.AmountRemaining = invoice.TotalAmount.Value - invoice.AmountPaid;
                }

                var createdInvoice = await _invoiceRepository.AddAsync(invoice);
                return _mapper.Map<InvoiceResponseDto>(createdInvoice);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating invoice: {ex.Message}", ex);
            }
        }

        public async Task<InvoiceResponseDto?> UpdateInvoiceAsync(int id, UpdateInvoiceDto updateInvoiceDto)
        {
            try
            {
                var existingInvoice = await _invoiceRepository.GetByIdAsync(id);
                if (existingInvoice == null)
                {
                    return null;
                }

                // Check if invoice number already exists (excluding current invoice)
                if (await _invoiceRepository.InvoiceNoExistsAsync(updateInvoiceDto.InvoiceNo, id))
                {
                    throw new InvalidOperationException($"Invoice with number '{updateInvoiceDto.InvoiceNo}' already exists.");
                }

                _mapper.Map(updateInvoiceDto, existingInvoice);

                // Recalculate amount remaining
                if (existingInvoice.TotalAmount.HasValue && existingInvoice.AmountPaid > 0)
                {
                    existingInvoice.AmountRemaining = existingInvoice.TotalAmount.Value - existingInvoice.AmountPaid;
                }

                await _invoiceRepository.UpdateAsync(existingInvoice);

                return _mapper.Map<InvoiceResponseDto>(existingInvoice);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating invoice with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id);
                if (invoice == null)
                {
                    return false;
                }

                await _invoiceRepository.DeleteAsync(invoice);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting invoice with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByCustomerIdAsync(int customerId)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByCustomerIdAsync(customerId);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoices for customer {customerId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByHotelIdAsync(int hotelId)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByHotelIdAsync(hotelId);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoices for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByReservationIdAsync(int reservationId)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByReservationIdAsync(reservationId);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoices for reservation {reservationId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByPaymentStatusAsync(string paymentStatus)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByPaymentStatusAsync(paymentStatus);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoices with payment status {paymentStatus}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoices by date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByTypeAsync(string invoiceType)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByInvoiceTypeAsync(invoiceType);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoices by type {invoiceType}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> SearchInvoicesByCustomerNameAsync(string customerName)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByCustomerNameAsync(customerName);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching invoices by customer name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> SearchInvoicesByHotelNameAsync(string hotelName)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByHotelNameAsync(hotelName);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching invoices by hotel name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> SearchInvoicesByNoAsync(string invoiceNo)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByInvoiceNoSearchAsync(invoiceNo);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching invoices by number: {ex.Message}", ex);
            }
        }

        public async Task<object> GetInvoiceStatisticsAsync()
        {
            try
            {
                return await _invoiceRepository.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoice statistics: {ex.Message}", ex);
            }
        }

        public async Task<bool> InvoiceNoExistsAsync(string invoiceNo, int? excludeId = null)
        {
            try
            {
                return await _invoiceRepository.InvoiceNoExistsAsync(invoiceNo, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking invoice number existence: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetUnpaidInvoicesAsync()
        {
            try
            {
                var invoices = await _invoiceRepository.GetUnpaidInvoicesAsync();
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving unpaid invoices: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetOverdueInvoicesAsync()
        {
            try
            {
                var invoices = await _invoiceRepository.GetOverdueInvoicesAsync();
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving overdue invoices: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByZatcaStatusAsync(bool isSentZatca)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByZatcaStatusAsync(isSentZatca);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoices by ZATCA status: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByPeriodRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var invoices = await _invoiceRepository.GetByPeriodRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoices by period range: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id);
                if (invoice == null)
                {
                    return false;
                }

                invoice.PaymentStatus = paymentStatus;
                await _invoiceRepository.UpdateAsync(invoice);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating payment status: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdatePaymentAmountAsync(int id, decimal amountPaid)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id);
                if (invoice == null)
                {
                    return false;
                }

                invoice.AmountPaid = amountPaid;
                if (invoice.TotalAmount.HasValue)
                {
                    invoice.AmountRemaining = invoice.TotalAmount.Value - amountPaid;
                    
                    // Update payment status based on amounts
                    if (amountPaid >= invoice.TotalAmount.Value)
                    {
                        invoice.PaymentStatus = "paid";
                    }
                    else if (amountPaid > 0)
                    {
                        invoice.PaymentStatus = "partial";
                    }
                    else
                    {
                        invoice.PaymentStatus = "unpaid";
                    }
                }

                await _invoiceRepository.UpdateAsync(invoice);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating payment amount: {ex.Message}", ex);
            }
        }

        public async Task<bool> MarkAsSentToZatcaAsync(int id, string zatcaUuid)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id);
                if (invoice == null)
                {
                    return false;
                }

                invoice.IsSentZatca = true;
                invoice.ZatcaUuid = zatcaUuid;
                await _invoiceRepository.UpdateAsync(invoice);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error marking invoice as sent to ZATCA: {ex.Message}", ex);
            }
        }

        public async Task<bool> CalculateInvoiceTotalsAsync(int id)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id);
                if (invoice == null)
                {
                    return false;
                }

                // Calculate VAT amount if rate and subtotal are provided
                if (invoice.VatRate.HasValue && invoice.Subtotal.HasValue)
                {
                    invoice.VatAmount = invoice.Subtotal.Value * (invoice.VatRate.Value / 100);
                }

                // Calculate lodging tax amount if rate and subtotal are provided
                if (invoice.LodgingTaxRate.HasValue && invoice.Subtotal.HasValue)
                {
                    invoice.LodgingTaxAmount = invoice.Subtotal.Value * (invoice.LodgingTaxRate.Value / 100);
                }

                // Calculate total amount
                if (invoice.Subtotal.HasValue)
                {
                    var total = invoice.Subtotal.Value;
                    if (invoice.VatAmount.HasValue) total += invoice.VatAmount.Value;
                    if (invoice.LodgingTaxAmount.HasValue) total += invoice.LodgingTaxAmount.Value;
                    invoice.TotalAmount = total;
                }

                // Recalculate amount remaining
                if (invoice.TotalAmount.HasValue)
                {
                    invoice.AmountRemaining = invoice.TotalAmount.Value - invoice.AmountPaid;
                }

                await _invoiceRepository.UpdateAsync(invoice);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error calculating invoice totals: {ex.Message}", ex);
            }
        }
    }
}
