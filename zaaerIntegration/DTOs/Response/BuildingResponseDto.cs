namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning building data
    /// </summary>
    public class BuildingResponseDto
    {
        public int BuildingId { get; set; }
        public int HotelId { get; set; }
        public string? BuildingNumber { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public string? Address { get; set; }

        // Related entity names for display
        public string? HotelName { get; set; }
        public int TotalFloors { get; set; }
        public int TotalApartments { get; set; }
        public int TotalReservations { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
