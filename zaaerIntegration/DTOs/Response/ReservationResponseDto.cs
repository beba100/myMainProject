using FinanceLedgerAPI.Enums;

namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning reservation data
    /// </summary>
    public class ReservationResponseDto
    {
        public int ReservationId { get; set; }
        public string ReservationNo { get; set; } = string.Empty;
        public int HotelId { get; set; }
        public int CustomerId { get; set; }
        public int? VisitPurposeId { get; set; }
        public int? CorporateId { get; set; }
        public string ReservationType { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public int? TotalNights { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? VatRate { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? LodgingTaxRate { get; set; }
        public decimal? LodgingTaxAmount { get; set; }
        public decimal? TotalTaxAmount { get; set; }
        public decimal? TotalExtra { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? AmountPaid { get; set; }
        public decimal? BalanceAmount { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Unconfirmed;
        public string StatusWord { get; set; } = "Unconfirmed";
        public string? Notes { get; set; }
        public string? SpecialRequests { get; set; }
        public string? ConfirmationNumber { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Related entity names for display
        public string? HotelName { get; set; }
        public string? CustomerName { get; set; }
        public string? CorporateName { get; set; }
        public string? VisitPurposeName { get; set; }
    }
}
