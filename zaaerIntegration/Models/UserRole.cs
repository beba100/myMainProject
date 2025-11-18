namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// UserRole Model - ربط المستخدم بالأدوار
    /// </summary>
    public class UserRole
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
        /// معرف الدور
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Navigation Property - المستخدم
        /// </summary>
        public MasterUser? User { get; set; }

        /// <summary>
        /// Navigation Property - الدور
        /// </summary>
        public Role? Role { get; set; }
    }
}

