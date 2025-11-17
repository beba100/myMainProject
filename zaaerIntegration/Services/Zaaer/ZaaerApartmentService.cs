using AutoMapper;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;
using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Services.Zaaer
{
    /// <summary>
    /// Service for Zaaer Apartment operations
    /// </summary>
    public interface IZaaerApartmentService
    {
        Task<IEnumerable<ZaaerApartmentResponseDto>> CreateApartmentsAsync(IEnumerable<ZaaerCreateApartmentDto> createApartmentDtos);
        Task<ZaaerApartmentResponseDto> CreateApartmentAsync(ZaaerCreateApartmentDto createApartmentDto);
        Task<ZaaerApartmentResponseDto?> UpdateApartmentAsync(int apartmentId, ZaaerUpdateApartmentDto updateApartmentDto);
        Task<ZaaerApartmentResponseDto?> UpdateApartmentByZaaerIdAsync(int zaaerId, ZaaerUpdateApartmentDto updateApartmentDto);
        Task<ZaaerApartmentResponseDto?> UpdateApartmentByCodeAsync(string apartmentCode, ZaaerUpdateApartmentDto updateApartmentDto);
        Task<IEnumerable<ZaaerApartmentResponseDto>> GetApartmentsByHotelIdAsync(int hotelId);
        Task<ZaaerApartmentResponseDto?> GetApartmentByIdAsync(int apartmentId);
        Task<bool> DeleteApartmentAsync(int apartmentId);
    }

    /// <summary>
    /// Implementation of Zaaer Apartment service
    /// </summary>
    public class ZaaerApartmentService : IZaaerApartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ZaaerApartmentService> _logger;

        public ZaaerApartmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ZaaerApartmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Create multiple apartments (bulk create)
        /// </summary>
        public async Task<IEnumerable<ZaaerApartmentResponseDto>> CreateApartmentsAsync(IEnumerable<ZaaerCreateApartmentDto> createApartmentDtos)
        {
            try
            {
                var apartments = _mapper.Map<IEnumerable<Apartment>>(createApartmentDtos);
                
                foreach (var apartment in apartments)
                {
                    // Set buildingId to null if it's 0 to avoid FK constraint issues
                    if (apartment.BuildingId == 0)
                    {
                        apartment.BuildingId = null;
                    }
                    
                    await _unitOfWork.Apartments.AddAsync(apartment);
                }

                await _unitOfWork.SaveChangesAsync();

                var createdApartments = apartments.ToList();
                return _mapper.Map<IEnumerable<ZaaerApartmentResponseDto>>(createdApartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating apartments");
                throw;
            }
        }

        /// <summary>
        /// Create a single apartment
        /// </summary>
        public async Task<ZaaerApartmentResponseDto> CreateApartmentAsync(ZaaerCreateApartmentDto createApartmentDto)
        {
            try
            {
                var apartment = _mapper.Map<Apartment>(createApartmentDto);

                // Normalize buildingId = 0 to null to avoid FK constraint issues
                if (apartment.BuildingId == 0)
                {
                    apartment.BuildingId = null;
                }

                await _unitOfWork.Apartments.AddAsync(apartment);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ZaaerApartmentResponseDto>(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating apartment");
                throw;
            }
        }

        /// <summary>
        /// Update an existing apartment
        /// </summary>
        public async Task<ZaaerApartmentResponseDto?> UpdateApartmentAsync(int apartmentId, ZaaerUpdateApartmentDto updateApartmentDto)
        {
            try
            {
                var apartment = await _unitOfWork.Apartments.GetByIdAsync(apartmentId);
                if (apartment == null)
                {
                    return null;
                }

                _mapper.Map(updateApartmentDto, apartment);
                
                // Set buildingId to null if it's 0 to avoid FK constraint issues
                if (apartment.BuildingId == 0)
                {
                    apartment.BuildingId = null;
                }
                
                await _unitOfWork.Apartments.UpdateAsync(apartment);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ZaaerApartmentResponseDto>(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating apartment with ID {ApartmentId}", apartmentId);
                throw;
            }
        }

        /// <summary>
        /// Update an existing apartment by Zaaer ID
        /// </summary>
        public async Task<ZaaerApartmentResponseDto?> UpdateApartmentByZaaerIdAsync(int zaaerId, ZaaerUpdateApartmentDto updateApartmentDto)
        {
            try
            {
                var apartments = await _unitOfWork.Apartments.GetAllAsync();
                var apartment = apartments.FirstOrDefault(a => a.ZaaerId == zaaerId);
                
                if (apartment == null)
                {
                    return null;
                }

                _mapper.Map(updateApartmentDto, apartment);
                
                // Set buildingId to null if it's 0 to avoid FK constraint issues
                if (apartment.BuildingId == 0)
                {
                    apartment.BuildingId = null;
                }
                
                await _unitOfWork.Apartments.UpdateAsync(apartment);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ZaaerApartmentResponseDto>(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating apartment with Zaaer ID {ZaaerId}", zaaerId);
                throw;
            }
        }

        /// <summary>
        /// Update an existing apartment by apartment code
        /// </summary>
        public async Task<ZaaerApartmentResponseDto?> UpdateApartmentByCodeAsync(string apartmentCode, ZaaerUpdateApartmentDto updateApartmentDto)
        {
            try
            {
                var apartments = await _unitOfWork.Apartments.GetAllAsync();
                var apartment = apartments.FirstOrDefault(a => a.ApartmentCode == apartmentCode);
                
                if (apartment == null)
                {
                    return null;
                }

                _mapper.Map(updateApartmentDto, apartment);
                
                // Set buildingId to null if it's 0 to avoid FK constraint issues
                if (apartment.BuildingId == 0)
                {
                    apartment.BuildingId = null;
                }
                
                await _unitOfWork.Apartments.UpdateAsync(apartment);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ZaaerApartmentResponseDto>(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating apartment with code {ApartmentCode}", apartmentCode);
                throw;
            }
        }

        /// <summary>
        /// Get all apartments for a specific hotel
        /// </summary>
        public async Task<IEnumerable<ZaaerApartmentResponseDto>> GetApartmentsByHotelIdAsync(int hotelId)
        {
            try
            {
                var apartments = await _unitOfWork.Apartments.GetAllAsync();
                var hotelApartments = apartments.Where(a => a.HotelId == hotelId);
                
                return _mapper.Map<IEnumerable<ZaaerApartmentResponseDto>>(hotelApartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments for hotel {HotelId}", hotelId);
                throw;
            }
        }

        /// <summary>
        /// Get a specific apartment by ID
        /// </summary>
        public async Task<ZaaerApartmentResponseDto?> GetApartmentByIdAsync(int apartmentId)
        {
            try
            {
                var apartment = await _unitOfWork.Apartments.GetByIdAsync(apartmentId);
                if (apartment == null)
                {
                    return null;
                }

                return _mapper.Map<ZaaerApartmentResponseDto>(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartment with ID {ApartmentId}", apartmentId);
                throw;
            }
        }

        /// <summary>
        /// Delete an apartment by ID
        /// </summary>
        public async Task<bool> DeleteApartmentAsync(int apartmentId)
        {
            try
            {
                var apartment = await _unitOfWork.Apartments.GetByIdAsync(apartmentId);
                if (apartment == null)
                {
                    return false;
                }

                await _unitOfWork.Apartments.DeleteAsync(apartment);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting apartment with ID {ApartmentId}", apartmentId);
                throw;
            }
        }
    }
}
