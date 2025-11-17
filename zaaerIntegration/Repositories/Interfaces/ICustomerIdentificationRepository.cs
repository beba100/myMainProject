using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for CustomerIdentification operations
    /// </summary>
    public interface ICustomerIdentificationRepository : IGenericRepository<CustomerIdentification>
    {
        /// <summary>
        /// Get customer identifications by customer ID
        /// </summary>
        Task<IEnumerable<CustomerIdentification>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get customer identifications by ID type
        /// </summary>
        Task<IEnumerable<CustomerIdentification>> GetByIdTypeAsync(int idTypeId);

        /// <summary>
        /// Get primary identification for a customer
        /// </summary>
        Task<CustomerIdentification?> GetPrimaryIdentificationAsync(int customerId);

        /// <summary>
        /// Get active identifications for a customer
        /// </summary>
        Task<IEnumerable<CustomerIdentification>> GetActiveIdentificationsAsync(int customerId);

        /// <summary>
        /// Check if identification number already exists
        /// </summary>
        Task<bool> IdentificationNumberExistsAsync(string idNumber, int? excludeId = null);

        /// <summary>
        /// Get identifications by ID number
        /// </summary>
        Task<CustomerIdentification?> GetByIdNumberAsync(string idNumber);
    }
}
