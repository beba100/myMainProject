using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for Building repository operations
    /// </summary>
    public interface IBuildingRepository : IGenericRepository<Building>
    {
        /// <summary>
        /// Get buildings with pagination and search
        /// </summary>
        Task<(IEnumerable<Building> Buildings, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<Building, bool>>? filter = null);

        /// <summary>
        /// Get buildings by hotel ID
        /// </summary>
        Task<IEnumerable<Building>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get buildings by building number
        /// </summary>
        Task<Building?> GetByBuildingNumberAsync(string buildingNumber);

        /// <summary>
        /// Get buildings by building name
        /// </summary>
        Task<IEnumerable<Building>> GetByBuildingNameAsync(string buildingName);

        /// <summary>
        /// Get buildings with full details (includes all navigation properties)
        /// </summary>
        Task<Building?> GetWithDetailsAsync(int id);

        /// <summary>
        /// Get buildings with full details by hotel ID
        /// </summary>
        Task<IEnumerable<Building>> GetWithDetailsByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get building statistics
        /// </summary>
        Task<object> GetStatisticsAsync();

        /// <summary>
        /// Search buildings by name
        /// </summary>
        Task<IEnumerable<Building>> SearchByNameAsync(string name);

        /// <summary>
        /// Search buildings by number
        /// </summary>
        Task<IEnumerable<Building>> SearchByNumberAsync(string number);

        /// <summary>
        /// Get buildings by hotel name
        /// </summary>
        Task<IEnumerable<Building>> GetByHotelNameAsync(string hotelName);

        /// <summary>
        /// Check if building number exists
        /// </summary>
        Task<bool> BuildingNumberExistsAsync(string buildingNumber, int? excludeId = null);

        /// <summary>
        /// Get buildings with floors
        /// </summary>
        Task<IEnumerable<Building>> GetWithFloorsAsync();

        /// <summary>
        /// Get buildings without floors
        /// </summary>
        Task<IEnumerable<Building>> GetWithoutFloorsAsync();

        /// <summary>
        /// Get buildings with apartments
        /// </summary>
        Task<IEnumerable<Building>> GetWithApartmentsAsync();

        /// <summary>
        /// Get buildings without apartments
        /// </summary>
        Task<IEnumerable<Building>> GetWithoutApartmentsAsync();

        /// <summary>
        /// Get buildings by floor count range
        /// </summary>
        Task<IEnumerable<Building>> GetByFloorCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get buildings by apartment count range
        /// </summary>
        Task<IEnumerable<Building>> GetByApartmentCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top buildings by floor count
        /// </summary>
        Task<IEnumerable<Building>> GetTopByFloorCountAsync(int topCount = 10);

        /// <summary>
        /// Get top buildings by apartment count
        /// </summary>
        Task<IEnumerable<Building>> GetTopByApartmentCountAsync(int topCount = 10);

        /// <summary>
        /// Get buildings by revenue range
        /// </summary>
        Task<IEnumerable<Building>> GetByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get top buildings by revenue
        /// </summary>
        Task<IEnumerable<Building>> GetTopByRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get buildings by reservation count range
        /// </summary>
        Task<IEnumerable<Building>> GetByReservationCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top buildings by reservation count
        /// </summary>
        Task<IEnumerable<Building>> GetTopByReservationCountAsync(int topCount = 10);

        /// <summary>
        /// Get building occupancy rate
        /// </summary>
        Task<decimal> GetOccupancyRateAsync(int buildingId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get building revenue
        /// </summary>
        Task<decimal> GetRevenueAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building reservation count
        /// </summary>
        Task<int> GetReservationCountAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building average stay duration
        /// </summary>
        Task<decimal> GetAverageStayDurationAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building utilization statistics
        /// </summary>
        Task<object> GetUtilizationStatisticsAsync(int buildingId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get buildings by address
        /// </summary>
        Task<IEnumerable<Building>> GetByAddressAsync(string address);

        /// <summary>
        /// Search buildings by address
        /// </summary>
        Task<IEnumerable<Building>> SearchByAddressAsync(string address);

        /// <summary>
        /// Get buildings by hotel and building number
        /// </summary>
        Task<Building?> GetByHotelAndBuildingNumberAsync(int hotelId, string buildingNumber);

        /// <summary>
        /// Get buildings by hotel and building name
        /// </summary>
        Task<IEnumerable<Building>> GetByHotelAndBuildingNameAsync(int hotelId, string buildingName);

        /// <summary>
        /// Get buildings by multiple criteria
        /// </summary>
        Task<IEnumerable<Building>> GetByMultipleCriteriaAsync(int? hotelId = null, string? buildingNumber = null, string? buildingName = null, string? address = null);

        /// <summary>
        /// Get building floor statistics
        /// </summary>
        Task<object> GetFloorStatisticsAsync(int buildingId);

        /// <summary>
        /// Get building apartment statistics
        /// </summary>
        Task<object> GetApartmentStatisticsAsync(int buildingId);

        /// <summary>
        /// Get building reservation statistics
        /// </summary>
        Task<object> GetReservationStatisticsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building revenue statistics
        /// </summary>
        Task<object> GetRevenueStatisticsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get building occupancy statistics
        /// </summary>
        Task<object> GetOccupancyStatisticsAsync(int buildingId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get building performance metrics
        /// </summary>
        Task<object> GetPerformanceMetricsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
