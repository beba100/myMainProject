using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Customer Service Interface
    /// واجهة خدمة العملاء
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Get all customers with pagination
        /// الحصول على جميع العملاء مع التصفح
        /// </summary>
        Task<(IEnumerable<CustomerResponseDto> Customers, int TotalCount)> GetAllCustomersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null);

        /// <summary>
        /// Get customer by ID
        /// الحصول على العميل بالمعرف
        /// </summary>
        Task<CustomerResponseDto?> GetCustomerByIdAsync(int customerId);

        /// <summary>
        /// Get customer by customer number
        /// الحصول على العميل برقم العميل
        /// </summary>
        Task<CustomerResponseDto?> GetCustomerByNoAsync(string customerNo);

        /// <summary>
        /// Create new customer
        /// إنشاء عميل جديد
        /// </summary>
        Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);

        /// <summary>
        /// Update existing customer
        /// تحديث العميل الموجود
        /// </summary>
        Task<CustomerResponseDto?> UpdateCustomerAsync(UpdateCustomerDto updateCustomerDto);

        /// <summary>
        /// Delete customer
        /// حذف العميل
        /// </summary>
        Task<bool> DeleteCustomerAsync(int customerId);

        /// <summary>
        /// Search customers by name
        /// البحث عن العملاء بالاسم
        /// </summary>
        Task<IEnumerable<CustomerResponseDto>> SearchCustomersAsync(string searchTerm);

        /// <summary>
        /// Get customers by nationality
        /// الحصول على العملاء بالجنسية
        /// </summary>
        Task<IEnumerable<CustomerResponseDto>> GetCustomersByNationalityAsync(int nationalityId);

        /// <summary>
        /// Get customers by guest type
        /// الحصول على العملاء بنوع الضيف
        /// </summary>
        Task<IEnumerable<CustomerResponseDto>> GetCustomersByGuestTypeAsync(int guestTypeId);

        /// <summary>
        /// Get customers by guest category
        /// الحصول على العملاء بفئة الضيف
        /// </summary>
        Task<IEnumerable<CustomerResponseDto>> GetCustomersByGuestCategoryAsync(int guestCategoryId);

        /// <summary>
        /// Get customers created in date range
        /// الحصول على العملاء المنشأين في نطاق تاريخ
        /// </summary>
        Task<IEnumerable<CustomerResponseDto>> GetCustomersByDateRangeAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get customer statistics
        /// الحصول على إحصائيات العملاء
        /// </summary>
        Task<object> GetCustomerStatisticsAsync();

        /// <summary>
        /// Check if customer exists
        /// التحقق من وجود العميل
        /// </summary>
        Task<bool> CustomerExistsAsync(int customerId);

        /// <summary>
        /// Check if customer number exists
        /// التحقق من وجود رقم العميل
        /// </summary>
        Task<bool> CustomerNoExistsAsync(string customerNo, int? excludeCustomerId = null);
    }
}
