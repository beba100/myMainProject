using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.MasterUser
{
    /// <summary>
    /// DTO for updating a Master User
    /// </summary>
    public class UpdateMasterUserDto
    {
        [StringLength(100, ErrorMessage = "اسم المستخدم يجب ألا يتجاوز 100 حرف")]
        public string? Username { get; set; }

        [StringLength(500, ErrorMessage = "كلمة المرور يجب ألا تتجاوز 500 حرف")]
        public string? Password { get; set; }

        public int? TenantId { get; set; }

        [StringLength(50, ErrorMessage = "رقم الجوال يجب ألا يتجاوز 50 حرف")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(200, ErrorMessage = "البريد الإلكتروني يجب ألا يتجاوز 200 حرف")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "الرقم الوظيفي يجب ألا يتجاوز 50 حرف")]
        public string? EmployeeNumber { get; set; }

        [StringLength(100, ErrorMessage = "الاسم الكامل يجب ألا يتجاوز 100 حرف")]
        public string? FullName { get; set; }

        public bool? IsActive { get; set; }

        /// <summary>
        /// List of Role IDs to assign to the user (replaces existing roles)
        /// </summary>
        public List<int>? RoleIds { get; set; }

        /// <summary>
        /// List of Tenant IDs (hotels) the user can access (replaces existing tenants)
        /// </summary>
        public List<int>? AdditionalTenantIds { get; set; }
    }
}

