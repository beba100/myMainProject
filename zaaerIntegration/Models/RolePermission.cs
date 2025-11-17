using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول ربط الأدوار بالصلاحيات - RolePermissions junction table
	/// </summary>
	[Table("role_permissions")]
	public class RolePermission
	{
		[Key]
		[Column("role_permission_id")]
		public int RolePermissionId { get; set; }

		[Column("role_id")]
		[Required]
		public int RoleId { get; set; }

		[Column("permission_id")]
		[Required]
		public int PermissionId { get; set; }

		[Column("granted")]
		[Required]
		public bool Granted { get; set; } = true;

		[Column("created_at")]
		[Required]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		[Column("created_by")]
		public int? CreatedBy { get; set; }

		// Navigation properties
		[ForeignKey("RoleId")]
		public Role Role { get; set; } = null!;

		[ForeignKey("PermissionId")]
		public Permission Permission { get; set; } = null!;

		[ForeignKey("CreatedBy")]
		public User? CreatedByUser { get; set; }
	}
}
