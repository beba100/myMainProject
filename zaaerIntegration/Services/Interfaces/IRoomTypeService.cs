using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for RoomType service operations
    /// </summary>
    public interface IRoomTypeService
    {
        /// <summary>
        /// Get all room types with pagination and search
        /// </summary>
        Task<(IEnumerable<RoomTypeResponseDto> RoomTypes, int TotalCount)> GetAllRoomTypesAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null);

        /// <summary>
        /// Get room type by ID
        /// </summary>
        Task<RoomTypeResponseDto?> GetRoomTypeByIdAsync(int id);

        /// <summary>
        /// Get room type by name in hotel
        /// </summary>
        Task<RoomTypeResponseDto?> GetRoomTypeByNameAsync(int hotelId, string roomTypeName);

        /// <summary>
        /// Create new room type
        /// </summary>
        Task<RoomTypeResponseDto> CreateRoomTypeAsync(CreateRoomTypeDto createRoomTypeDto);

        /// <summary>
        /// Update existing room type
        /// </summary>
        Task<RoomTypeResponseDto?> UpdateRoomTypeAsync(int id, UpdateRoomTypeDto updateRoomTypeDto);

        /// <summary>
        /// Delete room type
        /// </summary>
        Task<bool> DeleteRoomTypeAsync(int id);

        /// <summary>
        /// Get room types by hotel ID
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get room types by room type name
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByNameAsync(string roomTypeName);

        /// <summary>
        /// Get room type statistics
        /// </summary>
        Task<object> GetRoomTypeStatisticsAsync();

        /// <summary>
        /// Search room types by name
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> SearchRoomTypesByNameAsync(string name);

        /// <summary>
        /// Search room types by description
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> SearchRoomTypesByDescriptionAsync(string description);

        /// <summary>
        /// Search room types by hotel name
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> SearchRoomTypesByHotelNameAsync(string hotelName);

        /// <summary>
        /// Check if room type name exists in hotel
        /// </summary>
        Task<bool> RoomTypeNameExistsAsync(int hotelId, string roomTypeName, int? excludeId = null);

        /// <summary>
        /// Get room types with apartments
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesWithApartmentsAsync();

        /// <summary>
        /// Get room types without apartments
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesWithoutApartmentsAsync();

        /// <summary>
        /// Get room types by apartment count range
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByApartmentCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top room types by apartment count
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByApartmentCountAsync(int topCount = 10);

        /// <summary>
        /// Get room types by base rate range
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByBaseRateRangeAsync(decimal minRate, decimal maxRate);

        /// <summary>
        /// Get top room types by base rate
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByBaseRateAsync(int topCount = 10);

        /// <summary>
        /// Get room types by revenue range
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get top room types by revenue
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get room types by reservation count range
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByReservationCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top room types by reservation count
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByReservationCountAsync(int topCount = 10);

        /// <summary>
        /// Get room type occupancy rate
        /// </summary>
        Task<decimal> GetRoomTypeOccupancyRateAsync(int roomTypeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get room type revenue
        /// </summary>
        Task<decimal> GetRoomTypeRevenueAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room type reservation count
        /// </summary>
        Task<int> GetRoomTypeReservationCountAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room type average stay duration
        /// </summary>
        Task<decimal> GetRoomTypeAverageStayDurationAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room type utilization statistics
        /// </summary>
        Task<object> GetRoomTypeUtilizationStatisticsAsync(int roomTypeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get room types by multiple criteria
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByMultipleCriteriaAsync(int? hotelId = null, string? roomTypeName = null, decimal? minBaseRate = null, decimal? maxBaseRate = null);

        /// <summary>
        /// Get room type apartment statistics
        /// </summary>
        Task<object> GetRoomTypeApartmentStatisticsAsync(int roomTypeId);

        /// <summary>
        /// Get room type reservation statistics
        /// </summary>
        Task<object> GetRoomTypeReservationStatisticsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room type revenue statistics
        /// </summary>
        Task<object> GetRoomTypeRevenueStatisticsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room type occupancy statistics
        /// </summary>
        Task<object> GetRoomTypeOccupancyStatisticsAsync(int roomTypeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get room type performance metrics
        /// </summary>
        Task<object> GetRoomTypePerformanceMetricsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room types by hotel and base rate range
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByHotelAndBaseRateRangeAsync(int hotelId, decimal minRate, decimal maxRate);

        /// <summary>
        /// Get room types by hotel and apartment count range
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByHotelAndApartmentCountRangeAsync(int hotelId, int minCount, int maxCount);

        /// <summary>
        /// Get room types by hotel and revenue range
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByHotelAndRevenueRangeAsync(int hotelId, decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get room types by hotel and reservation count range
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByHotelAndReservationCountRangeAsync(int hotelId, int minCount, int maxCount);

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
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByAverageRevenueRangeAsync(decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get top room types by average revenue
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByAverageRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get room types by occupancy rate range
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByOccupancyRateRangeAsync(decimal minRate, decimal maxRate, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get top room types by occupancy rate
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByOccupancyRateAsync(DateTime startDate, DateTime endDate, int topCount = 10);

        /// <summary>
        /// Get room types by average stay duration range
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByAverageStayDurationRangeAsync(decimal minDuration, decimal maxDuration, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get top room types by average stay duration
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByAverageStayDurationAsync(int topCount = 10, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get room types by profitability
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByProfitabilityAsync(decimal minProfitability);

        /// <summary>
        /// Get top room types by profitability
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByProfitabilityAsync(int topCount = 10);

        /// <summary>
        /// Get room types by utilization efficiency
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypesByUtilizationEfficiencyAsync(decimal minEfficiency, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get top room types by utilization efficiency
        /// </summary>
        Task<IEnumerable<RoomTypeResponseDto>> GetTopRoomTypesByUtilizationEfficiencyAsync(DateTime startDate, DateTime endDate, int topCount = 10);
    }
}
