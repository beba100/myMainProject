namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning payment receipt data
    /// </summary>
    public class PaymentReceiptResponseDto
    {
        public int ReceiptId { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public int HotelId { get; set; }
        public int? ReservationId { get; set; }
        public int? UnitId { get; set; }
        public int? InvoiceId { get; set; }
        public int CustomerId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string ReceiptType { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public int? PaymentMethodId { get; set; }
        public string? PaymentMethod { get; set; }
        public int? BankId { get; set; }
        public string? TransactionNo { get; set; }
        public string? Notes { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Related entity names for display
        public string? HotelName { get; set; }
        public string? CustomerName { get; set; }
        public string? ReservationNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? BankName { get; set; }
    }
}
