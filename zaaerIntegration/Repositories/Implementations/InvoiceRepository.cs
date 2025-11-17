using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Invoice operations
    /// </summary>
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Invoice> Invoices, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<Invoice, bool>>? filter = null)
        {
            var query = _context.Invoices
                .Include(i => i.HotelSettings)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var invoices = await query
                .OrderByDescending(i => i.InvoiceDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (invoices, totalCount);
        }

        public async Task<Invoice?> GetByInvoiceNoAsync(string invoiceNo)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .FirstOrDefaultAsync(i => i.InvoiceNo == invoiceNo);
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByHotelIdAsync(int hotelId)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Where(i => i.HotelId == hotelId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByReservationIdAsync(int reservationId)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.ReservationId == reservationId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByPaymentStatusAsync(string paymentStatus)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.PaymentStatus == paymentStatus)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByInvoiceTypeAsync(string invoiceType)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.InvoiceType == invoiceType)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerNameAsync(string customerName)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.CustomerId.ToString().Contains(customerName) || 
                           i.InvoiceNo.Contains(customerName))
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByHotelNameAsync(string hotelName)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.HotelSettings.HotelName.Contains(hotelName))
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByInvoiceNoSearchAsync(string invoiceNo)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.InvoiceNo.Contains(invoiceNo))
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<bool> InvoiceNoExistsAsync(string invoiceNo, int? excludeId = null)
        {
            var query = _context.Invoices.Where(i => i.InvoiceNo == invoiceNo);
            
            if (excludeId.HasValue)
            {
                query = query.Where(i => i.InvoiceId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<object> GetStatisticsAsync()
        {
            var totalInvoices = await _context.Invoices.CountAsync();
            var paidInvoices = await _context.Invoices.CountAsync(i => i.PaymentStatus == "paid");
            var unpaidInvoices = await _context.Invoices.CountAsync(i => i.PaymentStatus == "unpaid");
            var partiallyPaidInvoices = await _context.Invoices.CountAsync(i => i.PaymentStatus == "partial");
            var overdueInvoices = await _context.Invoices.CountAsync(i => i.PaymentStatus == "overdue");

            var totalRevenue = await _context.Invoices
                .Where(i => i.TotalAmount.HasValue)
                .SumAsync(i => i.TotalAmount.Value);

            var paidAmount = await _context.Invoices
                .Where(i => i.AmountPaid > 0)
                .SumAsync(i => i.AmountPaid);

            var outstandingAmount = await _context.Invoices
                .Where(i => i.AmountRemaining.HasValue)
                .SumAsync(i => i.AmountRemaining.Value);

            var todayInvoices = await _context.Invoices
                .CountAsync(i => i.InvoiceDate.Date == DateTime.Today);

            var thisMonthInvoices = await _context.Invoices
                .CountAsync(i => i.InvoiceDate.Month == DateTime.Now.Month && 
                               i.InvoiceDate.Year == DateTime.Now.Year);

            var zatcaSentInvoices = await _context.Invoices
                .CountAsync(i => i.IsSentZatca);

            return new
            {
                TotalInvoices = totalInvoices,
                PaidInvoices = paidInvoices,
                UnpaidInvoices = unpaidInvoices,
                PartiallyPaidInvoices = partiallyPaidInvoices,
                OverdueInvoices = overdueInvoices,
                TotalRevenue = totalRevenue,
                PaidAmount = paidAmount,
                OutstandingAmount = outstandingAmount,
                TodayInvoices = todayInvoices,
                ThisMonthInvoices = thisMonthInvoices,
                ZatcaSentInvoices = zatcaSentInvoices,
                PaymentRate = totalInvoices > 0 ? (double)paidInvoices / totalInvoices * 100 : 0,
				CollectionRate = totalRevenue > 0 ? (double)paidAmount / (double)totalRevenue * 100 : 0
			};
        }

        public async Task<Invoice?> GetWithDetailsAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Include(i => i.PaymentReceipts)
                .Include(i => i.Refunds)
                .Include(i => i.CustomerTransactions)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

        public async Task<Invoice?> GetWithDetailsByInvoiceNoAsync(string invoiceNo)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Include(i => i.PaymentReceipts)
                .Include(i => i.Refunds)
                .Include(i => i.CustomerTransactions)
                .FirstOrDefaultAsync(i => i.InvoiceNo == invoiceNo);
        }

        public async Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.PaymentStatus == "unpaid")
                .OrderBy(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.PaymentStatus == "overdue")
                .OrderBy(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByZatcaStatusAsync(bool isSentZatca)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.IsSentZatca == isSentZatca)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByPeriodRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Invoices
                .Include(i => i.HotelSettings)
                .Include(i => i.Reservation)
                .Include(i => i.ReservationUnit)
                .Where(i => i.PeriodFrom >= startDate && i.PeriodTo <= endDate)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }
    }
}
