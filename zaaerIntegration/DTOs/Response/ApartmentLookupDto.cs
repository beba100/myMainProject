namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// Lightweight DTO for the expenses screen. Keeps the payload minimal.
    /// </summary>
    public class ApartmentLookupDto
    {
        public int ApartmentId { get; set; }
        public int? ZaaerId { get; set; }
        public string ApartmentCode { get; set; } = string.Empty;
        public string? ApartmentName { get; set; }
        public int HotelId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

