namespace zaaerIntegration.DTOs.Auth
{
    /// <summary>
    /// DTO لاستجابة تسجيل الدخول
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// JWT Token
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// معرف المستخدم
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// اسم المستخدم
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// الاسم الكامل للمستخدم
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// البريد الإلكتروني
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// رقم الجوال
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// الرقم الوظيفي
        /// </summary>
        public string? EmployeeNumber { get; set; }

        /// <summary>
        /// معرف الفندق (Tenant)
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// كود الفندق
        /// </summary>
        public string TenantCode { get; set; } = string.Empty;

        /// <summary>
        /// اسم الفندق
        /// </summary>
        public string TenantName { get; set; } = string.Empty;

        /// <summary>
        /// أدوار المستخدم
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// تاريخ انتهاء Token
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}

