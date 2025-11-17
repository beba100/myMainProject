using FinanceLedgerAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using zaaerIntegration.Data;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// خدمة للحصول على معلومات الفندق (Tenant) الحالي
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MasterDbContext _masterDbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TenantService> _logger;
        private Tenant? _currentTenant;

        /// <summary>
        /// Constructor for TenantService
        /// </summary>
        /// <param name="httpContextAccessor">HTTP context accessor</param>
        /// <param name="masterDbContext">Master database context</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="logger">Logger</param>
        public TenantService(
            IHttpContextAccessor httpContextAccessor, 
            MasterDbContext masterDbContext,
            IConfiguration configuration,
            ILogger<TenantService> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _masterDbContext = masterDbContext ?? throw new ArgumentNullException(nameof(masterDbContext));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على الفندق الحالي بناءً على X-Hotel-Code Header
        /// </summary>
        public Tenant? GetTenant()
        {
            // استخدام الـ Tenant المخزن مؤقتاً إذا كان موجوداً (Caching)
            if (_currentTenant != null)
                return _currentTenant;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext is null - cannot resolve tenant");
                throw new InvalidOperationException("HttpContext is not available. Cannot resolve tenant.");
            }

            // قراءة قيمة X-Hotel-Code من Header
            if (!httpContext.Request.Headers.TryGetValue("X-Hotel-Code", out var hotelCodeValues) || 
                string.IsNullOrWhiteSpace(hotelCodeValues))
            {
                _logger.LogWarning("Missing or empty X-Hotel-Code header");
                throw new UnauthorizedAccessException("Missing X-Hotel-Code header. Please provide a valid hotel code.");
            }

            // تحويل StringValues إلى string وtrim whitespace
            string hotelCode = hotelCodeValues.ToString().Trim();

            if (string.IsNullOrWhiteSpace(hotelCode))
            {
                _logger.LogWarning("X-Hotel-Code header is empty or whitespace only");
                throw new UnauthorizedAccessException("X-Hotel-Code header cannot be empty. Please provide a valid hotel code.");
            }

            try
            {
                // البحث عن الفندق في قاعدة البيانات المركزية (Case-insensitive)
                _currentTenant = _masterDbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefault(t => t.Code.ToLower() == hotelCode.ToLower());

                if (_currentTenant == null)
                {
                    _logger.LogError("Tenant not found for code: {HotelCode} in Master DB (db29328)", hotelCode);
                    throw new KeyNotFoundException($"Tenant not found for code: {hotelCode}. Please verify the hotel code exists in Master Database.");
                }

                // التحقق من وجود DatabaseName
                if (string.IsNullOrWhiteSpace(_currentTenant.DatabaseName))
                {
                    _logger.LogError("DatabaseName is not set for tenant: {TenantCode} (Id: {TenantId})", 
                        _currentTenant.Code, _currentTenant.Id);
                    throw new InvalidOperationException(
                        $"DatabaseName is not configured for tenant: {_currentTenant.Code}. " +
                        "Please add DatabaseName to the tenant record in Master Database.");
                }

                _logger.LogInformation("✅ Tenant resolved successfully: {TenantName} ({TenantCode}), Database: {DatabaseName}", 
                    _currentTenant.Name, _currentTenant.Code, _currentTenant.DatabaseName);

                return _currentTenant;
            }
            catch (KeyNotFoundException)
            {
                // Re-throw KeyNotFoundException as-is
                throw;
            }
            catch (InvalidOperationException)
            {
                // Re-throw InvalidOperationException as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Database error while resolving tenant for code: {HotelCode}. Error: {ErrorMessage}", 
                    hotelCode, ex.Message);
                
                // Check if it's a database connection error
                if (ex is Microsoft.Data.SqlClient.SqlException || 
                    ex is Microsoft.EntityFrameworkCore.DbUpdateException ||
                    ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Failed to connect to Master Database (db29328) while resolving tenant. " +
                        $"Please check Master Database connection. Error: {ex.Message}", ex);
                }
                
                throw new InvalidOperationException(
                    $"An error occurred while resolving tenant for code: {hotelCode}. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// الحصول على كود الفندق من HTTP Request
        /// </summary>
        public string? GetTenantCode()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return null;

                if (httpContext.Request.Headers.TryGetValue("X-Hotel-Code", out var hotelCodeValues))
                {
                    var code = hotelCodeValues.ToString().Trim();
                    return string.IsNullOrWhiteSpace(code) ? null : code;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting tenant code from header: {ErrorMessage}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// الحصول على Connection String للفندق الحالي بناءً على DatabaseName
        /// </summary>
        /// <returns>Connection String للفندق الحالي</returns>
        public string GetTenantConnectionString()
        {
            try
            {
                var tenant = GetTenant();
                
                if (tenant == null)
                {
                    _logger.LogError("Cannot get connection string - Tenant is null");
                    throw new InvalidOperationException("Tenant not resolved. Cannot get connection string.");
                }

                return BuildConnectionStringForTenant(tenant);
            }
            catch (KeyNotFoundException)
            {
                // Re-throw KeyNotFoundException as-is
                throw;
            }
            catch (InvalidOperationException)
            {
                // Re-throw InvalidOperationException as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting connection string for tenant: {TenantCode}. Error: {ErrorMessage}", 
                    _currentTenant?.Code ?? "Unknown", ex.Message);
                throw new InvalidOperationException(
                    $"Failed to get connection string for tenant. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// بناء Connection String لـ Tenant محدد
        /// </summary>
        /// <param name="tenant">الـ Tenant المطلوب</param>
        /// <returns>Connection String</returns>
        public string BuildConnectionStringForTenant(Tenant tenant)
        {
            if (tenant == null)
            {
                _logger.LogError("BuildConnectionStringForTenant: Tenant is null");
                throw new ArgumentNullException(nameof(tenant));
            }

            // التحقق من وجود DatabaseName
            if (string.IsNullOrWhiteSpace(tenant.DatabaseName))
            {
                _logger.LogError("DatabaseName is null or empty for tenant: {TenantCode} (Id: {TenantId})", 
                    tenant.Code, tenant.Id);
                throw new InvalidOperationException(
                    $"DatabaseName is not set for tenant: {tenant.Code}. " +
                    "Please add DatabaseName to the tenant record in Master Database.");
            }

            // قراءة إعدادات قاعدة البيانات من appsettings.json
            var server = _configuration["TenantDatabase:Server"]?.Trim();
            var userId = _configuration["TenantDatabase:UserId"]?.Trim();
            var password = _configuration["TenantDatabase:Password"]?.Trim();

            // التحقق من وجود جميع الإعدادات المطلوبة
            if (string.IsNullOrWhiteSpace(server))
            {
                _logger.LogError("TenantDatabase:Server is missing in appsettings.json");
                throw new InvalidOperationException(
                    "TenantDatabase:Server is not configured in appsettings.json. " +
                    "Please add TenantDatabase:Server setting.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("TenantDatabase:UserId is missing in appsettings.json");
                throw new InvalidOperationException(
                    "TenantDatabase:UserId is not configured in appsettings.json. " +
                    "Please add TenantDatabase:UserId setting.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogError("TenantDatabase:Password is missing in appsettings.json");
                throw new InvalidOperationException(
                    "TenantDatabase:Password is not configured in appsettings.json. " +
                    "Please add TenantDatabase:Password setting.");
            }

            // بناء Connection String ديناميكياً
            var connectionString = $"Server={server}; Database={tenant.DatabaseName}; User Id={userId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

            _logger.LogDebug("Built connection string for tenant: {TenantCode}, Database: {DatabaseName}, Server: {Server}", 
                tenant.Code, tenant.DatabaseName, server);

            return connectionString;
        }

        /// <summary>
        /// التحقق من الاتصال بقاعدة بيانات الفندق الحالي
        /// </summary>
        /// <returns>true إذا كان الاتصال ناجحاً</returns>
        public async Task<bool> ValidateTenantConnectionAsync()
        {
            Tenant? tenant = null;
            try
            {
                tenant = GetTenant();
                
                if (tenant == null)
                {
                    _logger.LogError("Cannot validate connection - Tenant is null");
                    return false;
                }

                var connectionString = GetTenantConnectionString();
                
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                // التحقق من اسم قاعدة البيانات الصحيحة
                var command = new SqlCommand("SELECT DB_NAME() AS CurrentDatabase", connection);
                var result = await command.ExecuteScalarAsync();
                var currentDatabase = result?.ToString();
                
                if (currentDatabase != tenant.DatabaseName)
                {
                    _logger.LogError("❌ Database mismatch! Expected: {Expected}, Actual: {Actual} for tenant: {TenantCode}", 
                        tenant.DatabaseName, currentDatabase, tenant.Code);
                    return false;
                }
                
                _logger.LogInformation("✅ Connection validated successfully for tenant: {TenantCode}, Database: {DatabaseName}", 
                    tenant.Code, tenant.DatabaseName);
                
                return true;
            }
            catch (Exception ex)
            {
                var tenantCode = tenant?.Code ?? _currentTenant?.Code ?? "Unknown";
                _logger.LogError(ex, "❌ Failed to validate connection for tenant: {TenantCode}. Error: {ErrorMessage}", 
                    tenantCode, ex.Message);
                return false;
            }
        }
    }
}
