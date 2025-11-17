using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Customer Repository Interface
    /// واجهة مستودع العملاء
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Get customer by customer number
        /// الحصول على العميل برقم العميل
        /// </summary>
        Task<Customer?> GetByCustomerNoAsync(string customerNo);

        /// <summary>
        /// Get customers by hotel ID
        /// الحصول على العملاء بمعرف الفندق
        /// </summary>
        Task<IEnumerable<Customer>> GetByHotelIdAsync(int hotelId);

        /// <summary>
        /// Search customers by name
        /// البحث عن العملاء بالاسم
        /// </summary>
        Task<IEnumerable<Customer>> SearchByNameAsync(string name);

        /// <summary>
        /// Get customers with their related data
        /// الحصول على العملاء مع البيانات المرتبطة
        /// </summary>
        Task<IEnumerable<Customer>> GetWithRelatedDataAsync();

        /// <summary>
        /// Get customer with related data by ID
        /// الحصول على العميل مع البيانات المرتبطة بالمعرف
        /// </summary>
        Task<Customer?> GetWithRelatedDataByIdAsync(int customerId);

        /// <summary>
        /// Get customers by nationality
        /// الحصول على العملاء بالجنسية
        /// </summary>
        Task<IEnumerable<Customer>> GetByNationalityAsync(int nationalityId);

        /// <summary>
        /// Get customers by guest type
        /// الحصول على العملاء بنوع الضيف
        /// </summary>
        Task<IEnumerable<Customer>> GetByGuestTypeAsync(int guestTypeId);

        /// <summary>
        /// Get customers by guest category
        /// الحصول على العملاء بفئة الضيف
        /// </summary>
        Task<IEnumerable<Customer>> GetByGuestCategoryAsync(int guestCategoryId);

        /// <summary>
        /// Get customers created in date range
        /// الحصول على العملاء المنشأين في نطاق تاريخ
        /// </summary>
        Task<IEnumerable<Customer>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get customer statistics
        /// الحصول على إحصائيات العملاء
        /// </summary>
        Task<object> GetCustomerStatisticsAsync();
    }
}
