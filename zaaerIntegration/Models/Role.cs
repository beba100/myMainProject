namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// Role Model - الدور في النظام
    /// </summary>
    public class Role
    {
        /// <summary>
        /// معرف الدور
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// اسم الدور (Administrator, Manager, etc.)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// كود الدور (Admin, Manager, etc.)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Navigation Property - المستخدمون الذين لديهم هذا الدور
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
