using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for Master User Service
    /// </summary>
    public interface IMasterUserService
    {
        /// <summary>
        /// الحصول على المستخدم بواسطة Username
        /// </summary>
        Task<MasterUser?> GetByUsernameAsync(string username);

        /// <summary>
        /// الحصول على المستخدم بواسطة Id
        /// </summary>
        Task<MasterUser?> GetByIdAsync(int userId);

        /// <summary>
        /// الحصول على أدوار المستخدم
        /// </summary>
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);

        /// <summary>
        /// التحقق من كلمة المرور
        /// </summary>
        bool ValidatePassword(string password, string passwordHash);

        /// <summary>
        /// تشفير كلمة المرور
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// إنشاء مستخدم جديد
        /// </summary>
        Task<MasterUser> CreateUserAsync(string username, string password, int tenantId, IEnumerable<int> roleIds);

        /// <summary>
        /// إنشاء مستخدم جديد مع الحقول الإضافية
        /// </summary>
        Task<MasterUser> CreateUserAsync(string username, string password, int tenantId, IEnumerable<int> roleIds, 
            string? phoneNumber, string? email, string? employeeNumber, string? fullName, 
            IEnumerable<int>? additionalTenantIds = null);

        /// <summary>
        /// الحصول على جميع المستخدمين
        /// </summary>
        Task<IEnumerable<MasterUser>> GetAllUsersAsync();

        /// <summary>
        /// تحديث مستخدم
        /// </summary>
        Task<MasterUser> UpdateUserAsync(int userId, string? username, string? password, int? tenantId, 
            string? phoneNumber, string? email, string? employeeNumber, string? fullName, 
            bool? isActive, IEnumerable<int>? roleIds, IEnumerable<int>? additionalTenantIds);

        /// <summary>
        /// حذف مستخدم
        /// </summary>
        Task<bool> DeleteUserAsync(int userId);

        /// <summary>
        /// التحقق من صحة بيانات تسجيل الدخول
        /// </summary>
        Task<MasterUser?> ValidateLoginAsync(string username, string password);
    }
}

