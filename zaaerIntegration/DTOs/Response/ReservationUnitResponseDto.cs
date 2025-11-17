using FinanceLedgerAPI.Enums;

namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning reservation unit data
    /// </summary>
    public class ReservationUnitResponseDto
    {
        public int UnitId { get; set; }
        public int ReservationId { get; set; }
        public int ApartmentId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        public int? NumberOfNights { get; set; }
        public decimal RentAmount { get; set; }
        public decimal VatRate { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal LodgingTaxRate { get; set; }
        public decimal? LodgingTaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public ReservationUnitStatus Status { get; set; } = ReservationUnitStatus.Reserved;
        public string StatusWord { get; set; } = "Reserved";
        public DateTime CreatedAt { get; set; }

        // Related entity names for display
        public string? ReservationNo { get; set; }
        public string? ApartmentName { get; set; }
        public string? ApartmentCode { get; set; }
        public string? BuildingName { get; set; }
        public string? FloorName { get; set; }
        public string? RoomTypeName { get; set; }
        public string? CustomerName { get; set; }
        public string? HotelName { get; set; }
    }
}
