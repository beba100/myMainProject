using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول المستخدمين - Users table
	/// </summary>
	[Table("users")]
	public class User
	{
		[Key]
		[Column("user_id")]
		public int UserId { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("status")]
		[Required]
		public bool Status { get; set; } = true;

		[Column("user_type")]
		[MaxLength(50)]
		public string UserType { get; set; } = string.Empty;

		[Column("first_name")]
		[Required]
		[MaxLength(100)]
		public string FirstName { get; set; } = string.Empty;

		[Column("last_name")]
		[Required]
		[MaxLength(100)]
		public string LastName { get; set; } = string.Empty;

		[Column("title")]
		[MaxLength(100)]
		public string? Title { get; set; }

		[Column("profile_picture_url")]
		[MaxLength(500)]
		public string? ProfilePictureUrl { get; set; }

		[Column("signature_url")]
		[MaxLength(500)]
		public string? SignatureUrl { get; set; }

		[Column("date_of_birth")]
		public DateTime? DateOfBirth { get; set; }

		[Column("gender")]
		[MaxLength(20)]
		public string? Gender { get; set; }

		[Column("department")]
		[MaxLength(100)]
		public string? Department { get; set; }

		[Column("description")]
		[MaxLength(1000)]
		public string? Description { get; set; }

		[Column("email")]
		[Required]
		[MaxLength(255)]
		public string Email { get; set; } = string.Empty;

		[Column("phone_number")]
		[MaxLength(20)]
		public string? PhoneNumber { get; set; }

		[Column("business_phone_number")]
		[MaxLength(20)]
		public string? BusinessPhoneNumber { get; set; }

		[Column("address")]
		[MaxLength(500)]
		public string? Address { get; set; }

		[Column("password_hash")]
		[Required]
		[MaxLength(255)]
		public string PasswordHash { get; set; } = string.Empty;

		[Column("change_password")]
		public bool ChangePassword { get; set; } = false;

		[Column("created_at")]
		[Required]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		[Column("last_login")]
		public DateTime? LastLogin { get; set; }

		[Column("is_active")]
		[Required]
		public bool IsActive { get; set; } = true;

		// Navigation properties
		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;

		[ForeignKey("RoleId")]
		public Role? Role { get; set; }

		[Column("role_id")]
		public int? RoleId { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }
	}
}
