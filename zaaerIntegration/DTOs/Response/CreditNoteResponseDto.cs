using System;

namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning credit note data
    /// </summary>
    public class CreditNoteResponseDto
    {
        public int CreditNoteId { get; set; }
        public string CreditNoteNo { get; set; } = string.Empty;
        public int HotelId { get; set; }
        public int InvoiceId { get; set; }
        public int? ReservationId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreditNoteDate { get; set; }
        public string? CreditNoteDateHijri { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal? OriginalInvoiceAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsSentZatca { get; set; }
        public string? ZatcaUuid { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Related entity names for display
        public string? HotelName { get; set; }
        public string? CustomerName { get; set; }
        public string? InvoiceNo { get; set; }
        public string? ReservationNo { get; set; }
    }
}
