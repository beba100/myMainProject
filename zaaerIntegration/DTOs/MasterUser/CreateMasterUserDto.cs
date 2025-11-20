using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.MasterUser
{
    /// <summary>
    /// DTO for creating a new Master User
    /// </summary>
    public class CreateMasterUserDto
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [StringLength(100, ErrorMessage = "اسم المستخدم يجب ألا يتجاوز 100 حرف")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(500, ErrorMessage = "كلمة المرور يجب ألا تتجاوز 500 حرف")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "معرف الفندق مطلوب")]
        public int TenantId { get; set; }

        [StringLength(50, ErrorMessage = "رقم الجوال يجب ألا يتجاوز 50 حرف")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(200, ErrorMessage = "البريد الإلكتروني يجب ألا يتجاوز 200 حرف")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "الرقم الوظيفي يجب ألا يتجاوز 50 حرف")]
        public string? EmployeeNumber { get; set; }

        [StringLength(100, ErrorMessage = "الاسم الكامل يجب ألا يتجاوز 100 حرف")]
        public string? FullName { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// List of Role IDs to assign to the user
        /// </summary>
        public List<int> RoleIds { get; set; } = new List<int>();

        /// <summary>
        /// List of Tenant IDs (hotels) the user can access (in addition to the primary TenantId)
        /// </summary>
        public List<int> AdditionalTenantIds { get; set; } = new List<int>();
    }
}

