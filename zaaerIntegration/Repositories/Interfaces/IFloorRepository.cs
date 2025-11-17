using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for Floor repository operations
    /// </summary>
    public interface IFloorRepository : IGenericRepository<Floor>
    {
        /// <summary>
        /// Get floors with pagination and search
        /// </summary>
        Task<(IEnumerable<Floor> Floors, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<Floor, bool>>? filter = null);

        /// <summary>
        /// Get floors by building ID
        /// </summary>
        Task<IEnumerable<Floor>> GetByBuildingIdAsync(int buildingId);

        /// <summary>
        /// Get floors by floor number
        /// </summary>
        Task<Floor?> GetByFloorNumberAsync(int buildingId, int floorNumber);

        /// <summary>
        /// Get floors by floor name
        /// </summary>
        Task<IEnumerable<Floor>> GetByFloorNameAsync(string floorName);

        /// <summary>
        /// Get floors with full details (includes all navigation properties)
        /// </summary>
        Task<Floor?> GetWithDetailsAsync(int id);

        /// <summary>
        /// Get floors with full details by building ID
        /// </summary>
        Task<IEnumerable<Floor>> GetWithDetailsByBuildingIdAsync(int buildingId);

        /// <summary>
        /// Get floor statistics
        /// </summary>
        Task<object> GetStatisticsAsync();

        /// <summary>
        /// Search floors by name
        /// </summary>
        Task<IEnumerable<Floor>> SearchByNameAsync(string name);

        /// <summary>
        /// Search floors by number
        /// </summary>
        Task<IEnumerable<Floor>> SearchByNumberAsync(int floorNumber);

        /// <summary>
        /// Get floors by building name
        /// </summary>
        Task<IEnumerable<Floor>> GetByBuildingNameAsync(string buildingName);

        /// <summary>
        /// Get floors by hotel name
        /// </summary>
        Task<IEnumerable<Floor>> GetByHotelNameAsync(string hotelName);

        /// <summary>
        /// Check if floor number exists in building
        /// </summary>
        Task<bool> FloorNumberExistsAsync(int buildingId, int floorNumber, int? excludeId = null);

        /// <summary>
        /// Get floors with apartments
        /// </summary>
        Task<IEnumerable<Floor>> GetWithApartmentsAsync();

        /// <summary>
        /// Get floors without apartments
        /// </summary>
        Task<IEnumerable<Floor>> GetWithoutApartmentsAsync();

        /// <summary>
        /// Get floors by apartment count range
        /// </summary>
        Task<IEnumerable<Floor>> GetByApartmentCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top floors by apartment count
        /// </summary>
        Task<IEnumerable<Floor>> GetTopByApartmentCountAsync(int topCount = 10);

        /// <summary>
        /// Get floors by revenue range
        /// </summary>
        Task<IEnumerable<Floor>> GetByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get top floors by revenue
        /// </summary>
        Task<IEnumerable<Floor>> GetTopByRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get floors by reservation count range
        /// </summary>
        Task<IEnumerable<Floor>> GetByReservationCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top floors by reservation count
        /// </summary>
        Task<IEnumerable<Floor>> GetTopByReservationCountAsync(int topCount = 10);

        /// <summary>
        /// Get floor occupancy rate
        /// </summary>
        Task<decimal> GetOccupancyRateAsync(int floorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get floor revenue
        /// </summary>
        Task<decimal> GetRevenueAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get floor reservation count
        /// </summary>
        Task<int> GetReservationCountAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get floor average stay duration
        /// </summary>
        Task<decimal> GetAverageStayDurationAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get floor utilization statistics
        /// </summary>
        Task<object> GetUtilizationStatisticsAsync(int floorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get floors by multiple criteria
        /// </summary>
        Task<IEnumerable<Floor>> GetByMultipleCriteriaAsync(int? buildingId = null, int? floorNumber = null, string? floorName = null);

        /// <summary>
        /// Get floor apartment statistics
        /// </summary>
        Task<object> GetApartmentStatisticsAsync(int floorId);

        /// <summary>
        /// Get floor reservation statistics
        /// </summary>
        Task<object> GetReservationStatisticsAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get floor revenue statistics
        /// </summary>
        Task<object> GetRevenueStatisticsAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get floor occupancy statistics
        /// </summary>
        Task<object> GetOccupancyStatisticsAsync(int floorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get floor performance metrics
        /// </summary>
        Task<object> GetPerformanceMetricsAsync(int floorId, DateTime? startDate = null, DateTime? endDate = null);

        // Bulk Operations
        /// <summary>
        /// Bulk create multiple floors for a building
        /// </summary>
        Task<(IEnumerable<Floor> CreatedFloors, List<string> Errors)> BulkCreateAsync(int buildingId, IEnumerable<CreateFloorItemDto> floors);

        /// <summary>
        /// Bulk update multiple floors
        /// </summary>
        Task<(IEnumerable<Floor> UpdatedFloors, List<string> Errors)> BulkUpdateAsync(IEnumerable<UpdateFloorItemDto> floors);

        /// <summary>
        /// Bulk delete multiple floors
        /// </summary>
        Task<(int DeletedCount, List<string> Errors)> BulkDeleteAsync(IEnumerable<int> floorIds);

        /// <summary>
        /// Get floors by building with floor numbers
        /// </summary>
        Task<IEnumerable<Floor>> GetByBuildingWithFloorNumbersAsync(int buildingId, IEnumerable<int> floorNumbers);

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
        Task<IEnumerable<Floor>> GetByBuildingAndFloorNumberRangeAsync(int buildingId, int minFloorNumber, int maxFloorNumber);

        /// <summary>
        /// Get floors by building and apartment count range
        /// </summary>
        Task<IEnumerable<Floor>> GetByBuildingAndApartmentCountRangeAsync(int buildingId, int minCount, int maxCount);

        /// <summary>
        /// Get floors by building and revenue range
        /// </summary>
        Task<IEnumerable<Floor>> GetByBuildingAndRevenueRangeAsync(int buildingId, decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get floors by building and reservation count range
        /// </summary>
        Task<IEnumerable<Floor>> GetByBuildingAndReservationCountRangeAsync(int buildingId, int minCount, int maxCount);

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
