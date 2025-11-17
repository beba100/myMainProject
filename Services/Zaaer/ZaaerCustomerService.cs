using AutoMapper;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Models;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
    /// <summary>
    /// Interface for Zaaer Customer Service
    /// </summary>
    public interface IZaaerCustomerService
    {
        Task<ZaaerCustomerResponseDto> CreateCustomerAsync(ZaaerCreateCustomerDto createCustomerDto);
        Task<ZaaerCustomerResponseDto> UpdateCustomerAsync(int customerId, ZaaerUpdateCustomerDto updateCustomerDto);
        Task<ZaaerCustomerResponseDto?> GetCustomerByIdAsync(int customerId);
        Task<IEnumerable<ZaaerCustomerResponseDto>> GetAllCustomersAsync(int hotelId);
        Task<bool> DeleteCustomerAsync(int customerId);
    }

    /// <summary>
    /// Service for handling Zaaer Customer operations
    /// </summary>
    public class ZaaerCustomerService : IZaaerCustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerIdentificationRepository _customerIdentificationRepository;
        private readonly IMapper _mapper;

        public ZaaerCustomerService(
            IUnitOfWork unitOfWork,
            ICustomerRepository customerRepository,
            ICustomerIdentificationRepository customerIdentificationRepository,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _customerIdentificationRepository = customerIdentificationRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Create a new customer via Zaaer integration
        /// </summary>
        public async Task<ZaaerCustomerResponseDto> CreateCustomerAsync(ZaaerCreateCustomerDto createCustomerDto)
        {
            var customer = new Customer
            {
                CustomerNo = createCustomerDto.CustomerNo,
                CustomerName = createCustomerDto.CustomerName,
                GtypeId = createCustomerDto.GtypeId,
                NId = createCustomerDto.NId,
                IdType = createCustomerDto.IdType,
                GuestCategoryId = createCustomerDto.GuestCategoryId,
                VisaNo = createCustomerDto.VisaNo,
                MobileNo = createCustomerDto.MobileNo,
                EnteredBy = createCustomerDto.EnteredBy,
                EnteredAt = createCustomerDto.EnteredAt ?? DateTime.UtcNow,
                BirthdateHijri = createCustomerDto.BirthdateHijri,
                HotelId = createCustomerDto.HotelId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                BirthdateGregorian = createCustomerDto.BirthdateGregorian
            };

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
                        HotelId = createdCustomer.HotelId,
                        IdTypeId = identificationDto.IdTypeId,
                        IdNumber = identificationDto.IdNumber,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _customerIdentificationRepository.AddAsync(identification);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return _mapper.Map<ZaaerCustomerResponseDto>(createdCustomer);
        }

        /// <summary>
        /// Update an existing customer via Zaaer integration
        /// </summary>
        public async Task<ZaaerCustomerResponseDto> UpdateCustomerAsync(int customerId, ZaaerUpdateCustomerDto updateCustomerDto)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(customerId);
            if (existingCustomer == null)
            {
                throw new ArgumentException($"Customer with ID {customerId} not found.");
            }

            // Update customer properties
            if (!string.IsNullOrEmpty(updateCustomerDto.CustomerNo))
                existingCustomer.CustomerNo = updateCustomerDto.CustomerNo;
            
            if (!string.IsNullOrEmpty(updateCustomerDto.CustomerName))
                existingCustomer.CustomerName = updateCustomerDto.CustomerName;
            
            if (updateCustomerDto.GtypeId.HasValue)
                existingCustomer.GtypeId = updateCustomerDto.GtypeId;
            
            if (!string.IsNullOrEmpty(updateCustomerDto.NId))
                existingCustomer.NId = updateCustomerDto.NId;
            
            if (updateCustomerDto.IdType.HasValue)
                existingCustomer.IdType = updateCustomerDto.IdType;
            
            if (updateCustomerDto.GuestCategoryId.HasValue)
                existingCustomer.GuestCategoryId = updateCustomerDto.GuestCategoryId;
            
            if (!string.IsNullOrEmpty(updateCustomerDto.VisaNo))
                existingCustomer.VisaNo = updateCustomerDto.VisaNo;
            
            if (!string.IsNullOrEmpty(updateCustomerDto.MobileNo))
                existingCustomer.MobileNo = updateCustomerDto.MobileNo;
            
            if (updateCustomerDto.EnteredBy.HasValue)
                existingCustomer.EnteredBy = updateCustomerDto.EnteredBy;
            
            if (updateCustomerDto.EnteredAt.HasValue)
                existingCustomer.EnteredAt = updateCustomerDto.EnteredAt;
            
            if (updateCustomerDto.BirthdateHijri.HasValue)
                existingCustomer.BirthdateHijri = updateCustomerDto.BirthdateHijri;
            
            if (updateCustomerDto.BirthdateGregorian.HasValue)
                existingCustomer.BirthdateGregorian = updateCustomerDto.BirthdateGregorian;

            existingCustomer.UpdatedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveChangesAsync();

            // Update customer identifications if provided
            if (updateCustomerDto.Identifications != null)
            {
                // Remove existing identifications
                var existingIdentifications = await _customerIdentificationRepository.GetAllAsync();
                var customerIdentifications = existingIdentifications.Where(ci => ci.CustomerId == customerId).ToList();
                
                foreach (var identification in customerIdentifications)
                {
                    await _customerIdentificationRepository.DeleteAsync(identification);
                }

                // Add new identifications
                foreach (var identificationDto in updateCustomerDto.Identifications)
                {
                    var identification = new CustomerIdentification
                    {
                        CustomerId = customerId,
                        HotelId = existingCustomer.HotelId,
                        IdTypeId = identificationDto.IdTypeId,
                        IdNumber = identificationDto.IdNumber,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _customerIdentificationRepository.AddAsync(identification);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return _mapper.Map<ZaaerCustomerResponseDto>(existingCustomer);
        }

        /// <summary>
        /// Get customer by ID via Zaaer integration
        /// </summary>
        public async Task<ZaaerCustomerResponseDto?> GetCustomerByIdAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            return customer != null ? _mapper.Map<ZaaerCustomerResponseDto>(customer) : null;
        }

        /// <summary>
        /// Get all customers for a hotel via Zaaer integration
        /// </summary>
        public async Task<IEnumerable<ZaaerCustomerResponseDto>> GetAllCustomersAsync(int hotelId)
        {
            var customers = await _customerRepository.GetAllAsync();
            var hotelCustomers = customers.Where(c => c.HotelId == hotelId);
            return _mapper.Map<IEnumerable<ZaaerCustomerResponseDto>>(hotelCustomers);
        }

        /// <summary>
        /// Delete customer via Zaaer integration
        /// </summary>
        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                return false;
            }

            // Delete customer identifications first
            var existingIdentifications = await _customerIdentificationRepository.GetAllAsync();
            var customerIdentifications = existingIdentifications.Where(ci => ci.CustomerId == customerId).ToList();
            
            foreach (var identification in customerIdentifications)
            {
                await _customerIdentificationRepository.DeleteAsync(identification);
            }

            // Delete customer
            await _customerRepository.DeleteAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
