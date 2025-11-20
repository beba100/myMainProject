namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// UserTenant Model - ربط المستخدم بالفنادق (للصلاحيات)
    /// يحدد أي فنادق يمكن للمستخدم الوصول إليها
    /// </summary>
    public class UserTenant
    {
        /// <summary>
        /// معرف السجل
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// معرف المستخدم
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// معرف الفندق (Tenant)
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// تاريخ الإنشاء
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation Property - المستخدم
        /// </summary>
        public MasterUser? User { get; set; }

        /// <summary>
        /// Navigation Property - الفندق
        /// </summary>
        public Tenant? Tenant { get; set; }
    }
}

