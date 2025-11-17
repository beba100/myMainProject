using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for Floor service operations
    /// </summary>
    public interface IFloorService
    {
        /// <summary>
        /// Get all floors with pagination and search
        /// </summary>
        Task<(IEnumerable<FloorResponseDto> Floors, int TotalCount)> GetAllFloorsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null);

        /// <summary>
        /// Get floor by ID
        /// </summary>
        Task<FloorResponseDto?> GetFloorByIdAsync(int id);

        /// <summary>
        /// Get floor by floor number
        /// </summary>
        Task<FloorResponseDto?> GetFloorByNumberAsync(int buildingId, int floorNumber);

        /// <summary>
        /// Create new floor
        /// </summary>
        Task<FloorResponseDto> CreateFloorAsync(CreateFloorDto createFloorDto);

        /// <summary>
        /// Update existing floor
        /// </summary>
        Task<FloorResponseDto?> UpdateFloorAsync(int id, UpdateFloorDto updateFloorDto);

        /// <summary>
        /// Delete floor
        /// </summary>
        Task<bool> DeleteFloorAsync(int id);

        /// <summary>
        /// Get floors by building ID
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByBuildingIdAsync(int buildingId);

        /// <summary>
        /// Get floors by floor name
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByFloorNameAsync(string floorName);

        /// <summary>
        /// Get floor statistics
        /// </summary>
        Task<object> GetFloorStatisticsAsync();

        /// <summary>
        /// Search floors by name
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> SearchFloorsByNameAsync(string name);

        /// <summary>
        /// Search floors by number
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> SearchFloorsByNumberAsync(int floorNumber);

        /// <summary>
        /// Search floors by building name
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> SearchFloorsByBuildingNameAsync(string buildingName);

        /// <summary>
        /// Search floors by hotel name
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> SearchFloorsByHotelNameAsync(string hotelName);

        /// <summary>
        /// Check if floor number exists in building
        /// </summary>
        Task<bool> FloorNumberExistsAsync(int buildingId, int floorNumber, int? excludeId = null);

        /// <summary>
        /// Get floors with apartments
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsWithApartmentsAsync();

        /// <summary>
        /// Get floors without apartments
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsWithoutApartmentsAsync();

        /// <summary>
        /// Get floors by apartment count range
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByApartmentCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top floors by apartment count
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetTopFloorsByApartmentCountAsync(int topCount = 10);

        /// <summary>
        /// Get floors by revenue range
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get top floors by revenue
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetTopFloorsByRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get floors by reservation count range
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByReservationCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top floors by reservation count
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetTopFloorsByReservationCountAsync(int topCount = 10);

        /// <summary>
        /// Get floor occupancy rate
        /// </summary>
        Task<decimal> GetFloorOccupancyRateAsync(int floorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get floor revenue
        /// </summary>
        Task<decimal> GetFloorRevenueAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get floor reservation count
        /// </summary>
        Task<int> GetFloorReservationCountAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get floor average stay duration
        /// </summary>
        Task<decimal> GetFloorAverageStayDurationAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get floor utilization statistics
        /// </summary>
        Task<object> GetFloorUtilizationStatisticsAsync(int floorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get floors by multiple criteria
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByMultipleCriteriaAsync(int? buildingId = null, int? floorNumber = null, string? floorName = null);

        /// <summary>
        /// Get floor apartment statistics
        /// </summary>
        Task<object> GetFloorApartmentStatisticsAsync(int floorId);

        /// <summary>
        /// Get floor reservation statistics
        /// </summary>
        Task<object> GetFloorReservationStatisticsAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get floor revenue statistics
        /// </summary>
        Task<object> GetFloorRevenueStatisticsAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get floor occupancy statistics
        /// </summary>
        Task<object> GetFloorOccupancyStatisticsAsync(int floorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get floor performance metrics
        /// </summary>
        Task<object> GetFloorPerformanceMetricsAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        // Bulk Operations
        /// <summary>
        /// Bulk create multiple floors for a building
        /// </summary>
        Task<BulkFloorResponseDto> BulkCreateFloorsAsync(BulkCreateFloorDto bulkCreateFloorDto);

        /// <summary>
        /// Bulk update multiple floors
        /// </summary>
        Task<BulkFloorResponseDto> BulkUpdateFloorsAsync(BulkUpdateFloorDto bulkUpdateFloorDto);

        /// <summary>
        /// Bulk delete multiple floors
        /// </summary>
        Task<(int DeletedCount, List<string> Errors)> BulkDeleteFloorsAsync(IEnumerable<int> floorIds);

        /// <summary>
        /// Get floors by building with floor numbers
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByBuildingWithFloorNumbersAsync(int buildingId, IEnumerable<int> floorNumbers);

        /// <summary>
        /// Get next available floor number for building
        /// </summary>
        Task<int> GetNextAvailableFloorNumberAsync(int buildingId);

        /// <summary>
        /// Get floor numbers in building
        /// </summary>
        Task<IEnumerable<int>> GetFloorNumbersInBuildingAsync(int buildingId);

        /// <summary>
        /// Check if floor numbers exist in building
        /// </summary>
        Task<Dictionary<int, bool>> CheckFloorNumbersExistAsync(int buildingId, IEnumerable<int> floorNumbers);

        /// <summary>
        /// Get floors by building and floor number range
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByBuildingAndFloorNumberRangeAsync(int buildingId, int minFloorNumber, int maxFloorNumber);

        /// <summary>
        /// Get floors by building and apartment count range
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByBuildingAndApartmentCountRangeAsync(int buildingId, int minCount, int maxCount);

        /// <summary>
        /// Get floors by building and revenue range
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByBuildingAndRevenueRangeAsync(int buildingId, decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get floors by building and reservation count range
        /// </summary>
        Task<IEnumerable<FloorResponseDto>> GetFloorsByBuildingAndReservationCountRangeAsync(int buildingId, int minCount, int maxCount);

        /// <summary>
        /// Get building floor statistics
        /// </summary>
        Task<object> GetBuildingFloorStatisticsAsync(int buildingId);

        /// <summary>
        /// Get building floor occupancy statistics
        /// </summary>
        Task<object> GetBuildingFloorOccupancyStatisticsAsync(int buildingId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get building floor revenue statistics
        /// </summary>
        Task<object> GetBuildingFloorRevenueStatisticsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building floor performance metrics
        /// </summary>
        Task<object> GetBuildingFloorPerformanceMetricsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
