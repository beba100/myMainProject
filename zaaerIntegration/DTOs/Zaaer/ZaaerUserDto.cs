using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using zaaerIntegration.Converters;

namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// DTO for creating a new user
	/// </summary>
	public class ZaaerCreateUserDto
	{
		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		[Required]
		public int HotelId { get; set; }

		[Required]
		[StringLength(100)]
		public string FirstName { get; set; } = string.Empty;

		[Required]
		[StringLength(100)]
		public string LastName { get; set; } = string.Empty;

		[StringLength(100)]
		public string? Title { get; set; }

		[StringLength(500)]
		public string? ProfilePictureUrl { get; set; }

		[StringLength(500)]
		public string? SignatureUrl { get; set; }

		public DateTime? DateOfBirth { get; set; }

		[StringLength(20)]
		public string? Gender { get; set; }

		[StringLength(100)]
		public string? Department { get; set; }

		[StringLength(1000)]
		public string? Description { get; set; }

		[Required]
		[EmailAddress]
		[StringLength(255)]
		public string Email { get; set; } = string.Empty;

		[StringLength(20)]
		public string? PhoneNumber { get; set; }

		[StringLength(20)]
		public string? BusinessPhoneNumber { get; set; }

		[StringLength(500)]
		public string? Address { get; set; }

		[StringLength(100)]
		public string? Password { get; set; }

		[StringLength(50)]
		public string? UserType { get; set; }

		public int? RoleId { get; set; }

		/// <summary>
		/// Status accepts integer (0/1) or boolean (true/false)
		/// </summary>
		[JsonConverter(typeof(FlexibleBooleanJsonConverter))]
		public bool Status { get; set; } = true;

		/// <summary>
		/// ChangePassword accepts integer (0/1) or boolean (true/false)
		/// </summary>
		[JsonConverter(typeof(FlexibleBooleanJsonConverter))]
		public bool ChangePassword { get; set; } = false;
	}

	/// <summary>
	/// DTO for updating an existing user
	/// </summary>
	public class ZaaerUpdateUserDto
	{
		/// <summary>
		/// Zaaer System ID (معرف Zaaer) - used to find the user to update
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		public int? HotelId { get; set; }

		[StringLength(100)]
		public string? FirstName { get; set; }

		[StringLength(100)]
		public string? LastName { get; set; }

		[StringLength(100)]
		public string? Title { get; set; }

		[StringLength(500)]
		public string? ProfilePictureUrl { get; set; }

		[StringLength(500)]
		public string? SignatureUrl { get; set; }

		public DateTime? DateOfBirth { get; set; }

		[StringLength(20)]
		public string? Gender { get; set; }

		[StringLength(100)]
		public string? Department { get; set; }

		[StringLength(1000)]
		public string? Description { get; set; }

		[EmailAddress]
		[StringLength(255)]
		public string? Email { get; set; }

		[StringLength(20)]
		public string? PhoneNumber { get; set; }

		[StringLength(20)]
		public string? BusinessPhoneNumber { get; set; }

		[StringLength(500)]
		public string? Address { get; set; }

		[StringLength(50)]
		public string? UserType { get; set; }

		public int? RoleId { get; set; }

		/// <summary>
		/// Status accepts integer (0/1) or boolean (true/false)
		/// </summary>
		[JsonConverter(typeof(FlexibleNullableBooleanJsonConverter))]
		public bool? Status { get; set; }

		/// <summary>
		/// ChangePassword accepts integer (0/1) or boolean (true/false)
		/// </summary>
		[JsonConverter(typeof(FlexibleNullableBooleanJsonConverter))]
		public bool? ChangePassword { get; set; }

		[StringLength(100)]
		public string? Password { get; set; }
	}

	/// <summary>
	/// DTO for user response
	/// </summary>
	public class ZaaerUserResponseDto
	{
		public int UserId { get; set; }
		public int HotelId { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string? Title { get; set; }
		public string? ProfilePictureUrl { get; set; }
		public string? SignatureUrl { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string? Gender { get; set; }
		public string? Department { get; set; }
		public string? Description { get; set; }
		public string Email { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
		public string? BusinessPhoneNumber { get; set; }
		public string? Address { get; set; }
		public string? UserType { get; set; }
		public int? RoleId { get; set; }
		public string? RoleName { get; set; }
		public bool Status { get; set; }
		public bool ChangePassword { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? LastLogin { get; set; }
		public bool IsActive { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }
	}
}
