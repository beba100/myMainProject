using AutoMapper;
using zaaerIntegration.DTOs.Zaaer;
using FinanceLedgerAPI.Models;
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
        Task<ZaaerCustomerResponseDto> UpdateCustomerByNumberAsync(string customerNo, ZaaerUpdateCustomerDto updateCustomerDto);
        Task<ZaaerCustomerResponseDto> UpdateCustomerByZaaerIdAsync(int zaaerId, ZaaerUpdateCustomerDto updateCustomerDto);
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
                GuestCategoryId = createCustomerDto.GuestCategoryId,
                VisaNo = string.IsNullOrWhiteSpace(createCustomerDto.VisaNo) ? null : createCustomerDto.VisaNo.Trim(),
                MobileNo = string.IsNullOrWhiteSpace(createCustomerDto.MobileNo) ? null : createCustomerDto.MobileNo,
                EnteredBy = createCustomerDto.EnteredBy,
                EnteredAt = createCustomerDto.EnteredAt ?? KsaTime.Now,
                BirthdateHijri = string.IsNullOrWhiteSpace(createCustomerDto.BirthdateHijri) ? null : createCustomerDto.BirthdateHijri.Trim(),
                HotelId = createCustomerDto.HotelId,
                ZaaerId = createCustomerDto.ZaaerId,
                CreatedAt = KsaTime.Now,
                UpdatedAt = KsaTime.Now,
                BirthdateGregorian = createCustomerDto.BirthdateGregorian
            };

            var createdCustomer = await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            // Create customer identifications
            // Use zaaer_id from customer (what Zaaer system sent) as customer_id in customer_identifications
            if (createCustomerDto.Identifications != null && createCustomerDto.Identifications.Any())
            {
                // Get the zaaer_id value to use as customer_id in identifications
                var customerIdForIdentifications = createdCustomer.ZaaerId ?? createdCustomer.CustomerId;
                
                foreach (var identificationDto in createCustomerDto.Identifications)
                {
                    var identification = new CustomerIdentification
                    {
                        // Use zaaer_id from customer (what Zaaer sent) as customer_id, not the database customer_id
                        CustomerId = customerIdForIdentifications,
                        ZaaerId = identificationDto.ZaaerId,
                        IdTypeId = identificationDto.IdTypeId,
                        IdNumber = identificationDto.IdNumber,
                        CreatedAt = KsaTime.Now,
                        UpdatedAt = KsaTime.Now
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
            
            if (updateCustomerDto.NId.HasValue)
                existingCustomer.NId = updateCustomerDto.NId;
            
            if (updateCustomerDto.GuestCategoryId.HasValue)
                existingCustomer.GuestCategoryId = updateCustomerDto.GuestCategoryId;
            
            // VisaNo: set to null if empty/null, otherwise update
            if (updateCustomerDto.VisaNo != null)
            {
                existingCustomer.VisaNo = string.IsNullOrWhiteSpace(updateCustomerDto.VisaNo) ? null : updateCustomerDto.VisaNo.Trim();
            }
            
            // MobileNo: set to null if empty/null, otherwise update
            if (updateCustomerDto.MobileNo != null)
            {
                existingCustomer.MobileNo = string.IsNullOrWhiteSpace(updateCustomerDto.MobileNo) ? null : updateCustomerDto.MobileNo;
            }
            
            if (updateCustomerDto.EnteredBy.HasValue)
                existingCustomer.EnteredBy = updateCustomerDto.EnteredBy;
            
            if (updateCustomerDto.EnteredAt.HasValue)
                existingCustomer.EnteredAt = updateCustomerDto.EnteredAt;
            
            // BirthdateHijri: set to null if empty/null, otherwise update
            if (updateCustomerDto.BirthdateHijri != null)
            {
                existingCustomer.BirthdateHijri = string.IsNullOrWhiteSpace(updateCustomerDto.BirthdateHijri) ? null : updateCustomerDto.BirthdateHijri.Trim();
            }
            
            if (updateCustomerDto.BirthdateGregorian.HasValue)
                existingCustomer.BirthdateGregorian = updateCustomerDto.BirthdateGregorian;

            if (updateCustomerDto.ZaaerId.HasValue)
                existingCustomer.ZaaerId = updateCustomerDto.ZaaerId;

            existingCustomer.UpdatedAt = KsaTime.Now;

            await _customerRepository.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveChangesAsync();

            // Update customer identifications if provided
            // Use zaaer_id from customer (what Zaaer system sent) as customer_id in customer_identifications
            if (updateCustomerDto.Identifications != null)
            {
                // Get the zaaer_id value to use as customer_id in identifications (what Zaaer sent)
                var customerIdForIdentifications = existingCustomer.ZaaerId ?? existingCustomer.CustomerId;
                
                // Remove existing identifications - find by both zaaer_id and database customer_id to handle migration
                var existingIdentifications = await _customerIdentificationRepository.GetAllAsync();
                var customerIdentifications = existingIdentifications
                    .Where(ci => ci.CustomerId == customerIdForIdentifications || 
                                 ci.CustomerId == existingCustomer.CustomerId)
                    .ToList();
                
                foreach (var identification in customerIdentifications)
                {
                    await _customerIdentificationRepository.DeleteAsync(identification);
                }

                // Add new identifications using zaaer_id as customer_id (what Zaaer system sent)
                foreach (var identificationDto in updateCustomerDto.Identifications)
                {
                    var identification = new CustomerIdentification
                    {
                        // Use zaaer_id from customer (what Zaaer sent) as customer_id, not the database customer_id
                        CustomerId = customerIdForIdentifications,
                        ZaaerId = identificationDto.ZaaerId,
                        IdTypeId = identificationDto.IdTypeId,
                        IdNumber = identificationDto.IdNumber,
                        CreatedAt = KsaTime.Now,
                        UpdatedAt = KsaTime.Now
                    };

                    await _customerIdentificationRepository.AddAsync(identification);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return _mapper.Map<ZaaerCustomerResponseDto>(existingCustomer);
        }

        /// <summary>
        /// Update an existing customer by ZaaerId via Zaaer integration
        /// </summary>
        public async Task<ZaaerCustomerResponseDto> UpdateCustomerByZaaerIdAsync(int zaaerId, ZaaerUpdateCustomerDto updateCustomerDto)
        {
            var candidates = await _customerRepository.FindAsync(c => c.ZaaerId == zaaerId);
            var customer = candidates.FirstOrDefault();
            if (customer == null)
            {
                throw new ArgumentException($"Customer with zaaerId {zaaerId} not found.");
            }

            // Reuse UpdateCustomerAsync logic by mapping
            return await UpdateCustomerAsync(customer.CustomerId, updateCustomerDto);
        }

        /// <summary>
        /// Update an existing customer by customer number via Zaaer integration
        /// </summary>
        public async Task<ZaaerCustomerResponseDto> UpdateCustomerByNumberAsync(string customerNo, ZaaerUpdateCustomerDto updateCustomerDto)
        {
            var existingCustomer = await _customerRepository.GetByCustomerNoAsync(customerNo);
            if (existingCustomer == null)
            {
                throw new ArgumentException($"Customer with customerNo {customerNo} not found.");
            }

            if (!string.IsNullOrEmpty(updateCustomerDto.CustomerNo))
                existingCustomer.CustomerNo = updateCustomerDto.CustomerNo;
            if (!string.IsNullOrEmpty(updateCustomerDto.CustomerName))
                existingCustomer.CustomerName = updateCustomerDto.CustomerName;
            if (updateCustomerDto.GtypeId.HasValue)
                existingCustomer.GtypeId = updateCustomerDto.GtypeId;
            if (updateCustomerDto.NId.HasValue)
                existingCustomer.NId = updateCustomerDto.NId;
            if (updateCustomerDto.GuestCategoryId.HasValue)
                existingCustomer.GuestCategoryId = updateCustomerDto.GuestCategoryId;
            
            // VisaNo: set to null if empty/null, otherwise update
            if (updateCustomerDto.VisaNo != null)
            {
                existingCustomer.VisaNo = string.IsNullOrWhiteSpace(updateCustomerDto.VisaNo) ? null : updateCustomerDto.VisaNo.Trim();
            }
            
            // MobileNo: set to null if empty/null, otherwise update
            if (updateCustomerDto.MobileNo != null)
            {
                existingCustomer.MobileNo = string.IsNullOrWhiteSpace(updateCustomerDto.MobileNo) ? null : updateCustomerDto.MobileNo;
            }
            
            if (updateCustomerDto.EnteredBy.HasValue)
                existingCustomer.EnteredBy = updateCustomerDto.EnteredBy;
            if (updateCustomerDto.EnteredAt.HasValue)
                existingCustomer.EnteredAt = updateCustomerDto.EnteredAt;
            
            // BirthdateHijri: set to null if empty/null, otherwise update
            if (updateCustomerDto.BirthdateHijri != null)
            {
                existingCustomer.BirthdateHijri = string.IsNullOrWhiteSpace(updateCustomerDto.BirthdateHijri) ? null : updateCustomerDto.BirthdateHijri.Trim();
            }
            if (updateCustomerDto.BirthdateGregorian.HasValue)
                existingCustomer.BirthdateGregorian = updateCustomerDto.BirthdateGregorian;

            if (updateCustomerDto.ZaaerId.HasValue)
                existingCustomer.ZaaerId = updateCustomerDto.ZaaerId;

            existingCustomer.UpdatedAt = KsaTime.Now;

            await _customerRepository.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveChangesAsync();

            // Replace identifications if provided
            // Use zaaer_id from customer (what Zaaer system sent) as customer_id in customer_identifications
            if (updateCustomerDto.Identifications != null)
            {
                // Get the zaaer_id value to use as customer_id in identifications (what Zaaer sent)
                var customerIdForIdentifications = existingCustomer.ZaaerId ?? existingCustomer.CustomerId;
                
                // Remove existing identifications - find by both zaaer_id and database customer_id to handle migration
                var existingIdentifications = await _customerIdentificationRepository.GetAllAsync();
                var customerIdentifications = existingIdentifications
                    .Where(ci => ci.CustomerId == customerIdForIdentifications || 
                                 ci.CustomerId == existingCustomer.CustomerId)
                    .ToList();
                
                foreach (var identification in customerIdentifications)
                {
                    await _customerIdentificationRepository.DeleteAsync(identification);
                }
                
                // Add new identifications using zaaer_id as customer_id (what Zaaer system sent)
                foreach (var identificationDto in updateCustomerDto.Identifications)
                {
                    var identification = new CustomerIdentification
                    {
                        // Use zaaer_id from customer (what Zaaer sent) as customer_id, not the database customer_id
                        CustomerId = customerIdForIdentifications,
                        ZaaerId = identificationDto.ZaaerId,
                        IdTypeId = identificationDto.IdTypeId,
                        IdNumber = identificationDto.IdNumber,
                        CreatedAt = KsaTime.Now,
                        UpdatedAt = KsaTime.Now
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
