using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using zaaerIntegration.Data;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service for managing Master Users
    /// </summary>
    public class MasterUserService : IMasterUserService
    {
        private readonly MasterDbContext _masterDbContext;
        private readonly ILogger<MasterUserService> _logger;

        /// <summary>
        /// Constructor for MasterUserService
        /// </summary>
        /// <param name="masterDbContext">Master database context</param>
        /// <param name="logger">Logger instance</param>
        public MasterUserService(
            MasterDbContext masterDbContext,
            ILogger<MasterUserService> logger)
        {
            _masterDbContext = masterDbContext ?? throw new ArgumentNullException(nameof(masterDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على المستخدم بواسطة Username
        /// ✅ مهم: يجلب المستخدم من Master DB فقط (ليس Tenant DB)
        /// </summary>
        public async Task<MasterUser?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            // ✅ جلب المستخدم من Master DB فقط
            return await _masterDbContext.MasterUsers
                .AsNoTracking()
                .Include(u => u.Tenant)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        /// <summary>
        /// الحصول على المستخدم بواسطة Id
        /// </summary>
        public async Task<MasterUser?> GetByIdAsync(int userId)
        {
            return await _masterDbContext.MasterUsers
                .AsNoTracking()
                .Include(u => u.Tenant)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        /// <summary>
        /// الحصول على أدوار المستخدم
        /// </summary>
        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            var userRoles = await _masterDbContext.UserRoles
                .AsNoTracking()
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role!.Code)
                .ToListAsync();

            return userRoles;
        }

        /// <summary>
        /// التحقق من كلمة المرور (Plain Text - بدون تشفير)
        /// </summary>
        public bool ValidatePassword(string password, string passwordHash)
        {
            // ✅ التحقق من البيانات المدخلة
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            {
                _logger.LogWarning("Password validation failed: Empty password or password hash");
                return false;
            }

            // ✅ مقارنة مباشرة (Plain Text)
            var isValid = string.Equals(password, passwordHash, StringComparison.Ordinal);
            
            if (!isValid)
            {
                _logger.LogWarning("❌ Password verification failed. Password: '{Password}' does not match stored password", password);
            }
            else
            {
                _logger.LogInformation("✅ Password verification successful (plain text)");
            }
            
            return isValid;
        }

        /// <summary>
        /// حفظ كلمة المرور (Plain Text - بدون تشفير)
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            // ✅ إرجاع كلمة المرور كما هي (Plain Text)
            return password;
        }

        /// <summary>
        /// إنشاء مستخدم جديد
        /// </summary>
        public async Task<MasterUser> CreateUserAsync(string username, string password, int tenantId, IEnumerable<int> roleIds)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            // التحقق من وجود Tenant
            var tenant = await _masterDbContext.Tenants.FindAsync(tenantId);
            if (tenant == null)
                throw new KeyNotFoundException($"Tenant with id {tenantId} not found");

            // التحقق من عدم وجود مستخدم بنفس Username
            var existingUser = await GetByUsernameAsync(username);
            if (existingUser != null)
                throw new InvalidOperationException($"User with username '{username}' already exists");

            // إنشاء المستخدم
            var user = new MasterUser
            {
                Username = username,
                PasswordHash = HashPassword(password),
                TenantId = tenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _masterDbContext.MasterUsers.Add(user);
            await _masterDbContext.SaveChangesAsync();

            // إضافة الأدوار
            if (roleIds != null && roleIds.Any())
            {
                foreach (var roleId in roleIds)
                {
                    var role = await _masterDbContext.Roles.FindAsync(roleId);
                    if (role != null)
                    {
                        var userRole = new UserRole
                        {
                            UserId = user.Id,
                            RoleId = roleId
                        };
                        _masterDbContext.UserRoles.Add(userRole);
                    }
                }
                await _masterDbContext.SaveChangesAsync();
            }

            _logger.LogInformation("✅ User created successfully: Username={Username}, TenantId={TenantId}", username, tenantId);

            return user;
        }

        /// <summary>
        /// التحقق من صحة بيانات تسجيل الدخول
        /// </summary>
        public async Task<MasterUser?> ValidateLoginAsync(string username, string password)
        {
            // ✅ 1. التحقق من البيانات المدخلة أولاً
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Login attempt with empty username or password");
                return null;
            }

            // ✅ 2. جلب المستخدم من Master DB فقط (ليس Tenant DB)
            var user = await GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("❌ Login failed: User not found in Master DB. Username: {Username}", username);
                return null;
            }

            _logger.LogDebug("✅ User found in Master DB. Username: {Username}, Id: {UserId}, TenantId: {TenantId}, IsActive: {IsActive}", 
                username, user.Id, user.TenantId, user.IsActive);

            // ✅ 3. التحقق من TenantId موجود وصحيح
            if (user.TenantId <= 0)
            {
                _logger.LogWarning("❌ Login failed: User has invalid TenantId. Username: {Username}, TenantId: {TenantId}", 
                    username, user.TenantId);
                return null;
            }

            // ✅ 4. التحقق من PasswordHash موجود
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                _logger.LogWarning("❌ Login failed: User has no password hash. Username: {Username}", username);
                return null;
            }

            _logger.LogDebug("🔍 Password hash found. Length: {HashLength}, Prefix: {HashPrefix}", 
                user.PasswordHash.Length,
                user.PasswordHash.Length > 30 ? user.PasswordHash.Substring(0, 30) + "..." : user.PasswordHash);

            // ✅ 5. التحقق من حالة المستخدم (IsActive) قبل التحقق من الباسورد
            if (!user.IsActive)
            {
                _logger.LogWarning("❌ Login failed: User is inactive. Username: {Username}", username);
                return null;
            }

            // ✅ 6. التحقق من كلمة المرور (يجب أن يكون آخر فحص)
            if (!ValidatePassword(password, user.PasswordHash))
            {
                _logger.LogWarning("❌ Login failed: Invalid password. Username: {Username}", username);
                return null;
            }

            _logger.LogInformation("✅ Login successful: Username={Username}, TenantId={TenantId}", username, user.TenantId);
            return user;
        }
    }
}

