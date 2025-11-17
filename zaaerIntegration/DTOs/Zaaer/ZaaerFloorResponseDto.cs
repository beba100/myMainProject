using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for floor response via Zaaer integration
    /// </summary>
    public class ZaaerFloorResponseDto
    {
        /// <summary>
        /// Floor ID
        /// </summary>
        public int FloorId { get; set; }

        /// <summary>
        /// Hotel ID
        /// </summary>
        public int HotelId { get; set; }

        /// <summary>
        /// Floor number
        /// </summary>
        public int FloorNumber { get; set; }

        /// <summary>
        /// Floor name
        /// </summary>
        public string? FloorName { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
