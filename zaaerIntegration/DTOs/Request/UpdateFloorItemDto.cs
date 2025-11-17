using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for updating a single floor item in bulk operations
    /// </summary>
    public class UpdateFloorItemDto
    {
        [Required(ErrorMessage = "Floor ID is required.")]
        public int FloorId { get; set; }

        [Required(ErrorMessage = "Floor number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Floor number must be a positive integer.")]
        public int FloorNumber { get; set; }

        [MaxLength(100, ErrorMessage = "Floor name cannot exceed 100 characters.")]
        public string? FloorName { get; set; }
    }
}
