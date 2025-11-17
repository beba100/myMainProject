using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;
using ExpenseModel = FinanceLedgerAPI.Models.Expense;

namespace zaaerIntegration.Services.Zaaer
{
    public interface IZaaerExpenseService
    {
        Task<ZaaerExpenseResponseDto> CreateAsync(ZaaerCreateExpenseDto dto);
        Task<ZaaerExpenseResponseDto?> UpdateAsync(int expenseId, ZaaerUpdateExpenseDto dto);
        Task<IEnumerable<ZaaerExpenseResponseDto>> GetAllAsync();
    }

    public class ZaaerExpenseService : IZaaerExpenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ZaaerExpenseService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ZaaerExpenseResponseDto> CreateAsync(ZaaerCreateExpenseDto dto)
        {
            var expense = new ExpenseModel
            {
                HotelId = dto.HotelId,
                DateTime = dto.DateTime,
                Comment = dto.Comment,
                CreatedAt = DateTime.Now
            };
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return await Map(expense);
        }

        public async Task<ZaaerExpenseResponseDto?> UpdateAsync(int expenseId, ZaaerUpdateExpenseDto dto)
        {
            var exp = await _context.Expenses.FirstOrDefaultAsync(e => e.ExpenseId == expenseId);
            if (exp == null) return null;

            // Update only fields that exist in the database schema
            if (dto.DateTime.HasValue) exp.DateTime = dto.DateTime.Value;
            if (dto.Comment != null) exp.Comment = dto.Comment;
            if (dto.HotelId.HasValue) exp.HotelId = dto.HotelId.Value;
            exp.UpdatedAt = DateTime.Now;

            // Note: VoucherType, PaidTo, ReceivedBy, Amount, PaymentMethodId, Purpose fields
            // were removed from Expense model to match actual database schema

            await _context.SaveChangesAsync();
            return await Map(exp);
        }

        public async Task<IEnumerable<ZaaerExpenseResponseDto>> GetAllAsync()
        {
            // Note: PaymentMethod Include removed because PaymentMethodId field was removed from Expense model
            var list = await _context.Expenses.OrderByDescending(e => e.DateTime).ToListAsync();
            var result = new List<ZaaerExpenseResponseDto>();
            foreach (var e in list) result.Add(await Map(e));
            return result;
        }

        private Task<ZaaerExpenseResponseDto> Map(ExpenseModel exp)
        {
            var dto = _mapper.Map<ZaaerExpenseResponseDto>(exp);
            // Note: PaymentMethod field was removed from Expense model to match database schema
            // dto.PaymentMethodName = exp.PaymentMethod?.MethodName;
            return Task.FromResult(dto);
        }
    }
}


