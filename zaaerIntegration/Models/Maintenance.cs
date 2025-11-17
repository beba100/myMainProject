using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Maintenance Entity - جدول الصيانة
	/// Represents maintenance records for apartments/units
	/// </summary>
	[Table("maintenances")]
	public class Maintenance
	{
		[Key]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }

		/// <summary>
		/// Hotel ID
		/// </summary>
		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		/// <summary>
		/// Unit ID (Apartment ID)
		/// </summary>
		[Column("unit_id")]
		[Required]
		public int UnitId { get; set; }

		/// <summary>
		/// User ID who created the maintenance record
		/// </summary>
		[Column("user_id")]
		[Required]
		public int UserId { get; set; }

		/// <summary>
		/// Maintenance start date
		/// </summary>
		[Column("from_date")]
		[Required]
		public DateTime FromDate { get; set; }

		/// <summary>
		/// Maintenance end date
		/// </summary>
		[Column("to_date")]
		[Required]
		public DateTime ToDate { get; set; }

		/// <summary>
		/// Reason for maintenance (e.g., "maintenance", "staff_shortage")
		/// </summary>
		[Column("reason")]
		[Required]
		[MaxLength(100)]
		public string Reason { get; set; } = string.Empty;

		/// <summary>
		/// Additional comments or notes
		/// </summary>
		[Column("comment")]
		[MaxLength(500)]
		public string? Comment { get; set; }

		/// <summary>
		/// Maintenance status (e.g., "active", "completed", "cancelled")
		/// </summary>
		[Column("status")]
		[MaxLength(50)]
		public string Status { get; set; } = "active";

		/// <summary>
		/// Created date
		/// </summary>
		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		/// <summary>
		/// Updated date
		/// </summary>
		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;

		[ForeignKey("UnitId")]
		public Apartment? Apartment { get; set; }

		[ForeignKey("UserId")]
		public User? User { get; set; }
	}
}

