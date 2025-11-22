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
        /// إنشاء مستخدم جديد مع الحقول الإضافية
        /// </summary>
        public async Task<MasterUser> CreateUserAsync(string username, string password, int tenantId, IEnumerable<int> roleIds, 
            string? phoneNumber, string? email, string? employeeNumber, string? fullName, 
            IEnumerable<int>? additionalTenantIds = null)
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
                PhoneNumber = phoneNumber,
                Email = email,
                EmployeeNumber = employeeNumber,
                FullName = fullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _masterDbContext.MasterUsers.Add(user);
            await _masterDbContext.SaveChangesAsync();

            // إضافة الأدوار
            var userRoles = new List<Role>();
            if (roleIds != null && roleIds.Any())
            {
                foreach (var roleId in roleIds)
                {
                    var role = await _masterDbContext.Roles.FindAsync(roleId);
                    if (role != null)
                    {
                        userRoles.Add(role);
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

            // إضافة UserTenants بناءً على الأدوار
            // القواعد:
            // - Supervisor: إذا تم تحديد فنادق إضافية، نضيف فقط الفنادق المحددة + الفندق الأساسي. إذا لم يتم تحديد، نضيف جميع الفنادق
            // - Manager, Accountant, Admin: إذا تم تحديد فنادق إضافية، نضيف الفنادق المحددة + الفندق الأساسي. إذا لم يتم تحديد، نضيف الفندق الأساسي فقط
            // - Staff: الفندق الأساسي فقط
            var hasSupervisorRole = userRoles.Any(r => r.Code.Equals("Supervisor", StringComparison.OrdinalIgnoreCase));
            var hasManagerRole = userRoles.Any(r => r.Code.Equals("Manager", StringComparison.OrdinalIgnoreCase));
            var hasAccountantRole = userRoles.Any(r => r.Code.Equals("Accountant", StringComparison.OrdinalIgnoreCase));
            var hasAdminRole = userRoles.Any(r => r.Code.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            var hasStaffRole = userRoles.Any(r => r.Code.Equals("Staff", StringComparison.OrdinalIgnoreCase));

            // جمع جميع الفنادق المطلوبة في HashSet لتجنب التكرار
            var tenantsToAdd = new HashSet<int>();
            
            // دائماً إضافة الفندق الأساسي
            tenantsToAdd.Add(tenantId);

            if (hasSupervisorRole)
            {
                // Supervisor: إذا تم تحديد فنادق إضافية، نضيف فقط الفنادق المحددة + الفندق الأساسي
                // إذا لم يتم تحديد، نضيف جميع الفنادق
                if (additionalTenantIds != null && additionalTenantIds.Any())
                {
                    // إضافة الفنادق المحددة يدوياً
                    foreach (var additionalTenantId in additionalTenantIds)
                    {
                        if (additionalTenantId != tenantId)
                        {
                            tenantsToAdd.Add(additionalTenantId);
                        }
                    }
                    
                    _logger.LogInformation("✅ Added selected tenants for Supervisor user: UserId={UserId}, TenantCount={Count}", 
                        user.Id, tenantsToAdd.Count);
                }
                else
                {
                    // إضافة جميع الفنادق
                    var allTenants = await _masterDbContext.Tenants
                        .Select(t => t.Id)
                        .ToListAsync();
                    
                    foreach (var tenantIdToAdd in allTenants)
                    {
                        tenantsToAdd.Add(tenantIdToAdd);
                    }
                    
                    _logger.LogInformation("✅ Added all tenants for Supervisor user: UserId={UserId}, TenantCount={Count}", 
                        user.Id, tenantsToAdd.Count);
                }
            }
            else if (hasManagerRole || hasAccountantRole || hasAdminRole)
            {
                // Manager, Accountant, Admin: إضافة الفنادق المحددة يدوياً + الفندق الأساسي
                if (additionalTenantIds != null && additionalTenantIds.Any())
                {
                    foreach (var additionalTenantId in additionalTenantIds)
                    {
                        if (additionalTenantId != tenantId)
                        {
                            tenantsToAdd.Add(additionalTenantId);
                        }
                    }
                    
                    var roleName = hasManagerRole ? "Manager" : hasAccountantRole ? "Accountant" : "Admin";
                    _logger.LogInformation("✅ Added selected tenants for {Role} user: UserId={UserId}, TenantCount={Count}", 
                        roleName, user.Id, tenantsToAdd.Count);
                }
                else
                {
                    // فقط الفندق الأساسي (تم إضافته بالفعل)
                    var roleName = hasManagerRole ? "Manager" : hasAccountantRole ? "Accountant" : "Admin";
                    _logger.LogInformation("✅ Added primary tenant only for {Role} user: UserId={UserId}, TenantId={TenantId}", 
                        roleName, user.Id, tenantId);
                }
            }
            else if (hasStaffRole)
            {
                // Staff: الفندق الأساسي فقط (تم إضافته بالفعل)
                _logger.LogInformation("✅ Added primary tenant only for Staff user: UserId={UserId}, TenantId={TenantId}", 
                    user.Id, tenantId);
            }

            // إضافة جميع الفنادق المجمعة إلى UserTenants
            foreach (var tenantIdToAdd in tenantsToAdd)
            {
                var existingUserTenant = await _masterDbContext.UserTenants
                    .FirstOrDefaultAsync(ut => ut.UserId == user.Id && ut.TenantId == tenantIdToAdd);
                
                if (existingUserTenant == null)
                {
                    var userTenant = new UserTenant
                    {
                        UserId = user.Id,
                        TenantId = tenantIdToAdd,
                        CreatedAt = DateTime.UtcNow
                    };
                    _masterDbContext.UserTenants.Add(userTenant);
                }
            }

            // حفظ جميع التغييرات في UserTenants
            if (_masterDbContext.ChangeTracker.HasChanges())
            {
                await _masterDbContext.SaveChangesAsync();
            }

            _logger.LogInformation("✅ User created successfully with additional fields: Username={Username}, TenantId={TenantId}, Email={Email}", 
                username, tenantId, email);

            return user;
        }

        /// <summary>
        /// الحصول على جميع المستخدمين
        /// </summary>
        public async Task<IEnumerable<MasterUser>> GetAllUsersAsync()
        {
            return await _masterDbContext.MasterUsers
                .AsNoTracking()
                .Include(u => u.Tenant)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        /// <summary>
        /// تحديث مستخدم
        /// </summary>
        public async Task<MasterUser> UpdateUserAsync(int userId, string? username, string? password, int? tenantId, 
            string? phoneNumber, string? email, string? employeeNumber, string? fullName, 
            bool? isActive, IEnumerable<int>? roleIds, IEnumerable<int>? additionalTenantIds)
        {
            var user = await _masterDbContext.MasterUsers
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException($"User with id {userId} not found");

            // تحديث الحقول
            if (!string.IsNullOrWhiteSpace(username) && username != user.Username)
            {
                // التحقق من عدم وجود مستخدم آخر بنفس Username
                var existingUser = await GetByUsernameAsync(username);
                if (existingUser != null && existingUser.Id != userId)
                    throw new InvalidOperationException($"User with username '{username}' already exists");
                
                user.Username = username;
            }

            if (!string.IsNullOrWhiteSpace(password))
                user.PasswordHash = HashPassword(password);

            if (tenantId.HasValue)
            {
                var tenant = await _masterDbContext.Tenants.FindAsync(tenantId.Value);
                if (tenant == null)
                    throw new KeyNotFoundException($"Tenant with id {tenantId.Value} not found");
                user.TenantId = tenantId.Value;
            }

            if (phoneNumber != null)
                user.PhoneNumber = phoneNumber;

            if (email != null)
                user.Email = email;

            if (employeeNumber != null)
                user.EmployeeNumber = employeeNumber;

            if (fullName != null)
                user.FullName = fullName;

            if (isActive.HasValue)
                user.IsActive = isActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            // تحديث الأدوار
            var updatedRoles = new List<Role>();
            if (roleIds != null)
            {
                // حذف الأدوار الحالية
                var existingRoles = _masterDbContext.UserRoles.Where(ur => ur.UserId == userId);
                _masterDbContext.UserRoles.RemoveRange(existingRoles);

                // إضافة الأدوار الجديدة
                foreach (var roleId in roleIds)
                {
                    var role = await _masterDbContext.Roles.FindAsync(roleId);
                    if (role != null)
                    {
                        updatedRoles.Add(role);
                        var userRole = new UserRole
                        {
                            UserId = user.Id,
                            RoleId = roleId
                        };
                        _masterDbContext.UserRoles.Add(userRole);
                    }
                }
            }

            // تحديث UserTenants بناءً على الأدوار الجديدة
            // القواعد:
            // - Supervisor: جميع الفنادق ما عدا الفندق الأساسي
            // - Manager, Accountant, Admin: الفندق الأساسي فقط
            // إذا تم تحديث الأدوار، نحتاج إلى إعادة بناء UserTenants
            if (roleIds != null)
            {
                // حذف جميع UserTenants الحالية (سنعيد بناؤها بناءً على الأدوار)
                var existingUserTenants = _masterDbContext.UserTenants
                    .Where(ut => ut.UserId == userId);
                _masterDbContext.UserTenants.RemoveRange(existingUserTenants);

                var hasSupervisorRole = updatedRoles.Any(r => r.Code.Equals("Supervisor", StringComparison.OrdinalIgnoreCase));
                var hasManagerRole = updatedRoles.Any(r => r.Code.Equals("Manager", StringComparison.OrdinalIgnoreCase));
                var hasAccountantRole = updatedRoles.Any(r => r.Code.Equals("Accountant", StringComparison.OrdinalIgnoreCase));
                var hasAdminRole = updatedRoles.Any(r => r.Code.Equals("Admin", StringComparison.OrdinalIgnoreCase));
                var hasStaffRole = updatedRoles.Any(r => r.Code.Equals("Staff", StringComparison.OrdinalIgnoreCase));

                var currentTenantId = tenantId ?? user.TenantId;

                // جمع جميع الفنادق المطلوبة في HashSet لتجنب التكرار
                var tenantsToAdd = new HashSet<int>();
                
                // دائماً إضافة الفندق الأساسي
                tenantsToAdd.Add(currentTenantId);

                if (hasSupervisorRole)
                {
                    // Supervisor: إذا تم تحديد فنادق إضافية، نضيف فقط الفنادق المحددة + الفندق الأساسي
                    // إذا لم يتم تحديد، نضيف جميع الفنادق
                    if (additionalTenantIds != null && additionalTenantIds.Any())
                    {
                        // إضافة الفنادق المحددة يدوياً
                        foreach (var additionalTenantId in additionalTenantIds)
                        {
                            if (additionalTenantId != currentTenantId)
                            {
                                tenantsToAdd.Add(additionalTenantId);
                            }
                        }
                        
                        _logger.LogInformation("✅ Updated UserTenants for Supervisor user (selected tenants): UserId={UserId}, TenantCount={Count}", 
                            user.Id, tenantsToAdd.Count);
                    }
                    else
                    {
                        // إضافة جميع الفنادق
                        var allTenants = await _masterDbContext.Tenants
                            .Select(t => t.Id)
                            .ToListAsync();
                        
                        foreach (var tenantIdToAdd in allTenants)
                        {
                            tenantsToAdd.Add(tenantIdToAdd);
                        }
                        
                        _logger.LogInformation("✅ Updated UserTenants for Supervisor user (all tenants): UserId={UserId}, TenantCount={Count}", 
                            user.Id, tenantsToAdd.Count);
                    }
                }
                else if (hasManagerRole || hasAccountantRole || hasAdminRole)
                {
                    // Manager, Accountant, Admin: إضافة الفنادق المحددة يدوياً + الفندق الأساسي
                    if (additionalTenantIds != null && additionalTenantIds.Any())
                    {
                        foreach (var additionalTenantId in additionalTenantIds)
                        {
                            if (additionalTenantId != currentTenantId)
                            {
                                tenantsToAdd.Add(additionalTenantId);
                            }
                        }
                        
                        var roleName = hasManagerRole ? "Manager" : hasAccountantRole ? "Accountant" : "Admin";
                        _logger.LogInformation("✅ Updated UserTenants for {Role} user (selected tenants): UserId={UserId}, TenantCount={Count}", 
                            roleName, user.Id, tenantsToAdd.Count);
                    }
                    else
                    {
                        // فقط الفندق الأساسي (تم إضافته بالفعل)
                        var roleName = hasManagerRole ? "Manager" : hasAccountantRole ? "Accountant" : "Admin";
                        _logger.LogInformation("✅ Updated UserTenants for {Role} user (primary only): UserId={UserId}, TenantId={TenantId}", 
                            roleName, user.Id, currentTenantId);
                    }
                }
                else if (hasStaffRole)
                {
                    // Staff: الفندق الأساسي فقط (تم إضافته بالفعل)
                    _logger.LogInformation("✅ Updated UserTenants for Staff user (primary only): UserId={UserId}, TenantId={TenantId}", 
                        user.Id, currentTenantId);
                }

                // إضافة جميع الفنادق المجمعة إلى UserTenants
                foreach (var tenantIdToAdd in tenantsToAdd)
                {
                    var userTenant = new UserTenant
                    {
                        UserId = user.Id,
                        TenantId = tenantIdToAdd,
                        CreatedAt = DateTime.UtcNow
                    };
                    _masterDbContext.UserTenants.Add(userTenant);
                }
            }
            else
            {
                // إذا لم يتم تحديث الأدوار، فقط تحديث الفنادق الإضافية المحددة يدوياً
                if (additionalTenantIds != null)
                {
                    // حذف الفنادق الإضافية الحالية (وليس الفندق الأساسي)
                    var existingTenants = _masterDbContext.UserTenants
                        .Where(ut => ut.UserId == userId);
                    _masterDbContext.UserTenants.RemoveRange(existingTenants);

                    // إضافة الفنادق الجديدة
                    if (additionalTenantIds.Any())
                    {
                        var currentTenantId = tenantId ?? user.TenantId;
                        foreach (var additionalTenantId in additionalTenantIds)
                        {
                            if (additionalTenantId != currentTenantId)
                            {
                                var additionalTenant = await _masterDbContext.Tenants.FindAsync(additionalTenantId);
                                if (additionalTenant != null)
                                {
                                    var userTenant = new UserTenant
                                    {
                                        UserId = user.Id,
                                        TenantId = additionalTenantId,
                                        CreatedAt = DateTime.UtcNow
                                    };
                                    _masterDbContext.UserTenants.Add(userTenant);
                                }
                            }
                        }
                    }
                }
            }

            await _masterDbContext.SaveChangesAsync();

            _logger.LogInformation("✅ User updated successfully: UserId={UserId}, Username={Username}", userId, user.Username);

            return user;
        }

        /// <summary>
        /// حذف مستخدم
        /// </summary>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _masterDbContext.MasterUsers.FindAsync(userId);
            if (user == null)
                return false;

            _masterDbContext.MasterUsers.Remove(user);
            await _masterDbContext.SaveChangesAsync();

            _logger.LogInformation("✅ User deleted successfully: UserId={UserId}, Username={Username}", userId, user.Username);

            return true;
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

