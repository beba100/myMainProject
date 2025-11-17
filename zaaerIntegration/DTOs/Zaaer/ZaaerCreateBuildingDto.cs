using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// DTO for creating a building with its floors in a single request
	/// </summary>
	public class ZaaerCreateBuildingDto
	{
		[Required]
		public int HotelId { get; set; }

		[Required]
		[StringLength(200)]
		public string BuildingName { get; set; } = string.Empty;

		[StringLength(50)]
		public string? BuildingNumber { get; set; }

		[StringLength(500)]
		public string? Address { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		public List<ZaaerCreateBuildingFloorItemDto> Floors { get; set; } = new();
	}

	/// <summary>
	/// Inner floor item used when creating a building with floors
	/// </summary>
	public class ZaaerCreateBuildingFloorItemDto
	{
		[Required]
		public int FloorNumber { get; set; }

		[StringLength(100)]
		public string? FloorName { get; set; }
	}
}


