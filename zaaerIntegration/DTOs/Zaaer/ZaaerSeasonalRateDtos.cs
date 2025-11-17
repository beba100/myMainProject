using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
	public class ZaaerSeasonalRateItemDto
	{
		[Required]
		public int RoomTypeId { get; set; }
		public decimal? DailyRateLowWeekdays { get; set; }
		public decimal? DailyRateHighWeekdays { get; set; }
		public decimal? OtaRateLowWeekdays { get; set; }
		public decimal? OtaRateHighWeekdays { get; set; }
	}

	public class ZaaerCreateSeasonalRateDto
	{
		[Required]
		public int HotelId { get; set; }
		[Required, MaxLength(200)]
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		[Required]
		public DateTime DateFrom { get; set; }
		[Required]
		public DateTime DateTo { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		[Required]
		public List<ZaaerSeasonalRateItemDto> Items { get; set; } = new();
	}

	public class ZaaerUpdateSeasonalRateDto
	{
		[Required]
		public int SeasonId { get; set; }
		public int? HotelId { get; set; }
		public string? Title { get; set; }
		public string? Description { get; set; }
		public DateTime? DateFrom { get; set; }
		public DateTime? DateTo { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		[Required]
		public List<ZaaerSeasonalRateItemDto> Items { get; set; } = new();
	}

	public class ZaaerSeasonalRateItemResponseDto : ZaaerSeasonalRateItemDto
	{
		public int ItemId { get; set; }
	}

	public class ZaaerSeasonalRateResponseDto
	{
		public int SeasonId { get; set; }
		public int HotelId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		public List<ZaaerSeasonalRateItemResponseDto> Items { get; set; } = new();
	}
}


