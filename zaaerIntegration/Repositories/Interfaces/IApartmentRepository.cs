using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for Apartment repository operations
    /// </summary>
    public interface IApartmentRepository : IGenericRepository<Apartment>
    {
        /// <summary>
        /// Get apartments with pagination and search
        /// </summary>
        Task<(IEnumerable<Apartment> Apartments, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<Apartment, bool>>? filter = null);

        /// <summary>
        /// Get apartments by hotel ID
        /// </summary>
        Task<IEnumerable<Apartment>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get apartments by building ID
        /// </summary>
        Task<IEnumerable<Apartment>> GetByBuildingIdAsync(int buildingId);

        /// <summary>
        /// Get apartments by floor ID
        /// </summary>
        Task<IEnumerable<Apartment>> GetByFloorIdAsync(int floorId);

        /// <summary>
        /// Get apartments by room type ID
        /// </summary>
        Task<IEnumerable<Apartment>> GetByRoomTypeIdAsync(int roomTypeId);

        /// <summary>
        /// Get apartments by status
        /// </summary>
        Task<IEnumerable<Apartment>> GetByStatusAsync(string status);

        /// <summary>
        /// Get apartments by apartment code
        /// </summary>
        Task<Apartment?> GetByApartmentCodeAsync(string apartmentCode);

        /// <summary>
        /// Get apartments by apartment name
        /// </summary>
        Task<IEnumerable<Apartment>> GetByApartmentNameAsync(string apartmentName);

        /// <summary>
        /// Get available apartments
        /// </summary>
        Task<IEnumerable<Apartment>> GetAvailableAsync();

        /// <summary>
        /// Get occupied apartments
        /// </summary>
        Task<IEnumerable<Apartment>> GetOccupiedAsync();

        /// <summary>
        /// Get maintenance apartments
        /// </summary>
        Task<IEnumerable<Apartment>> GetMaintenanceAsync();

        /// <summary>
        /// Get apartments with full details (includes all navigation properties)
        /// </summary>
        Task<Apartment?> GetWithDetailsAsync(int id);

        /// <summary>
        /// Get apartments with full details by hotel ID
        /// </summary>
        Task<IEnumerable<Apartment>> GetWithDetailsByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get apartments with full details by building ID
        /// </summary>
        Task<IEnumerable<Apartment>> GetWithDetailsByBuildingIdAsync(int buildingId);

        /// <summary>
        /// Get apartments with full details by floor ID
        /// </summary>
        Task<IEnumerable<Apartment>> GetWithDetailsByFloorIdAsync(int floorId);

        /// <summary>
        /// Get apartments with full details by room type ID
        /// </summary>
        Task<IEnumerable<Apartment>> GetWithDetailsByRoomTypeIdAsync(int roomTypeId);

        /// <summary>
        /// Get apartment statistics
        /// </summary>
        Task<object> GetStatisticsAsync();

        /// <summary>
        /// Search apartments by name
        /// </summary>
        Task<IEnumerable<Apartment>> SearchByNameAsync(string name);

        /// <summary>
        /// Search apartments by code
        /// </summary>
        Task<IEnumerable<Apartment>> SearchByCodeAsync(string code);

        /// <summary>
        /// Get apartments by hotel name
        /// </summary>
        Task<IEnumerable<Apartment>> GetByHotelNameAsync(string hotelName);

        /// <summary>
        /// Get apartments by building name
        /// </summary>
        Task<IEnumerable<Apartment>> GetByBuildingNameAsync(string buildingName);

        /// <summary>
        /// Get apartments by floor name
        /// </summary>
        Task<IEnumerable<Apartment>> GetByFloorNameAsync(string floorName);

        /// <summary>
        /// Get apartments by room type name
        /// </summary>
        Task<IEnumerable<Apartment>> GetByRoomTypeNameAsync(string roomTypeName);

        /// <summary>
        /// Check if apartment code exists
        /// </summary>
        Task<bool> ApartmentCodeExistsAsync(string apartmentCode, int? excludeId = null);

        /// <summary>
        /// Get apartments with reservations
        /// </summary>
        Task<IEnumerable<Apartment>> GetWithReservationsAsync();

        /// <summary>
        /// Get apartments without reservations
        /// </summary>
        Task<IEnumerable<Apartment>> GetWithoutReservationsAsync();

        /// <summary>
        /// Get apartments by reservation count range
        /// </summary>
        Task<IEnumerable<Apartment>> GetByReservationCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top apartments by reservation count
        /// </summary>
        Task<IEnumerable<Apartment>> GetTopByReservationCountAsync(int topCount = 10);

        /// <summary>
        /// Get apartments by revenue range
        /// </summary>
        Task<IEnumerable<Apartment>> GetByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get top apartments by revenue
        /// </summary>
        Task<IEnumerable<Apartment>> GetTopByRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get apartment availability for date range
        /// </summary>
        Task<IEnumerable<Apartment>> GetAvailableForDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get apartments with overlapping reservations
        /// </summary>
        Task<IEnumerable<Apartment>> GetWithOverlappingReservationsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get apartments by hotel and building
        /// </summary>
        Task<IEnumerable<Apartment>> GetByHotelAndBuildingAsync(int hotelId, int buildingId);

        /// <summary>
        /// Get apartments by hotel and floor
        /// </summary>
        Task<IEnumerable<Apartment>> GetByHotelAndFloorAsync(int hotelId, int floorId);

        /// <summary>
        /// Get apartments by hotel and room type
        /// </summary>
        Task<IEnumerable<Apartment>> GetByHotelAndRoomTypeAsync(int hotelId, int roomTypeId);

        /// <summary>
        /// Get apartments by building and floor
        /// </summary>
        Task<IEnumerable<Apartment>> GetByBuildingAndFloorAsync(int buildingId, int floorId);

        /// <summary>
        /// Get apartments by building and room type
        /// </summary>
        Task<IEnumerable<Apartment>> GetByBuildingAndRoomTypeAsync(int buildingId, int roomTypeId);

        /// <summary>
        /// Get apartments by floor and room type
        /// </summary>
        Task<IEnumerable<Apartment>> GetByFloorAndRoomTypeAsync(int floorId, int roomTypeId);

        /// <summary>
        /// Get apartments by multiple criteria
        /// </summary>
        Task<IEnumerable<Apartment>> GetByMultipleCriteriaAsync(int? hotelId = null, int? buildingId = null, int? floorId = null, int? roomTypeId = null, string? status = null);

        /// <summary>
        /// Get apartment occupancy rate
        /// </summary>
        Task<decimal> GetOccupancyRateAsync(int apartmentId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get apartment revenue
        /// </summary>
        Task<decimal> GetRevenueAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get apartment reservation count
        /// </summary>
        Task<int> GetReservationCountAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get apartment average stay duration
        /// </summary>
        Task<decimal> GetAverageStayDurationAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get apartment utilization statistics
        /// </summary>
        Task<object> GetUtilizationStatisticsAsync(int apartmentId, DateTime startDate, DateTime endDate);
    }
}
