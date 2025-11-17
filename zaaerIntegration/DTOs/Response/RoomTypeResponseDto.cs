namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning room type data
    /// </summary>
    public class RoomTypeResponseDto
    {
        public int RoomTypeId { get; set; }
        public int HotelId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string? RoomTypeDesc { get; set; }
        public decimal? BaseRate { get; set; }

        // Related entity names for display
        public string? HotelName { get; set; }
        public int TotalApartments { get; set; }
        public int TotalReservations { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenue { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal AverageStayDuration { get; set; }
    }
}
