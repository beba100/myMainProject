using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for refund response via Zaaer integration
    /// </summary>
    public class ZaaerRefundResponseDto
    {
        public int RefundId { get; set; }
        public string RefundNo { get; set; } = string.Empty;
        public int HotelId { get; set; }
        public int? ReservationId { get; set; }
        public int? InvoiceId { get; set; }
        public int CustomerId { get; set; }
        public DateTime RefundDate { get; set; }
        public decimal RefundAmount { get; set; }
        public string? RefundReason { get; set; }
        public string? PaymentMethod { get; set; }
        public int? PaymentMethodId { get; set; }
        public int? BankId { get; set; }
        public string? TransactionNo { get; set; }
        public string? Notes { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RefundType { get; set; } = "refund";

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
