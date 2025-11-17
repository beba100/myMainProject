using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for updating an existing payment receipt
    /// </summary>
    public class UpdatePaymentReceiptDto
    {
        [Required]
        [StringLength(50)]
        public string ReceiptNo { get; set; } = string.Empty;

        [Required]
        public int HotelId { get; set; }

        public int? ReservationId { get; set; }

        public int? UnitId { get; set; }

        public int? InvoiceId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public DateTime? ReceiptDate { get; set; }

        [StringLength(50)]
        public string ReceiptType { get; set; } = "receipt";

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount paid must be greater than 0")]
        public decimal AmountPaid { get; set; }

        public int? PaymentMethodId { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        public int? BankId { get; set; }

        [StringLength(100)]
        public string? TransactionNo { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

    }
}
