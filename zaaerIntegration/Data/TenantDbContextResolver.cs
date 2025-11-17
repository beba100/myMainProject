using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Data
{
    /// <summary>
    /// يقوم بإنشاء DbContext ديناميكي بناءً على الفندق (Tenant) الحالي
    /// </summary>
    public class TenantDbContextResolver
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantDbContextResolver> _logger;

        /// <summary>
        /// Constructor for TenantDbContextResolver
        /// </summary>
        /// <param name="tenantService">Tenant service</param>
        /// <param name="logger">Logger</param>
        public TenantDbContextResolver(
            ITenantService tenantService,
            ILogger<TenantDbContextResolver> logger)
        {
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// إنشاء ApplicationDbContext للفندق الحالي
        /// </summary>
        /// <returns>DbContext متصل بقاعدة بيانات الفندق</returns>
        public ApplicationDbContext GetCurrentDbContext()
        {
            try
            {
                var tenant = _tenantService.GetTenant();
                
                if (tenant == null)
                {
                    _logger.LogError("Cannot create DbContext - Tenant is null");
                    throw new InvalidOperationException("Tenant not resolved. Cannot create database context.");
                }

                _logger.LogDebug("Creating DbContext for tenant: {TenantCode}, Database: {DatabaseName}", 
                    tenant.Code, tenant.DatabaseName);

                string connectionString;
                try
                {
                    connectionString = _tenantService.GetTenantConnectionString();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get connection string for tenant: {TenantCode}. Error: {ErrorMessage}", 
                        tenant.Code, ex.Message);
                    throw new InvalidOperationException(
                        $"Failed to get connection string for tenant: {tenant.Code}. Error: {ex.Message}", ex);
                }

                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                // تمكين Logging في حالة Development
                #if DEBUG
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
                #endif

                var dbContext = new ApplicationDbContext(optionsBuilder.Options);
                
                _logger.LogDebug("DbContext created successfully for tenant: {TenantCode}, Database: {DatabaseName}", 
                    tenant.Code, tenant.DatabaseName);

                return dbContext;
            }
            catch (InvalidOperationException)
            {
                // Re-throw InvalidOperationException as-is
                throw;
            }
            catch (KeyNotFoundException)
            {
                // Re-throw KeyNotFoundException as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating DbContext. Error: {ErrorMessage}", ex.Message);
                throw new InvalidOperationException(
                    $"Failed to create database context. Error: {ex.Message}", ex);
            }
        }
    }
}

