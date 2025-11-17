using Microsoft.EntityFrameworkCore;
using FinanceLedgerAPI.Models;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Customer Repository Implementation
    /// تنفيذ مستودع العملاء
    /// </summary>
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByCustomerNoAsync(string customerNo)
        {
            return await _dbSet
                .Include(c => c.GuestType)
                .Include(c => c.Nationality)
                .Include(c => c.GuestCategory)
                .FirstOrDefaultAsync(c => c.CustomerNo == customerNo);
        }

        public async Task<IEnumerable<Customer>> GetByHotelIdAsync(int hotelId)
        {
            return await _dbSet
                .Include(c => c.GuestType)
                .Include(c => c.Nationality)
                .Include(c => c.GuestCategory)
                .Where(c => c.EnteredBy == hotelId) // Assuming EnteredBy represents hotel context
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> SearchByNameAsync(string name)
        {
            return await _dbSet
                .Include(c => c.GuestType)
                .Include(c => c.Nationality)
                .Include(c => c.GuestCategory)
                .Where(c => c.CustomerName.Contains(name))
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetWithRelatedDataAsync()
        {
            return await _dbSet
                .Include(c => c.GuestType)
                .Include(c => c.Nationality)
                .Include(c => c.GuestCategory)
                .Include(c => c.Identifications)
                .Include(c => c.CustomerAccounts)
                .ToListAsync();
        }

        public async Task<Customer?> GetWithRelatedDataByIdAsync(int customerId)
        {
            return await _dbSet
                .Include(c => c.GuestType)
                .Include(c => c.Nationality)
                .Include(c => c.GuestCategory)
                .Include(c => c.Identifications)
                .Include(c => c.CustomerAccounts)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<IEnumerable<Customer>> GetByNationalityAsync(int nationalityId)
        {
            return await _dbSet
                .Include(c => c.GuestType)
                .Include(c => c.Nationality)
                .Include(c => c.GuestCategory)
                .Where(c => c.NId == nationalityId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetByGuestTypeAsync(int guestTypeId)
        {
            return await _dbSet
                .Include(c => c.GuestType)
                .Include(c => c.Nationality)
                .Include(c => c.GuestCategory)
                .Where(c => c.GtypeId == guestTypeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetByGuestCategoryAsync(int guestCategoryId)
        {
            return await _dbSet
                .Include(c => c.GuestType)
                .Include(c => c.Nationality)
                .Include(c => c.GuestCategory)
                .Where(c => c.GuestCategoryId == guestCategoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Include(c => c.GuestType)
                .Include(c => c.Nationality)
                .Include(c => c.GuestCategory)
                .Where(c => c.EnteredAt >= fromDate && c.EnteredAt <= toDate)
                .ToListAsync();
        }

        public async Task<object> GetCustomerStatisticsAsync()
        {
            var totalCustomers = await _dbSet.CountAsync();
            var customersByGender = await _dbSet
                .GroupBy(c => c.Gender)
                .Select(g => new { Gender = g.Key, Count = g.Count() })
                .ToListAsync();
            
            var customersByNationality = await _dbSet
                .Include(c => c.Nationality)
                .GroupBy(c => c.Nationality != null ? c.Nationality.NName : "Unknown")
                .Select(g => new { Nationality = g.Key, Count = g.Count() })
                .ToListAsync();

            var recentCustomers = await _dbSet
                .Where(c => c.EnteredAt >= DateTime.Now.AddDays(-30))
                .CountAsync();

            return new
            {
                TotalCustomers = totalCustomers,
                RecentCustomers = recentCustomers,
                CustomersByGender = customersByGender,
                CustomersByNationality = customersByNationality
            };
        }
    }
}
