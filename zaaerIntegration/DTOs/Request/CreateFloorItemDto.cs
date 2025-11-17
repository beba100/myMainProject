using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a single floor item in bulk operations
    /// </summary>
    public class CreateFloorItemDto
    {
        [Required(ErrorMessage = "Floor number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Floor number must be a positive integer.")]
        public int FloorNumber { get; set; }

        [MaxLength(100, ErrorMessage = "Floor name cannot exceed 100 characters.")]
        public string? FloorName { get; set; }
    }
}
