using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for updating an existing invoice
    /// </summary>
    public class UpdateInvoiceDto
    {
        [Required]
        [StringLength(50)]
        public string InvoiceNo { get; set; } = string.Empty;

        [Required]
        public int HotelId { get; set; }

        public int? ReservationId { get; set; }

        public int? UnitId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [StringLength(20)]
        public string? InvoiceDateHijri { get; set; }

        public DateTime? PeriodFrom { get; set; }

        public DateTime? PeriodTo { get; set; }

        [StringLength(50)]
        public string InvoiceType { get; set; } = "rent";

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

        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be a positive number")]
        public decimal? TotalAmount { get; set; }

        [StringLength(20)]
        public string PaymentStatus { get; set; } = "unpaid";

        [Range(0, double.MaxValue, ErrorMessage = "Amount paid must be a positive number")]
        public decimal AmountPaid { get; set; } = 0.00M;

        [Range(0, double.MaxValue, ErrorMessage = "Amount remaining must be a positive number")]
        public decimal? AmountRemaining { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount refunded must be a positive number")]
        public decimal? AmountRefunded { get; set; }

        public bool IsSentZatca { get; set; } = false;

        [StringLength(255)]
        public string? ZatcaUuid { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

    }
}
