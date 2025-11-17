using System.ComponentModel.DataAnnotations;
using FinanceLedgerAPI.Enums;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new reservation
    /// </summary>
    public class CreateReservationDto
    {
        [Required]
        [StringLength(50)]
        public string ReservationNo { get; set; } = string.Empty;

        [Required]
        public int HotelId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public int? VisitPurposeId { get; set; }

        public int? CorporateId { get; set; }

        [StringLength(20)]
        public string ReservationType { get; set; } = "Individual";

        public DateTime? ReservationDate { get; set; }

        public int? TotalNights { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Subtotal must be a positive number")]
        public decimal? Subtotal { get; set; }

        [Range(0, 100, ErrorMessage = "VAT rate must be between 0 and 100")]
        public decimal? VatRate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "VAT amount must be a positive number")]
        public decimal? VatAmount { get; set; }

        [Range(0, 100, ErrorMessage = "Lodging tax rate must be between 0 and 100")]
        public decimal? LodgingTaxRate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Lodging tax amount must be a positive number")]
        public decimal? LodgingTaxAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total tax amount must be a positive number")]
        public decimal? TotalTaxAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total extra must be a positive number")]
        public decimal? TotalExtra { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be a positive number")]
        public decimal? TotalAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount paid must be a positive number")]
        public decimal? AmountPaid { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Balance amount must be a positive number")]
        public decimal? BalanceAmount { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Unconfirmed;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(500)]
        public string? SpecialRequests { get; set; }

        [StringLength(100)]
        public string? ConfirmationNumber { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CheckInDate { get; set; }

        public DateTime? CheckOutDate { get; set; }

        public DateTime? DepartureDate { get; set; }

        /// <summary>
        /// List of apartment IDs to create reservation units for
        /// </summary>
        public List<int> ApartmentIds { get; set; } = new List<int>();
    }
}
