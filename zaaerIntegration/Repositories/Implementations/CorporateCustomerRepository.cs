using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for CorporateCustomer operations
    /// </summary>
    public class CorporateCustomerRepository : GenericRepository<CorporateCustomer>, ICorporateCustomerRepository
    {
        public CorporateCustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<CorporateCustomer> CorporateCustomers, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<CorporateCustomer, bool>>? filter = null)
        {
            var query = _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var corporateCustomers = await query
                .OrderByDescending(cc => cc.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (corporateCustomers, totalCount);
        }

        public async Task<CorporateCustomer?> GetByCorporateNameAsync(string corporateName)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .FirstOrDefaultAsync(cc => cc.CorporateName == corporateName);
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByHotelIdAsync(int hotelId)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.HotelId == hotelId)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByCountryAsync(string country)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.Country == country)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByCityAsync(string city)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.City == city)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<CorporateCustomer?> GetByVatRegistrationNoAsync(string vatRegistrationNo)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .FirstOrDefaultAsync(cc => cc.VatRegistrationNo == vatRegistrationNo);
        }

        public async Task<CorporateCustomer?> GetByCommercialRegistrationNoAsync(string commercialRegistrationNo)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .FirstOrDefaultAsync(cc => cc.CommercialRegistrationNo == commercialRegistrationNo);
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByContactPersonNameAsync(string contactPersonName)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.ContactPersonName.Contains(contactPersonName))
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<CorporateCustomer?> GetByEmailAsync(string email)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .FirstOrDefaultAsync(cc => cc.Email == email);
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByPhoneAsync(string phone)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.CorporatePhone.Contains(phone) || cc.ContactPersonPhone.Contains(phone))
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetActiveAsync()
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.IsActive)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetInactiveAsync()
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => !cc.IsActive)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> SearchByNameAsync(string name)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.CorporateName.Contains(name))
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> SearchByNameArAsync(string nameAr)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.CorporateNameAr.Contains(nameAr))
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetWithDiscountAsync()
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.DiscountValue.HasValue && cc.DiscountValue > 0)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByDiscountMethodAsync(string discountMethod)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.DiscountMethod == discountMethod)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByDiscountValueRangeAsync(decimal minValue, decimal maxValue)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.DiscountValue >= minValue && cc.DiscountValue <= maxValue)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CorporateNameExistsAsync(string corporateName, int? excludeId = null)
        {
            var query = _context.CorporateCustomers.Where(cc => cc.CorporateName == corporateName);
            
            if (excludeId.HasValue)
            {
                query = query.Where(cc => cc.CorporateId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> VatRegistrationNoExistsAsync(string vatRegistrationNo, int? excludeId = null)
        {
            var query = _context.CorporateCustomers.Where(cc => cc.VatRegistrationNo == vatRegistrationNo);
            
            if (excludeId.HasValue)
            {
                query = query.Where(cc => cc.CorporateId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> CommercialRegistrationNoExistsAsync(string commercialRegistrationNo, int? excludeId = null)
        {
            var query = _context.CorporateCustomers.Where(cc => cc.CommercialRegistrationNo == commercialRegistrationNo);
            
            if (excludeId.HasValue)
            {
                query = query.Where(cc => cc.CorporateId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.CorporateCustomers.Where(cc => cc.Email == email);
            
            if (excludeId.HasValue)
            {
                query = query.Where(cc => cc.CorporateId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<object> GetStatisticsAsync()
        {
            var totalCorporateCustomers = await _context.CorporateCustomers.CountAsync();
            var activeCorporateCustomers = await _context.CorporateCustomers.CountAsync(cc => cc.IsActive);
            var inactiveCorporateCustomers = await _context.CorporateCustomers.CountAsync(cc => !cc.IsActive);
            var withDiscount = await _context.CorporateCustomers.CountAsync(cc => cc.DiscountValue.HasValue && cc.DiscountValue > 0);
            var withVatRegistration = await _context.CorporateCustomers.CountAsync(cc => !string.IsNullOrEmpty(cc.VatRegistrationNo));
            var withCommercialRegistration = await _context.CorporateCustomers.CountAsync(cc => !string.IsNullOrEmpty(cc.CommercialRegistrationNo));

            var todayCorporateCustomers = await _context.CorporateCustomers
                .CountAsync(cc => cc.CreatedAt.Date == DateTime.Today);

            var thisMonthCorporateCustomers = await _context.CorporateCustomers
                .CountAsync(cc => cc.CreatedAt.Month == DateTime.Now.Month && 
                               cc.CreatedAt.Year == DateTime.Now.Year);

            var countries = await _context.CorporateCustomers
                .Where(cc => !string.IsNullOrEmpty(cc.Country))
                .GroupBy(cc => cc.Country)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var cities = await _context.CorporateCustomers
                .Where(cc => !string.IsNullOrEmpty(cc.City))
                .GroupBy(cc => cc.City)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var discountMethods = await _context.CorporateCustomers
                .Where(cc => !string.IsNullOrEmpty(cc.DiscountMethod))
                .GroupBy(cc => cc.DiscountMethod)
                .Select(g => new { Method = g.Key, Count = g.Count() })
                .ToListAsync();

            var topCorporateCustomers = await _context.CorporateCustomers
                .GroupBy(cc => cc.CorporateId)
                .Select(g => new { 
                    CorporateId = g.Key, 
                    CorporateName = g.First().CorporateName,
                    ReservationCount = g.First().Reservations.Count,
                    HotelName = g.First().HotelSettings.HotelName
                })
                .OrderByDescending(x => x.ReservationCount)
                .Take(10)
                .ToListAsync();

            return new
            {
                TotalCorporateCustomers = totalCorporateCustomers,
                ActiveCorporateCustomers = activeCorporateCustomers,
                InactiveCorporateCustomers = inactiveCorporateCustomers,
                WithDiscount = withDiscount,
                WithVatRegistration = withVatRegistration,
                WithCommercialRegistration = withCommercialRegistration,
                TodayCorporateCustomers = todayCorporateCustomers,
                ThisMonthCorporateCustomers = thisMonthCorporateCustomers,
                Countries = countries,
                Cities = cities,
                DiscountMethods = discountMethods,
                TopCorporateCustomers = topCorporateCustomers
            };
        }

        public async Task<CorporateCustomer?> GetWithDetailsAsync(int id)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .FirstOrDefaultAsync(cc => cc.CorporateId == id);
        }

        public async Task<CorporateCustomer?> GetWithDetailsByCorporateNameAsync(string corporateName)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .FirstOrDefaultAsync(cc => cc.CorporateName == corporateName);
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.CreatedAt >= startDate && cc.CreatedAt <= endDate)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByCreatedDateAsync(DateTime createdDate)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.CreatedAt.Date == createdDate.Date)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetWithReservationsAsync()
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.Reservations.Any())
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByReservationCountRangeAsync(int minCount, int maxCount)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.Reservations.Count >= minCount && cc.Reservations.Count <= maxCount)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetTopByReservationCountAsync(int topCount = 10)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .OrderByDescending(cc => cc.Reservations.Count)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByPostalCodeAsync(string postalCode)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.PostalCode == postalCode)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CorporateCustomer>> GetByAddressAsync(string address)
        {
            return await _context.CorporateCustomers
                .Include(cc => cc.HotelSettings)
                .Include(cc => cc.Reservations)
                .Where(cc => cc.Address.Contains(address))
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }
    }
}
