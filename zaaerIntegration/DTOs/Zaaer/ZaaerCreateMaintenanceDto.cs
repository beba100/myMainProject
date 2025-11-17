using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// DTO for creating a maintenance record via Zaaer integration
	/// </summary>
	public class ZaaerCreateMaintenanceDto
	{
		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		/// <summary>
		/// Hotel ID
		/// </summary>
		[Required]
		public int HotelId { get; set; }

		/// <summary>
		/// Unit ID (Apartment ID)
		/// </summary>
		[Required]
		public int UnitId { get; set; }

		/// <summary>
		/// User ID who created the maintenance record
		/// </summary>
		[Required]
		public int UserId { get; set; }

		/// <summary>
		/// Maintenance start date (format: YYYY-MM-DD)
		/// </summary>
		[Required]
		public DateTime FromDate { get; set; }

		/// <summary>
		/// Maintenance end date (format: YYYY-MM-DD)
		/// </summary>
		[Required]
		public DateTime ToDate { get; set; }

		/// <summary>
		/// Reason for maintenance (e.g., "maintenance", "staff_shortage")
		/// </summary>
		[Required]
		[StringLength(100)]
		public string Reason { get; set; } = string.Empty;

		/// <summary>
		/// Additional comments or notes
		/// </summary>
		[StringLength(500)]
		public string? Comment { get; set; }
	}
}

