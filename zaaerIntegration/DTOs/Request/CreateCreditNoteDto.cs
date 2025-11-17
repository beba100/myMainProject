using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new credit note
    /// </summary>
    public class CreateCreditNoteDto
    {
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

        [StringLength(20)]
        public string? CreditNoteDateHijri { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Credit amount must be greater than 0")]
        public decimal CreditAmount { get; set; }

        public decimal? OriginalInvoiceAmount { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int? CreatedBy { get; set; }
    }
}
