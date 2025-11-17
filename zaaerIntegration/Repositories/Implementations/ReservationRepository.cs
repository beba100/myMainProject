using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Reservation operations
    /// </summary>
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Reservation> Reservations, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<Reservation, bool>>? filter = null)
        {
            var query = _context.Reservations
                .Include(r => r.HotelSettings)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var reservations = await query
                .OrderByDescending(r => r.ReservationDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reservations, totalCount);
        }

        public async Task<Reservation?> GetByReservationNoAsync(string reservationNo)
        {
            return await _context.Reservations
                .Include(r => r.HotelSettings)
                .FirstOrDefaultAsync(r => r.ReservationNo == reservationNo);
        }

        public async Task<IEnumerable<Reservation>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Reservations
                .Include(r => r.HotelSettings)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByHotelIdAsync(int hotelId)
        {
            return await _context.Reservations
                .Include(r => r.HotelSettings)
                .Where(r => r.HotelId == hotelId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByStatusAsync(string status)
        {
            return await _context.Reservations
                .Include(r => r.HotelSettings)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Reservations
                .Include(r => r.HotelSettings)
                .Where(r => r.ReservationDate >= startDate && r.ReservationDate <= endDate)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }


        public async Task<IEnumerable<Reservation>> GetByCustomerNameAsync(string customerName)
        {
            return await _context.Reservations
                .Include(r => r.HotelSettings)
                .Where(r => r.CustomerId.ToString().Contains(customerName) || 
                           r.ReservationNo.Contains(customerName))
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByHotelNameAsync(string hotelName)
        {
            return await _context.Reservations
                .Include(r => r.HotelSettings)
                .Where(r => r.HotelSettings.HotelName.Contains(hotelName))
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByReservationNoSearchAsync(string reservationNo)
        {
            return await _context.Reservations
                .Include(r => r.HotelSettings)
                .Where(r => r.ReservationNo.Contains(reservationNo))
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<bool> ReservationNoExistsAsync(string reservationNo, int? excludeId = null)
        {
            var query = _context.Reservations.Where(r => r.ReservationNo == reservationNo);
            
            if (excludeId.HasValue)
            {
                query = query.Where(r => r.ReservationId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<object> GetStatisticsAsync()
        {
            var totalReservations = await _context.Reservations.CountAsync();
            var confirmedReservations = await _context.Reservations.CountAsync(r => r.Status == "Confirmed");
            var cancelledReservations = await _context.Reservations.CountAsync(r => r.Status == "Cancelled");
            var completedReservations = await _context.Reservations.CountAsync(r => r.Status == "Completed");
            var pendingReservations = await _context.Reservations.CountAsync(r => r.Status == "Pending");
            var unconfirmedReservations = await _context.Reservations.CountAsync(r => r.Status == "Unconfirmed");

            var totalRevenue = await _context.Reservations
                .Where(r => r.TotalAmount.HasValue)
                .SumAsync(r => r.TotalAmount.Value);

            var paidAmount = await _context.Reservations
                .Where(r => r.AmountPaid.HasValue)
                .SumAsync(r => r.AmountPaid.Value);

            var outstandingAmount = await _context.Reservations
                .Where(r => r.BalanceAmount.HasValue)
                .SumAsync(r => r.BalanceAmount.Value);

            var todayReservations = await _context.Reservations
                .CountAsync(r => r.ReservationDate.Date == DateTime.Today);

            var thisMonthReservations = await _context.Reservations
                .CountAsync(r => r.ReservationDate.Month == DateTime.Now.Month && 
                               r.ReservationDate.Year == DateTime.Now.Year);

            return new
            {
                TotalReservations = totalReservations,
                ConfirmedReservations = confirmedReservations,
                CancelledReservations = cancelledReservations,
                CompletedReservations = completedReservations,
                PendingReservations = pendingReservations,
                UnconfirmedReservations = unconfirmedReservations,
                TotalRevenue = totalRevenue,
                PaidAmount = paidAmount,
                OutstandingAmount = outstandingAmount,
                TodayReservations = todayReservations,
                ThisMonthReservations = thisMonthReservations,
                ConfirmationRate = totalReservations > 0 ? (double)confirmedReservations / totalReservations * 100 : 0,
                CancellationRate = totalReservations > 0 ? (double)cancelledReservations / totalReservations * 100 : 0
            };
        }

        public async Task<Reservation?> GetWithDetailsAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.HotelSettings)
                .FirstOrDefaultAsync(r => r.ReservationId == id);
        }

        public async Task<Reservation?> GetWithDetailsByReservationNoAsync(string reservationNo)
        {
            return await _context.Reservations
                .Include(r => r.HotelSettings)
                .FirstOrDefaultAsync(r => r.ReservationNo == reservationNo);
        }

    }
}
