using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for creating a floor via Zaaer integration
    /// </summary>
    public class ZaaerCreateFloorDto
    {
        /// <summary>
        /// Hotel ID
        /// </summary>
        [Required]
        public int HotelId { get; set; }

        /// <summary>
        /// Floor number
        /// </summary>
        [Required]
        public int FloorNumber { get; set; }

        /// <summary>
        /// Floor name
        /// </summary>
        [StringLength(100)]
        public string? FloorName { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
