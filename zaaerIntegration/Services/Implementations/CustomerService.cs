using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Customer Service Implementation
    ///  ‰›Ì– Œœ„… «·⁄„·«¡
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerIdentificationRepository _customerIdentificationRepository;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, ICustomerRepository customerRepository, ICustomerIdentificationRepository customerIdentificationRepository, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _customerIdentificationRepository = customerIdentificationRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<CustomerResponseDto> Customers, int TotalCount)> GetAllCustomersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null)
        {
            try
            {
                var (customers, totalCount) = await _customerRepository.GetPagedAsync(
                    pageNumber, 
                    pageSize, 
                    filter: string.IsNullOrEmpty(searchTerm) ? null : c => c.CustomerName.Contains(searchTerm),
                    includeProperties: "GuestType,Nationality,GuestCategory");

                var customerDtos = _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
                return (customerDtos, totalCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving customers: {ex.Message}", ex);
            }
        }

        public async Task<CustomerResponseDto?> GetCustomerByIdAsync(int customerId)
        {
            var customer = await _customerRepository.GetWithRelatedDataByIdAsync(customerId);
            return customer != null ? _mapper.Map<CustomerResponseDto>(customer) : null;
        }

        public async Task<CustomerResponseDto?> GetCustomerByNoAsync(string customerNo)
        {
            var customer = await _customerRepository.GetByCustomerNoAsync(customerNo);
            return customer != null ? _mapper.Map<CustomerResponseDto>(customer) : null;
        }

        public async Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
        {
            // Check if customer number already exists
            if (!string.IsNullOrEmpty(createCustomerDto.CustomerNo))
            {
                var existingCustomer = await _customerRepository.GetByCustomerNoAsync(createCustomerDto.CustomerNo);
                if (existingCustomer != null)
                {
                    throw new InvalidOperationException($"Customer with number '{createCustomerDto.CustomerNo}' already exists.");
                }
            }

            var customer = _mapper.Map<Customer>(createCustomerDto);
            customer.EnteredAt = KsaTime.Now;

            var createdCustomer = await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            // Create customer identifications
            if (createCustomerDto.Identifications != null && createCustomerDto.Identifications.Any())
            {
                foreach (var identificationDto in createCustomerDto.Identifications)
                {
                    var identification = new CustomerIdentification
                    {
                        CustomerId = createdCustomer.CustomerId,
                        IdTypeId = identificationDto.IdTypeId,
                        IdNumber = identificationDto.IdNumber,
                        VersionNumber = identificationDto.VersionNumber,
                        IssuePlace = identificationDto.IssuePlace,
                        IssuePlaceAr = identificationDto.IssuePlaceAr,
                        IssueDate = identificationDto.IssueDate,
                        ExpiryDate = identificationDto.ExpiryDate,
                        Notes = identificationDto.Notes,
                        IsPrimary = identificationDto.IsPrimary,
                        IsActive = identificationDto.IsActive,
                        CreatedAt = KsaTime.Now,
                        UpdatedAt = KsaTime.Now
                    };

                    await _customerIdentificationRepository.AddAsync(identification);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return _mapper.Map<CustomerResponseDto>(createdCustomer);
        }

        public async Task<CustomerResponseDto?> UpdateCustomerAsync(UpdateCustomerDto updateCustomerDto)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(updateCustomerDto.CustomerId);
            if (existingCustomer == null)
            {
                return null;
            }

            // Check if customer number already exists (excluding current customer)
            if (!string.IsNullOrEmpty(updateCustomerDto.CustomerNo))
            {
                var customerWithSameNo = await _customerRepository.GetByCustomerNoAsync(updateCustomerDto.CustomerNo);
                if (customerWithSameNo != null && customerWithSameNo.CustomerId != updateCustomerDto.CustomerId)
                {
                    throw new InvalidOperationException($"Customer with number '{updateCustomerDto.CustomerNo}' already exists.");
                }
            }

            _mapper.Map(updateCustomerDto, existingCustomer);
            
            var updatedCustomer = await _customerRepository.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CustomerResponseDto>(updatedCustomer);
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                return false;
            }

            await _customerRepository.DeleteAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CustomerResponseDto>> SearchCustomersAsync(string searchTerm)
        {
            var customers = await _customerRepository.SearchByNameAsync(searchTerm);
            return _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetCustomersByNationalityAsync(int nationalityId)
        {
            var customers = await _customerRepository.GetByNationalityAsync(nationalityId);
            return _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetCustomersByGuestTypeAsync(int guestTypeId)
        {
            var customers = await _customerRepository.GetByGuestTypeAsync(guestTypeId);
            return _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetCustomersByGuestCategoryAsync(int guestCategoryId)
        {
            var customers = await _customerRepository.GetByGuestCategoryAsync(guestCategoryId);
            return _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetCustomersByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var customers = await _customerRepository.GetByDateRangeAsync(fromDate, toDate);
            return _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
        }

        public async Task<object> GetCustomerStatisticsAsync()
        {
            return await _customerRepository.GetCustomerStatisticsAsync();
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _customerRepository.ExistsAsync(c => c.CustomerId == customerId);
        }

        public async Task<bool> CustomerNoExistsAsync(string customerNo, int? excludeCustomerId = null)
        {
            var customer = await _customerRepository.GetByCustomerNoAsync(customerNo);
            if (customer == null)
                return false;

            if (excludeCustomerId.HasValue)
                return customer.CustomerId != excludeCustomerId.Value;

            return true;
        }
    }
}
