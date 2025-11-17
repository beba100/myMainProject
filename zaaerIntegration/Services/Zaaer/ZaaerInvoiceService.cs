using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
    /// <summary>
    /// Service interface for Zaaer invoice integration
    /// </summary>
    public interface IZaaerInvoiceService
    {
        Task<ZaaerInvoiceResponseDto> CreateInvoiceAsync(ZaaerCreateInvoiceDto createInvoiceDto);
        Task<ZaaerInvoiceResponseDto?> UpdateInvoiceAsync(int invoiceId, ZaaerUpdateInvoiceDto updateInvoiceDto);
        Task<ZaaerInvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId);
        Task<IEnumerable<ZaaerInvoiceResponseDto>> GetInvoicesByHotelIdAsync(int hotelId);
        Task<bool> DeleteInvoiceAsync(int invoiceId);
    }

    /// <summary>
    /// Service implementation for Zaaer invoice integration
    /// </summary>
    public class ZaaerInvoiceService : IZaaerInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ZaaerInvoiceService(IUnitOfWork unitOfWork, IInvoiceRepository invoiceRepository, ApplicationDbContext context, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _invoiceRepository = invoiceRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ZaaerInvoiceResponseDto> CreateInvoiceAsync(ZaaerCreateInvoiceDto createInvoiceDto)
        {
            // Idempotent behavior: if an invoice with same ZaaerId or InvoiceNo exists, update it instead of creating duplicates
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i =>
                    (createInvoiceDto.ZaaerId.HasValue && i.ZaaerId == createInvoiceDto.ZaaerId) ||
                    i.InvoiceNo == createInvoiceDto.InvoiceNo);

            if (existingInvoice != null)
            {
                _mapper.Map(createInvoiceDto, existingInvoice);
                await _invoiceRepository.UpdateAsync(existingInvoice);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<ZaaerInvoiceResponseDto>(existingInvoice);
            }

            var invoice = _mapper.Map<Invoice>(createInvoiceDto);
            invoice.CreatedAt = KsaTime.Now;

            var createdInvoice = await _invoiceRepository.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ZaaerInvoiceResponseDto>(createdInvoice);
        }

        public async Task<ZaaerInvoiceResponseDto?> UpdateInvoiceAsync(int invoiceId, ZaaerUpdateInvoiceDto updateInvoiceDto)
        {
            var existingInvoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (existingInvoice == null)
            {
                return null;
            }

            _mapper.Map(updateInvoiceDto, existingInvoice);
            await _invoiceRepository.UpdateAsync(existingInvoice);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ZaaerInvoiceResponseDto>(existingInvoice);
        }

        public async Task<ZaaerInvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
            {
                return null;
            }

            var responseDto = _mapper.Map<ZaaerInvoiceResponseDto>(invoice);
            
            // جلب سندات القبض المرتبطة
            var paymentReceipts = await _context.PaymentReceipts
                .Where(pr => pr.InvoiceId == invoiceId)
                .ToListAsync();
            responseDto.PaymentReceipts = _mapper.Map<List<ZaaerPaymentReceiptResponseDto>>(paymentReceipts);

            // جلب الاستردادات المرتبطة
            var refunds = await _context.Refunds
                .Where(r => r.InvoiceId == invoiceId)
                .ToListAsync();
            responseDto.Refunds = _mapper.Map<List<ZaaerRefundResponseDto>>(refunds);

            // جلب الإشعارات الدائنة المرتبطة
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.InvoiceId == invoiceId)
                .ToListAsync();
            responseDto.CreditNotes = _mapper.Map<List<ZaaerCreditNoteResponseDto>>(creditNotes);

            return responseDto;
        }

        public async Task<IEnumerable<ZaaerInvoiceResponseDto>> GetInvoicesByHotelIdAsync(int hotelId)
        {
            var invoices = await _invoiceRepository.GetByHotelIdAsync(hotelId);
            var responseDtos = new List<ZaaerInvoiceResponseDto>();

            foreach (var invoice in invoices)
            {
                var responseDto = _mapper.Map<ZaaerInvoiceResponseDto>(invoice);
                
                // جلب سندات القبض المرتبطة
                var paymentReceipts = await _context.PaymentReceipts
                    .Where(pr => pr.InvoiceId == invoice.InvoiceId)
                    .ToListAsync();
                responseDto.PaymentReceipts = _mapper.Map<List<ZaaerPaymentReceiptResponseDto>>(paymentReceipts);

                // جلب الاستردادات المرتبطة
                var refunds = await _context.Refunds
                    .Where(r => r.InvoiceId == invoice.InvoiceId)
                    .ToListAsync();
                responseDto.Refunds = _mapper.Map<List<ZaaerRefundResponseDto>>(refunds);

                // جلب الإشعارات الدائنة المرتبطة
                var creditNotes = await _context.CreditNotes
                    .Where(cn => cn.InvoiceId == invoice.InvoiceId)
                    .ToListAsync();
                responseDto.CreditNotes = _mapper.Map<List<ZaaerCreditNoteResponseDto>>(creditNotes);

                responseDtos.Add(responseDto);
            }

            return responseDtos;
        }

        public async Task<bool> DeleteInvoiceAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
            {
                return false;
            }

            await _invoiceRepository.DeleteAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
