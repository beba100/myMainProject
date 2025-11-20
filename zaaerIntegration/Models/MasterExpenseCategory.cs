namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// Master Expense Category Model - فئة المصروفات في قاعدة البيانات المركزية
    /// </summary>
    public class MasterExpenseCategory
    {
        /// <summary>
        /// معرف الفئة
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// البند الرئيسي
        /// </summary>
        public string MainCategory { get; set; } = string.Empty;

        /// <summary>
        /// التفصيل
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// هل الفئة نشطة
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
        /// كود الفئة (مثل CAT_BUILDING, CAT_RECEPTION, CAT_CORRIDORS)
        /// Category Code (e.g., CAT_BUILDING, CAT_RECEPTION, CAT_CORRIDORS)
        /// </summary>
        public string? CategoryCode { get; set; }
    }
}

