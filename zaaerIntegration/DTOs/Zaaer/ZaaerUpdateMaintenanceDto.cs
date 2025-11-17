using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// DTO for updating a maintenance record via Zaaer integration
	/// </summary>
	public class ZaaerUpdateMaintenanceDto
	{
		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		/// <summary>
		/// Hotel ID
		/// </summary>
		public int? HotelId { get; set; }

		/// <summary>
		/// Unit ID (Apartment ID)
		/// </summary>
		public int? UnitId { get; set; }

		/// <summary>
		/// User ID who created the maintenance record
		/// </summary>
		public int? UserId { get; set; }

		/// <summary>
		/// Maintenance start date (format: YYYY-MM-DD)
		/// </summary>
		public DateTime? FromDate { get; set; }

		/// <summary>
		/// Maintenance end date (format: YYYY-MM-DD)
		/// </summary>
		public DateTime? ToDate { get; set; }

		/// <summary>
		/// Reason for maintenance (e.g., "maintenance", "staff_shortage")
		/// </summary>
		[StringLength(100)]
		public string? Reason { get; set; }

		/// <summary>
		/// Additional comments or notes
		/// </summary>
		[StringLength(500)]
		public string? Comment { get; set; }
	}
}

