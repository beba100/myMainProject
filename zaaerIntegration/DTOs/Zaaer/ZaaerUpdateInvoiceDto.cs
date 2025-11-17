using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using zaaerIntegration.Converters;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for updating an invoice via Zaaer integration
    /// </summary>
    public class ZaaerUpdateInvoiceDto
    {
        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        [StringLength(50)]
        public string? InvoiceNo { get; set; }

        public int? HotelId { get; set; }

        public int? ReservationId { get; set; }

        public int? CustomerId { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [StringLength(20)]
        public string? InvoiceDateHijri { get; set; }

        [StringLength(50)]
        public string? InvoiceType { get; set; }

        public decimal? Subtotal { get; set; }

        public decimal? VatRate { get; set; }

        public decimal? LodgingTaxRate { get; set; }

        public decimal? TotalAmount { get; set; }

        public bool? IsSentZatca { get; set; }

        [JsonConverter(typeof(NullableIntJsonConverter))]
        public int? CreatedBy { get; set; }

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
