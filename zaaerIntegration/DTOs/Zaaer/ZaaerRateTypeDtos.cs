using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// DTO for unit item within a rate type
	/// </summary>
	public class ZaaerRateTypeUnitItemDto
	{
		[Required]
		[MaxLength(100)]
		public string UnitTypeName { get; set; } = string.Empty;

		[Required]
		public decimal Rate { get; set; }

		public bool IsEnabled { get; set; } = false;
	}

	/// <summary>
	/// DTO for creating a rate type
	/// </summary>
	public class ZaaerCreateRateTypeDto
	{
		[Required]
		public int HotelId { get; set; }

		[Required]
		[MaxLength(50)]
		public string ShortCode { get; set; } = string.Empty;

		[Required]
		[MaxLength(255)]
		public string Title { get; set; } = string.Empty;

		public bool Status { get; set; } = true;

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		[Required]
		public List<ZaaerRateTypeUnitItemDto> UnitItems { get; set; } = new();
	}

	/// <summary>
	/// DTO for updating a rate type
	/// Note: RateTypeId is optional because it can come from route parameter or be ignored when using ZaaerId
	/// </summary>
	public class ZaaerUpdateRateTypeDto
	{
		/// <summary>
		/// Internal RateTypeId (optional - not used when updating by ZaaerId from route)
		/// </summary>
		public int? RateTypeId { get; set; }

		public int? HotelId { get; set; }

		[MaxLength(50)]
		public string? ShortCode { get; set; }

		[MaxLength(255)]
		public string? Title { get; set; }

		public bool? Status { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		/// <summary>
		/// Unit items list (optional - Zaaer may send empty array or unit item fields in root)
		/// </summary>
		public List<ZaaerRateTypeUnitItemDto>? UnitItems { get; set; }

		/// <summary>
		/// Support for Zaaer's alternative format where unit item fields are in root level
		/// These are ignored if UnitItems array is provided and not empty
		/// </summary>
		[MaxLength(100)]
		public string? UnitTypeName { get; set; }

		public decimal? Rate { get; set; }

		public bool? IsEnabled { get; set; }
	}

	/// <summary>
	/// DTO for unit item response
	/// </summary>
	public class ZaaerRateTypeUnitItemResponseDto : ZaaerRateTypeUnitItemDto
	{
		public int Id { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}

	/// <summary>
	/// DTO for rate type response
	/// </summary>
	public class ZaaerRateTypeResponseDto
	{
		public int Id { get; set; }
		public int HotelId { get; set; }
		public string ShortCode { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public bool Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		public List<ZaaerRateTypeUnitItemResponseDto> UnitItems { get; set; } = new();
	}
}
