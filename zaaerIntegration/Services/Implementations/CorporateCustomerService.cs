using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service implementation for CorporateCustomer operations
    /// </summary>
    public class CorporateCustomerService : ICorporateCustomerService
    {
        private readonly ICorporateCustomerRepository _corporateCustomerRepository;
        private readonly IMapper _mapper;

        public CorporateCustomerService(ICorporateCustomerRepository corporateCustomerRepository, IMapper mapper)
        {
            _corporateCustomerRepository = corporateCustomerRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<CorporateCustomerResponseDto> CorporateCustomers, int TotalCount)> GetAllCorporateCustomersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null)
        {
            try
            {
                System.Linq.Expressions.Expression<Func<CorporateCustomer, bool>>? filter = null;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filter = cc => cc.CorporateName.Contains(searchTerm) || 
                                 (cc.CorporateNameAr != null && cc.CorporateNameAr.Contains(searchTerm)) ||
                                 (cc.Email != null && cc.Email.Contains(searchTerm)) ||
                                 (cc.ContactPersonName != null && cc.ContactPersonName.Contains(searchTerm)) ||
                                 (cc.Notes != null && cc.Notes.Contains(searchTerm));
                }

                var (corporateCustomers, totalCount) = await _corporateCustomerRepository.GetPagedAsync(
                    pageNumber, 
                    pageSize, 
                    filter);

                var corporateCustomerDtos = _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
                return (corporateCustomerDtos, totalCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers: {ex.Message}", ex);
            }
        }

        public async Task<CorporateCustomerResponseDto?> GetCorporateCustomerByIdAsync(int id)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerRepository.GetWithDetailsAsync(id);
                return corporateCustomer != null ? _mapper.Map<CorporateCustomerResponseDto>(corporateCustomer) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customer with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<CorporateCustomerResponseDto?> GetCorporateCustomerByNameAsync(string corporateName)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerRepository.GetWithDetailsByCorporateNameAsync(corporateName);
                return corporateCustomer != null ? _mapper.Map<CorporateCustomerResponseDto>(corporateCustomer) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customer with name {corporateName}: {ex.Message}", ex);
            }
        }

        public async Task<CorporateCustomerResponseDto> CreateCorporateCustomerAsync(CreateCorporateCustomerDto createCorporateCustomerDto)
        {
            try
            {
                // Check if corporate name already exists
                if (await _corporateCustomerRepository.CorporateNameExistsAsync(createCorporateCustomerDto.CorporateName))
                {
                    throw new InvalidOperationException($"Corporate customer with name '{createCorporateCustomerDto.CorporateName}' already exists.");
                }

                // Check if VAT registration number already exists (if provided)
                if (!string.IsNullOrEmpty(createCorporateCustomerDto.VatRegistrationNo) && 
                    await _corporateCustomerRepository.VatRegistrationNoExistsAsync(createCorporateCustomerDto.VatRegistrationNo))
                {
                    throw new InvalidOperationException($"Corporate customer with VAT registration number '{createCorporateCustomerDto.VatRegistrationNo}' already exists.");
                }

                // Check if commercial registration number already exists (if provided)
                if (!string.IsNullOrEmpty(createCorporateCustomerDto.CommercialRegistrationNo) && 
                    await _corporateCustomerRepository.CommercialRegistrationNoExistsAsync(createCorporateCustomerDto.CommercialRegistrationNo))
                {
                    throw new InvalidOperationException($"Corporate customer with commercial registration number '{createCorporateCustomerDto.CommercialRegistrationNo}' already exists.");
                }

                // Check if email already exists (if provided)
                if (!string.IsNullOrEmpty(createCorporateCustomerDto.Email) && 
                    await _corporateCustomerRepository.EmailExistsAsync(createCorporateCustomerDto.Email))
                {
                    throw new InvalidOperationException($"Corporate customer with email '{createCorporateCustomerDto.Email}' already exists.");
                }

                var corporateCustomer = _mapper.Map<CorporateCustomer>(createCorporateCustomerDto);
                corporateCustomer.CreatedAt = KsaTime.Now;

                var createdCorporateCustomer = await _corporateCustomerRepository.AddAsync(corporateCustomer);
                return _mapper.Map<CorporateCustomerResponseDto>(createdCorporateCustomer);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating corporate customer: {ex.Message}", ex);
            }
        }

        public async Task<CorporateCustomerResponseDto?> UpdateCorporateCustomerAsync(int id, UpdateCorporateCustomerDto updateCorporateCustomerDto)
        {
            try
            {
                var existingCorporateCustomer = await _corporateCustomerRepository.GetByIdAsync(id);
                if (existingCorporateCustomer == null)
                {
                    return null;
                }

                // Check if corporate name already exists (excluding current corporate customer)
                if (await _corporateCustomerRepository.CorporateNameExistsAsync(updateCorporateCustomerDto.CorporateName, id))
                {
                    throw new InvalidOperationException($"Corporate customer with name '{updateCorporateCustomerDto.CorporateName}' already exists.");
                }

                // Check if VAT registration number already exists (if provided)
                if (!string.IsNullOrEmpty(updateCorporateCustomerDto.VatRegistrationNo) && 
                    await _corporateCustomerRepository.VatRegistrationNoExistsAsync(updateCorporateCustomerDto.VatRegistrationNo, id))
                {
                    throw new InvalidOperationException($"Corporate customer with VAT registration number '{updateCorporateCustomerDto.VatRegistrationNo}' already exists.");
                }

                // Check if commercial registration number already exists (if provided)
                if (!string.IsNullOrEmpty(updateCorporateCustomerDto.CommercialRegistrationNo) && 
                    await _corporateCustomerRepository.CommercialRegistrationNoExistsAsync(updateCorporateCustomerDto.CommercialRegistrationNo, id))
                {
                    throw new InvalidOperationException($"Corporate customer with commercial registration number '{updateCorporateCustomerDto.CommercialRegistrationNo}' already exists.");
                }

                // Check if email already exists (if provided)
                if (!string.IsNullOrEmpty(updateCorporateCustomerDto.Email) && 
                    await _corporateCustomerRepository.EmailExistsAsync(updateCorporateCustomerDto.Email, id))
                {
                    throw new InvalidOperationException($"Corporate customer with email '{updateCorporateCustomerDto.Email}' already exists.");
                }

                _mapper.Map(updateCorporateCustomerDto, existingCorporateCustomer);
                existingCorporateCustomer.UpdatedAt = KsaTime.Now;

                await _corporateCustomerRepository.UpdateAsync(existingCorporateCustomer);

                return _mapper.Map<CorporateCustomerResponseDto>(existingCorporateCustomer);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating corporate customer with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteCorporateCustomerAsync(int id)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerRepository.GetByIdAsync(id);
                if (corporateCustomer == null)
                {
                    return false;
                }

                await _corporateCustomerRepository.DeleteAsync(corporateCustomer);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting corporate customer with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByHotelIdAsync(int hotelId)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByHotelIdAsync(hotelId);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByCountryAsync(string country)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByCountryAsync(country);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by country {country}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByCityAsync(string city)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByCityAsync(city);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by city {city}: {ex.Message}", ex);
            }
        }

        public async Task<CorporateCustomerResponseDto?> GetCorporateCustomerByVatRegistrationNoAsync(string vatRegistrationNo)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerRepository.GetByVatRegistrationNoAsync(vatRegistrationNo);
                return corporateCustomer != null ? _mapper.Map<CorporateCustomerResponseDto>(corporateCustomer) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customer by VAT registration number {vatRegistrationNo}: {ex.Message}", ex);
            }
        }

        public async Task<CorporateCustomerResponseDto?> GetCorporateCustomerByCommercialRegistrationNoAsync(string commercialRegistrationNo)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerRepository.GetByCommercialRegistrationNoAsync(commercialRegistrationNo);
                return corporateCustomer != null ? _mapper.Map<CorporateCustomerResponseDto>(corporateCustomer) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customer by commercial registration number {commercialRegistrationNo}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByContactPersonNameAsync(string contactPersonName)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByContactPersonNameAsync(contactPersonName);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by contact person name {contactPersonName}: {ex.Message}", ex);
            }
        }

        public async Task<CorporateCustomerResponseDto?> GetCorporateCustomerByEmailAsync(string email)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerRepository.GetByEmailAsync(email);
                return corporateCustomer != null ? _mapper.Map<CorporateCustomerResponseDto>(corporateCustomer) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customer by email {email}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByPhoneAsync(string phone)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByPhoneAsync(phone);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by phone {phone}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetActiveCorporateCustomersAsync()
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetActiveAsync();
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving active corporate customers: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetInactiveCorporateCustomersAsync()
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetInactiveAsync();
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving inactive corporate customers: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> SearchCorporateCustomersByNameAsync(string name)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.SearchByNameAsync(name);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching corporate customers by name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> SearchCorporateCustomersByNameArAsync(string nameAr)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.SearchByNameArAsync(nameAr);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching corporate customers by Arabic name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersWithDiscountAsync()
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetWithDiscountAsync();
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers with discount: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByDiscountMethodAsync(string discountMethod)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByDiscountMethodAsync(discountMethod);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by discount method {discountMethod}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByDiscountValueRangeAsync(decimal minValue, decimal maxValue)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByDiscountValueRangeAsync(minValue, maxValue);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by discount value range: {ex.Message}", ex);
            }
        }

        public async Task<object> GetCorporateCustomerStatisticsAsync()
        {
            try
            {
                return await _corporateCustomerRepository.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customer statistics: {ex.Message}", ex);
            }
        }

        public async Task<bool> CorporateNameExistsAsync(string corporateName, int? excludeId = null)
        {
            try
            {
                return await _corporateCustomerRepository.CorporateNameExistsAsync(corporateName, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking corporate name existence: {ex.Message}", ex);
            }
        }

        public async Task<bool> VatRegistrationNoExistsAsync(string vatRegistrationNo, int? excludeId = null)
        {
            try
            {
                return await _corporateCustomerRepository.VatRegistrationNoExistsAsync(vatRegistrationNo, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking VAT registration number existence: {ex.Message}", ex);
            }
        }

        public async Task<bool> CommercialRegistrationNoExistsAsync(string commercialRegistrationNo, int? excludeId = null)
        {
            try
            {
                return await _corporateCustomerRepository.CommercialRegistrationNoExistsAsync(commercialRegistrationNo, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking commercial registration number existence: {ex.Message}", ex);
            }
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            try
            {
                return await _corporateCustomerRepository.EmailExistsAsync(email, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking email existence: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByCreatedDateAsync(DateTime createdDate)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByCreatedDateAsync(createdDate);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by created date: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersWithReservationsAsync()
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetWithReservationsAsync();
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers with reservations: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByReservationCountRangeAsync(int minCount, int maxCount)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByReservationCountRangeAsync(minCount, maxCount);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by reservation count range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetTopCorporateCustomersByReservationCountAsync(int topCount = 10)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetTopByReservationCountAsync(topCount);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top corporate customers by reservation count: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByPostalCodeAsync(string postalCode)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByPostalCodeAsync(postalCode);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by postal code {postalCode}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CorporateCustomerResponseDto>> GetCorporateCustomersByAddressAsync(string address)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerRepository.GetByAddressAsync(address);
                return _mapper.Map<IEnumerable<CorporateCustomerResponseDto>>(corporateCustomers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving corporate customers by address: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateCorporateCustomerStatusAsync(int id, bool isActive)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerRepository.GetByIdAsync(id);
                if (corporateCustomer == null)
                {
                    return false;
                }

                corporateCustomer.IsActive = isActive;
                corporateCustomer.UpdatedAt = KsaTime.Now;
                await _corporateCustomerRepository.UpdateAsync(corporateCustomer);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating corporate customer status: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateCorporateCustomerDiscountAsync(int id, string? discountMethod, decimal? discountValue)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerRepository.GetByIdAsync(id);
                if (corporateCustomer == null)
                {
                    return false;
                }

                corporateCustomer.DiscountMethod = discountMethod;
                corporateCustomer.DiscountValue = discountValue;
                corporateCustomer.UpdatedAt = KsaTime.Now;
                await _corporateCustomerRepository.UpdateAsync(corporateCustomer);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating corporate customer discount: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateCorporateCustomerContactAsync(int id, string? contactPersonName, string? contactPersonPhone, string? email)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerRepository.GetByIdAsync(id);
                if (corporateCustomer == null)
                {
                    return false;
                }

                corporateCustomer.ContactPersonName = contactPersonName;
                corporateCustomer.ContactPersonPhone = contactPersonPhone;
                corporateCustomer.Email = email;
                corporateCustomer.UpdatedAt = KsaTime.Now;
                await _corporateCustomerRepository.UpdateAsync(corporateCustomer);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating corporate customer contact information: {ex.Message}", ex);
            }
        }
    }
}
