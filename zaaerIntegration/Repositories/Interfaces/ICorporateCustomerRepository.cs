using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for CorporateCustomer repository operations
    /// </summary>
    public interface ICorporateCustomerRepository : IGenericRepository<CorporateCustomer>
    {
        /// <summary>
        /// Get corporate customers with pagination and search
        /// </summary>
        Task<(IEnumerable<CorporateCustomer> CorporateCustomers, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<CorporateCustomer, bool>>? filter = null);

        /// <summary>
        /// Get corporate customer by corporate name
        /// </summary>
        Task<CorporateCustomer?> GetByCorporateNameAsync(string corporateName);

        /// <summary>
        /// Get corporate customers by hotel ID
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get corporate customers by country
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByCountryAsync(string country);

        /// <summary>
        /// Get corporate customers by city
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByCityAsync(string city);

        /// <summary>
        /// Get corporate customers by VAT registration number
        /// </summary>
        Task<CorporateCustomer?> GetByVatRegistrationNoAsync(string vatRegistrationNo);

        /// <summary>
        /// Get corporate customers by commercial registration number
        /// </summary>
        Task<CorporateCustomer?> GetByCommercialRegistrationNoAsync(string commercialRegistrationNo);

        /// <summary>
        /// Get corporate customers by contact person name
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByContactPersonNameAsync(string contactPersonName);

        /// <summary>
        /// Get corporate customers by email
        /// </summary>
        Task<CorporateCustomer?> GetByEmailAsync(string email);

        /// <summary>
        /// Get corporate customers by phone
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByPhoneAsync(string phone);

        /// <summary>
        /// Get active corporate customers
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetActiveAsync();

        /// <summary>
        /// Get inactive corporate customers
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetInactiveAsync();

        /// <summary>
        /// Search corporate customers by name
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> SearchByNameAsync(string name);

        /// <summary>
        /// Search corporate customers by name in Arabic
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> SearchByNameArAsync(string nameAr);

        /// <summary>
        /// Get corporate customers with discount
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetWithDiscountAsync();

        /// <summary>
        /// Get corporate customers by discount method
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByDiscountMethodAsync(string discountMethod);

        /// <summary>
        /// Get corporate customers by discount value range
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByDiscountValueRangeAsync(decimal minValue, decimal maxValue);

        /// <summary>
        /// Check if corporate name exists
        /// </summary>
        Task<bool> CorporateNameExistsAsync(string corporateName, int? excludeId = null);

        /// <summary>
        /// Check if VAT registration number exists
        /// </summary>
        Task<bool> VatRegistrationNoExistsAsync(string vatRegistrationNo, int? excludeId = null);

        /// <summary>
        /// Check if commercial registration number exists
        /// </summary>
        Task<bool> CommercialRegistrationNoExistsAsync(string commercialRegistrationNo, int? excludeId = null);

        /// <summary>
        /// Check if email exists
        /// </summary>
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);

        /// <summary>
        /// Get corporate customer statistics
        /// </summary>
        Task<object> GetStatisticsAsync();

        /// <summary>
        /// Get corporate customers with full details (includes all navigation properties)
        /// </summary>
        Task<CorporateCustomer?> GetWithDetailsAsync(int id);

        /// <summary>
        /// Get corporate customers with full details by corporate name
        /// </summary>
        Task<CorporateCustomer?> GetWithDetailsByCorporateNameAsync(string corporateName);

        /// <summary>
        /// Get corporate customers by date range
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get corporate customers by created date
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByCreatedDateAsync(DateTime createdDate);

        /// <summary>
        /// Get corporate customers with reservations
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetWithReservationsAsync();

        /// <summary>
        /// Get corporate customers by reservation count range
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByReservationCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top corporate customers by reservation count
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetTopByReservationCountAsync(int topCount = 10);

        /// <summary>
        /// Get corporate customers by postal code
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByPostalCodeAsync(string postalCode);

        /// <summary>
        /// Get corporate customers by address
        /// </summary>
        Task<IEnumerable<CorporateCustomer>> GetByAddressAsync(string address);
    }
}
