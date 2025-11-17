using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new floor
    /// </summary>
    public class CreateFloorDto
    {
        [Required]
        public int BuildingId { get; set; }

        [Required]
        public int FloorNumber { get; set; }

        [StringLength(100)]
        public string? FloorName { get; set; }
    }
}
