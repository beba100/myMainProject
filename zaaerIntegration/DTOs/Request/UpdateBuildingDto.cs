using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for updating an existing building
    /// </summary>
    public class UpdateBuildingDto
    {
        [Required]
        public int BuildingId { get; set; }

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
