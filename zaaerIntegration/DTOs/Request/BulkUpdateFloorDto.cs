using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for bulk updating multiple floors
    /// </summary>
    public class BulkUpdateFloorDto
    {
        [Required]
        public int BuildingId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one floor must be provided")]
        public List<UpdateFloorItemDto> Floors { get; set; } = new List<UpdateFloorItemDto>();
    }

}
