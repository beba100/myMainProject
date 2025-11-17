using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Seasonal rate per room type
	/// </summary>
	[Table("seasonal_rate_items")]
	public class SeasonalRateItem
	{
		[Key]
		[Column("item_id")]
		public int ItemId { get; set; }

		[Column("season_id")]
		[Required]
		public int SeasonId { get; set; }

		[Column("roomtype_id")]
		[Required]
		public int RoomTypeId { get; set; }

		// Daily rates
		[Column("daily_rate_low_weekdays", TypeName = "decimal(12,2)")]
		public decimal? DailyRateLowWeekdays { get; set; }

		[Column("daily_rate_high_weekdays", TypeName = "decimal(12,2)")]
		public decimal? DailyRateHighWeekdays { get; set; }

		// OTA rates
		[Column("ota_rate_low_weekdays", TypeName = "decimal(12,2)")]
		public decimal? OtaRateLowWeekdays { get; set; }

		[Column("ota_rate_high_weekdays", TypeName = "decimal(12,2)")]
		public decimal? OtaRateHighWeekdays { get; set; }

		[ForeignKey("SeasonId")]
		public SeasonalRate Season { get; set; } = null!;

		[ForeignKey("RoomTypeId")]
		public RoomType RoomType { get; set; } = null!;
	}
}


