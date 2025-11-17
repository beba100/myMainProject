using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// جدول أسعار أنواع الغرف - Room Type Rates table
    /// Base Rates for Daily, Monthly, and OTA pricing
    /// </summary>
    [Table("room_type_rates")]
    public class RoomTypeRate
    {
        [Key]
        [Column("rate_id")]
        public int RateId { get; set; }

        [Column("roomtype_id")]
        [Required]
        public int RoomTypeId { get; set; }

        [Column("hotel_id")]
        [Required]
        public int HotelId { get; set; }

        // Daily Rates
        [Column("daily_rate_low_weekdays", TypeName = "decimal(12,2)")]
        public decimal? DailyRateLowWeekdays { get; set; }

        [Column("daily_rate_high_weekdays", TypeName = "decimal(12,2)")]
        public decimal? DailyRateHighWeekdays { get; set; }

        [Column("daily_rate_min", TypeName = "decimal(12,2)")]
        public decimal? DailyRateMin { get; set; }

        // Monthly Rates
        [Column("monthly_rate", TypeName = "decimal(12,2)")]
        public decimal? MonthlyRate { get; set; }

        [Column("monthly_rate_min", TypeName = "decimal(12,2)")]
        public decimal? MonthlyRateMin { get; set; }

        // OTA Rates
        [Column("ota_rate_low_weekdays", TypeName = "decimal(12,2)")]
        public decimal? OtaRateLowWeekdays { get; set; }

        [Column("ota_rate_high_weekdays", TypeName = "decimal(12,2)")]
        public decimal? OtaRateHighWeekdays { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = KsaTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        [Column("zaaer_id")]
        public int? ZaaerId { get; set; }

        // Navigation properties
        [ForeignKey("HotelId")]
        public HotelSettings HotelSettings { get; set; } = null!;

        [ForeignKey("RoomTypeId")]
        public RoomType RoomType { get; set; }

    }
}

