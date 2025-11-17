namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning invoice data
    /// </summary>
    public class InvoiceResponseDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public int HotelId { get; set; }
        public int? ReservationId { get; set; }
        public int? UnitId { get; set; }
        public int CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? InvoiceDateHijri { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string InvoiceType { get; set; } = string.Empty;
        public decimal? Subtotal { get; set; }
        public decimal? VatRate { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? LodgingTaxRate { get; set; }
        public decimal? LodgingTaxAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public decimal? AmountRemaining { get; set; }
        public decimal? AmountRefunded { get; set; }
        public bool IsSentZatca { get; set; }
        public string? ZatcaUuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string? Notes { get; set; }

        // Related entity names for display
        public string? HotelName { get; set; }
        public string? CustomerName { get; set; }
        public string? ReservationNo { get; set; }
    }
}
