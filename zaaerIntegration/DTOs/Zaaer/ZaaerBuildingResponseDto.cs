namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// Response DTO for building with its created floors
	/// </summary>
	public class ZaaerBuildingResponseDto
	{
		public int BuildingId { get; set; }
		public string BuildingName { get; set; } = string.Empty;
		public int HotelId { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		public IEnumerable<ZaaerFloorResponseItemDto> Floors { get; set; } = Enumerable.Empty<ZaaerFloorResponseItemDto>();
	}

	public class ZaaerFloorResponseItemDto
	{
		public int FloorId { get; set; }
		public int FloorNumber { get; set; }
		public string? FloorName { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }
	}
}


