using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for RoomType repository operations
    /// </summary>
    public interface IRoomTypeRepository : IGenericRepository<RoomType>
    {
        /// <summary>
        /// Get room types with pagination and search
        /// </summary>
        Task<(IEnumerable<RoomType> RoomTypes, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<RoomType, bool>>? filter = null);

        /// <summary>
        /// Get room types by hotel ID
        /// </summary>
        Task<IEnumerable<RoomType>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get room type by name
        /// </summary>
        Task<RoomType?> GetByNameAsync(int hotelId, string roomTypeName);

        /// <summary>
        /// Get room types by name
        /// </summary>
        Task<IEnumerable<RoomType>> GetByNameAsync(string roomTypeName);

        /// <summary>
        /// Get room types with full details (includes all navigation properties)
        /// </summary>
        Task<RoomType?> GetWithDetailsAsync(int id);

        /// <summary>
        /// Get room types with full details by hotel ID
        /// </summary>
        Task<IEnumerable<RoomType>> GetWithDetailsByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get room type statistics
        /// </summary>
        Task<object> GetStatisticsAsync();

        /// <summary>
        /// Search room types by name
        /// </summary>
        Task<IEnumerable<RoomType>> SearchByNameAsync(string name);

        /// <summary>
        /// Search room types by description
        /// </summary>
        Task<IEnumerable<RoomType>> SearchByDescriptionAsync(string description);

        /// <summary>
        /// Get room types by hotel name
        /// </summary>
        Task<IEnumerable<RoomType>> GetByHotelNameAsync(string hotelName);

        /// <summary>
        /// Check if room type name exists in hotel
        /// </summary>
        Task<bool> RoomTypeNameExistsAsync(int hotelId, string roomTypeName, int? excludeId = null);

        /// <summary>
        /// Get room types with apartments
        /// </summary>
        Task<IEnumerable<RoomType>> GetWithApartmentsAsync();

        /// <summary>
        /// Get room types without apartments
        /// </summary>
        Task<IEnumerable<RoomType>> GetWithoutApartmentsAsync();

        /// <summary>
        /// Get room types by apartment count range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByApartmentCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top room types by apartment count
        /// </summary>
        Task<IEnumerable<RoomType>> GetTopByApartmentCountAsync(int topCount = 10);

        /// <summary>
        /// Get room types by base rate range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByBaseRateRangeAsync(decimal minRate, decimal maxRate);

        /// <summary>
        /// Get top room types by base rate
        /// </summary>
        Task<IEnumerable<RoomType>> GetTopByBaseRateAsync(int topCount = 10);

        /// <summary>
        /// Get room types by revenue range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get top room types by revenue
        /// </summary>
        Task<IEnumerable<RoomType>> GetTopByRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get room types by reservation count range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByReservationCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top room types by reservation count
        /// </summary>
        Task<IEnumerable<RoomType>> GetTopByReservationCountAsync(int topCount = 10);

        /// <summary>
        /// Get room type occupancy rate
        /// </summary>
        Task<decimal> GetOccupancyRateAsync(int roomTypeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get room type revenue
        /// </summary>
        Task<decimal> GetRevenueAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room type reservation count
        /// </summary>
        Task<int> GetReservationCountAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room type average stay duration
        /// </summary>
        Task<decimal> GetAverageStayDurationAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room type utilization statistics
        /// </summary>
        Task<object> GetUtilizationStatisticsAsync(int roomTypeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get room types by multiple criteria
        /// </summary>
        Task<IEnumerable<RoomType>> GetByMultipleCriteriaAsync(int? hotelId = null, string? roomTypeName = null, decimal? minBaseRate = null, decimal? maxBaseRate = null);

        /// <summary>
        /// Get room type apartment statistics
        /// </summary>
        Task<object> GetApartmentStatisticsAsync(int roomTypeId);

        /// <summary>
        /// Get room type reservation statistics
        /// </summary>
        Task<object> GetReservationStatisticsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room type revenue statistics
        /// </summary>
        Task<object> GetRevenueStatisticsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room type occupancy statistics
        /// </summary>
        Task<object> GetOccupancyStatisticsAsync(int roomTypeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get room type performance metrics
        /// </summary>
        Task<object> GetPerformanceMetricsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room types by hotel and base rate range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByHotelAndBaseRateRangeAsync(int hotelId, decimal minRate, decimal maxRate);

        /// <summary>
        /// Get room types by hotel and apartment count range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByHotelAndApartmentCountRangeAsync(int hotelId, int minCount, int maxCount);

        /// <summary>
        /// Get room types by hotel and revenue range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByHotelAndRevenueRangeAsync(int hotelId, decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get room types by hotel and reservation count range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByHotelAndReservationCountRangeAsync(int hotelId, int minCount, int maxCount);

        /// <summary>
        /// Get hotel room type statistics
        /// </summary>
        Task<object> GetHotelRoomTypeStatisticsAsync(int hotelId);

        /// <summary>
        /// Get hotel room type occupancy statistics
        /// </summary>
        Task<object> GetHotelRoomTypeOccupancyStatisticsAsync(int hotelId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get hotel room type revenue statistics
        /// </summary>
        Task<object> GetHotelRoomTypeRevenueStatisticsAsync(int hotelId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get hotel room type performance metrics
        /// </summary>
        Task<object> GetHotelRoomTypePerformanceMetricsAsync(int hotelId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room types by average revenue range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByAverageRevenueRangeAsync(decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get top room types by average revenue
        /// </summary>
        Task<IEnumerable<RoomType>> GetTopByAverageRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get room types by occupancy rate range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByOccupancyRateRangeAsync(decimal minRate, decimal maxRate, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get top room types by occupancy rate
        /// </summary>
        Task<IEnumerable<RoomType>> GetTopByOccupancyRateAsync(DateTime startDate, DateTime endDate, int topCount = 10);

        /// <summary>
        /// Get room types by average stay duration range
        /// </summary>
        Task<IEnumerable<RoomType>> GetByAverageStayDurationRangeAsync(decimal minDuration, decimal maxDuration, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get top room types by average stay duration
        /// </summary>
        Task<IEnumerable<RoomType>> GetTopByAverageStayDurationAsync(int topCount = 10, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room types by profitability (revenue vs base rate)
        /// </summary>
        Task<IEnumerable<RoomType>> GetByProfitabilityAsync(decimal minProfitability);

        /// <summary>
        /// Get top room types by profitability
        /// </summary>
        Task<IEnumerable<RoomType>> GetTopByProfitabilityAsync(int topCount = 10);

        /// <summary>
        /// Get room types by utilization efficiency
        /// </summary>
        Task<IEnumerable<RoomType>> GetByUtilizationEfficiencyAsync(decimal minEfficiency, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get top room types by utilization efficiency
        /// </summary>
        Task<IEnumerable<RoomType>> GetTopByUtilizationEfficiencyAsync(DateTime startDate, DateTime endDate, int topCount = 10);
    }
}
