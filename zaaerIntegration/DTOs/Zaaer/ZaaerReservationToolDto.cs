using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// Minimal payload for Reservation Tools (Swagger-friendly like frontend)
    /// </summary>
    public class ZaaerCreateReservationToolDto
    {
        [Required]
        [StringLength(50)]
        public string ReservationNo { get; set; } = string.Empty;

        [Required]
        public int HotelId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Rental type: Daily, Monthly, Yearly, InHour
        /// </summary>
        [StringLength(20)]
        public string? RentalType { get; set; }

        /// <summary>
        /// Number of months for monthly rental type (عدد الشهور للحجز الشهري)
        /// </summary>
        public int? NumberOfMonths { get; set; }

        /// <summary>
        /// Total penalties to be added to the reservation total
        /// </summary>
        public decimal? TotalPenalties { get; set; }

        /// <summary>
        /// Total discounts to be subtracted from the reservation total
        /// </summary>
        public decimal? TotalDiscounts { get; set; }

        /// <summary>
        /// Total extra charges (إجمالي الإضافات)
        /// </summary>
        public decimal? TotalExtra { get; set; }

        /// <summary>
        /// Corporate ID
        /// </summary>
        public int? CorporateId { get; set; }

        /// <summary>
        /// Planned check-in date for the reservation
        /// </summary>
        public DateTime? CheckInDate { get; set; }

        /// <summary>
        /// Planned check-out date for the reservation
        /// </summary>
        public DateTime? CheckOutDate { get; set; }

        /// <summary>
        /// Departure date (actual or expected)
        /// </summary>
        public DateTime? DepartureDate { get; set; }

        [Required]
        public List<ZaaerReservationUnitToolDto> ReservationUnits { get; set; } = new();
    }

    /// <summary>
    /// Minimal reservation unit for tools
    /// </summary>
    public class ZaaerReservationUnitToolDto
    {
        /// <summary>
        /// For updates inside a PUT body: 0 for new unit.
        /// </summary>
        public int ReservationId { get; set; } = 0;

        [Required]
        public int ApartmentId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public DateTime? DepartureDate { get; set; }

        /// <summary>
        /// Optional partner-sent rent amount; if provided, server will use it.
        /// </summary>
        public decimal? RentAmount { get; set; }
    }
}


