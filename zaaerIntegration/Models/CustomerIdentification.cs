using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Customer Identification Entity - هوية العميل
	/// Represents identification documents for customers (multiple IDs per customer)
	/// </summary>
	[Table("customer_identifications")]
	public class CustomerIdentification
	{
		[Key]
		[Column("identification_id")]
		public int IdentificationId { get; set; }

		[Required]
		[Column("customer_id")]
		public int CustomerId { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }

		// ID Type
		[Required]
		[Column("id_type_id")]
		public int IdTypeId { get; set; }

		// ID Details
		[Required]
		[MaxLength(50)]
		[Column("id_number")]
		public string IdNumber { get; set; } = string.Empty;

		[MaxLength(20)]
		[Column("version_number")]
		public string? VersionNumber { get; set; }

		// Issue Place
		[MaxLength(100)]
		[Column("issue_place")]
		public string? IssuePlace { get; set; }

		[MaxLength(100)]
		[Column("issue_place_ar")]
		public string? IssuePlaceAr { get; set; }

		// Dates
		[Column("issue_date")]
		public DateTime? IssueDate { get; set; }

		[Column("expiry_date")]
		public DateTime? ExpiryDate { get; set; }

		// Notes
		[MaxLength(500)]
		[Column("notes")]
		public string? Notes { get; set; }

		// Is Primary
		[Required]
		[Column("is_primary")]
		public bool IsPrimary { get; set; } = false;

		// Tracking
		[Required]
		[Column("is_active")]
		public bool IsActive { get; set; } = true;

		[Required]
		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		// Navigation Properties
		public virtual Customer? Customer { get; set; }
		public virtual IdType? IdType { get; set; }
	}
}

