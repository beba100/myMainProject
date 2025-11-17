using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
    public interface IZaaerBankService
    {
        Task<ZaaerBankResponseDto> CreateBankAsync(ZaaerCreateBankDto dto);
        Task<ZaaerBankResponseDto?> UpdateBankAsync(int bankId, ZaaerUpdateBankDto dto);
        Task<IEnumerable<ZaaerBankResponseDto>> GetAllBanksAsync();
    }

    public class ZaaerBankService : IZaaerBankService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ZaaerBankService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ZaaerBankResponseDto> CreateBankAsync(ZaaerCreateBankDto dto)
        {
            var bank = new Bank
            {
                BankNameEn = dto.BankNameEn,
                BankNameAr = dto.BankNameAr,
                IsActive = dto.IsActive,
                AccountNumber = dto.AccountNumber,
                Iban = dto.Iban,
                CurrencyCode = dto.CurrencyCode,
                Description = dto.Description,
                CreatedAt = DateTime.Now
            };

            _context.Banks.Add(bank);
            await _context.SaveChangesAsync();

            return _mapper.Map<ZaaerBankResponseDto>(bank);
        }

        public async Task<ZaaerBankResponseDto?> UpdateBankAsync(int bankId, ZaaerUpdateBankDto dto)
        {
            var bank = await _context.Banks.FirstOrDefaultAsync(b => b.BankId == bankId);
            if (bank == null) return null;

            if (dto.BankNameEn != null) bank.BankNameEn = dto.BankNameEn;
            if (dto.BankNameAr != null) bank.BankNameAr = dto.BankNameAr;
            if (dto.IsActive.HasValue) bank.IsActive = dto.IsActive.Value;
            if (dto.AccountNumber != null) bank.AccountNumber = dto.AccountNumber;
            if (dto.Iban != null) bank.Iban = dto.Iban;
            if (dto.CurrencyCode != null) bank.CurrencyCode = dto.CurrencyCode;
            if (dto.Description != null) bank.Description = dto.Description;
            bank.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return _mapper.Map<ZaaerBankResponseDto>(bank);
        }

        public async Task<IEnumerable<ZaaerBankResponseDto>> GetAllBanksAsync()
        {
            var banks = await _context.Banks
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.BankNameEn)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ZaaerBankResponseDto>>(banks);
        }
    }
}


