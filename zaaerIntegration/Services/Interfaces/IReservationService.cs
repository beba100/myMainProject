using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for Reservation service operations
    /// </summary>
    public interface IReservationService
    {
        /// <summary>
        /// Get all reservations with pagination and search
        /// </summary>
        Task<(IEnumerable<ReservationResponseDto> Reservations, int TotalCount)> GetAllReservationsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null);

        /// <summary>
        /// Get reservation by ID
        /// </summary>
        Task<ReservationResponseDto?> GetReservationByIdAsync(int id);

        /// <summary>
        /// Get reservation by reservation number
        /// </summary>
        Task<ReservationResponseDto?> GetReservationByNoAsync(string reservationNo);

        /// <summary>
        /// Create new reservation
        /// </summary>
        Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto createReservationDto);

        /// <summary>
        /// Update existing reservation
        /// </summary>
        Task<ReservationResponseDto?> UpdateReservationAsync(int id, UpdateReservationDto updateReservationDto);

        /// <summary>
        /// Delete reservation
        /// </summary>
        Task<bool> DeleteReservationAsync(int id);

        /// <summary>
        /// Get reservations by customer ID
        /// </summary>
        Task<IEnumerable<ReservationResponseDto>> GetReservationsByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get reservations by hotel ID
        /// </summary>
        Task<IEnumerable<ReservationResponseDto>> GetReservationsByHotelIdAsync(int hotelId);

        /// <summary>
        /// Get reservations by status
        /// </summary>
        Task<IEnumerable<ReservationResponseDto>> GetReservationsByStatusAsync(string status);

        /// <summary>
        /// Get reservations by date range
        /// </summary>
        Task<IEnumerable<ReservationResponseDto>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);


        /// <summary>
        /// Search reservations by customer name
        /// </summary>
        Task<IEnumerable<ReservationResponseDto>> SearchReservationsByCustomerNameAsync(string customerName);

        /// <summary>
        /// Search reservations by hotel name
        /// </summary>
        Task<IEnumerable<ReservationResponseDto>> SearchReservationsByHotelNameAsync(string hotelName);

        /// <summary>
        /// Search reservations by reservation number
        /// </summary>
        Task<IEnumerable<ReservationResponseDto>> SearchReservationsByNoAsync(string reservationNo);

        /// <summary>
        /// Get reservation statistics
        /// </summary>
        Task<object> GetReservationStatisticsAsync();

        /// <summary>
        /// Check if reservation number exists
        /// </summary>
        Task<bool> ReservationNoExistsAsync(string reservationNo, int? excludeId = null);


        /// <summary>
        /// Update reservation status
        /// </summary>
        Task<bool> UpdateReservationStatusAsync(int id, string status);


        /// <summary>
        /// Cancel reservation
        /// </summary>
        Task<bool> CancelReservationAsync(int id, string cancellationReason);

        /// <summary>
        /// Complete reservation
        /// </summary>
        Task<bool> CompleteReservationAsync(int id);
    }
}
