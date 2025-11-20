namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// Master User Model - المستخدم في قاعدة البيانات المركزية
    /// </summary>
    public class MasterUser
    {
        /// <summary>
        /// معرف المستخدم
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// اسم المستخدم (Username)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// كلمة المرور المشفرة (BCrypt Hash)
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// معرف الفندق (Tenant) المرتبط بهذا المستخدم
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// هل المستخدم نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاريخ الإنشاء
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// تاريخ آخر تحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// رقم الجوال
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// البريد الإلكتروني
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// الرقم الوظيفي
        /// </summary>
        public string? EmployeeNumber { get; set; }

        /// <summary>
        /// الاسم الكامل
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Navigation Property - الفندق المرتبط
        /// </summary>
        public Tenant? Tenant { get; set; }

        /// <summary>
        /// Navigation Property - أدوار المستخدم
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

