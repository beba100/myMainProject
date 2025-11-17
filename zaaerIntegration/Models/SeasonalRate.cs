using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Seasonal rate header. Contains meta and date range; per-room-type rates are in SeasonalRateItem.
	/// </summary>
	[Table("seasonal_rates")]
	public class SeasonalRate
	{
		[Key]
		[Column("season_id")]
		public int SeasonId { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("title")]
		[MaxLength(200)]
		[Required]
		public string Title { get; set; } = string.Empty;

		[Column("description")]
		[MaxLength(1000)]
		public string? Description { get; set; }

		[Column("date_from")]
		[Required]
		public DateTime DateFrom { get; set; }

		[Column("date_to")]
		[Required]
		public DateTime DateTo { get; set; }

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

		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;
		public ICollection<SeasonalRateItem> Items { get; set; } = new List<SeasonalRateItem>();
	}
}


