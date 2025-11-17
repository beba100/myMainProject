using AutoMapper;
using FinanceLedgerAPI.Models;
using System.Linq;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
    /// <summary>
    /// Interface for Zaaer Payment Receipt Service
    /// </summary>
    public interface IZaaerPaymentReceiptService
    {
        Task<ZaaerPaymentReceiptResponseDto> CreatePaymentReceiptAsync(ZaaerCreatePaymentReceiptDto createPaymentReceiptDto);
        Task<ZaaerPaymentReceiptResponseDto?> UpdatePaymentReceiptAsync(int receiptId, ZaaerUpdatePaymentReceiptDto updatePaymentReceiptDto);
        Task<ZaaerPaymentReceiptResponseDto?> UpdatePaymentReceiptByReceiptNoAsync(string receiptNo, ZaaerUpdatePaymentReceiptDto updatePaymentReceiptDto);
        Task<ZaaerPaymentReceiptResponseDto?> UpdatePaymentReceiptByZaaerIdAsync(int zaaerId, ZaaerUpdatePaymentReceiptDto updatePaymentReceiptDto);
        Task<ZaaerPaymentReceiptResponseDto?> GetPaymentReceiptByIdAsync(int receiptId);
        Task<ZaaerPaymentReceiptResponseDto?> GetPaymentReceiptByReceiptNoAsync(string receiptNo);
        Task<IEnumerable<ZaaerPaymentReceiptResponseDto>> GetPaymentReceiptsByHotelIdAsync(int hotelId);
        Task<bool> DeletePaymentReceiptAsync(int receiptId);
        Task<bool> DeletePaymentReceiptByReceiptNoAsync(string receiptNo);
        Task<bool> DeletePaymentReceiptByZaaerIdAsync(int zaaerId);
    }

    /// <summary>
    /// Service for Zaaer Payment Receipt integration
    /// </summary>
    public class ZaaerPaymentReceiptService : IZaaerPaymentReceiptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentReceiptRepository _paymentReceiptRepository;
		private readonly ICustomerLedgerService _customerLedgerService;
        private readonly IMapper _mapper;

        public ZaaerPaymentReceiptService(
            IUnitOfWork unitOfWork,
            IPaymentReceiptRepository paymentReceiptRepository,
			ICustomerLedgerService customerLedgerService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _paymentReceiptRepository = paymentReceiptRepository;
			_customerLedgerService = customerLedgerService;
            _mapper = mapper;
        }

        public async Task<ZaaerPaymentReceiptResponseDto> CreatePaymentReceiptAsync(ZaaerCreatePaymentReceiptDto createPaymentReceiptDto)
        {
			// Guard: if a receipt with same ZaaerId or ReceiptNo already exists treat as update (webhook retries)
			if (createPaymentReceiptDto.ZaaerId.HasValue || !string.IsNullOrWhiteSpace(createPaymentReceiptDto.ReceiptNo))
			{
				var existing = await _paymentReceiptRepository.FindAsync(pr =>
					(createPaymentReceiptDto.ZaaerId.HasValue && pr.ZaaerId == createPaymentReceiptDto.ZaaerId) ||
					pr.ReceiptNo == createPaymentReceiptDto.ReceiptNo);
				var existingReceipt = existing.FirstOrDefault();
				if (existingReceipt != null)
				{
					var updateDto = new ZaaerUpdatePaymentReceiptDto
					{
						ZaaerId = createPaymentReceiptDto.ZaaerId,
						ReceiptNo = createPaymentReceiptDto.ReceiptNo,
						HotelId = createPaymentReceiptDto.HotelId,
						ReservationId = createPaymentReceiptDto.ReservationId,
						InvoiceId = createPaymentReceiptDto.InvoiceId,
						UnitId = createPaymentReceiptDto.UnitId,
						CustomerId = createPaymentReceiptDto.CustomerId,
						ReceiptDate = createPaymentReceiptDto.ReceiptDate,
						AmountPaid = createPaymentReceiptDto.AmountPaid,
						PaymentMethod = createPaymentReceiptDto.PaymentMethod,
						PaymentMethodId = createPaymentReceiptDto.PaymentMethodId,
						BankId = createPaymentReceiptDto.BankId,
						TransactionNo = createPaymentReceiptDto.TransactionNo,
						Notes = createPaymentReceiptDto.Notes,
						ReceiptStatus = createPaymentReceiptDto.ReceiptStatus,
						ReceiptType = createPaymentReceiptDto.ReceiptType,
						VoucherCode = createPaymentReceiptDto.VoucherCode,
						CreatedBy = createPaymentReceiptDto.CreatedBy
					};
					return (await UpdatePaymentReceiptAsync(existingReceipt.ReceiptId, updateDto))!;
				}
			}

            // Check if this is an expense/deposit receipt by voucherCode or receiptType
            // Both "expense" and "deposit" voucherCodes allow null CustomerId
            bool isExpenseOrDepositReceipt = (!string.IsNullOrEmpty(createPaymentReceiptDto.VoucherCode) && 
                                             (string.Equals(createPaymentReceiptDto.VoucherCode, "expense", StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(createPaymentReceiptDto.VoucherCode, "deposit", StringComparison.OrdinalIgnoreCase))) ||
                                             string.Equals(createPaymentReceiptDto.ReceiptType, "expense", StringComparison.OrdinalIgnoreCase);
            
            if (!createPaymentReceiptDto.CustomerId.HasValue && !isExpenseOrDepositReceipt)
            {
                throw new ArgumentException("CustomerId is required when voucherCode is not 'expense' or 'deposit'");
            }

            var paymentReceipt = _mapper.Map<PaymentReceipt>(createPaymentReceiptDto);
            paymentReceipt.CreatedAt = KsaTime.Now;

            // If caller provided an initial status, honor it; otherwise default remains "active"
            if (!string.IsNullOrWhiteSpace(createPaymentReceiptDto.ReceiptStatus))
            {
                paymentReceipt.ReceiptStatus = createPaymentReceiptDto.ReceiptStatus!;
            }

            // Handle null CustomerId for expense/deposit receipts: set to 0 as default (database requires NOT NULL)
            if (isExpenseOrDepositReceipt && (!createPaymentReceiptDto.CustomerId.HasValue || createPaymentReceiptDto.CustomerId.Value == 0))
            {
                paymentReceipt.CustomerId = 0; // Default value for expense/deposit receipts without customer
            }
            else if (!createPaymentReceiptDto.CustomerId.HasValue)
            {
                throw new ArgumentException("CustomerId is required");
            }

            // Handle null ReservationId for expense receipts (already nullable, so no action needed)

            // ��� �� ����� InvoiceId� ���� �� ���� ��������
            if (createPaymentReceiptDto.InvoiceId.HasValue)
            {
                var invoiceRepository = _unitOfWork.Invoices;
                var invoice = await invoiceRepository.GetByIdAsync(createPaymentReceiptDto.InvoiceId.Value);
                if (invoice == null)
                {
                    throw new ArgumentException($"Invoice with ID {createPaymentReceiptDto.InvoiceId.Value} not found");
                }
            }

            var createdPaymentReceipt = await _paymentReceiptRepository.AddAsync(paymentReceipt);
            await _unitOfWork.SaveChangesAsync();

			await _customerLedgerService.SyncReceiptAsync(createdPaymentReceipt);

            return _mapper.Map<ZaaerPaymentReceiptResponseDto>(createdPaymentReceipt);
        }

        public async Task<ZaaerPaymentReceiptResponseDto?> UpdatePaymentReceiptAsync(int receiptId, ZaaerUpdatePaymentReceiptDto updatePaymentReceiptDto)
        {
            var existingPaymentReceipt = await _paymentReceiptRepository.GetByIdAsync(receiptId);
            if (existingPaymentReceipt == null)
            {
                return null;
            }

            // Determine if this is an expense/deposit receipt by voucherCode or receiptType
            // Both "expense" and "deposit" voucherCodes allow null CustomerId
            string receiptType = updatePaymentReceiptDto.ReceiptType ?? existingPaymentReceipt.ReceiptType;
            string? voucherCode = updatePaymentReceiptDto.VoucherCode ?? existingPaymentReceipt.VoucherCode;
            bool isExpenseOrDepositReceipt = (!string.IsNullOrEmpty(voucherCode) && 
                                             (string.Equals(voucherCode, "expense", StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(voucherCode, "deposit", StringComparison.OrdinalIgnoreCase))) ||
                                             string.Equals(receiptType, "expense", StringComparison.OrdinalIgnoreCase);

            // Validate CustomerId: Required unless voucherCode is "expense" or "deposit"
            if (updatePaymentReceiptDto.CustomerId.HasValue == false && !isExpenseOrDepositReceipt)
            {
                // If CustomerId is null and not expense/deposit, check if existing receipt has a valid CustomerId
                if (existingPaymentReceipt.CustomerId == 0)
                {
                    throw new ArgumentException("CustomerId is required when voucherCode is not 'expense' or 'deposit'");
                }
            }

            // ��� �� ����� InvoiceId� ���� �� ���� ��������
            if (updatePaymentReceiptDto.InvoiceId.HasValue)
            {
                var invoiceRepository = _unitOfWork.Invoices;
                var invoice = await invoiceRepository.GetByIdAsync(updatePaymentReceiptDto.InvoiceId.Value);
                if (invoice == null)
                {
                    throw new ArgumentException($"Invoice with ID {updatePaymentReceiptDto.InvoiceId.Value} not found");
                }
            }

            var prevStatus = existingPaymentReceipt.ReceiptStatus;
            _mapper.Map(updatePaymentReceiptDto, existingPaymentReceipt);
            // Preserve status if not provided in DTO
            if (updatePaymentReceiptDto.ReceiptStatus == null)
            {
                existingPaymentReceipt.ReceiptStatus = prevStatus;
            }

            // Handle null CustomerId for expense/deposit receipts: set to 0 as default (database requires NOT NULL)
            if (isExpenseOrDepositReceipt && (!updatePaymentReceiptDto.CustomerId.HasValue || updatePaymentReceiptDto.CustomerId.Value == 0))
            {
                existingPaymentReceipt.CustomerId = 0; // Default value for expense/deposit receipts without customer
            }

            await _paymentReceiptRepository.UpdateAsync(existingPaymentReceipt);
            await _unitOfWork.SaveChangesAsync();

			await _customerLedgerService.SyncReceiptAsync(existingPaymentReceipt);

            return _mapper.Map<ZaaerPaymentReceiptResponseDto>(existingPaymentReceipt);
        }

        public async Task<ZaaerPaymentReceiptResponseDto?> UpdatePaymentReceiptByReceiptNoAsync(string receiptNo, ZaaerUpdatePaymentReceiptDto updatePaymentReceiptDto)
        {
            var existingPaymentReceipts = await _paymentReceiptRepository.FindAsync(pr => pr.ReceiptNo == receiptNo);
            var paymentReceipt = existingPaymentReceipts.FirstOrDefault();
            
            if (paymentReceipt == null)
            {
                return null;
            }

            // Determine if this is an expense/deposit receipt by voucherCode or receiptType
            // Both "expense" and "deposit" voucherCodes allow null CustomerId
            string receiptType = updatePaymentReceiptDto.ReceiptType ?? paymentReceipt.ReceiptType;
            string? voucherCode = updatePaymentReceiptDto.VoucherCode ?? paymentReceipt.VoucherCode;
            bool isExpenseOrDepositReceipt = (!string.IsNullOrEmpty(voucherCode) && 
                                             (string.Equals(voucherCode, "expense", StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(voucherCode, "deposit", StringComparison.OrdinalIgnoreCase))) ||
                                             string.Equals(receiptType, "expense", StringComparison.OrdinalIgnoreCase);

            // Validate CustomerId: Required unless voucherCode is "expense" or "deposit"
            if (updatePaymentReceiptDto.CustomerId.HasValue == false && !isExpenseOrDepositReceipt)
            {
                // If CustomerId is null and not expense/deposit, check if existing receipt has a valid CustomerId
                if (paymentReceipt.CustomerId == 0)
                {
                    throw new ArgumentException("CustomerId is required when voucherCode is not 'expense' or 'deposit'");
                }
            }

            // ��� �� ����� InvoiceId� ���� �� ���� ��������
            if (updatePaymentReceiptDto.InvoiceId.HasValue)
            {
                var invoiceRepository = _unitOfWork.Invoices;
                var invoice = await invoiceRepository.GetByIdAsync(updatePaymentReceiptDto.InvoiceId.Value);
                if (invoice == null)
                {
                    throw new ArgumentException($"Invoice with ID {updatePaymentReceiptDto.InvoiceId.Value} not found");
                }
            }

            var prevStatus2 = paymentReceipt.ReceiptStatus;
            _mapper.Map(updatePaymentReceiptDto, paymentReceipt);
            if (updatePaymentReceiptDto.ReceiptStatus == null)
            {
                paymentReceipt.ReceiptStatus = prevStatus2;
            }

            // Handle null CustomerId for expense/deposit receipts: set to 0 as default (database requires NOT NULL)
            if (isExpenseOrDepositReceipt && (!updatePaymentReceiptDto.CustomerId.HasValue || updatePaymentReceiptDto.CustomerId.Value == 0))
            {
                paymentReceipt.CustomerId = 0; // Default value for expense/deposit receipts without customer
            }

            await _paymentReceiptRepository.UpdateAsync(paymentReceipt);
            await _unitOfWork.SaveChangesAsync();

			await _customerLedgerService.SyncReceiptAsync(paymentReceipt);

            return _mapper.Map<ZaaerPaymentReceiptResponseDto>(paymentReceipt);
        }

        public async Task<ZaaerPaymentReceiptResponseDto?> UpdatePaymentReceiptByZaaerIdAsync(int zaaerId, ZaaerUpdatePaymentReceiptDto updatePaymentReceiptDto)
        {
            var existingPaymentReceipts = await _paymentReceiptRepository.FindAsync(pr => pr.ZaaerId == zaaerId);
            
            // Filter by HotelId if provided (multi-tenancy)
            IEnumerable<PaymentReceipt> filtered = existingPaymentReceipts;
            if (updatePaymentReceiptDto.HotelId.HasValue)
            {
                filtered = existingPaymentReceipts.Where(pr => pr.HotelId == updatePaymentReceiptDto.HotelId.Value);
            }
            
            var paymentReceipt = filtered.FirstOrDefault();

            if (paymentReceipt == null)
            {
                return null;
            }

            // Determine if this is an expense/deposit receipt by voucherCode or receiptType
            // Both "expense" and "deposit" voucherCodes allow null CustomerId
            string receiptType = updatePaymentReceiptDto.ReceiptType ?? paymentReceipt.ReceiptType;
            string? voucherCode = updatePaymentReceiptDto.VoucherCode ?? paymentReceipt.VoucherCode;
            bool isExpenseOrDepositReceipt = (!string.IsNullOrEmpty(voucherCode) && 
                                             (string.Equals(voucherCode, "expense", StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(voucherCode, "deposit", StringComparison.OrdinalIgnoreCase))) ||
                                             string.Equals(receiptType, "expense", StringComparison.OrdinalIgnoreCase);

            // Validate CustomerId: Required unless voucherCode is "expense" or "deposit"
            if (updatePaymentReceiptDto.CustomerId.HasValue == false && !isExpenseOrDepositReceipt)
            {
                // If CustomerId is null and not expense/deposit, check if existing receipt has a valid CustomerId
                if (paymentReceipt.CustomerId == 0)
                {
                    throw new ArgumentException("CustomerId is required when voucherCode is not 'expense' or 'deposit'");
                }
            }

            if (updatePaymentReceiptDto.InvoiceId.HasValue)
            {
                var invoiceRepository = _unitOfWork.Invoices;
                var invoice = await invoiceRepository.GetByIdAsync(updatePaymentReceiptDto.InvoiceId.Value);
                if (invoice == null)
                {
                    throw new ArgumentException($"Invoice with ID {updatePaymentReceiptDto.InvoiceId.Value} not found");
                }
            }

            var prevStatus3 = paymentReceipt.ReceiptStatus;
            _mapper.Map(updatePaymentReceiptDto, paymentReceipt);
            if (updatePaymentReceiptDto.ReceiptStatus == null)
            {
                paymentReceipt.ReceiptStatus = prevStatus3;
            }
            
            // Explicitly set ZaaerId from DTO if provided
            if (updatePaymentReceiptDto.ZaaerId.HasValue)
            {
                paymentReceipt.ZaaerId = updatePaymentReceiptDto.ZaaerId.Value;
            }

            // Handle null CustomerId for expense/deposit receipts: set to 0 as default (database requires NOT NULL)
            if (isExpenseOrDepositReceipt && (!updatePaymentReceiptDto.CustomerId.HasValue || updatePaymentReceiptDto.CustomerId.Value == 0))
            {
                paymentReceipt.CustomerId = 0; // Default value for expense/deposit receipts without customer
            }
            
            await _paymentReceiptRepository.UpdateAsync(paymentReceipt);
            await _unitOfWork.SaveChangesAsync();

			await _customerLedgerService.SyncReceiptAsync(paymentReceipt);

            return _mapper.Map<ZaaerPaymentReceiptResponseDto>(paymentReceipt);
        }

        public async Task<ZaaerPaymentReceiptResponseDto?> GetPaymentReceiptByIdAsync(int receiptId)
        {
            var paymentReceipt = await _paymentReceiptRepository.GetByIdAsync(receiptId);
            if (paymentReceipt == null)
            {
                return null;
            }

            return _mapper.Map<ZaaerPaymentReceiptResponseDto>(paymentReceipt);
        }

        public async Task<ZaaerPaymentReceiptResponseDto?> GetPaymentReceiptByReceiptNoAsync(string receiptNo)
        {
            var existingPaymentReceipts = await _paymentReceiptRepository.FindAsync(pr => pr.ReceiptNo == receiptNo);
            var paymentReceipt = existingPaymentReceipts.FirstOrDefault();
            
            if (paymentReceipt == null)
            {
                return null;
            }

            return _mapper.Map<ZaaerPaymentReceiptResponseDto>(paymentReceipt);
        }

        public async Task<IEnumerable<ZaaerPaymentReceiptResponseDto>> GetPaymentReceiptsByHotelIdAsync(int hotelId)
        {
            var paymentReceipts = await _paymentReceiptRepository.FindAsync(pr => pr.HotelId == hotelId);
            return _mapper.Map<IEnumerable<ZaaerPaymentReceiptResponseDto>>(paymentReceipts);
        }

        public async Task<bool> DeletePaymentReceiptAsync(int receiptId)
        {
            var paymentReceipt = await _paymentReceiptRepository.GetByIdAsync(receiptId);
            if (paymentReceipt == null)
            {
                return false;
            }

            // Soft delete: mark as cancelled
            paymentReceipt.ReceiptStatus = "cancelled";
            await _paymentReceiptRepository.UpdateAsync(paymentReceipt);
            await _unitOfWork.SaveChangesAsync();
			await _customerLedgerService.CancelReceiptAsync(paymentReceipt);

            return true;
        }

        public async Task<bool> DeletePaymentReceiptByReceiptNoAsync(string receiptNo)
        {
            var existingPaymentReceipts = await _paymentReceiptRepository.FindAsync(pr => pr.ReceiptNo == receiptNo);
            var paymentReceipt = existingPaymentReceipts.FirstOrDefault();
            
            if (paymentReceipt == null)
            {
                return false;
            }

            // Soft delete: mark as cancelled
            paymentReceipt.ReceiptStatus = "cancelled";
            await _paymentReceiptRepository.UpdateAsync(paymentReceipt);
            await _unitOfWork.SaveChangesAsync();
			await _customerLedgerService.CancelReceiptAsync(paymentReceipt);

            return true;
        }

        public async Task<bool> DeletePaymentReceiptByZaaerIdAsync(int zaaerId)
        {
            var existingPaymentReceipts = await _paymentReceiptRepository.FindAsync(pr => pr.ZaaerId == zaaerId);
            var paymentReceipt = existingPaymentReceipts.FirstOrDefault();
            if (paymentReceipt == null)
            {
                return false;
            }

            paymentReceipt.ReceiptStatus = "cancelled";
            await _paymentReceiptRepository.UpdateAsync(paymentReceipt);
            await _unitOfWork.SaveChangesAsync();
			await _customerLedgerService.CancelReceiptAsync(paymentReceipt);
            return true;
        }
    }
}
