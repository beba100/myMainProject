using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for ReservationUnit repository operations
    /// </summary>
    public interface IReservationUnitRepository : IGenericRepository<ReservationUnit>
    {
        /// <summary>
        /// Get reservation units with pagination and search
        /// </summary>
        Task<(IEnumerable<ReservationUnit> ReservationUnits, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<ReservationUnit, bool>>? filter = null);

        /// <summary>
        /// Get reservation units by reservation ID
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get reservation units by apartment ID
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByApartmentIdAsync(int apartmentId);

        /// <summary>
        /// Get reservation units by status
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByStatusAsync(string status);

        /// <summary>
        /// Get reservation units by check-in date range
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByCheckInDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get reservation units by check-out date range
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByCheckOutDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get reservation units by date range (overlapping dates)
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get reservation units by rent amount range
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByRentAmountRangeAsync(decimal minAmount, decimal maxAmount);

        /// <summary>
        /// Get reservation units by total amount range
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByTotalAmountRangeAsync(decimal minAmount, decimal maxAmount);

        /// <summary>
        /// Get reservation units by number of nights range
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByNumberOfNightsRangeAsync(int minNights, int maxNights);

        /// <summary>
        /// Get reservation units by VAT rate range
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByVatRateRangeAsync(decimal minRate, decimal maxRate);

        /// <summary>
        /// Get reservation units by lodging tax rate range
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByLodgingTaxRateRangeAsync(decimal minRate, decimal maxRate);

        /// <summary>
        /// Get reservation units with full details (includes all navigation properties)
        /// </summary>
        Task<ReservationUnit?> GetWithDetailsAsync(int id);

        /// <summary>
        /// Get reservation units with full details by reservation ID
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetWithDetailsByReservationIdAsync(int reservationId);

        /// <summary>
        /// Get reservation units with full details by apartment ID
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetWithDetailsByApartmentIdAsync(int apartmentId);

        /// <summary>
        /// Get reservation unit statistics
        /// </summary>
        Task<object> GetStatisticsAsync();

        /// <summary>
        /// Get reservation units by hotel ID
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get reservation units by building ID
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByBuildingIdAsync(int buildingId);

        /// <summary>
        /// Get reservation units by floor ID
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByFloorIdAsync(int floorId);

        /// <summary>
        /// Get reservation units by room type ID
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByRoomTypeIdAsync(int roomTypeId);

        /// <summary>
        /// Get reservation units by customer ID
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get reservation units by corporate customer ID
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByCorporateCustomerIdAsync(int corporateCustomerId);

        /// <summary>
        /// Get reservation units by created date
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByCreatedDateAsync(DateTime createdDate);

        /// <summary>
        /// Get reservation units by created date range
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByCreatedDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get active reservation units (not cancelled or completed)
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetActiveAsync();

        /// <summary>
        /// Get cancelled reservation units
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetCancelledAsync();

        /// <summary>
        /// Get completed reservation units
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetCompletedAsync();

        /// <summary>
        /// Get reservation units by apartment name
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByApartmentNameAsync(string apartmentName);

        /// <summary>
        /// Get reservation units by building name
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByBuildingNameAsync(string buildingName);

        /// <summary>
        /// Get reservation units by room type name
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByRoomTypeNameAsync(string roomTypeName);

        /// <summary>
        /// Get reservation units by customer name
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByCustomerNameAsync(string customerName);

        /// <summary>
        /// Get reservation units by corporate customer name
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByCorporateCustomerNameAsync(string corporateCustomerName);

        /// <summary>
        /// Get reservation units by hotel name
        /// </summary>
        Task<IEnumerable<ReservationUnit>> GetByHotelNameAsync(string hotelName);

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
        Task<IEnumerable<ReservationUnit>> GetOverlappingDatesAsync(int apartmentId, DateTime checkInDate, DateTime checkOutDate, int? excludeUnitId = null);
    }
}
