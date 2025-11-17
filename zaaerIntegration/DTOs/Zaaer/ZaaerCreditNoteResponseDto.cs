using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for credit note response via Zaaer integration
    /// </summary>
    public class ZaaerCreditNoteResponseDto
    {
        public int CreditNoteId { get; set; }
        public string CreditNoteNo { get; set; } = string.Empty;
        public int HotelId { get; set; }
        public int InvoiceId { get; set; }
        public int? ReservationId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreditNoteDate { get; set; }
        public string? CreditNoteDateHijri { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? VatRate { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? LodgingTaxRate { get; set; }
        public decimal? LodgingTaxAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// Credit Note Type (e.g., "refund", "discount", "adjustment", "cancellation", "simplified")
        /// </summary>
        [JsonPropertyName("creditNoteType")]
        public string CreditType { get; set; } = "refund";
        
        public string? Notes { get; set; }
        public bool IsSentZatca { get; set; } = false;
        public string? ZatcaUuid { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
