using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for payment receipt response via Zaaer integration
    /// </summary>
    public class ZaaerPaymentReceiptResponseDto
    {
        /// <summary>
        /// Receipt ID
        /// </summary>
        public int ReceiptId { get; set; }

        /// <summary>
        /// Receipt number
        /// </summary>
        public string? ReceiptNo { get; set; }

        /// <summary>
        /// Hotel ID
        /// </summary>
        public int HotelId { get; set; }

        /// <summary>
        /// Reservation ID
        /// </summary>
        public int? ReservationId { get; set; }

        /// <summary>
        /// Invoice ID (linked invoice)
        /// </summary>
        public int? InvoiceId { get; set; }

        /// <summary>
        /// Unit ID (linked unit/apartment)
        /// </summary>
        public int? UnitId { get; set; }

        /// <summary>
        /// Customer ID
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Receipt date
        /// </summary>
        public DateTime ReceiptDate { get; set; }

        /// <summary>
        /// Amount paid
        /// </summary>
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Payment method (legacy string)
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Payment method ID
        /// </summary>
        public int? PaymentMethodId { get; set; }

        /// <summary>
        /// Bank ID
        /// </summary>
        public int? BankId { get; set; }

        /// <summary>
        /// Transaction number
        /// </summary>
        public string? TransactionNo { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Receipt status: active | cancelled
        /// </summary>
        public string? ReceiptStatus { get; set; }

        /// <summary>
        /// Receipt type
        /// </summary>
        public string ReceiptType { get; set; } = "receipt";

        /// <summary>
        /// Voucher code (for discount vouchers)
        /// </summary>
        public string? VoucherCode { get; set; }

        /// <summary>
        /// Created by user ID
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// Created at
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
