using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new refund
    /// </summary>
    public class CreateRefundDto
    {
        [Required]
        [StringLength(50)]
        public string RefundNo { get; set; } = string.Empty;

        [Required]
        public int HotelId { get; set; }

        public int? ReservationId { get; set; }

        public int? UnitId { get; set; }

        public int? InvoiceId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [StringLength(50)]
        public string RefundType { get; set; } = "refund";

        [StringLength(20)]
        public string PaidFrom { get; set; } = "drawer";

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
        public decimal RefundAmount { get; set; }

        [StringLength(500)]
        public string? RefundReason { get; set; }

        public int? PaymentMethodId { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        public int? BankId { get; set; }

        [StringLength(100)]
        public string? TransactionNo { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int? CreatedBy { get; set; }
    }
}
