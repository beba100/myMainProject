using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for PaymentReceipt operations
    /// </summary>
    public class PaymentReceiptRepository : GenericRepository<PaymentReceipt>, IPaymentReceiptRepository
    {
        public PaymentReceiptRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<PaymentReceipt> PaymentReceipts, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<PaymentReceipt, bool>>? filter = null)
        {
            var query = _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var paymentReceipts = await query
                .OrderByDescending(pr => pr.ReceiptDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (paymentReceipts, totalCount);
        }

        public async Task<PaymentReceipt?> GetByReceiptNoAsync(string receiptNo)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .FirstOrDefaultAsync(pr => pr.ReceiptNo == receiptNo);
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.CustomerId == customerId)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByHotelIdAsync(int hotelId)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.HotelId == hotelId)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByReservationIdAsync(int reservationId)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.ReservationId == reservationId)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.InvoiceId == invoiceId)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByReceiptTypeAsync(string receiptType)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.ReceiptType == receiptType)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByPaymentMethodAsync(string paymentMethod)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.PaymentMethod == paymentMethod)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.ReceiptDate >= startDate && pr.ReceiptDate <= endDate)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.AmountPaid >= minAmount && pr.AmountPaid <= maxAmount)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByCustomerNameAsync(string customerName)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.CustomerId.ToString().Contains(customerName) || 
                           pr.ReceiptNo.Contains(customerName))
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByHotelNameAsync(string hotelName)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.HotelSettings.HotelName.Contains(hotelName))
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByReceiptNoSearchAsync(string receiptNo)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.ReceiptNo.Contains(receiptNo))
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByTransactionNoAsync(string transactionNo)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.TransactionNo.Contains(transactionNo))
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<bool> ReceiptNoExistsAsync(string receiptNo, int? excludeId = null)
        {
            var query = _context.PaymentReceipts.Where(pr => pr.ReceiptNo == receiptNo);
            
            if (excludeId.HasValue)
            {
                query = query.Where(pr => pr.ReceiptId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<object> GetStatisticsAsync()
        {
            var totalReceipts = await _context.PaymentReceipts.CountAsync();
            var totalAmount = await _context.PaymentReceipts.SumAsync(pr => pr.AmountPaid);
            var todayReceipts = await _context.PaymentReceipts
                .CountAsync(pr => pr.ReceiptDate.Date == DateTime.Today);
            var thisMonthReceipts = await _context.PaymentReceipts
                .CountAsync(pr => pr.ReceiptDate.Month == DateTime.Now.Month && 
                               pr.ReceiptDate.Year == DateTime.Now.Year);

            var receiptTypes = await _context.PaymentReceipts
                .GroupBy(pr => pr.ReceiptType)
                .Select(g => new { Type = g.Key, Count = g.Count(), Amount = g.Sum(pr => pr.AmountPaid) })
                .ToListAsync();

            var paymentMethods = await _context.PaymentReceipts
                .Where(pr => pr.PaymentMethod != null)
                .GroupBy(pr => pr.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count(), Amount = g.Sum(pr => pr.AmountPaid) })
                .ToListAsync();

            var topCustomers = await _context.PaymentReceipts
                .GroupBy(pr => pr.CustomerId)
                .Select(g => new { CustomerId = g.Key, Count = g.Count(), Amount = g.Sum(pr => pr.AmountPaid) })
                .OrderByDescending(x => x.Amount)
                .Take(10)
                .ToListAsync();

            var topHotels = await _context.PaymentReceipts
                .GroupBy(pr => pr.HotelId)
                .Select(g => new { HotelId = g.Key, Count = g.Count(), Amount = g.Sum(pr => pr.AmountPaid) })
                .OrderByDescending(x => x.Amount)
                .Take(10)
                .ToListAsync();

            return new
            {
                TotalReceipts = totalReceipts,
                TotalAmount = totalAmount,
                TodayReceipts = todayReceipts,
                ThisMonthReceipts = thisMonthReceipts,
                AverageAmount = totalReceipts > 0 ? totalAmount / totalReceipts : 0,
                ReceiptTypes = receiptTypes,
                PaymentMethods = paymentMethods,
                TopCustomers = topCustomers,
                TopHotels = topHotels
            };
        }

        public async Task<PaymentReceipt?> GetWithDetailsAsync(int id)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Include(pr => pr.CustomerTransactions)
                .FirstOrDefaultAsync(pr => pr.ReceiptId == id);
        }

        public async Task<PaymentReceipt?> GetWithDetailsByReceiptNoAsync(string receiptNo)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Include(pr => pr.CustomerTransactions)
                .FirstOrDefaultAsync(pr => pr.ReceiptNo == receiptNo);
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByBankIdAsync(int bankId)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.BankId == bankId)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByPaymentMethodIdAsync(int paymentMethodId)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.PaymentMethodId == paymentMethodId)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByCreatedByAsync(int createdBy)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.CreatedBy == createdBy)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }


        public async Task<decimal> GetTotalAmountByCustomerAsync(int customerId)
        {
            return await _context.PaymentReceipts
                .Where(pr => pr.CustomerId == customerId)
                .SumAsync(pr => pr.AmountPaid);
        }

        public async Task<decimal> GetTotalAmountByHotelAsync(int hotelId)
        {
            return await _context.PaymentReceipts
                .Where(pr => pr.HotelId == hotelId)
                .SumAsync(pr => pr.AmountPaid);
        }

        public async Task<IEnumerable<PaymentReceipt>> GetByPeriodRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PaymentReceipts
                .Include(pr => pr.HotelSettings)
                .Include(pr => pr.Reservation)
                .Include(pr => pr.Invoice)
                .Include(pr => pr.PaymentMethodNavigation)
                .Include(pr => pr.BankNavigation)
                .Where(pr => pr.ReceiptDate >= startDate && pr.ReceiptDate <= endDate)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }
    }
}
