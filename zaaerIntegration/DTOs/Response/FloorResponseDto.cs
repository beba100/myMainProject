namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning floor data
    /// </summary>
    public class FloorResponseDto
    {
        public int FloorId { get; set; }
        public int BuildingId { get; set; }
        public int FloorNumber { get; set; }
        public string? FloorName { get; set; }

        // Related entity names for display
        public string? BuildingName { get; set; }
        public string? HotelName { get; set; }
        public int TotalApartments { get; set; }
        public int TotalReservations { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
