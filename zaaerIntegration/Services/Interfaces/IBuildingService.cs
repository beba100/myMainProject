using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for Building service operations
    /// </summary>
    public interface IBuildingService
    {
        /// <summary>
        /// Get all buildings with pagination and search
        /// </summary>
        Task<(IEnumerable<BuildingResponseDto> Buildings, int TotalCount)> GetAllBuildingsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null);

        /// <summary>
        /// Get building by ID
        /// </summary>
        Task<BuildingResponseDto?> GetBuildingByIdAsync(int id);

        /// <summary>
        /// Get building by building number
        /// </summary>
        Task<BuildingResponseDto?> GetBuildingByNumberAsync(string buildingNumber);

        /// <summary>
        /// Create new building
        /// </summary>
        Task<BuildingResponseDto> CreateBuildingAsync(CreateBuildingDto createBuildingDto);

        /// <summary>
        /// Update existing building
        /// </summary>
        Task<BuildingResponseDto?> UpdateBuildingAsync(int id, UpdateBuildingDto updateBuildingDto);

        /// <summary>
        /// Delete building
        /// </summary>
        Task<bool> DeleteBuildingAsync(int id);

        /// <summary>
        /// Get buildings by hotel ID
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get buildings by building name
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsByBuildingNameAsync(string buildingName);

        /// <summary>
        /// Get building statistics
        /// </summary>
        Task<object> GetBuildingStatisticsAsync();

        /// <summary>
        /// Search buildings by name
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> SearchBuildingsByNameAsync(string name);

        /// <summary>
        /// Search buildings by number
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> SearchBuildingsByNumberAsync(string number);

        /// <summary>
        /// Search buildings by hotel name
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> SearchBuildingsByHotelNameAsync(string hotelName);

        /// <summary>
        /// Check if building number exists
        /// </summary>
        Task<bool> BuildingNumberExistsAsync(string buildingNumber, int? excludeId = null);

        /// <summary>
        /// Get buildings with floors
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsWithFloorsAsync();

        /// <summary>
        /// Get buildings without floors
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsWithoutFloorsAsync();

        /// <summary>
        /// Get buildings with apartments
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsWithApartmentsAsync();

        /// <summary>
        /// Get buildings without apartments
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsWithoutApartmentsAsync();

        /// <summary>
        /// Get buildings by floor count range
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsByFloorCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get buildings by apartment count range
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsByApartmentCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top buildings by floor count
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetTopBuildingsByFloorCountAsync(int topCount = 10);

        /// <summary>
        /// Get top buildings by apartment count
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetTopBuildingsByApartmentCountAsync(int topCount = 10);

        /// <summary>
        /// Get buildings by revenue range
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get top buildings by revenue
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetTopBuildingsByRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get buildings by reservation count range
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsByReservationCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top buildings by reservation count
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetTopBuildingsByReservationCountAsync(int topCount = 10);

        /// <summary>
        /// Get building occupancy rate
        /// </summary>
        Task<decimal> GetBuildingOccupancyRateAsync(int buildingId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get building revenue
        /// </summary>
        Task<decimal> GetBuildingRevenueAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building reservation count
        /// </summary>
        Task<int> GetBuildingReservationCountAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building average stay duration
        /// </summary>
        Task<decimal> GetBuildingAverageStayDurationAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building utilization statistics
        /// </summary>
        Task<object> GetBuildingUtilizationStatisticsAsync(int buildingId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get buildings by address
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsByAddressAsync(string address);

        /// <summary>
        /// Search buildings by address
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> SearchBuildingsByAddressAsync(string address);

        /// <summary>
        /// Get buildings by hotel and building number
        /// </summary>
        Task<BuildingResponseDto?> GetBuildingsByHotelAndBuildingNumberAsync(int hotelId, string buildingNumber);

        /// <summary>
        /// Get buildings by hotel and building name
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsByHotelAndBuildingNameAsync(int hotelId, string buildingName);

        /// <summary>
        /// Get buildings by multiple criteria
        /// </summary>
        Task<IEnumerable<BuildingResponseDto>> GetBuildingsByMultipleCriteriaAsync(int? hotelId = null, string? buildingNumber = null, string? buildingName = null, string? address = null);

        /// <summary>
        /// Get building floor statistics
        /// </summary>
        Task<object> GetBuildingFloorStatisticsAsync(int buildingId);

        /// <summary>
        /// Get building apartment statistics
        /// </summary>
        Task<object> GetBuildingApartmentStatisticsAsync(int buildingId);

        /// <summary>
        /// Get building reservation statistics
        /// </summary>
        Task<object> GetBuildingReservationStatisticsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building revenue statistics
        /// </summary>
        Task<object> GetBuildingRevenueStatisticsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building occupancy statistics
        /// </summary>
        Task<object> GetBuildingOccupancyStatisticsAsync(int buildingId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get building performance metrics
        /// </summary>
        Task<object> GetBuildingPerformanceMetricsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
