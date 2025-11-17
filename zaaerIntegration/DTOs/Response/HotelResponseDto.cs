namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning hotel data
    /// </summary>
    public class HotelResponseDto
    {
        public int HotelId { get; set; }
        public string HotelCode { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
