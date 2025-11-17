using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new hotel
    /// </summary>
    public class CreateHotelDto
    {
        [Required]
        [StringLength(50)]
        public string HotelCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string HotelName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;
    }
}
