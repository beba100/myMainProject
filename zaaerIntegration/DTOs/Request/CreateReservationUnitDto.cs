using System.ComponentModel.DataAnnotations;
using FinanceLedgerAPI.Enums;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new reservation unit
    /// </summary>
    public class CreateReservationUnitDto
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public int ApartmentId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public DateTime? DepartureDate { get; set; }

        public int? NumberOfNights { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rent amount must be greater than 0")]
        public decimal RentAmount { get; set; }

        [Range(0, 100, ErrorMessage = "VAT rate must be between 0 and 100")]
        public decimal VatRate { get; set; } = 15.00M;

        [Range(0, double.MaxValue, ErrorMessage = "VAT amount must be a positive number")]
        public decimal? VatAmount { get; set; }

        [Range(0, 100, ErrorMessage = "Lodging tax rate must be between 0 and 100")]
        public decimal LodgingTaxRate { get; set; } = 2.50M;

        [Range(0, double.MaxValue, ErrorMessage = "Lodging tax amount must be a positive number")]
        public decimal? LodgingTaxAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal TotalAmount { get; set; }

        public ReservationUnitStatus Status { get; set; } = ReservationUnitStatus.Reserved;
    }
}
