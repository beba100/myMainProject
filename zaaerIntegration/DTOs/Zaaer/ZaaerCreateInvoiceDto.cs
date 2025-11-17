using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using zaaerIntegration.Converters;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for creating an invoice via Zaaer integration
    /// </summary>
    public class ZaaerCreateInvoiceDto
    {
        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNo { get; set; } = string.Empty;

        [Required]
        public int HotelId { get; set; }

        public int? ReservationId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [StringLength(20)]
        public string? InvoiceDateHijri { get; set; }

        [StringLength(50)]
        public string InvoiceType { get; set; } = "invoice";

        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? Subtotal { get; set; }

        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? VatRate { get; set; }

        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? VatAmount { get; set; }

        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? LodgingTaxRate { get; set; }

        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? LodgingTaxAmount { get; set; }

        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? TotalAmount { get; set; }

        public bool IsSentZatca { get; set; } = false;

        [JsonConverter(typeof(NullableIntJsonConverter))]
        public int? CreatedBy { get; set; }

        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? TotalDiscounts { get; set; }

        public DateTime? PeriodFrom { get; set; }

        public DateTime? PeriodTo { get; set; }

        [StringLength(50)]
        public string? PaymentStatus { get; set; }

        public string? Notes { get; set; }

        [StringLength(255)]
        public string? ZatcaUuid { get; set; }
    }
}
