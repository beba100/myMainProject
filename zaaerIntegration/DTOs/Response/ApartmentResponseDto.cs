namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning apartment data
    /// </summary>
    public class ApartmentResponseDto
    {
        public int ApartmentId { get; set; }
        public int HotelId { get; set; }
        public int? BuildingId { get; set; }
        public int? FloorId { get; set; }
        public int? RoomTypeId { get; set; }
        public string ApartmentCode { get; set; } = string.Empty;
        public string? ApartmentName { get; set; }
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        // Related entity names for display
        public string? HotelName { get; set; }
        public string? BuildingName { get; set; }
        public string? FloorName { get; set; }
        public string? RoomTypeName { get; set; }
        public int TotalReservations { get; set; }
        public decimal TotalRevenue { get; set; }
        public bool IsAvailable { get; set; }
    }
}
