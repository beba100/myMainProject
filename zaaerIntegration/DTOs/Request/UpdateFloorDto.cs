using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for updating an existing floor
    /// </summary>
    public class UpdateFloorDto
    {
        [Required]
        public int FloorId { get; set; }

        [Required]
        public int BuildingId { get; set; }

        [Required]
        public int FloorNumber { get; set; }

        [StringLength(100)]
        public string? FloorName { get; set; }
    }
}
