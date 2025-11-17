using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for invoice response via Zaaer integration
    /// </summary>
    public class ZaaerInvoiceResponseDto
    {
        public int InvoiceId { get; set; }

        public string InvoiceNo { get; set; } = string.Empty;

        public int HotelId { get; set; }

        public int? ReservationId { get; set; }

        public int CustomerId { get; set; }

        public DateTime InvoiceDate { get; set; }

        public string? InvoiceDateHijri { get; set; }

        public string InvoiceType { get; set; } = "invoice";

        public decimal? Subtotal { get; set; }

        public decimal? VatRate { get; set; }

        public decimal? VatAmount { get; set; }

        public decimal? LodgingTaxRate { get; set; }

        public decimal? LodgingTaxAmount { get; set; }

        public decimal? TotalAmount { get; set; }

        public bool IsSentZatca { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public decimal? TotalDiscounts { get; set; }

        public DateTime? PeriodFrom { get; set; }

        public DateTime? PeriodTo { get; set; }

        public string? PaymentStatus { get; set; }

        public string? Notes { get; set; }

        public string? ZatcaUuid { get; set; }

        /// <summary>
        /// List of payment receipts associated with this invoice
        /// قائمة سندات القبض المرتبطة بهذه الفاتورة
        /// </summary>
        public List<ZaaerPaymentReceiptResponseDto> PaymentReceipts { get; set; } = new List<ZaaerPaymentReceiptResponseDto>();

        /// <summary>
        /// List of refunds associated with this invoice
        /// قائمة الاستردادات المرتبطة بهذه الفاتورة
        /// </summary>
        public List<ZaaerRefundResponseDto> Refunds { get; set; } = new List<ZaaerRefundResponseDto>();

        /// <summary>
        /// List of credit notes associated with this invoice
        /// قائمة الإشعارات الدائنة المرتبطة بهذه الفاتورة
        /// </summary>
        public List<ZaaerCreditNoteResponseDto> CreditNotes { get; set; } = new List<ZaaerCreditNoteResponseDto>();

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
