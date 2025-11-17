using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول الصلاحيات - Permissions table
	/// </summary>
	[Table("permissions")]
	public class Permission
	{
		[Key]
		[Column("permission_id")]
		public int PermissionId { get; set; }

		[Column("permission_name")]
		[Required]
		[MaxLength(100)]
		public string PermissionName { get; set; } = string.Empty;

		[Column("permission_code")]
		[Required]
		[MaxLength(100)]
		public string PermissionCode { get; set; } = string.Empty;

		[Column("module_name")]
		[Required]
		[MaxLength(50)]
		public string ModuleName { get; set; } = string.Empty;

		[Column("action_name")]
		[Required]
		[MaxLength(50)]
		public string ActionName { get; set; } = string.Empty;

		[Column("description")]
		[MaxLength(500)]
		public string? Description { get; set; }

		[Column("is_active")]
		[Required]
		public bool IsActive { get; set; } = true;

		[Column("created_at")]
		[Required]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		// Navigation properties
		public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
	}
}
