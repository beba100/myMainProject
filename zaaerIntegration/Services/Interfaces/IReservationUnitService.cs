using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for ReservationUnit service operations
    /// </summary>
    public interface IReservationUnitService
    {
        /// <summary>
        /// Get all reservation units with pagination and search
        /// </summary>
        Task<(IEnumerable<ReservationUnitResponseDto> ReservationUnits, int TotalCount)> GetAllReservationUnitsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null);

        /// <summary>
        /// Get reservation unit by ID
        /// </summary>
        Task<ReservationUnitResponseDto?> GetReservationUnitByIdAsync(int id);

        /// <summary>
        /// Create new reservation unit
        /// </summary>
        Task<ReservationUnitResponseDto> CreateReservationUnitAsync(CreateReservationUnitDto createReservationUnitDto);

        /// <summary>
        /// Update existing reservation unit
        /// </summary>
        Task<ReservationUnitResponseDto?> UpdateReservationUnitAsync(int id, UpdateReservationUnitDto updateReservationUnitDto);

        /// <summary>
        /// Delete reservation unit
        /// </summary>
        Task<bool> DeleteReservationUnitAsync(int id);

        /// <summary>
        /// Get reservation units by reservation ID
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get reservation units by apartment ID
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByApartmentIdAsync(int apartmentId);

        /// <summary>
        /// Get reservation units by status
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByStatusAsync(string status);

        /// <summary>
        /// Get reservation units by check-in date range
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCheckInDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get reservation units by check-out date range
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCheckOutDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get reservation units by date range (overlapping dates)
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get reservation units by rent amount range
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByRentAmountRangeAsync(decimal minAmount, decimal maxAmount);

        /// <summary>
        /// Get reservation units by total amount range
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByTotalAmountRangeAsync(decimal minAmount, decimal maxAmount);

        /// <summary>
        /// Get reservation units by number of nights range
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByNumberOfNightsRangeAsync(int minNights, int maxNights);

        /// <summary>
        /// Get reservation units by VAT rate range
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByVatRateRangeAsync(decimal minRate, decimal maxRate);

        /// <summary>
        /// Get reservation units by lodging tax rate range
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByLodgingTaxRateRangeAsync(decimal minRate, decimal maxRate);

        /// <summary>
        /// Get reservation unit statistics
        /// </summary>
        Task<object> GetReservationUnitStatisticsAsync();

        /// <summary>
        /// Get reservation units by hotel ID
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get reservation units by building ID
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByBuildingIdAsync(int buildingId);

        /// <summary>
        /// Get reservation units by floor ID
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByFloorIdAsync(int floorId);

        /// <summary>
        /// Get reservation units by room type ID
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByRoomTypeIdAsync(int roomTypeId);

        /// <summary>
        /// Get reservation units by customer ID
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get reservation units by corporate customer ID
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCorporateCustomerIdAsync(int corporateCustomerId);

        /// <summary>
        /// Get reservation units by created date
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCreatedDateAsync(DateTime createdDate);

        /// <summary>
        /// Get reservation units by created date range
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCreatedDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get active reservation units
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetActiveReservationUnitsAsync();

        /// <summary>
        /// Get cancelled reservation units
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetCancelledReservationUnitsAsync();

        /// <summary>
        /// Get completed reservation units
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetCompletedReservationUnitsAsync();

        /// <summary>
        /// Search reservation units by apartment name
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByApartmentNameAsync(string apartmentName);

        /// <summary>
        /// Search reservation units by building name
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByBuildingNameAsync(string buildingName);

        /// <summary>
        /// Search reservation units by room type name
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByRoomTypeNameAsync(string roomTypeName);

        /// <summary>
        /// Search reservation units by customer name
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByCustomerNameAsync(string customerName);

        /// <summary>
        /// Search reservation units by corporate customer name
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByCorporateCustomerNameAsync(string corporateCustomerName);

        /// <summary>
        /// Search reservation units by hotel name
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByHotelNameAsync(string hotelName);

        /// <summary>
        /// Get total revenue by reservation ID
        /// </summary>
        Task<decimal> GetTotalRevenueByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get total revenue by apartment ID
        /// </summary>
        Task<decimal> GetTotalRevenueByApartmentIdAsync(int apartmentId);

        /// <summary>
        /// Get total revenue by hotel ID
        /// </summary>
        Task<decimal> GetTotalRevenueByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get total revenue by date range
        /// </summary>
        Task<decimal> GetTotalRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get average rent amount by apartment ID
        /// </summary>
        Task<decimal> GetAverageRentAmountByApartmentIdAsync(int apartmentId);

        /// <summary>
        /// Get average total amount by apartment ID
        /// </summary>
        Task<decimal> GetAverageTotalAmountByApartmentIdAsync(int apartmentId);

        /// <summary>
        /// Get top apartments by revenue
        /// </summary>
        Task<IEnumerable<object>> GetTopApartmentsByRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get top hotels by revenue
        /// </summary>
        Task<IEnumerable<object>> GetTopHotelsByRevenueAsync(int topCount = 10);

        /// <summary>
        /// Get reservation units by overlapping dates (for availability checking)
        /// </summary>
        Task<IEnumerable<ReservationUnitResponseDto>> GetOverlappingDatesAsync(int apartmentId, DateTime checkInDate, DateTime checkOutDate, int? excludeUnitId = null);

        /// <summary>
        /// Update reservation unit status
        /// </summary>
        Task<bool> UpdateReservationUnitStatusAsync(int id, string status);

        /// <summary>
        /// Update reservation unit amounts
        /// </summary>
        Task<bool> UpdateReservationUnitAmountsAsync(int id, decimal rentAmount, decimal? vatAmount, decimal? lodgingTaxAmount, decimal totalAmount);

        /// <summary>
        /// Update reservation unit dates
        /// </summary>
        Task<bool> UpdateReservationUnitDatesAsync(int id, DateTime checkInDate, DateTime checkOutDate, int? numberOfNights);

        /// <summary>
        /// Check apartment availability
        /// </summary>
        Task<bool> CheckApartmentAvailabilityAsync(int apartmentId, DateTime checkInDate, DateTime checkOutDate, int? excludeUnitId = null);
    }
}
