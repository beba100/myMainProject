using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for CustomerIdentification operations
    /// </summary>
    public class CustomerIdentificationRepository : GenericRepository<CustomerIdentification>, ICustomerIdentificationRepository
    {
        public CustomerIdentificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CustomerIdentification>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.CustomerIdentifications
                .Where(ci => ci.CustomerId == customerId)
                .OrderBy(ci => ci.IsPrimary ? 0 : 1)
                .ThenBy(ci => ci.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerIdentification>> GetByIdTypeAsync(int idTypeId)
        {
            return await _context.CustomerIdentifications
                .Where(ci => ci.IdTypeId == idTypeId)
                .OrderBy(ci => ci.CustomerId)
                .ThenBy(ci => ci.IsPrimary ? 0 : 1)
                .ToListAsync();
        }

        public async Task<CustomerIdentification?> GetPrimaryIdentificationAsync(int customerId)
        {
            return await _context.CustomerIdentifications
                .FirstOrDefaultAsync(ci => ci.CustomerId == customerId && ci.IsPrimary && ci.IsActive);
        }

        public async Task<IEnumerable<CustomerIdentification>> GetActiveIdentificationsAsync(int customerId)
        {
            return await _context.CustomerIdentifications
                .Where(ci => ci.CustomerId == customerId && ci.IsActive)
                .OrderBy(ci => ci.IsPrimary ? 0 : 1)
                .ThenBy(ci => ci.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IdentificationNumberExistsAsync(string idNumber, int? excludeId = null)
        {
            var query = _context.CustomerIdentifications
                .Where(ci => ci.IdNumber == idNumber);

            if (excludeId.HasValue)
            {
                query = query.Where(ci => ci.IdentificationId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<CustomerIdentification?> GetByIdNumberAsync(string idNumber)
        {
            return await _context.CustomerIdentifications
                .FirstOrDefaultAsync(ci => ci.IdNumber == idNumber);
        }
    }
}
