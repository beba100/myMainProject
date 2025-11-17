using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service implementation for Apartment operations
    /// يستخدم ITenantService للحصول على HotelId من X-Hotel-Code header
    /// </summary>
    public class ApartmentService : IApartmentService
    {
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly ILogger<ApartmentService> _logger;

        public ApartmentService(
            IApartmentRepository apartmentRepository, 
            IMapper mapper,
            ApplicationDbContext context,
            ITenantService tenantService,
            ILogger<ApartmentService> logger)
        {
            _apartmentRepository = apartmentRepository ?? throw new ArgumentNullException(nameof(apartmentRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على HotelId من Tenant (يُقرأ من X-Hotel-Code header)
        /// </summary>
        private async Task<int> GetCurrentHotelIdAsync()
        {
            var tenant = _tenantService.GetTenant();
            if (tenant == null)
            {
                _logger.LogError("Tenant not resolved in ApartmentService.");
                throw new UnauthorizedAccessException("Tenant not resolved. Please ensure X-Hotel-Code header is provided.");
            }

            // البحث عن HotelSettings في Tenant DB باستخدام HotelCode
            var hotelSettings = await _context.HotelSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.HotelCode == tenant.Code);

            if (hotelSettings == null)
            {
                _logger.LogError("HotelSettings not found for tenant code: {TenantCode} in Tenant DB", tenant.Code);
                throw new InvalidOperationException(
                    $"HotelSettings not found for hotel code: {tenant.Code}. " +
                    "Please ensure hotel settings are configured in the tenant database.");
            }

            return hotelSettings.HotelId;
        }

        public async Task<(IEnumerable<ApartmentResponseDto> Apartments, int TotalCount)> GetAllApartmentsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null)
        {
            try
            {
                // Get current HotelId from X-Hotel-Code header
                var hotelId = await GetCurrentHotelIdAsync();
                _logger.LogInformation("Fetching apartments for HotelId: {HotelId}, PageNumber: {PageNumber}, PageSize: {PageSize}", 
                    hotelId, pageNumber, pageSize);

                // Build filter: always filter by current hotel, and optionally by search term
                System.Linq.Expressions.Expression<Func<Apartment, bool>>? filter = a => a.HotelId == hotelId;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var searchFilter = filter;
                    filter = a => a.HotelId == hotelId && (
                        a.ApartmentName.Contains(searchTerm) ||
                        a.ApartmentCode.Contains(searchTerm) ||
                        a.Status.Contains(searchTerm) ||
                        (a.HotelSettings != null && a.HotelSettings.HotelName.Contains(searchTerm)) ||
                        (a.Building != null && a.Building.BuildingName.Contains(searchTerm)) ||
                        (a.Floor != null && a.Floor.FloorName.Contains(searchTerm)) ||
                        (a.RoomType != null && a.RoomType.RoomTypeName.Contains(searchTerm))
                    );
                }

                var (apartments, totalCount) = await _apartmentRepository.GetPagedAsync(
                    pageNumber, 
                    pageSize, 
                    filter);

                _logger.LogInformation("Successfully retrieved {Count} apartments (Total: {TotalCount}) for HotelId: {HotelId}", 
                    apartments.Count(), totalCount, hotelId);

                var apartmentDtos = _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
                return (apartmentDtos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving apartments: {Message}", ex.Message);
                throw new InvalidOperationException($"Error retrieving apartments: {ex.Message}", ex);
            }
        }

        public async Task<ApartmentResponseDto?> GetApartmentByIdAsync(int id)
        {
            try
            {
                var apartment = await _apartmentRepository.GetWithDetailsAsync(id);
                return apartment != null ? _mapper.Map<ApartmentResponseDto>(apartment) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<ApartmentResponseDto?> GetApartmentByCodeAsync(string apartmentCode)
        {
            try
            {
                var apartment = await _apartmentRepository.GetByApartmentCodeAsync(apartmentCode);
                return apartment != null ? _mapper.Map<ApartmentResponseDto>(apartment) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment with code {apartmentCode}: {ex.Message}", ex);
            }
        }

        public async Task<ApartmentResponseDto> CreateApartmentAsync(CreateApartmentDto createApartmentDto)
        {
            try
            {
                // Check if apartment code already exists
                if (await _apartmentRepository.ApartmentCodeExistsAsync(createApartmentDto.ApartmentCode))
                {
                    throw new InvalidOperationException($"Apartment with code '{createApartmentDto.ApartmentCode}' already exists.");
                }

                var apartment = _mapper.Map<Apartment>(createApartmentDto);

                var createdApartment = await _apartmentRepository.AddAsync(apartment);
                return _mapper.Map<ApartmentResponseDto>(createdApartment);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating apartment: {ex.Message}", ex);
            }
        }

        public async Task<ApartmentResponseDto?> UpdateApartmentAsync(int id, UpdateApartmentDto updateApartmentDto)
        {
            try
            {
                var existingApartment = await _apartmentRepository.GetByIdAsync(id);
                if (existingApartment == null)
                {
                    return null;
                }

                // Check if apartment code already exists (excluding current apartment)
                if (await _apartmentRepository.ApartmentCodeExistsAsync(updateApartmentDto.ApartmentCode, id))
                {
                    throw new InvalidOperationException($"Apartment with code '{updateApartmentDto.ApartmentCode}' already exists.");
                }

                _mapper.Map(updateApartmentDto, existingApartment);

                await _apartmentRepository.UpdateAsync(existingApartment);

                return _mapper.Map<ApartmentResponseDto>(existingApartment);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating apartment with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteApartmentAsync(int id)
        {
            try
            {
                var apartment = await _apartmentRepository.GetByIdAsync(id);
                if (apartment == null)
                {
                    return false;
                }

                await _apartmentRepository.DeleteAsync(apartment);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting apartment with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelIdAsync(int hotelId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithDetailsByHotelIdAsync(hotelId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByBuildingIdAsync(int buildingId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithDetailsByBuildingIdAsync(buildingId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments for building {buildingId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByFloorIdAsync(int floorId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithDetailsByFloorIdAsync(floorId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments for floor {floorId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByRoomTypeIdAsync(int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithDetailsByRoomTypeIdAsync(roomTypeId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments for room type {roomTypeId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByStatusAsync(string status)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByStatusAsync(status);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by status {status}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetAvailableApartmentsAsync()
        {
            try
            {
                var apartments = await _apartmentRepository.GetAvailableAsync();
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving available apartments: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetOccupiedApartmentsAsync()
        {
            try
            {
                var apartments = await _apartmentRepository.GetOccupiedAsync();
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving occupied apartments: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetMaintenanceApartmentsAsync()
        {
            try
            {
                var apartments = await _apartmentRepository.GetMaintenanceAsync();
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving maintenance apartments: {ex.Message}", ex);
            }
        }

        public async Task<object> GetApartmentStatisticsAsync()
        {
            try
            {
                return await _apartmentRepository.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment statistics: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByNameAsync(string name)
        {
            try
            {
                var apartments = await _apartmentRepository.SearchByNameAsync(name);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByCodeAsync(string code)
        {
            try
            {
                var apartments = await _apartmentRepository.SearchByCodeAsync(code);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by code: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByHotelNameAsync(string hotelName)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByHotelNameAsync(hotelName);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by hotel name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByBuildingNameAsync(string buildingName)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByBuildingNameAsync(buildingName);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by building name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByFloorNameAsync(string floorName)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByFloorNameAsync(floorName);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by floor name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByRoomTypeNameAsync(string roomTypeName)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByRoomTypeNameAsync(roomTypeName);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by room type name: {ex.Message}", ex);
            }
        }

        public async Task<bool> ApartmentCodeExistsAsync(string apartmentCode, int? excludeId = null)
        {
            try
            {
                return await _apartmentRepository.ApartmentCodeExistsAsync(apartmentCode, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking apartment code existence: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsWithReservationsAsync()
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithReservationsAsync();
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments with reservations: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsWithoutReservationsAsync()
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithoutReservationsAsync();
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments without reservations: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByReservationCountRangeAsync(int minCount, int maxCount)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByReservationCountRangeAsync(minCount, maxCount);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by reservation count range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetTopApartmentsByReservationCountAsync(int topCount = 10)
        {
            try
            {
                var apartments = await _apartmentRepository.GetTopByReservationCountAsync(topCount);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top apartments by reservation count: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByRevenueRangeAsync(minRevenue, maxRevenue);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by revenue range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetTopApartmentsByRevenueAsync(int topCount = 10)
        {
            try
            {
                var apartments = await _apartmentRepository.GetTopByRevenueAsync(topCount);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top apartments by revenue: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetAvailableApartmentsForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var apartments = await _apartmentRepository.GetAvailableForDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving available apartments for date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsWithOverlappingReservationsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithOverlappingReservationsAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments with overlapping reservations: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelAndBuildingAsync(int hotelId, int buildingId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByHotelAndBuildingAsync(hotelId, buildingId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by hotel and building: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelAndFloorAsync(int hotelId, int floorId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByHotelAndFloorAsync(hotelId, floorId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by hotel and floor: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelAndRoomTypeAsync(int hotelId, int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByHotelAndRoomTypeAsync(hotelId, roomTypeId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by hotel and room type: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByBuildingAndFloorAsync(int buildingId, int floorId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByBuildingAndFloorAsync(buildingId, floorId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by building and floor: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByBuildingAndRoomTypeAsync(int buildingId, int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByBuildingAndRoomTypeAsync(buildingId, roomTypeId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by building and room type: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByFloorAndRoomTypeAsync(int floorId, int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByFloorAndRoomTypeAsync(floorId, roomTypeId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by floor and room type: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByMultipleCriteriaAsync(int? hotelId = null, int? buildingId = null, int? floorId = null, int? roomTypeId = null, string? status = null)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByMultipleCriteriaAsync(hotelId, buildingId, floorId, roomTypeId, status);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by multiple criteria: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetApartmentOccupancyRateAsync(int apartmentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _apartmentRepository.GetOccupancyRateAsync(apartmentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment occupancy rate: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetApartmentRevenueAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _apartmentRepository.GetRevenueAsync(apartmentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment revenue: {ex.Message}", ex);
            }
        }

        public async Task<int> GetApartmentReservationCountAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _apartmentRepository.GetReservationCountAsync(apartmentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment reservation count: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetApartmentAverageStayDurationAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _apartmentRepository.GetAverageStayDurationAsync(apartmentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment average stay duration: {ex.Message}", ex);
            }
        }

        public async Task<object> GetApartmentUtilizationStatisticsAsync(int apartmentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _apartmentRepository.GetUtilizationStatisticsAsync(apartmentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment utilization statistics: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateApartmentStatusAsync(int id, string status)
        {
            try
            {
                var apartment = await _apartmentRepository.GetByIdAsync(id);
                if (apartment == null)
                {
                    return false;
                }

                apartment.Status = status;
                await _apartmentRepository.UpdateAsync(apartment);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating apartment status: {ex.Message}", ex);
            }
        }

        public async Task<bool> CheckApartmentAvailabilityAsync(int apartmentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var availableApartments = await _apartmentRepository.GetAvailableForDateRangeAsync(startDate, endDate);
                return availableApartments.Any(a => a.ApartmentId == apartmentId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking apartment availability: {ex.Message}", ex);
            }
        }
    }
}
