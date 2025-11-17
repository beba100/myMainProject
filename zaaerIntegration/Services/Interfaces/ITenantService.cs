using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// خدمة للحصول على معلومات الفندق (Tenant) الحالي من HTTP Request
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// الحصول على الفندق الحالي بناءً على X-Hotel-Code Header
        /// </summary>
        /// <returns>معلومات الفندق أو null إذا لم يتم العثور عليه</returns>
        Tenant? GetTenant();

        /// <summary>
        /// الحصول على كود الفندق من HTTP Request
        /// </summary>
        /// <returns>كود الفندق أو null</returns>
        string? GetTenantCode();

        /// <summary>
        /// الحصول على Connection String للفندق الحالي بناءً على DatabaseName
        /// </summary>
        /// <returns>Connection String للفندق الحالي</returns>
        string GetTenantConnectionString();

        /// <summary>
        /// التحقق من الاتصال بقاعدة بيانات الفندق الحالي
        /// </summary>
        /// <returns>true إذا كان الاتصال ناجحاً</returns>
        Task<bool> ValidateTenantConnectionAsync();
    }
}

