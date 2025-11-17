namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// DTO for maintenance response via Zaaer integration
	/// </summary>
	public class ZaaerMaintenanceResponseDto
	{
		/// <summary>
		/// Maintenance ID
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		/// <summary>
		/// Hotel ID
		/// </summary>
		public int HotelId { get; set; }

		/// <summary>
		/// Unit ID (Apartment ID)
		/// </summary>
		public int UnitId { get; set; }

		/// <summary>
		/// User ID who created the maintenance record
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// Maintenance start date
		/// </summary>
		public DateTime FromDate { get; set; }

		/// <summary>
		/// Maintenance end date
		/// </summary>
		public DateTime ToDate { get; set; }

		/// <summary>
		/// Reason for maintenance
		/// </summary>
		public string Reason { get; set; } = string.Empty;

		/// <summary>
		/// Additional comments or notes
		/// </summary>
		public string? Comment { get; set; }

		/// <summary>
		/// Maintenance status
		/// </summary>
		public string Status { get; set; } = "active";

		/// <summary>
		/// Created date
		/// </summary>
		public DateTime CreatedAt { get; set; }

		/// <summary>
		/// Updated date
		/// </summary>
		public DateTime? UpdatedAt { get; set; }
	}
}

