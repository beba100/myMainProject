using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for creating a credit note via Zaaer integration
    /// </summary>
    public class ZaaerCreateCreditNoteDto
    {
        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        [Required]
        [StringLength(50)]
        public string CreditNoteNo { get; set; } = string.Empty;

        [Required]
        public int HotelId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        public int? ReservationId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public DateTime CreditNoteDate { get; set; } = KsaTime.Now;

        /// <summary>
        /// Subtotal amount before tax
        /// </summary>
        public decimal? Subtotal { get; set; }

        /// <summary>
        /// VAT Rate percentage
        /// </summary>
        public decimal? VatRate { get; set; }

        /// <summary>
        /// VAT Amount
        /// </summary>
        public decimal? VatAmount { get; set; }

        /// <summary>
        /// Lodging Tax Rate percentage
        /// </summary>
        public decimal? LodgingTaxRate { get; set; }

        /// <summary>
        /// Lodging Tax Amount
        /// </summary>
        public decimal? LodgingTaxAmount { get; set; }

        [Required]
        public decimal CreditAmount { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Credit Note Type (e.g., "refund", "discount", "adjustment", "cancellation", "simplified")
        /// </summary>
        [JsonPropertyName("creditNoteType")]
        [StringLength(50)]
        public string CreditType { get; set; } = "refund";

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool IsSentZatca { get; set; } = false;

        [StringLength(255)]
        public string? ZatcaUuid { get; set; }

        public int? CreatedBy { get; set; }
    }
}
