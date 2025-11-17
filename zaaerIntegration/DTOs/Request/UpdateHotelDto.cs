using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for updating an existing hotel
    /// </summary>
    public class UpdateHotelDto
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
