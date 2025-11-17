using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Interface for Reservation repository operations
    /// </summary>
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        /// <summary>
        /// Get reservations with pagination and search
        /// </summary>
        Task<(IEnumerable<Reservation> Reservations, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<Reservation, bool>>? filter = null);

        /// <summary>
        /// Get reservation by reservation number
        /// </summary>
        Task<Reservation?> GetByReservationNoAsync(string reservationNo);

        /// <summary>
        /// Get reservations by customer ID
        /// </summary>
        Task<IEnumerable<Reservation>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get reservations by hotel ID
        /// </summary>
        Task<IEnumerable<Reservation>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get reservations by status
        /// </summary>
        Task<IEnumerable<Reservation>> GetByStatusAsync(string status);

        /// <summary>
        /// Get reservations by date range
        /// </summary>
        Task<IEnumerable<Reservation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);


        /// <summary>
        /// Get reservations by customer name (search)
        /// </summary>
        Task<IEnumerable<Reservation>> GetByCustomerNameAsync(string customerName);

        /// <summary>
        /// Get reservations by hotel name (search)
        /// </summary>
        Task<IEnumerable<Reservation>> GetByHotelNameAsync(string hotelName);

        /// <summary>
        /// Get reservations by reservation number (search)
        /// </summary>
        Task<IEnumerable<Reservation>> GetByReservationNoSearchAsync(string reservationNo);

        /// <summary>
        /// Check if reservation number exists
        /// </summary>
        Task<bool> ReservationNoExistsAsync(string reservationNo, int? excludeId = null);

        /// <summary>
        /// Get reservation statistics
        /// </summary>
        Task<object> GetStatisticsAsync();

        /// <summary>
        /// Get reservations with full details (includes all navigation properties)
        /// </summary>
        Task<Reservation?> GetWithDetailsAsync(int id);

        /// <summary>
        /// Get reservations with full details by reservation number
        /// </summary>
        Task<Reservation?> GetWithDetailsByReservationNoAsync(string reservationNo);

    }
}
