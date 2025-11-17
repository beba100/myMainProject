using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for room type response via Zaaer integration
    /// </summary>
    public class ZaaerRoomTypeResponseDto
    {
        /// <summary>
        /// Room type ID
        /// </summary>
        public int RoomTypeId { get; set; }

        /// <summary>
        /// Hotel ID
        /// </summary>
        public int HotelId { get; set; }

        /// <summary>
        /// Room type name
        /// </summary>
        public string RoomTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Room type description
        /// </summary>
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
