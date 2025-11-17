using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository for CreditNote data access
    /// </summary>
    public class CreditNoteRepository : GenericRepository<CreditNote>, ICreditNoteRepository
    {
        public CreditNoteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CreditNote?> GetCreditNoteWithDetailsAsync(int id)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .FirstOrDefaultAsync(cn => cn.CreditNoteId == id);
        }

        public async Task<IEnumerable<CreditNote>> GetByHotelIdAsync(int hotelId)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.HotelId == hotelId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditNote>> GetByReservationIdAsync(int reservationId)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.ReservationId == reservationId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditNote>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditNote>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.CreditNoteDate >= startDate && cn.CreditNoteDate <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditNote>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.CreditAmount >= minAmount && cn.CreditAmount <= maxAmount)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditNote>> GetByCreditNoteNumberAsync(string creditNoteNumber)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.CreditNoteNo.Contains(creditNoteNumber))
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditNote>> GetByCreatedDateAsync(DateTime createdDate)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.CreatedAt.Date == createdDate.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditNote>> GetByCreatedDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.CreatedAt >= startDate && cn.CreatedAt <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditNote>> GetByCreatedByAsync(string createdBy)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.CreatedBy.HasValue && cn.CreatedBy.Value.ToString().Contains(createdBy))
                .ToListAsync();
        }

        public async Task<CreditNoteStatisticsDto> GetCreditNoteStatisticsAsync()
        {
            var allCreditNotes = await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .ToListAsync();

            return new CreditNoteStatisticsDto
            {
                TotalCreditNotes = allCreditNotes.Count,
                TotalCreditNoteAmount = allCreditNotes.Sum(cn => cn.CreditAmount),
                AverageCreditNoteAmount = allCreditNotes.Any() ? allCreditNotes.Average(cn => cn.CreditAmount) : 0,
                MaxCreditNoteAmount = allCreditNotes.Any() ? allCreditNotes.Max(cn => cn.CreditAmount) : 0,
                MinCreditNoteAmount = allCreditNotes.Any() ? allCreditNotes.Min(cn => cn.CreditAmount) : 0,
                CreditNotesByHotel = allCreditNotes.GroupBy(cn => cn.HotelId).ToDictionary(g => g.Key.ToString(), g => g.Count()),
                CreditNotesByMonth = allCreditNotes.GroupBy(cn => cn.CreditNoteDate.ToString("yyyy-MM")).ToDictionary(g => g.Key, g => g.Count()),
                TotalCreditNoteAmountByHotel = allCreditNotes.GroupBy(cn => cn.HotelId).ToDictionary(g => g.Key.ToString(), g => g.Sum(cn => cn.CreditAmount))
            };
        }

        public async Task<IEnumerable<CreditNote>> SearchByCreditNoteNumberAsync(string creditNoteNumber)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.CreditNoteNo.Contains(creditNoteNumber))
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditNote>> SearchByCustomerNameAsync(string customerName)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.CustomerId != null)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditNote>> SearchByHotelNameAsync(string hotelName)
        {
            return await _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .Where(cn => cn.HotelSettings != null && cn.HotelSettings.HotelName.Contains(hotelName))
                .ToListAsync();
        }

        public async Task<decimal> GetTotalCreditAmountByHotelIdAsync(int hotelId)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.HotelId == hotelId)
                .ToListAsync();
            return creditNotes.Sum(cn => cn.CreditAmount);
        }

        public async Task<decimal> GetTotalCreditAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.CreditNoteDate >= startDate && cn.CreditNoteDate <= endDate)
                .ToListAsync();
            return creditNotes.Sum(cn => cn.CreditAmount);
        }

        public async Task<decimal> GetTotalCreditAmountByCustomerIdAsync(int customerId)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.CustomerId == customerId)
                .ToListAsync();
            return creditNotes.Sum(cn => cn.CreditAmount);
        }

        public async Task<decimal> GetTotalCreditAmountByReservationIdAsync(int reservationId)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.ReservationId == reservationId)
                .ToListAsync();
            return creditNotes.Sum(cn => cn.CreditAmount);
        }

        public async Task<decimal> GetAverageCreditAmountByHotelIdAsync(int hotelId)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.HotelId == hotelId)
                .ToListAsync();
            return creditNotes.Any() ? creditNotes.Average(cn => cn.CreditAmount) : 0;
        }

        public async Task<decimal> GetAverageCreditAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.CreditNoteDate >= startDate && cn.CreditNoteDate <= endDate)
                .ToListAsync();
            return creditNotes.Any() ? creditNotes.Average(cn => cn.CreditAmount) : 0;
        }

        public async Task<IEnumerable<CreditNote>> GetCreditNotesByCriteriaAsync(
            int? hotelId = null,
            int? customerId = null,
            int? reservationId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            string? creditNoteNumber = null,
            string? createdBy = null)
        {
            var query = _context.CreditNotes
                .Include(cn => cn.HotelSettings)
                .Include(cn => cn.Reservation)
                .Include(cn => cn.Invoice)
                .AsQueryable();

            if (hotelId.HasValue)
                query = query.Where(cn => cn.HotelId == hotelId.Value);

            if (customerId.HasValue)
                query = query.Where(cn => cn.CustomerId == customerId.Value);

            if (reservationId.HasValue)
                query = query.Where(cn => cn.ReservationId == reservationId.Value);

            if (startDate.HasValue)
                query = query.Where(cn => cn.CreditNoteDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(cn => cn.CreditNoteDate <= endDate.Value);

            if (minAmount.HasValue)
                query = query.Where(cn => cn.CreditAmount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(cn => cn.CreditAmount <= maxAmount.Value);

            if (!string.IsNullOrEmpty(creditNoteNumber))
                query = query.Where(cn => cn.CreditNoteNo.Contains(creditNoteNumber));

            if (!string.IsNullOrEmpty(createdBy) && int.TryParse(createdBy, out int createdById))
                query = query.Where(cn => cn.CreatedBy == createdById);

            return await query.ToListAsync();
        }

        public async Task<decimal> GetTotalCreditNoteAmountByHotelIdAsync(int hotelId)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.HotelId == hotelId)
                .ToListAsync();
            return creditNotes.Sum(cn => cn.CreditAmount);
        }

        public async Task<decimal> GetTotalCreditNoteAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.CreditNoteDate >= startDate && cn.CreditNoteDate <= endDate)
                .ToListAsync();
            return creditNotes.Sum(cn => cn.CreditAmount);
        }

        public async Task<decimal> GetTotalCreditNoteAmountByCustomerIdAsync(int customerId)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.CustomerId == customerId)
                .ToListAsync();
            return creditNotes.Sum(cn => cn.CreditAmount);
        }

        public async Task<decimal> GetTotalCreditNoteAmountByReservationIdAsync(int reservationId)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.ReservationId == reservationId)
                .ToListAsync();
            return creditNotes.Sum(cn => cn.CreditAmount);
        }

        public async Task<decimal> GetAverageCreditNoteAmountByHotelIdAsync(int hotelId)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.HotelId == hotelId)
                .ToListAsync();
            return creditNotes.Any() ? creditNotes.Average(cn => cn.CreditAmount) : 0;
        }

        public async Task<decimal> GetAverageCreditNoteAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var creditNotes = await _context.CreditNotes
                .Where(cn => cn.CreditNoteDate >= startDate && cn.CreditNoteDate <= endDate)
                .ToListAsync();
            return creditNotes.Any() ? creditNotes.Average(cn => cn.CreditAmount) : 0;
        }
    }
}
