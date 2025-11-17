using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for updating a room type via Zaaer integration
    /// </summary>
    public class ZaaerUpdateRoomTypeDto
    {
        /// <summary>
        /// Hotel ID
        /// </summary>
        public int? HotelId { get; set; }

        /// <summary>
        /// Room type name
        /// </summary>
        [StringLength(200)]
        public string? RoomTypeName { get; set; }

        /// <summary>
        /// Room type description
        /// </summary>
        [StringLength(500)]
        public string? RoomTypeDesc { get; set; }

        /// <summary>
        /// Base rate
        /// </summary>
        public decimal? BaseRate { get; set; }

        /// <summary>
        /// Season rate
        /// </summary>
        public decimal? SeasonRate { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
