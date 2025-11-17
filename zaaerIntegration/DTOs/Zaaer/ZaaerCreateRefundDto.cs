using System.ComponentModel.DataAnnotations;
using zaaerIntegration.Converters;
using System.Text.Json.Serialization;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for creating a refund via Zaaer integration
    /// </summary>
    public class ZaaerCreateRefundDto
    {
        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        [Required]
        [StringLength(50)]
        public string RefundNo { get; set; } = string.Empty;

        [Required]
        public int HotelId { get; set; }

        public int? ReservationId { get; set; }
        public int? InvoiceId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public DateTime RefundDate { get; set; } = KsaTime.Now;

        [Required]
        public decimal RefundAmount { get; set; }

        [StringLength(500)]
        public string? RefundReason { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Payment method ID (can be null, 0, or empty string for Cash payments)
        /// </summary>
        [JsonConverter(typeof(NullableIntConverter))]
        public int? PaymentMethodId { get; set; }

        /// <summary>
        /// Bank ID (can be null, 0, or empty string)
        /// </summary>
        [JsonConverter(typeof(NullableIntConverter))]
        public int? BankId { get; set; }

        [StringLength(100)]
        public string? TransactionNo { get; set; }

        public string? Notes { get; set; }

        public int? CreatedBy { get; set; }

        [StringLength(50)]
        public string RefundType { get; set; } = "refund";
    }
}
