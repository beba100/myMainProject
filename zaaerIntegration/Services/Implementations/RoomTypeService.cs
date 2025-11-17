using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service implementation for RoomType operations
    /// </summary>
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly IMapper _mapper;

        public RoomTypeService(IRoomTypeRepository roomTypeRepository, IMapper mapper)
        {
            _roomTypeRepository = roomTypeRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<RoomTypeResponseDto> RoomTypes, int TotalCount)> GetAllRoomTypesAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null)
        {
            try
            {
                System.Linq.Expressions.Expression<Func<RoomType, bool>>? filter = null;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filter = rt => rt.RoomTypeName.Contains(searchTerm) ||
                                 (rt.RoomTypeDesc != null && rt.RoomTypeDesc.Contains(searchTerm)) ||
                                 (rt.HotelSettings != null && rt.HotelSettings.HotelName.Contains(searchTerm));
                }

                var (roomTypes, totalCount) = await _roomTypeRepository.GetPagedAsync(
                    pageNumber, 
                    pageSize, 
                    filter);

                var roomTypeDtos = _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
                return (roomTypeDtos, totalCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types: {ex.Message}", ex);
            }
        }

        public async Task<RoomTypeResponseDto?> GetRoomTypeByIdAsync(int id)
        {
            try
            {
                var roomType = await _roomTypeRepository.GetWithDetailsAsync(id);
                return roomType != null ? _mapper.Map<RoomTypeResponseDto>(roomType) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<RoomTypeResponseDto?> GetRoomTypeByNameAsync(int hotelId, string roomTypeName)
        {
            try
            {
                var roomType = await _roomTypeRepository.GetByNameAsync(hotelId, roomTypeName);
                return roomType != null ? _mapper.Map<RoomTypeResponseDto>(roomType) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type with name {roomTypeName} in hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<RoomTypeResponseDto> CreateRoomTypeAsync(CreateRoomTypeDto createRoomTypeDto)
        {
            try
            {
                // Check if room type name already exists in hotel
                if (await _roomTypeRepository.RoomTypeNameExistsAsync(createRoomTypeDto.HotelId, createRoomTypeDto.RoomTypeName))
                {
                    throw new InvalidOperationException($"Room type with name '{createRoomTypeDto.RoomTypeName}' already exists in hotel {createRoomTypeDto.HotelId}.");
                }

                var roomType = _mapper.Map<RoomType>(createRoomTypeDto);

                var createdRoomType = await _roomTypeRepository.AddAsync(roomType);
                return _mapper.Map<RoomTypeResponseDto>(createdRoomType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating room type: {ex.Message}", ex);
            }
        }

        public async Task<RoomTypeResponseDto?> UpdateRoomTypeAsync(int id, UpdateRoomTypeDto updateRoomTypeDto)
        {
            try
            {
                var existingRoomType = await _roomTypeRepository.GetByIdAsync(id);
                if (existingRoomType == null)
                {
                    return null;
                }

                // Check if room type name already exists (excluding current room type)
                if (await _roomTypeRepository.RoomTypeNameExistsAsync(existingRoomType.HotelId, updateRoomTypeDto.RoomTypeName, id))
                {
                    throw new InvalidOperationException($"Room type with name '{updateRoomTypeDto.RoomTypeName}' already exists in hotel {existingRoomType.HotelId}.");
                }

                _mapper.Map(updateRoomTypeDto, existingRoomType);

                await _roomTypeRepository.UpdateAsync(existingRoomType);

                return _mapper.Map<RoomTypeResponseDto>(existingRoomType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating room type with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteRoomTypeAsync(int id)
        {
            try
            {
                var roomType = await _roomTypeRepository.GetByIdAsync(id);
                if (roomType == null)
                {
                    return false;
                }

                await _roomTypeRepository.DeleteAsync(roomType);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting room type with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByHotelIdAsync(int hotelId)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetWithDetailsByHotelIdAsync(hotelId);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByNameAsync(string roomTypeName)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByNameAsync(roomTypeName);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by name: {ex.Message}", ex);
            }
        }

        public async Task<object> GetRoomTypeStatisticsAsync()
        {
            try
            {
                return await _roomTypeRepository.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type statistics: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> SearchRoomTypesByNameAsync(string name)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.SearchByNameAsync(name);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching room types by name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> SearchRoomTypesByDescriptionAsync(string description)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.SearchByDescriptionAsync(description);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching room types by description: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> SearchRoomTypesByHotelNameAsync(string hotelName)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByHotelNameAsync(hotelName);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching room types by hotel name: {ex.Message}", ex);
            }
        }

        public async Task<bool> RoomTypeNameExistsAsync(int hotelId, string roomTypeName, int? excludeId = null)
        {
            try
            {
                return await _roomTypeRepository.RoomTypeNameExistsAsync(hotelId, roomTypeName, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking room type name existence: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesWithApartmentsAsync()
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetWithApartmentsAsync();
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types with apartments: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesWithoutApartmentsAsync()
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetWithoutApartmentsAsync();
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types without apartments: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByApartmentCountRangeAsync(int minCount, int maxCount)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByApartmentCountRangeAsync(minCount, maxCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by apartment count range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByApartmentCountAsync(int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetTopByApartmentCountAsync(topCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top room types by apartment count: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByBaseRateRangeAsync(decimal minRate, decimal maxRate)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByBaseRateRangeAsync(minRate, maxRate);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by base rate range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByBaseRateAsync(int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetTopByBaseRateAsync(topCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top room types by base rate: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByRevenueRangeAsync(minRevenue, maxRevenue);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by revenue range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByRevenueAsync(int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetTopByRevenueAsync(topCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top room types by revenue: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByReservationCountRangeAsync(int minCount, int maxCount)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByReservationCountRangeAsync(minCount, maxCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by reservation count range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByReservationCountAsync(int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetTopByReservationCountAsync(topCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top room types by reservation count: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetRoomTypeOccupancyRateAsync(int roomTypeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _roomTypeRepository.GetOccupancyRateAsync(roomTypeId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type occupancy rate: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetRoomTypeRevenueAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _roomTypeRepository.GetRevenueAsync(roomTypeId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type revenue: {ex.Message}", ex);
            }
        }

        public async Task<int> GetRoomTypeReservationCountAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _roomTypeRepository.GetReservationCountAsync(roomTypeId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type reservation count: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetRoomTypeAverageStayDurationAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _roomTypeRepository.GetAverageStayDurationAsync(roomTypeId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type average stay duration: {ex.Message}", ex);
            }
        }

        public async Task<object> GetRoomTypeUtilizationStatisticsAsync(int roomTypeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _roomTypeRepository.GetUtilizationStatisticsAsync(roomTypeId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type utilization statistics: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByMultipleCriteriaAsync(int? hotelId = null, string? roomTypeName = null, decimal? minBaseRate = null, decimal? maxBaseRate = null)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByMultipleCriteriaAsync(hotelId, roomTypeName, minBaseRate, maxBaseRate);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by multiple criteria: {ex.Message}", ex);
            }
        }

        public async Task<object> GetRoomTypeApartmentStatisticsAsync(int roomTypeId)
        {
            try
            {
                return await _roomTypeRepository.GetApartmentStatisticsAsync(roomTypeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type apartment statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetRoomTypeReservationStatisticsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _roomTypeRepository.GetReservationStatisticsAsync(roomTypeId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type reservation statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetRoomTypeRevenueStatisticsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _roomTypeRepository.GetRevenueStatisticsAsync(roomTypeId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type revenue statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetRoomTypeOccupancyStatisticsAsync(int roomTypeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _roomTypeRepository.GetOccupancyStatisticsAsync(roomTypeId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type occupancy statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetRoomTypePerformanceMetricsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _roomTypeRepository.GetPerformanceMetricsAsync(roomTypeId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room type performance metrics: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByHotelAndBaseRateRangeAsync(int hotelId, decimal minRate, decimal maxRate)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByHotelAndBaseRateRangeAsync(hotelId, minRate, maxRate);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by hotel and base rate range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByHotelAndApartmentCountRangeAsync(int hotelId, int minCount, int maxCount)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByHotelAndApartmentCountRangeAsync(hotelId, minCount, maxCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by hotel and apartment count range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByHotelAndRevenueRangeAsync(int hotelId, decimal minRevenue, decimal maxRevenue)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByHotelAndRevenueRangeAsync(hotelId, minRevenue, maxRevenue);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by hotel and revenue range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByHotelAndReservationCountRangeAsync(int hotelId, int minCount, int maxCount)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByHotelAndReservationCountRangeAsync(hotelId, minCount, maxCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by hotel and reservation count range: {ex.Message}", ex);
            }
        }

        public async Task<object> GetHotelRoomTypeStatisticsAsync(int hotelId)
        {
            try
            {
                return await _roomTypeRepository.GetHotelRoomTypeStatisticsAsync(hotelId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving hotel room type statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetHotelRoomTypeOccupancyStatisticsAsync(int hotelId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _roomTypeRepository.GetHotelRoomTypeOccupancyStatisticsAsync(hotelId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving hotel room type occupancy statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetHotelRoomTypeRevenueStatisticsAsync(int hotelId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _roomTypeRepository.GetHotelRoomTypeRevenueStatisticsAsync(hotelId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving hotel room type revenue statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetHotelRoomTypePerformanceMetricsAsync(int hotelId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _roomTypeRepository.GetHotelRoomTypePerformanceMetricsAsync(hotelId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving hotel room type performance metrics: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByAverageRevenueRangeAsync(decimal minRevenue, decimal maxRevenue)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByAverageRevenueRangeAsync(minRevenue, maxRevenue);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by average revenue range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByAverageRevenueAsync(int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetTopByAverageRevenueAsync(topCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top room types by average revenue: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByOccupancyRateRangeAsync(decimal minRate, decimal maxRate, DateTime startDate, DateTime endDate)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByOccupancyRateRangeAsync(minRate, maxRate, startDate, endDate);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by occupancy rate range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByOccupancyRateAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetTopByOccupancyRateAsync(startDate, endDate, topCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top room types by occupancy rate: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByAverageStayDurationRangeAsync(decimal minDuration, decimal maxDuration, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByAverageStayDurationRangeAsync(minDuration, maxDuration, startDate, endDate);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by average stay duration range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByAverageStayDurationAsync(int topCount = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetTopByAverageStayDurationAsync(topCount, startDate, endDate);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top room types by average stay duration: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByProfitabilityAsync(decimal minProfitability)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByProfitabilityAsync(minProfitability);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by profitability: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByProfitabilityAsync(int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetTopByProfitabilityAsync(topCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top room types by profitability: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByUtilizationEfficiencyAsync(decimal minEfficiency, DateTime startDate, DateTime endDate)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByUtilizationEfficiencyAsync(minEfficiency, startDate, endDate);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving room types by utilization efficiency: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByUtilizationEfficiencyAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetTopByUtilizationEfficiencyAsync(startDate, endDate, topCount);
                return _mapper.Map<IEnumerable<RoomTypeResponseDto>>(roomTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top room types by utilization efficiency: {ex.Message}", ex);
            }
        }
    }
}
