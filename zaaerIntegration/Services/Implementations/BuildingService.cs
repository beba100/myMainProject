using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service implementation for Building operations
    /// </summary>
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IMapper _mapper;

        public BuildingService(IBuildingRepository buildingRepository, IMapper mapper)
        {
            _buildingRepository = buildingRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<BuildingResponseDto> Buildings, int TotalCount)> GetAllBuildingsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null)
        {
            try
            {
                System.Linq.Expressions.Expression<Func<Building, bool>>? filter = null;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filter = b => b.BuildingName.Contains(searchTerm) ||
                                 b.BuildingNumber.Contains(searchTerm) ||
                                 b.Address.Contains(searchTerm) ||
                                 (b.HotelSettings != null && b.HotelSettings.HotelName.Contains(searchTerm));
                }

                var (buildings, totalCount) = await _buildingRepository.GetPagedAsync(
                    pageNumber, 
                    pageSize, 
                    filter);

                var buildingDtos = _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
                return (buildingDtos, totalCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings: {ex.Message}", ex);
            }
        }

        public async Task<BuildingResponseDto?> GetBuildingByIdAsync(int id)
        {
            try
            {
                var building = await _buildingRepository.GetWithDetailsAsync(id);
                return building != null ? _mapper.Map<BuildingResponseDto>(building) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<BuildingResponseDto?> GetBuildingByNumberAsync(string buildingNumber)
        {
            try
            {
                var building = await _buildingRepository.GetByBuildingNumberAsync(buildingNumber);
                return building != null ? _mapper.Map<BuildingResponseDto>(building) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building with number {buildingNumber}: {ex.Message}", ex);
            }
        }

        public async Task<BuildingResponseDto> CreateBuildingAsync(CreateBuildingDto createBuildingDto)
        {
            try
            {
                // Check if building number already exists
                if (!string.IsNullOrEmpty(createBuildingDto.BuildingNumber) && 
                    await _buildingRepository.BuildingNumberExistsAsync(createBuildingDto.BuildingNumber))
                {
                    throw new InvalidOperationException($"Building with number '{createBuildingDto.BuildingNumber}' already exists.");
                }

                var building = _mapper.Map<Building>(createBuildingDto);

                var createdBuilding = await _buildingRepository.AddAsync(building);
                return _mapper.Map<BuildingResponseDto>(createdBuilding);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating building: {ex.Message}", ex);
            }
        }

        public async Task<BuildingResponseDto?> UpdateBuildingAsync(int id, UpdateBuildingDto updateBuildingDto)
        {
            try
            {
                var existingBuilding = await _buildingRepository.GetByIdAsync(id);
                if (existingBuilding == null)
                {
                    return null;
                }

                // Check if building number already exists (excluding current building)
                if (!string.IsNullOrEmpty(updateBuildingDto.BuildingNumber) && 
                    await _buildingRepository.BuildingNumberExistsAsync(updateBuildingDto.BuildingNumber, id))
                {
                    throw new InvalidOperationException($"Building with number '{updateBuildingDto.BuildingNumber}' already exists.");
                }

                _mapper.Map(updateBuildingDto, existingBuilding);

                await _buildingRepository.UpdateAsync(existingBuilding);

                return _mapper.Map<BuildingResponseDto>(existingBuilding);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating building with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteBuildingAsync(int id)
        {
            try
            {
                var building = await _buildingRepository.GetByIdAsync(id);
                if (building == null)
                {
                    return false;
                }

                await _buildingRepository.DeleteAsync(building);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting building with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsByHotelIdAsync(int hotelId)
        {
            try
            {
                var buildings = await _buildingRepository.GetWithDetailsByHotelIdAsync(hotelId);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsByBuildingNameAsync(string buildingName)
        {
            try
            {
                var buildings = await _buildingRepository.GetByBuildingNameAsync(buildingName);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings by name: {ex.Message}", ex);
            }
        }

        public async Task<object> GetBuildingStatisticsAsync()
        {
            try
            {
                return await _buildingRepository.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building statistics: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> SearchBuildingsByNameAsync(string name)
        {
            try
            {
                var buildings = await _buildingRepository.SearchByNameAsync(name);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching buildings by name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> SearchBuildingsByNumberAsync(string number)
        {
            try
            {
                var buildings = await _buildingRepository.SearchByNumberAsync(number);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching buildings by number: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> SearchBuildingsByHotelNameAsync(string hotelName)
        {
            try
            {
                var buildings = await _buildingRepository.GetByHotelNameAsync(hotelName);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching buildings by hotel name: {ex.Message}", ex);
            }
        }

        public async Task<bool> BuildingNumberExistsAsync(string buildingNumber, int? excludeId = null)
        {
            try
            {
                return await _buildingRepository.BuildingNumberExistsAsync(buildingNumber, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking building number existence: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsWithFloorsAsync()
        {
            try
            {
                var buildings = await _buildingRepository.GetWithFloorsAsync();
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings with floors: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsWithoutFloorsAsync()
        {
            try
            {
                var buildings = await _buildingRepository.GetWithoutFloorsAsync();
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings without floors: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsWithApartmentsAsync()
        {
            try
            {
                var buildings = await _buildingRepository.GetWithApartmentsAsync();
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings with apartments: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsWithoutApartmentsAsync()
        {
            try
            {
                var buildings = await _buildingRepository.GetWithoutApartmentsAsync();
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings without apartments: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsByFloorCountRangeAsync(int minCount, int maxCount)
        {
            try
            {
                var buildings = await _buildingRepository.GetByFloorCountRangeAsync(minCount, maxCount);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings by floor count range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsByApartmentCountRangeAsync(int minCount, int maxCount)
        {
            try
            {
                var buildings = await _buildingRepository.GetByApartmentCountRangeAsync(minCount, maxCount);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings by apartment count range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetTopBuildingsByFloorCountAsync(int topCount = 10)
        {
            try
            {
                var buildings = await _buildingRepository.GetTopByFloorCountAsync(topCount);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top buildings by floor count: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetTopBuildingsByApartmentCountAsync(int topCount = 10)
        {
            try
            {
                var buildings = await _buildingRepository.GetTopByApartmentCountAsync(topCount);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top buildings by apartment count: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue)
        {
            try
            {
                var buildings = await _buildingRepository.GetByRevenueRangeAsync(minRevenue, maxRevenue);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings by revenue range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetTopBuildingsByRevenueAsync(int topCount = 10)
        {
            try
            {
                var buildings = await _buildingRepository.GetTopByRevenueAsync(topCount);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top buildings by revenue: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsByReservationCountRangeAsync(int minCount, int maxCount)
        {
            try
            {
                var buildings = await _buildingRepository.GetByReservationCountRangeAsync(minCount, maxCount);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings by reservation count range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetTopBuildingsByReservationCountAsync(int topCount = 10)
        {
            try
            {
                var buildings = await _buildingRepository.GetTopByReservationCountAsync(topCount);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top buildings by reservation count: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetBuildingOccupancyRateAsync(int buildingId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _buildingRepository.GetOccupancyRateAsync(buildingId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building occupancy rate: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetBuildingRevenueAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _buildingRepository.GetRevenueAsync(buildingId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building revenue: {ex.Message}", ex);
            }
        }

        public async Task<int> GetBuildingReservationCountAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _buildingRepository.GetReservationCountAsync(buildingId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building reservation count: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetBuildingAverageStayDurationAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _buildingRepository.GetAverageStayDurationAsync(buildingId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building average stay duration: {ex.Message}", ex);
            }
        }

        public async Task<object> GetBuildingUtilizationStatisticsAsync(int buildingId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _buildingRepository.GetUtilizationStatisticsAsync(buildingId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building utilization statistics: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsByAddressAsync(string address)
        {
            try
            {
                var buildings = await _buildingRepository.GetByAddressAsync(address);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings by address: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> SearchBuildingsByAddressAsync(string address)
        {
            try
            {
                var buildings = await _buildingRepository.SearchByAddressAsync(address);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching buildings by address: {ex.Message}", ex);
            }
        }

        public async Task<BuildingResponseDto?> GetBuildingsByHotelAndBuildingNumberAsync(int hotelId, string buildingNumber)
        {
            try
            {
                var building = await _buildingRepository.GetByHotelAndBuildingNumberAsync(hotelId, buildingNumber);
                return building != null ? _mapper.Map<BuildingResponseDto>(building) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building by hotel and number: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsByHotelAndBuildingNameAsync(int hotelId, string buildingName)
        {
            try
            {
                var buildings = await _buildingRepository.GetByHotelAndBuildingNameAsync(hotelId, buildingName);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings by hotel and name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetBuildingsByMultipleCriteriaAsync(int? hotelId = null, string? buildingNumber = null, string? buildingName = null, string? address = null)
        {
            try
            {
                var buildings = await _buildingRepository.GetByMultipleCriteriaAsync(hotelId, buildingNumber, buildingName, address);
                return _mapper.Map<IEnumerable<BuildingResponseDto>>(buildings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving buildings by multiple criteria: {ex.Message}", ex);
            }
        }

        public async Task<object> GetBuildingFloorStatisticsAsync(int buildingId)
        {
            try
            {
                return await _buildingRepository.GetFloorStatisticsAsync(buildingId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building floor statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetBuildingApartmentStatisticsAsync(int buildingId)
        {
            try
            {
                return await _buildingRepository.GetApartmentStatisticsAsync(buildingId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building apartment statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetBuildingReservationStatisticsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _buildingRepository.GetReservationStatisticsAsync(buildingId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building reservation statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetBuildingRevenueStatisticsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _buildingRepository.GetRevenueStatisticsAsync(buildingId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building revenue statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetBuildingOccupancyStatisticsAsync(int buildingId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _buildingRepository.GetOccupancyStatisticsAsync(buildingId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building occupancy statistics: {ex.Message}", ex);
            }
        }

        public async Task<object> GetBuildingPerformanceMetricsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _buildingRepository.GetPerformanceMetricsAsync(buildingId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving building performance metrics: {ex.Message}", ex);
            }
        }
    }
}
