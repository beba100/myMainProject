using AutoMapper;
using Microsoft.Extensions.Logging;
using zaaerIntegration.DTOs.Zaaer;
using FinanceLedgerAPI.Models;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
    /// <summary>
    /// Service for Zaaer Hotel Settings operations
    /// </summary>
    public interface IZaaerHotelSettingsService
    {
        Task<ZaaerHotelSettingsResponseDto> CreateHotelSettingsAsync(ZaaerCreateHotelSettingsDto createHotelSettingsDto);
        Task<ZaaerHotelSettingsResponseDto?> UpdateHotelSettingsAsync(int hotelId, ZaaerUpdateHotelSettingsDto updateHotelSettingsDto);
        Task<ZaaerHotelSettingsResponseDto?> UpdateHotelSettingsByZaaerIdAsync(int zaaerId, ZaaerUpdateHotelSettingsDto updateHotelSettingsDto);
        Task<IEnumerable<ZaaerHotelSettingsResponseDto>> GetAllHotelSettingsAsync();
        Task<ZaaerHotelSettingsResponseDto?> GetHotelSettingsByIdAsync(int hotelId);
        Task<bool> DeleteHotelSettingsAsync(int hotelId);
    }

    /// <summary>
    /// Implementation of Zaaer Hotel Settings service
    /// </summary>
    public class ZaaerHotelSettingsService : IZaaerHotelSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ZaaerHotelSettingsService> _logger;

        public ZaaerHotelSettingsService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ZaaerHotelSettingsService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Create new hotel settings
        /// </summary>
        public async Task<ZaaerHotelSettingsResponseDto> CreateHotelSettingsAsync(ZaaerCreateHotelSettingsDto createHotelSettingsDto)
        {
            try
            {
                var hotelSettings = _mapper.Map<HotelSettings>(createHotelSettingsDto);
                hotelSettings.CreatedAt = KsaTime.Now;
                
                // WORKAROUND: Convert null to empty string for logoUrl to avoid database constraint violation
                if (hotelSettings.LogoUrl == null)
                {
                    hotelSettings.LogoUrl = string.Empty;
                }
                
                await _unitOfWork.HotelSettings.AddAsync(hotelSettings);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ZaaerHotelSettingsResponseDto>(hotelSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel settings");
                throw;
            }
        }

        /// <summary>
        /// Update existing hotel settings
        /// </summary>
        public async Task<ZaaerHotelSettingsResponseDto?> UpdateHotelSettingsAsync(int hotelId, ZaaerUpdateHotelSettingsDto updateHotelSettingsDto)
        {
            try
            {
                var hotelSettings = await _unitOfWork.HotelSettings.GetByIdAsync(hotelId);
                if (hotelSettings == null)
                {
                    return null;
                }

                // Store original logoUrl before mapping
                var originalLogoUrl = hotelSettings.LogoUrl;
                
                _mapper.Map(updateHotelSettingsDto, hotelSettings);
                
                // Explicitly set ZaaerId if provided in the DTO
                if (updateHotelSettingsDto.ZaaerId.HasValue)
                {
                    hotelSettings.ZaaerId = updateHotelSettingsDto.ZaaerId.Value;
                }
                
                // WORKAROUND: Convert null to empty string for logoUrl to avoid database constraint violation
                // This handles both cases: when logoUrl is explicitly sent as null, or when it's omitted
                // After AutoMapper mapping, ensure logoUrl is never null (convert to empty string)
                if (hotelSettings.LogoUrl == null)
                {
                    hotelSettings.LogoUrl = string.Empty;
                }
                
                await _unitOfWork.HotelSettings.UpdateAsync(hotelSettings);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ZaaerHotelSettingsResponseDto>(hotelSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel settings with ID {HotelId}", hotelId);
                throw;
            }
        }

        /// <summary>
        /// Update existing hotel settings by Zaaer ID
        /// </summary>
        public async Task<ZaaerHotelSettingsResponseDto?> UpdateHotelSettingsByZaaerIdAsync(int zaaerId, ZaaerUpdateHotelSettingsDto updateHotelSettingsDto)
        {
            try
            {
                var hotelSettings = await _unitOfWork.HotelSettings.FindSingleAsync(h => h.ZaaerId.HasValue && h.ZaaerId.Value == zaaerId);
                if (hotelSettings == null)
                {
                    _logger.LogWarning("Hotel settings with Zaaer ID {ZaaerId} not found.", zaaerId);
                    return null;
                }

                // Preserve important fields that shouldn't be overwritten
                var originalHotelId = hotelSettings.HotelId;
                var originalCreatedAt = hotelSettings.CreatedAt;
                
                // Map the DTO to the entity
                _mapper.Map(updateHotelSettingsDto, hotelSettings);
                
                // Restore preserved fields
                hotelSettings.HotelId = originalHotelId;
                hotelSettings.CreatedAt = originalCreatedAt;
                
                // Explicitly set ZaaerId if provided in the DTO
                if (updateHotelSettingsDto.ZaaerId.HasValue)
                {
                    hotelSettings.ZaaerId = updateHotelSettingsDto.ZaaerId.Value;
                }
                
                // Map other nullable fields explicitly to preserve null values
                hotelSettings.HotelCode = updateHotelSettingsDto.HotelCode;
                hotelSettings.HotelName = updateHotelSettingsDto.HotelName;
                hotelSettings.DefaultCurrency = updateHotelSettingsDto.DefaultCurrency;
                hotelSettings.CompanyName = updateHotelSettingsDto.CompanyName;
                hotelSettings.TaxNumber = updateHotelSettingsDto.TaxNumber;
                hotelSettings.CrNumber = updateHotelSettingsDto.CrNumber;
                hotelSettings.Phone = updateHotelSettingsDto.Phone;
                hotelSettings.Email = updateHotelSettingsDto.Email;
                hotelSettings.CountryCode = updateHotelSettingsDto.CountryCode;
                hotelSettings.City = updateHotelSettingsDto.City;
                hotelSettings.ContactPerson = updateHotelSettingsDto.ContactPerson;
                hotelSettings.Address = updateHotelSettingsDto.Address;
                hotelSettings.Latitude = updateHotelSettingsDto.Latitude;
                hotelSettings.Longitude = updateHotelSettingsDto.Longitude;
                hotelSettings.PropertyType = updateHotelSettingsDto.PropertyType;
                
                // WORKAROUND: Convert null to empty string for logoUrl to avoid database constraint violation
                // This must be done AFTER all field mappings to ensure we catch any null values
                // This handles both cases: when logoUrl is explicitly sent as null, or when it's omitted
                if (hotelSettings.LogoUrl == null)
                {
                    hotelSettings.LogoUrl = string.Empty;
                }
                
                // Handle nullable int fields
                if (updateHotelSettingsDto.Enabled.HasValue)
                {
                    hotelSettings.Enabled = updateHotelSettingsDto.Enabled.Value;
                }
                if (updateHotelSettingsDto.TotalRooms.HasValue)
                {
                    hotelSettings.TotalRooms = updateHotelSettingsDto.TotalRooms.Value;
                }
                
                await _unitOfWork.HotelSettings.UpdateAsync(hotelSettings);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ZaaerHotelSettingsResponseDto>(hotelSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel settings with Zaaer ID {ZaaerId}. Error: {ErrorMessage}", zaaerId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get all hotel settings
        /// </summary>
        public async Task<IEnumerable<ZaaerHotelSettingsResponseDto>> GetAllHotelSettingsAsync()
        {
            try
            {
                var hotelSettings = await _unitOfWork.HotelSettings.GetAllAsync();
                return _mapper.Map<IEnumerable<ZaaerHotelSettingsResponseDto>>(hotelSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all hotel settings");
                throw;
            }
        }

        /// <summary>
        /// Get hotel settings by ID
        /// </summary>
        public async Task<ZaaerHotelSettingsResponseDto?> GetHotelSettingsByIdAsync(int hotelId)
        {
            try
            {
                var hotelSettings = await _unitOfWork.HotelSettings.GetByIdAsync(hotelId);
                if (hotelSettings == null)
                {
                    return null;
                }

                return _mapper.Map<ZaaerHotelSettingsResponseDto>(hotelSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotel settings with ID {HotelId}", hotelId);
                throw;
            }
        }

        /// <summary>
        /// Delete hotel settings
        /// </summary>
        public async Task<bool> DeleteHotelSettingsAsync(int hotelId)
        {
            try
            {
                var hotelSettings = await _unitOfWork.HotelSettings.GetByIdAsync(hotelId);
                if (hotelSettings == null)
                {
                    return false;
                }

                await _unitOfWork.HotelSettings.DeleteAsync(hotelSettings);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hotel settings with ID {HotelId}", hotelId);
                throw;
            }
        }
    }
}
