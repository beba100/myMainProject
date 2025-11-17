using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول الأدوار - Roles table
	/// </summary>
	[Table("roles")]
	public class Role
	{
		[Key]
		[Column("role_id")]
		public int RoleId { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("role_name")]
		[Required]
		[MaxLength(100)]
		public string RoleName { get; set; } = string.Empty;

		[Column("role_description")]
		[MaxLength(500)]
		public string? RoleDescription { get; set; }

		[Column("is_active")]
		[Required]
		public bool IsActive { get; set; } = true;

		[Column("created_at")]
		[Required]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		[Column("created_by")]
		public int? CreatedBy { get; set; }

		[Column("updated_by")]
		public int? UpdatedBy { get; set; }

		// Navigation properties
		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;

		[ForeignKey("CreatedBy")]
		public User? CreatedByUser { get; set; }

		[ForeignKey("UpdatedBy")]
		public User? UpdatedByUser { get; set; }

		public ICollection<User> Users { get; set; } = new List<User>();
		public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }
	}
}
