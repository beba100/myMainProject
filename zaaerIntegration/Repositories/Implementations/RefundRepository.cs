using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository for Refund data access
    /// </summary>
    public class RefundRepository : GenericRepository<Refund>, IRefundRepository
    {
        public RefundRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Refund?> GetRefundWithDetailsAsync(int id)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Include(r => r.PaymentMethodNavigation)
                .Include(r => r.BankNavigation)
                .FirstOrDefaultAsync(r => r.RefundId == id);
        }

        public async Task<IEnumerable<Refund>> GetByHotelIdAsync(int hotelId)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByReservationIdAsync(int reservationId)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.ReservationId == reservationId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.RefundDate >= startDate && r.RefundDate <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.RefundAmount >= minAmount && r.RefundAmount <= maxAmount)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByRefundNumberAsync(string refundNumber)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.RefundNo.Contains(refundNumber))
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByPaymentMethodIdAsync(int paymentMethodId)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.PaymentMethodId == paymentMethodId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByBankIdAsync(int bankId)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.BankId == bankId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByTransactionNumberAsync(string transactionNumber)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.TransactionNo.Contains(transactionNumber))
                .ToListAsync();
        }


        public async Task<IEnumerable<Refund>> GetByCreatedDateAsync(DateTime createdDate)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.CreatedAt.Date == createdDate.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByCreatedDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByCreatedByAsync(string createdBy)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.CreatedBy.HasValue && r.CreatedBy.Value.ToString().Contains(createdBy))
                .ToListAsync();
        }

        public async Task<RefundStatisticsDto> GetRefundStatisticsAsync()
        {
                var allRefunds = await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.Invoice)
                .ToListAsync();

            return new RefundStatisticsDto
            {
                TotalRefunds = allRefunds.Count,
                TotalRefundAmount = allRefunds.Sum(r => r.RefundAmount),
                AverageRefundAmount = allRefunds.Any() ? allRefunds.Average(r => r.RefundAmount) : 0,
                MaxRefundAmount = allRefunds.Any() ? allRefunds.Max(r => r.RefundAmount) : 0,
                MinRefundAmount = allRefunds.Any() ? allRefunds.Min(r => r.RefundAmount) : 0,
                RefundsByHotel = allRefunds.GroupBy(r => r.HotelId).ToDictionary(g => g.Key.ToString(), g => g.Count()),
                RefundsByMonth = allRefunds.GroupBy(r => r.RefundDate.ToString("yyyy-MM")).ToDictionary(g => g.Key, g => g.Count()),
                TotalRefundAmountByHotel = allRefunds.GroupBy(r => r.HotelId).ToDictionary(g => g.Key.ToString(), g => g.Sum(r => r.RefundAmount))
            };
        }

        public async Task<IEnumerable<Refund>> SearchByRefundNumberAsync(string refundNumber)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.RefundNo.Contains(refundNumber))
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> SearchByCustomerNameAsync(string customerName)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.CustomerId != null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> SearchByHotelNameAsync(string hotelName)
        {
            return await _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .Where(r => r.HotelSettings != null && r.HotelSettings.HotelName.Contains(hotelName))
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRefundAmountByHotelIdAsync(int hotelId)
        {
            var refunds = await _context.Refunds
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();
            return refunds.Sum(r => r.RefundAmount);
        }

        public async Task<decimal> GetTotalRefundAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var refunds = await _context.Refunds
                .Where(r => r.RefundDate >= startDate && r.RefundDate <= endDate)
                .ToListAsync();
            return refunds.Sum(r => r.RefundAmount);
        }

        public async Task<decimal> GetTotalRefundAmountByCustomerIdAsync(int customerId)
        {
            var refunds = await _context.Refunds
                .Where(r => r.CustomerId == customerId)
                .ToListAsync();
            return refunds.Sum(r => r.RefundAmount);
        }

        public async Task<decimal> GetTotalRefundAmountByReservationIdAsync(int reservationId)
        {
            var refunds = await _context.Refunds
                .Where(r => r.ReservationId == reservationId)
                .ToListAsync();
            return refunds.Sum(r => r.RefundAmount);
        }

        public async Task<decimal> GetAverageRefundAmountByHotelIdAsync(int hotelId)
        {
            var refunds = await _context.Refunds
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();
            return refunds.Any() ? refunds.Average(r => r.RefundAmount) : 0;
        }

        public async Task<decimal> GetAverageRefundAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var refunds = await _context.Refunds
                .Where(r => r.RefundDate >= startDate && r.RefundDate <= endDate)
                .ToListAsync();
            return refunds.Any() ? refunds.Average(r => r.RefundAmount) : 0;
        }

        public async Task<IEnumerable<Refund>> GetRefundsByCriteriaAsync(
            int? hotelId = null,
            int? customerId = null,
            int? reservationId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            string? refundNumber = null,
            string? transactionNumber = null,
            string? createdBy = null)
        {
            var query = _context.Refunds
                .Include(r => r.HotelSettings)
                .Include(r => r.Reservation)
                .Include(r => r.ReservationUnit)
                .Include(r => r.Invoice)
                .AsQueryable();

            if (hotelId.HasValue)
                query = query.Where(r => r.HotelId == hotelId.Value);

            if (customerId.HasValue)
                query = query.Where(r => r.CustomerId == customerId.Value);

            if (reservationId.HasValue)
                query = query.Where(r => r.ReservationId == reservationId.Value);

            if (startDate.HasValue)
                query = query.Where(r => r.RefundDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.RefundDate <= endDate.Value);

            if (minAmount.HasValue)
                query = query.Where(r => r.RefundAmount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(r => r.RefundAmount <= maxAmount.Value);

            if (!string.IsNullOrEmpty(refundNumber))
                query = query.Where(r => r.RefundNo.Contains(refundNumber));

            if (!string.IsNullOrEmpty(transactionNumber))
                query = query.Where(r => r.TransactionNo.Contains(transactionNumber));


            if (!string.IsNullOrEmpty(createdBy) && int.TryParse(createdBy, out int createdById))
                query = query.Where(r => r.CreatedBy == createdById);

            return await query.ToListAsync();
        }
    }
}
