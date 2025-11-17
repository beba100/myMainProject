using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for CorporateCustomer service operations
    /// </summary>
    public interface ICorporateCustomerService
    {
        /// <summary>
        /// Get all corporate customers with pagination and search
        /// </summary>
        Task<(IEnumerable<CorporateCustomerResponseDto> CorporateCustomers, int TotalCount)> GetAllCorporateCustomersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null);

        /// <summary>
        /// Get corporate customer by ID
        /// </summary>
        Task<CorporateCustomerResponseDto?> GetCorporateCustomerByIdAsync(int id);

        /// <summary>
        /// Get corporate customer by corporate name
        /// </summary>
        Task<CorporateCustomerResponseDto?> GetCorporateCustomerByNameAsync(string corporateName);

        /// <summary>
        /// Create new corporate customer
        /// </summary>
        Task<CorporateCustomerResponseDto> CreateCorporateCustomerAsync(CreateCorporateCustomerDto createCorporateCustomerDto);

        /// <summary>
        /// Update existing corporate customer
        /// </summary>
        Task<CorporateCustomerResponseDto?> UpdateCorporateCustomerAsync(int id, UpdateCorporateCustomerDto updateCorporateCustomerDto);

        /// <summary>
        /// Delete corporate customer
        /// </summary>
        Task<bool> DeleteCorporateCustomerAsync(int id);

        /// <summary>
        /// Get corporate customers by hotel ID
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get corporate customers by country
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByCountryAsync(string country);

        /// <summary>
        /// Get corporate customers by city
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByCityAsync(string city);

        /// <summary>
        /// Get corporate customer by VAT registration number
        /// </summary>
        Task<CorporateCustomerResponseDto?> GetCorporateCustomerByVatRegistrationNoAsync(string vatRegistrationNo);

        /// <summary>
        /// Get corporate customer by commercial registration number
        /// </summary>
        Task<CorporateCustomerResponseDto?> GetCorporateCustomerByCommercialRegistrationNoAsync(string commercialRegistrationNo);

        /// <summary>
        /// Get corporate customers by contact person name
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByContactPersonNameAsync(string contactPersonName);

        /// <summary>
        /// Get corporate customer by email
        /// </summary>
        Task<CorporateCustomerResponseDto?> GetCorporateCustomerByEmailAsync(string email);

        /// <summary>
        /// Get corporate customers by phone
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByPhoneAsync(string phone);

        /// <summary>
        /// Get active corporate customers
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetActiveCorporateCustomersAsync();

        /// <summary>
        /// Get inactive corporate customers
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetInactiveCorporateCustomersAsync();

        /// <summary>
        /// Search corporate customers by name
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> SearchCorporateCustomersByNameAsync(string name);

        /// <summary>
        /// Search corporate customers by name in Arabic
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> SearchCorporateCustomersByNameArAsync(string nameAr);

        /// <summary>
        /// Get corporate customers with discount
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersWithDiscountAsync();

        /// <summary>
        /// Get corporate customers by discount method
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByDiscountMethodAsync(string discountMethod);

        /// <summary>
        /// Get corporate customers by discount value range
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByDiscountValueRangeAsync(decimal minValue, decimal maxValue);

        /// <summary>
        /// Get corporate customer statistics
        /// </summary>
        Task<object> GetCorporateCustomerStatisticsAsync();

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
        /// Get corporate customers by date range
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get corporate customers by created date
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByCreatedDateAsync(DateTime createdDate);

        /// <summary>
        /// Get corporate customers with reservations
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersWithReservationsAsync();

        /// <summary>
        /// Get corporate customers by reservation count range
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByReservationCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top corporate customers by reservation count
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetTopCorporateCustomersByReservationCountAsync(int topCount = 10);

        /// <summary>
        /// Get corporate customers by postal code
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByPostalCodeAsync(string postalCode);

        /// <summary>
        /// Get corporate customers by address
        /// </summary>
        Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByAddressAsync(string address);

        /// <summary>
        /// Update corporate customer status
        /// </summary>
        Task<bool> UpdateCorporateCustomerStatusAsync(int id, bool isActive);

        /// <summary>
        /// Update corporate customer discount
        /// </summary>
        Task<bool> UpdateCorporateCustomerDiscountAsync(int id, string? discountMethod, decimal? discountValue);

        /// <summary>
        /// Update corporate customer contact information
        /// </summary>
        Task<bool> UpdateCorporateCustomerContactAsync(int id, string? contactPersonName, string? contactPersonPhone, string? email);
    }
}
