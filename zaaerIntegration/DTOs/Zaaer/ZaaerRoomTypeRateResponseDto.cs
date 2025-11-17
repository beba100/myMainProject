namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for room type rate response via Zaaer integration
    /// </summary>
    public class ZaaerRoomTypeRateResponseDto
    {
        /// <summary>
        /// Rate ID
        /// </summary>
        public int RateId { get; set; }

        /// <summary>
        /// Room Type ID
        /// </summary>
        public int RoomTypeId { get; set; }

        /// <summary>
        /// Room Type Name
        /// </summary>
        public string? RoomTypeName { get; set; }

        /// <summary>
        /// Hotel ID
        /// </summary>
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
        /// Created at
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated at
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}

