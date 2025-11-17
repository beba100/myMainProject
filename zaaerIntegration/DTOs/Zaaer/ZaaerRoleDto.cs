using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// DTO for creating a new role
	/// </summary>
	public class ZaaerCreateRoleDto
	{
		[Required]
		public int HotelId { get; set; }

		[Required]
		[StringLength(100)]
		public string RoleName { get; set; } = string.Empty;

		[StringLength(500)]
		public string? RoleDescription { get; set; }

		public List<int> PermissionIds { get; set; } = new List<int>();

		public bool IsActive { get; set; } = true;

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }
	}

	/// <summary>
	/// DTO for updating an existing role
	/// </summary>
	public class ZaaerUpdateRoleDto
	{
		[Required]
		public int RoleId { get; set; }

		[Required]
		public int HotelId { get; set; }

		[Required]
		[StringLength(100)]
		public string RoleName { get; set; } = string.Empty;

		[StringLength(500)]
		public string? RoleDescription { get; set; }

		public List<int> PermissionIds { get; set; } = new List<int>();

		public bool IsActive { get; set; } = true;

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }
	}

	/// <summary>
	/// DTO for role response
	/// </summary>
	public class ZaaerRoleResponseDto
	{
		public int RoleId { get; set; }
		public int HotelId { get; set; }
		public string RoleName { get; set; } = string.Empty;
		public string? RoleDescription { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public int? CreatedBy { get; set; }
		public int? UpdatedBy { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		public List<ZaaerPermissionResponseDto> Permissions { get; set; } = new List<ZaaerPermissionResponseDto>();
	}

	/// <summary>
	/// DTO for permission response
	/// </summary>
	public class ZaaerPermissionResponseDto
	{
		public int PermissionId { get; set; }
		public string PermissionName { get; set; } = string.Empty;
		public string PermissionCode { get; set; } = string.Empty;
		public string ModuleName { get; set; } = string.Empty;
		public string ActionName { get; set; } = string.Empty;
		public string? Description { get; set; }
		public bool IsActive { get; set; }
		public bool Granted { get; set; }
	}
}
