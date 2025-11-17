using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for creating room type rates via Zaaer integration
    /// </summary>
    public class ZaaerCreateRoomTypeRateDto
    {
        /// <summary>
        /// Room Type ID
        /// </summary>
        [Required]
        public int RoomTypeId { get; set; }

        /// <summary>
        /// Hotel ID
        /// </summary>
        [Required]
        public int HotelId { get; set; }

        /// <summary>
        /// Daily rate for low weekdays
        /// </summary>
        public decimal? DailyRateLowWeekdays { get; set; }

        /// <summary>
        /// Daily rate for high weekdays
        /// </summary>
        public decimal? DailyRateHighWeekdays { get; set; }

        /// <summary>
        /// Minimum daily rate
        /// </summary>
        public decimal? DailyRateMin { get; set; }

        /// <summary>
        /// Monthly rate
        /// </summary>
        public decimal? MonthlyRate { get; set; }

        /// <summary>
        /// Minimum monthly rate
        /// </summary>
        public decimal? MonthlyRateMin { get; set; }

        /// <summary>
        /// OTA rate for low weekdays
        /// </summary>
        public decimal? OtaRateLowWeekdays { get; set; }

        /// <summary>
        /// OTA rate for high weekdays
        /// </summary>
        public decimal? OtaRateHighWeekdays { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}

