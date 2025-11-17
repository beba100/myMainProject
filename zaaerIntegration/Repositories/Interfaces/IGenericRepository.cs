using System.Linq.Expressions;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Generic Repository Interface
    /// واجهة المستودع العام
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Get entity by ID
        /// الحصول على الكيان بالمعرف
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Get all entities
        /// الحصول على جميع الكيانات
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Get entities with pagination
        /// الحصول على الكيانات مع التصفح
        /// </summary>
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null);

        /// <summary>
        /// Find entities by condition
        /// البحث عن الكيانات بالشرط
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Find single entity by condition
        /// البحث عن كيان واحد بالشرط
        /// </summary>
        Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Add entity
        /// إضافة كيان
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Add multiple entities
        /// إضافة عدة كيانات
        /// </summary>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Update entity
        /// تحديث كيان
        /// </summary>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Update entity (synchronous)
        /// تحديث كيان (متزامن)
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Delete entity
        /// حذف كيان
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Delete entity (synchronous)
        /// حذف كيان (متزامن)
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// Delete entity by ID
        /// حذف كيان بالمعرف
        /// </summary>
        Task DeleteByIdAsync(int id);

        /// <summary>
        /// Check if entity exists
        /// التحقق من وجود الكيان
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Count entities
        /// عدد الكيانات
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
