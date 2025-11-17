using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service implementation for PaymentReceipt operations
    /// </summary>
    public class PaymentReceiptService : IPaymentReceiptService
    {
        private readonly IPaymentReceiptRepository _paymentReceiptRepository;
        private readonly IMapper _mapper;

        public PaymentReceiptService(IPaymentReceiptRepository paymentReceiptRepository, IMapper mapper)
        {
            _paymentReceiptRepository = paymentReceiptRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<PaymentReceiptResponseDto> PaymentReceipts, int TotalCount)> GetAllPaymentReceiptsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null)
        {
            try
            {
                System.Linq.Expressions.Expression<Func<PaymentReceipt, bool>>? filter = null;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filter = pr => pr.ReceiptNo.Contains(searchTerm) || 
                                 (pr.TransactionNo != null && pr.TransactionNo.Contains(searchTerm)) ||
                                 (pr.Notes != null && pr.Notes.Contains(searchTerm));
                }

                var (paymentReceipts, totalCount) = await _paymentReceiptRepository.GetPagedAsync(
                    pageNumber, 
                    pageSize, 
                    filter);

                var paymentReceiptDtos = _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
                return (paymentReceiptDtos, totalCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts: {ex.Message}", ex);
            }
        }

        public async Task<PaymentReceiptResponseDto?> GetPaymentReceiptByIdAsync(int id)
        {
            try
            {
                var paymentReceipt = await _paymentReceiptRepository.GetWithDetailsAsync(id);
                return paymentReceipt != null ? _mapper.Map<PaymentReceiptResponseDto>(paymentReceipt) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipt with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<PaymentReceiptResponseDto?> GetPaymentReceiptByNoAsync(string receiptNo)
        {
            try
            {
                var paymentReceipt = await _paymentReceiptRepository.GetWithDetailsByReceiptNoAsync(receiptNo);
                return paymentReceipt != null ? _mapper.Map<PaymentReceiptResponseDto>(paymentReceipt) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipt with number {receiptNo}: {ex.Message}", ex);
            }
        }

        public async Task<PaymentReceiptResponseDto> CreatePaymentReceiptAsync(CreatePaymentReceiptDto createPaymentReceiptDto)
        {
            try
            {
                // Check if receipt number already exists
                if (await _paymentReceiptRepository.ReceiptNoExistsAsync(createPaymentReceiptDto.ReceiptNo))
                {
                    throw new InvalidOperationException($"Payment receipt with number '{createPaymentReceiptDto.ReceiptNo}' already exists.");
                }

                var paymentReceipt = _mapper.Map<PaymentReceipt>(createPaymentReceiptDto);
                paymentReceipt.ReceiptDate = createPaymentReceiptDto.ReceiptDate ?? DateTime.Now;
                paymentReceipt.CreatedAt = KsaTime.Now;

                var createdPaymentReceipt = await _paymentReceiptRepository.AddAsync(paymentReceipt);
                return _mapper.Map<PaymentReceiptResponseDto>(createdPaymentReceipt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating payment receipt: {ex.Message}", ex);
            }
        }

        public async Task<PaymentReceiptResponseDto?> UpdatePaymentReceiptAsync(int id, UpdatePaymentReceiptDto updatePaymentReceiptDto)
        {
            try
            {
                var existingPaymentReceipt = await _paymentReceiptRepository.GetByIdAsync(id);
                if (existingPaymentReceipt == null)
                {
                    return null;
                }

                // Check if receipt number already exists (excluding current receipt)
                if (await _paymentReceiptRepository.ReceiptNoExistsAsync(updatePaymentReceiptDto.ReceiptNo, id))
                {
                    throw new InvalidOperationException($"Payment receipt with number '{updatePaymentReceiptDto.ReceiptNo}' already exists.");
                }

                _mapper.Map(updatePaymentReceiptDto, existingPaymentReceipt);

                await _paymentReceiptRepository.UpdateAsync(existingPaymentReceipt);

                return _mapper.Map<PaymentReceiptResponseDto>(existingPaymentReceipt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating payment receipt with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeletePaymentReceiptAsync(int id)
        {
            try
            {
                var paymentReceipt = await _paymentReceiptRepository.GetByIdAsync(id);
                if (paymentReceipt == null)
                {
                    return false;
                }

                await _paymentReceiptRepository.DeleteAsync(paymentReceipt);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting payment receipt with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByCustomerIdAsync(int customerId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByCustomerIdAsync(customerId);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts for customer {customerId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByHotelIdAsync(int hotelId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByHotelIdAsync(hotelId);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByReservationIdAsync(int reservationId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByReservationIdAsync(reservationId);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts for reservation {reservationId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByInvoiceIdAsync(int invoiceId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByInvoiceIdAsync(invoiceId);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts for invoice {invoiceId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByTypeAsync(string receiptType)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByReceiptTypeAsync(receiptType);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts by type {receiptType}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByPaymentMethodAsync(string paymentMethod)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByPaymentMethodAsync(paymentMethod);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts by payment method {paymentMethod}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts by date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByAmountRangeAsync(minAmount, maxAmount);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts by amount range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> SearchPaymentReceiptsByCustomerNameAsync(string customerName)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByCustomerNameAsync(customerName);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching payment receipts by customer name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> SearchPaymentReceiptsByHotelNameAsync(string hotelName)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByHotelNameAsync(hotelName);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching payment receipts by hotel name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> SearchPaymentReceiptsByNoAsync(string receiptNo)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByReceiptNoSearchAsync(receiptNo);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching payment receipts by number: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> SearchPaymentReceiptsByTransactionNoAsync(string transactionNo)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByTransactionNoAsync(transactionNo);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching payment receipts by transaction number: {ex.Message}", ex);
            }
        }

        public async Task<object> GetPaymentReceiptStatisticsAsync()
        {
            try
            {
                return await _paymentReceiptRepository.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipt statistics: {ex.Message}", ex);
            }
        }

        public async Task<bool> ReceiptNoExistsAsync(string receiptNo, int? excludeId = null)
        {
            try
            {
                return await _paymentReceiptRepository.ReceiptNoExistsAsync(receiptNo, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking receipt number existence: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByBankIdAsync(int bankId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByBankIdAsync(bankId);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts for bank {bankId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByPaymentMethodIdAsync(int paymentMethodId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByPaymentMethodIdAsync(paymentMethodId);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts for payment method {paymentMethodId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByCreatedByAsync(int createdBy)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByCreatedByAsync(createdBy);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts created by user {createdBy}: {ex.Message}", ex);
            }
        }


        public async Task<decimal> GetTotalAmountByCustomerAsync(int customerId)
        {
            try
            {
                return await _paymentReceiptRepository.GetTotalAmountByCustomerAsync(customerId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving total amount for customer {customerId}: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetTotalAmountByHotelAsync(int hotelId)
        {
            try
            {
                return await _paymentReceiptRepository.GetTotalAmountByHotelAsync(hotelId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving total amount for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PaymentReceiptResponseDto>> GetPaymentReceiptsByPeriodRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptRepository.GetByPeriodRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<PaymentReceiptResponseDto>>(paymentReceipts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment receipts by period range: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdatePaymentAmountAsync(int id, decimal amountPaid)
        {
            try
            {
                var paymentReceipt = await _paymentReceiptRepository.GetByIdAsync(id);
                if (paymentReceipt == null)
                {
                    return false;
                }

                paymentReceipt.AmountPaid = amountPaid;
                await _paymentReceiptRepository.UpdateAsync(paymentReceipt);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating payment amount: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateTransactionNumberAsync(int id, string transactionNo)
        {
            try
            {
                var paymentReceipt = await _paymentReceiptRepository.GetByIdAsync(id);
                if (paymentReceipt == null)
                {
                    return false;
                }

                paymentReceipt.TransactionNo = transactionNo;
                await _paymentReceiptRepository.UpdateAsync(paymentReceipt);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating transaction number: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateNotesAsync(int id, string notes)
        {
            try
            {
                var paymentReceipt = await _paymentReceiptRepository.GetByIdAsync(id);
                if (paymentReceipt == null)
                {
                    return false;
                }

                paymentReceipt.Notes = notes;
                await _paymentReceiptRepository.UpdateAsync(paymentReceipt);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating notes: {ex.Message}", ex);
            }
        }
    }
}
