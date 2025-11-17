using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
    /// <summary>
    /// Service for Zaaer Credit Note integration
    /// </summary>
    public interface IZaaerCreditNoteService
    {
        Task<ZaaerCreditNoteResponseDto> CreateCreditNoteAsync(ZaaerCreateCreditNoteDto createCreditNoteDto);
        Task<IEnumerable<ZaaerCreditNoteResponseDto>> GetCreditNotesByHotelIdAsync(int hotelId);
    }

    public class ZaaerCreditNoteService : IZaaerCreditNoteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ZaaerCreditNoteService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ZaaerCreditNoteResponseDto> CreateCreditNoteAsync(ZaaerCreateCreditNoteDto createCreditNoteDto)
        {
            var creditNote = _mapper.Map<CreditNote>(createCreditNoteDto);
            creditNote.CreatedAt = KsaTime.Now;

            _context.CreditNotes.Add(creditNote);
            await _context.SaveChangesAsync();

            return _mapper.Map<ZaaerCreditNoteResponseDto>(creditNote);
        }

        public async Task<IEnumerable<ZaaerCreditNoteResponseDto>> GetCreditNotesByHotelIdAsync(int hotelId)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.HotelId == hotelId)
                .OrderByDescending(cn => cn.CreditNoteDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ZaaerCreditNoteResponseDto>>(creditNotes);
        }
    }
}
