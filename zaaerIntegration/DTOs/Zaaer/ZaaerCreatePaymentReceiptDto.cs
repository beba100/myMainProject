using System.ComponentModel.DataAnnotations;
using zaaerIntegration.Converters;
using System.Text.Json.Serialization;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for creating a payment receipt via Zaaer integration
    /// </summary>
    public class ZaaerCreatePaymentReceiptDto
    {
        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        /// <summary>
        /// Receipt number
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ReceiptNo { get; set; } = string.Empty;

        /// <summary>
        /// Hotel ID
        /// </summary>
        [Required]
        public int HotelId { get; set; }

        /// <summary>
        /// Reservation ID
        /// </summary>
        public int? ReservationId { get; set; }

        /// <summary>
        /// Invoice ID (optional - for linking receipt to invoice)
        /// </summary>
        public int? InvoiceId { get; set; }

        /// <summary>
        /// Unit ID (optional - for linking receipt to unit/apartment)
        /// </summary>
        [JsonConverter(typeof(NullableIntConverter))]
        public int? UnitId { get; set; }

        /// <summary>
        /// Customer ID (nullable for expense receipts)
        /// </summary>
        [JsonConverter(typeof(NullableIntJsonConverter))]
        public int? CustomerId { get; set; }

        /// <summary>
        /// Receipt date
        /// </summary>
        [Required]
        public DateTime ReceiptDate { get; set; }

        /// <summary>
        /// Amount paid
        /// </summary>
        [Required]
		[JsonConverter(typeof(FlexibleNonNullableDecimalJsonConverter))]
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Payment method (legacy string)
        /// </summary>
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

        /// <summary>
        /// Transaction number
        /// </summary>
        [StringLength(100)]
        public string? TransactionNo { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Receipt status: active | cancelled (optional on create)
        /// </summary>
        [StringLength(50)]
        [JsonPropertyName("receiptstatus")]
        public string? ReceiptStatus { get; set; }

        /// <summary>
        /// Receipt type
        /// </summary>
        [StringLength(50)]
        public string ReceiptType { get; set; } = "receipt";

        /// <summary>
        /// Voucher code (optional - for discount vouchers)
        /// </summary>
        [StringLength(50)]
        public string? VoucherCode { get; set; }

        /// <summary>
        /// Created by user ID
        /// </summary>
        public int? CreatedBy { get; set; }

    }
}
