using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Visit Purpose Model
	/// نموذج أغراض الزيارة
	/// </summary>
	[Table("visitpurpose")]
	public class VisitPurpose
	{
		[Key]
		[Column("vp_id")]
		public int VpId { get; set; }

		[Column("vp_name")]
		[Required]
		[MaxLength(100)]
		public string VpName { get; set; } = string.Empty;

		[Column("vp_name_ar")]
		[MaxLength(100)]
		public string? VpNameAr { get; set; }

		[Column("description")]
		[MaxLength(500)]
		public string? Description { get; set; }

		[Column("description_ar")]
		[MaxLength(500)]
		public string? DescriptionAr { get; set; }

		[Column("is_active")]
		public bool IsActive { get; set; } = true;

		[Column("sort_order")]
		public int SortOrder { get; set; } = 0;

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		// Navigation Properties
		/// <summary>
		/// Reservations with this visit purpose
		/// الحجوزات بهذا الغرض
		/// </summary>
		public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
	}
}
