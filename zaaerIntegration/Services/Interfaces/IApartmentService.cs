using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for Apartment service operations
    /// </summary>
    public interface IApartmentService
    {
        /// <summary>
        /// Get all apartments with pagination and search
        /// </summary>
        Task<(IEnumerable<ApartmentResponseDto> Apartments, int TotalCount)> GetAllApartmentsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null);

        /// <summary>
        /// Lightweight list used by the expenses page (apartment dropdown).
        /// </summary>
        Task<IEnumerable<ApartmentLookupDto>> GetApartmentLookupAsync();

        /// <summary>
        /// Get apartment by ID
        /// </summary>
        Task<ApartmentResponseDto?> GetApartmentByIdAsync(int id);

        /// <summary>
        /// Get apartment by apartment code
        /// </summary>
        Task<ApartmentResponseDto?> GetApartmentByCodeAsync(string apartmentCode);

        /// <summary>
        /// Create new apartment
        /// </summary>
        Task<ApartmentResponseDto> CreateApartmentAsync(CreateApartmentDto createApartmentDto);

        /// <summary>
        /// Update existing apartment
        /// </summary>
        Task<ApartmentResponseDto?> UpdateApartmentAsync(int id, UpdateApartmentDto updateApartmentDto);

        /// <summary>
        /// Delete apartment
        /// </summary>
        Task<bool> DeleteApartmentAsync(int id);

        /// <summary>
        /// Get apartments by hotel ID
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get apartments by building ID
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByBuildingIdAsync(int buildingId);

        /// <summary>
        /// Get apartments by floor ID
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByFloorIdAsync(int floorId);

        /// <summary>
        /// Get apartments by room type ID
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByRoomTypeIdAsync(int roomTypeId);

        /// <summary>
        /// Get apartments by status
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByStatusAsync(string status);

        /// <summary>
        /// Get available apartments
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetAvailableApartmentsAsync();

        /// <summary>
        /// Get occupied apartments
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetOccupiedApartmentsAsync();

        /// <summary>
        /// Get maintenance apartments
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetMaintenanceApartmentsAsync();

        /// <summary>
        /// Get apartment statistics
        /// </summary>
        Task<object> GetApartmentStatisticsAsync();

        /// <summary>
        /// Search apartments by name
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByNameAsync(string name);

        /// <summary>
        /// Search apartments by code
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByCodeAsync(string code);

        /// <summary>
        /// Search apartments by hotel name
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByHotelNameAsync(string hotelName);

        /// <summary>
        /// Search apartments by building name
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByBuildingNameAsync(string buildingName);

        /// <summary>
        /// Search apartments by floor name
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByFloorNameAsync(string floorName);

        /// <summary>
        /// Search apartments by room type name
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByRoomTypeNameAsync(string roomTypeName);

        /// <summary>
        /// Check if apartment code exists
        /// </summary>
        Task<bool> ApartmentCodeExistsAsync(string apartmentCode, int? excludeId = null);

        /// <summary>
        /// Get apartments with reservations
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsWithReservationsAsync();

        /// <summary>
        /// Get apartments without reservations
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsWithoutReservationsAsync();

        /// <summary>
        /// Get apartments by reservation count range
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByReservationCountRangeAsync(int minCount, int maxCount);

        /// <summary>
        /// Get top apartments by reservation count
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetTopApartmentsByReservationCountAsync(int topCount = 10);

        /// <summary>
        /// Get apartments by revenue range
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue);

        /// <summary>
        /// Get top apartments by revenue
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetTopApartmentsByRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get available apartments for date range
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetAvailableApartmentsForDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get apartments with overlapping reservations
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsWithOverlappingReservationsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get apartments by hotel and building
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelAndBuildingAsync(int hotelId, int buildingId);

        /// <summary>
        /// Get apartments by hotel and floor
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelAndFloorAsync(int hotelId, int floorId);

        /// <summary>
        /// Get apartments by hotel and room type
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelAndRoomTypeAsync(int hotelId, int roomTypeId);

        /// <summary>
        /// Get apartments by building and floor
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByBuildingAndFloorAsync(int buildingId, int floorId);

        /// <summary>
        /// Get apartments by building and room type
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByBuildingAndRoomTypeAsync(int buildingId, int roomTypeId);

        /// <summary>
        /// Get apartments by floor and room type
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByFloorAndRoomTypeAsync(int floorId, int roomTypeId);

        /// <summary>
        /// Get apartments by multiple criteria
        /// </summary>
        Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByMultipleCriteriaAsync(int? hotelId = null, int? buildingId = null, int? floorId = null, int? roomTypeId = null, string? status = null);

        /// <summary>
        /// Get apartment occupancy rate
        /// </summary>
        Task<decimal> GetApartmentOccupancyRateAsync(int apartmentId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get apartment revenue
        /// </summary>
        Task<decimal> GetApartmentRevenueAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get apartment reservation count
        /// </summary>
        Task<int> GetApartmentReservationCountAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get apartment average stay duration
        /// </summary>
        Task<decimal> GetApartmentAverageStayDurationAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get apartment utilization statistics
        /// </summary>
        Task<object> GetApartmentUtilizationStatisticsAsync(int apartmentId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Update apartment status
        /// </summary>
        Task<bool> UpdateApartmentStatusAsync(int id, string status);

        /// <summary>
        /// Check apartment availability for date range
        /// </summary>
        Task<bool> CheckApartmentAvailabilityAsync(int apartmentId, DateTime startDate, DateTime endDate);
    }
}
