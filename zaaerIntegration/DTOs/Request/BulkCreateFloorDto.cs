using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for bulk creating multiple floors
    /// </summary>
    public class BulkCreateFloorDto
    {
        [Required]
        public int BuildingId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one floor must be provided")]
        public List<CreateFloorItemDto> Floors { get; set; } = new List<CreateFloorItemDto>();
    }

}
