using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new building
    /// </summary>
    public class CreateBuildingDto
    {
        [Required]
        public int HotelId { get; set; }

        [StringLength(50)]
        public string? BuildingNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string BuildingName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }
    }
}
