using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Expense;
using zaaerIntegration.Services.Expense;
using zaaerIntegration.Services.PartnerQueueing;
using zaaerIntegration.Services.Interfaces;
using zaaerIntegration.Models;
using System.Text.Json;
using System.Linq;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller لإدارة النفقات (Expenses)
    /// جميع Endpoints تستخدم X-Hotel-Code header للحصول على HotelId
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;
        private readonly TenantDbContextResolver _dbContextResolver;
        private readonly ITenantService _tenantService;
        private readonly ILogger<ExpenseController> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor for ExpenseController
        /// </summary>
        /// <param name="expenseService">Expense service</param>
        /// <param name="queueService">Partner queue service</param>
        /// <param name="queueSettings">Queue settings provider</param>
        /// <param name="dbContextResolver">Tenant database context resolver</param>
        /// <param name="tenantService">Tenant service</param>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration for reading app settings</param>
        public ExpenseController(
            IExpenseService expenseService,
            IPartnerQueueService queueService,
            IQueueSettingsProvider queueSettings,
            TenantDbContextResolver dbContextResolver,
            ITenantService tenantService,
            ILogger<ExpenseController> logger,
            IConfiguration configuration)
        {
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
            _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
            _queueSettings = queueSettings ?? throw new ArgumentNullException(nameof(queueSettings));
            _dbContextResolver = dbContextResolver ?? throw new ArgumentNullException(nameof(dbContextResolver));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// الحصول على جميع النفقات للفندق الحالي
        /// </summary>
        /// <returns>قائمة النفقات</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetAll()
        {
            try
            {
                _logger.LogInformation("📋 Fetching all expenses for current hotel");

                var expenses = await _expenseService.GetAllAsync();

                _logger.LogInformation("✅ Successfully retrieved {Count} expenses", expenses.Count());

                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching expenses: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch expenses", details = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على نفقة محددة بالمعرف
        /// </summary>
        /// <param name="id">معرف النفقة</param>
        /// <returns>معلومات النفقة</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExpenseResponseDto>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("🔍 Fetching expense with id: {ExpenseId}", id);

                // ✅ الحصول على X-Hotel-Code header إذا كان موجوداً (للمشرفين)
                string? hotelCode = null;
                if (HttpContext.Request.Headers.TryGetValue("X-Hotel-Code", out var hotelCodeValues) && 
                    !string.IsNullOrWhiteSpace(hotelCodeValues))
                {
                    hotelCode = hotelCodeValues.ToString().Trim();
                    _logger.LogInformation("✅ [GetById] X-Hotel-Code header found: {HotelCode}", hotelCode);
                }

                ExpenseResponseDto? expense = null;

                // ✅ Check if user is supervisor/manager/accountant/admin
                var userIdClaim = HttpContext.Items["UserId"]?.ToString();
                if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                    var rolesList = await masterDb.UserRoles
                        .AsNoTracking()
                        .Include(ur => ur.Role)
                        .Where(ur => ur.UserId == userId)
                        .Select(ur => ur.Role!.Code.ToLower())
                        .ToListAsync();

                    var isSupervisorOrManagerOrAdminOrAccountant = rolesList.Contains("supervisor") || 
                                                                   rolesList.Contains("manager") || 
                                                                   rolesList.Contains("admin") || 
                                                                   rolesList.Contains("accountant");

                    if (isSupervisorOrManagerOrAdminOrAccountant)
                    {
                        // ✅ For supervisors/managers/admins/accountants: search across all accessible hotels
                if (!string.IsNullOrWhiteSpace(hotelCode))
                {
                            // ✅ If X-Hotel-Code header is provided, use it to target specific hotel
                            _logger.LogInformation("✅ [GetById] Supervisor/Manager/Admin/Accountant with X-Hotel-Code header: {HotelCode}", hotelCode);
                    expense = await GetExpenseByIdForSupervisorAsync(id, hotelCode);
                }
                else
                {
                            // ✅ Search across all accessible hotels
                            _logger.LogInformation("✅ [GetById] Supervisor/Manager/Admin/Accountant - searching across all accessible hotels");
                            expense = await GetExpenseByIdForSupervisorAcrossAllHotelsAsync(id, userId);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(hotelCode))
                    {
                        // ✅ Regular user with X-Hotel-Code header
                        expense = await GetExpenseByIdForSupervisorAsync(id, hotelCode);
                    }
                    else
                    {
                        // ✅ Regular user - use standard service method
                        expense = await _expenseService.GetByIdAsync(id);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(hotelCode))
                {
                    // ✅ No userId but X-Hotel-Code header provided
                    expense = await GetExpenseByIdForSupervisorAsync(id, hotelCode);
                }
                else
                {
                    // ✅ Regular user - use standard service method
                    expense = await _expenseService.GetByIdAsync(id);
                }

                if (expense == null)
                {
                    _logger.LogWarning("⚠️ Expense not found with id: {ExpenseId}", id);
                    return NotFound(new { error = $"Expense with id {id} not found" });
                }

                _logger.LogInformation("✅ Expense found: ExpenseId={ExpenseId}", id);

                return Ok(expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching expense: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch expense", details = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على تفاصيل مصروف للمشرف (مع تحديد قاعدة البيانات الصحيحة)
        /// Get expense details for supervisor (with correct database identification)
        /// </summary>
        private async Task<ExpenseResponseDto?> GetExpenseByIdForSupervisorAsync(int expenseId, string hotelCode)
        {
            try
            {
                _logger.LogInformation("🔍 [GetExpenseByIdForSupervisor] Fetching expense: ExpenseId={ExpenseId}, HotelCode={HotelCode}", 
                    expenseId, hotelCode);

                // ✅ الحصول على معلومات Tenant من Master DB
                var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                var tenant = await masterDb.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Code.ToLower() == hotelCode.ToLower());

                if (tenant == null)
                {
                    _logger.LogError("❌ [GetExpenseByIdForSupervisor] Tenant not found for HotelCode: {HotelCode}", hotelCode);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(tenant.DatabaseName))
                {
                    _logger.LogError("❌ [GetExpenseByIdForSupervisor] DatabaseName not set for Tenant: {Code}", tenant.Code);
                    return null;
                }

                // ✅ بناء connection string للـ tenant
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var server = configuration["TenantDatabase:Server"]?.Trim();
                var dbUserId = configuration["TenantDatabase:UserId"]?.Trim();
                var password = configuration["TenantDatabase:Password"]?.Trim();

                if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(dbUserId) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogError("❌ [GetExpenseByIdForSupervisor] TenantDatabase settings not found in configuration");
                    return null;
                }

                var connectionString = $"Server={server}; Database={tenant.DatabaseName}; User Id={dbUserId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

                // ✅ إنشاء DbContext للـ tenant
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(connectionString);
                using var tenantContext = new ApplicationDbContext(optionsBuilder.Options);

                // ✅ الحصول على HotelId من HotelSettings
                var hotelSettings = await tenantContext.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelCode != null && h.HotelCode.ToLower() == hotelCode.ToLower());

                if (hotelSettings == null)
                {
                    _logger.LogError("❌ [GetExpenseByIdForSupervisor] HotelSettings not found for HotelCode: {HotelCode}", hotelCode);
                    return null;
                }

                // ✅ البحث عن المصروف في قاعدة البيانات الصحيحة
                var expense = await tenantContext.Expenses
                    .AsNoTracking()
                    .Include(e => e.HotelSettings)
                    .Include(e => e.ExpenseRooms)
                        .ThenInclude(er => er.Apartment)
                    .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelSettings.HotelId);

                if (expense == null)
                {
                    _logger.LogWarning("⚠️ [GetExpenseByIdForSupervisor] Expense not found: ExpenseId={ExpenseId}, HotelId={HotelId}, HotelCode={HotelCode}", 
                        expenseId, hotelSettings.HotelId, hotelCode);
                    return null;
                }

                // ✅ Get category name from Master DB
                string? categoryName = null;
                if (expense.ExpenseCategoryId.HasValue)
                {
                    var masterCategory = await masterDb.ExpenseCategories
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ec => ec.Id == expense.ExpenseCategoryId.Value);
                    categoryName = masterCategory?.MainCategory;
                }

                // ✅ Get approved by user info (full name, role, tenant) from Master DB
                string? approvedByFullName = null;
                string? approvedByRole = null;
                string? approvedByTenantName = null;
                if (expense.ApprovedBy.HasValue)
                {
                    var masterUser = await masterDb.MasterUsers
                        .AsNoTracking()
                        .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                        .Include(u => u.Tenant)
                        .FirstOrDefaultAsync(u => u.Id == expense.ApprovedBy.Value);
                    
                    if (masterUser != null)
                    {
                        approvedByFullName = masterUser.FullName ?? masterUser.Username;
                        var primaryRole = masterUser.UserRoles?.FirstOrDefault()?.Role;
                        approvedByRole = GetRoleDisplayName(primaryRole?.Code);
                        approvedByTenantName = masterUser.Tenant?.Name;
                    }
                }

                // ✅ تحويل إلى DTO
                var expenseRooms = expense.ExpenseRooms.Select(er => new ExpenseRoomResponseDto
                {
                    ExpenseRoomId = er.ExpenseRoomId,
                    ExpenseId = er.ExpenseId,
                    ZaaerId = er.ZaaerId,
                    Purpose = er.Purpose,
                    Amount = er.Amount,
                    CreatedAt = er.CreatedAt,
                    ApartmentId = er.Apartment?.ApartmentId,
                    ApartmentCode = er.Apartment?.ApartmentCode,
                    ApartmentName = er.Apartment?.ApartmentName
                }).ToList();

                return new ExpenseResponseDto
                {
                    ExpenseId = expense.ExpenseId,
                    HotelId = expense.HotelId,
                    HotelName = expense.HotelSettings?.HotelName,
                    HotelCode = hotelCode,
                    DateTime = expense.DateTime,
                    DueDate = expense.DueDate,
                    Comment = expense.Comment,
                    ExpenseCategoryId = expense.ExpenseCategoryId,
                    ExpenseCategoryName = categoryName, // ✅ From Master DB
                    TaxRate = expense.TaxRate,
                    TaxAmount = expense.TaxAmount,
                    TotalAmount = expense.TotalAmount,
                    CreatedAt = expense.CreatedAt,
                    UpdatedAt = expense.UpdatedAt,
                    ApprovalStatus = expense.ApprovalStatus,
                    ApprovedBy = expense.ApprovedBy,
                    ApprovedByFullName = approvedByFullName,
                    ApprovedByRole = approvedByRole,
                    ApprovedByTenantName = approvedByTenantName,
                    ApprovedAt = expense.ApprovedAt,
                    RejectionReason = expense.RejectionReason,
                    ExpenseRooms = expenseRooms
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetExpenseByIdForSupervisor] Error fetching expense: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get expense by ID for supervisor across all accessible hotels (searches all tenant databases)
        /// </summary>
        private async Task<ExpenseResponseDto?> GetExpenseByIdForSupervisorAcrossAllHotelsAsync(int expenseId, int userId)
        {
            try
            {
                _logger.LogInformation("🔍 [GetExpenseByIdForSupervisorAcrossAllHotels] Searching for expense: ExpenseId={ExpenseId}, UserId={UserId}", 
                    expenseId, userId);

                // ✅ Get all tenants the user has access to
                var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                var userTenants = await masterDb.UserTenants
                    .AsNoTracking()
                    .Include(ut => ut.Tenant)
                    .Where(ut => ut.UserId == userId)
                    .Select(ut => new { ut.TenantId, ut.Tenant!.Code, ut.Tenant.DatabaseName, ut.Tenant.Name })
                    .ToListAsync();

                // ✅ Get user roles to check if manager/admin/accountant (should see all tenants)
                var rolesList = await masterDb.UserRoles
                    .AsNoTracking()
                    .Include(ur => ur.Role)
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.Role!.Code.ToLower())
                    .ToListAsync();

                var isManagerOrAdminOrAccountant = rolesList.Contains("manager") || 
                                                   rolesList.Contains("admin") || 
                                                   rolesList.Contains("accountant");

                if (isManagerOrAdminOrAccountant)
                {
                    _logger.LogInformation("✅ [GetExpenseByIdForSupervisorAcrossAllHotels] Manager/Admin/Accountant - loading all tenants");
                    userTenants = await masterDb.Tenants
                        .AsNoTracking()
                        .Select(t => new { TenantId = t.Id, Code = t.Code, DatabaseName = t.DatabaseName, Name = t.Name })
                        .ToListAsync();
                }

                if (!userTenants.Any())
                {
                    _logger.LogWarning("⚠️ [GetExpenseByIdForSupervisorAcrossAllHotels] No tenants found for user: UserId={UserId}", userId);
                    return null;
                }

                // ✅ Get configuration
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var server = configuration["TenantDatabase:Server"]?.Trim();
                var dbUserId = configuration["TenantDatabase:UserId"]?.Trim();
                var password = configuration["TenantDatabase:Password"]?.Trim();

                if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(dbUserId) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogError("❌ [GetExpenseByIdForSupervisorAcrossAllHotels] TenantDatabase settings not found");
                    return null;
                }

                // ✅ Search across all tenant databases
                foreach (var userTenant in userTenants)
                {
                    try
                    {
                        var connectionString = $"Server={server}; Database={userTenant.DatabaseName}; User Id={dbUserId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

                        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                        optionsBuilder.UseSqlServer(connectionString);
                        using var tenantContext = new ApplicationDbContext(optionsBuilder.Options);

                        // ✅ Check if expense exists in this tenant database
                        var expense = await tenantContext.Expenses
                            .AsNoTracking()
                            .Include(e => e.HotelSettings)
                            .Include(e => e.ExpenseRooms)
                                .ThenInclude(er => er.Apartment)
                            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);

                        if (expense != null)
                        {
                            // ✅ Found the expense - get its details
                            _logger.LogInformation("✅ [GetExpenseByIdForSupervisorAcrossAllHotels] Found expense in tenant: {Code}", userTenant.Code);

                            // ✅ Get category name from Master DB
                            string? categoryName = null;
                            if (expense.ExpenseCategoryId.HasValue)
                            {
                                var masterCategory = await masterDb.ExpenseCategories
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(ec => ec.Id == expense.ExpenseCategoryId.Value);
                                categoryName = masterCategory?.MainCategory;
                            }

                            // ✅ Convert to DTO
                            var expenseRooms = expense.ExpenseRooms.Select(er => new ExpenseRoomResponseDto
                            {
                                ExpenseRoomId = er.ExpenseRoomId,
                                ExpenseId = er.ExpenseId,
                                ZaaerId = er.ZaaerId,
                                Purpose = er.Purpose,
                                Amount = er.Amount,
                                CreatedAt = er.CreatedAt,
                                ApartmentId = er.Apartment?.ApartmentId,
                                ApartmentCode = er.Apartment?.ApartmentCode,
                                ApartmentName = er.Apartment?.ApartmentName
                            }).ToList();

                            return new ExpenseResponseDto
                            {
                                ExpenseId = expense.ExpenseId,
                                HotelId = expense.HotelId,
                                HotelName = expense.HotelSettings?.HotelName,
                                HotelCode = userTenant.Code,
                                DateTime = expense.DateTime,
                                DueDate = expense.DueDate,
                                Comment = expense.Comment,
                                ExpenseCategoryId = expense.ExpenseCategoryId,
                                ExpenseCategoryName = categoryName, // ✅ From Master DB
                                TaxRate = expense.TaxRate,
                                TaxAmount = expense.TaxAmount,
                                TotalAmount = expense.TotalAmount,
                                CreatedAt = expense.CreatedAt,
                                UpdatedAt = expense.UpdatedAt,
                                ApprovalStatus = expense.ApprovalStatus,
                                ApprovedBy = expense.ApprovedBy,
                                ApprovedAt = expense.ApprovedAt,
                                RejectionReason = expense.RejectionReason,
                                ExpenseRooms = expenseRooms
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ [GetExpenseByIdForSupervisorAcrossAllHotels] Error searching tenant {Code}: {Message}", 
                            userTenant.Code, ex.Message);
                        // Continue searching other tenants
                    }
                }

                _logger.LogWarning("⚠️ [GetExpenseByIdForSupervisorAcrossAllHotels] Expense not found in any tenant database: ExpenseId={ExpenseId}", expenseId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetExpenseByIdForSupervisorAcrossAllHotels] Error: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// إنشاء نفقة جديدة
        /// </summary>
        /// <param name="dto">بيانات النفقة</param>
        /// <returns>النفقة المُنشأة</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExpenseResponseDto>> Create([FromBody] CreateExpenseDto dto)
        {
            try
            {
                // Log received DTO for debugging
                _logger.LogInformation("📥 Creating expense - TaxRate: {TaxRate}, TaxAmount: {TaxAmount}, TotalAmount: {TotalAmount}", 
                    dto.TaxRate, dto.TaxAmount, dto.TotalAmount);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    // Get HotelId from tenant service (from X-Hotel-Code header)
                    var tenantService = HttpContext.RequestServices.GetRequiredService<ITenantService>();
                    var tenant = tenantService.GetTenant();
                    
                    var q = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = "/api/expenses",
                        OperationKey = "Expense.Create",
                        PayloadType = nameof(CreateExpenseDto),
                        PayloadJson = JsonSerializer.Serialize(dto),
                        HotelId = tenant?.Id // Use tenant ID from X-Hotel-Code header
                    };
                    await _queueService.EnqueueAsync(q);
                    return Accepted(new { queued = true, requestRef = q.RequestRef });
                }

                _logger.LogInformation(" Creating new expense");

                var expense = await _expenseService.CreateAsync(dto);

                _logger.LogInformation("✅ Expense created successfully: ExpenseId={ExpenseId}, ApprovalStatus={ApprovalStatus}", 
                    expense.ExpenseId, expense.ApprovalStatus);

                // ✅ إضافة رابط الموافقة إذا كان المصروف في حالة pending
                if (expense.ApprovalStatus == "pending")
                {
                    // ✅ استخدام ApprovalBaseUrl من appsettings.json
                    var approvalBaseUrl = _configuration["AppSettings:ApprovalBaseUrl"] ?? "https://aleery.tryasp.net";
                    // إزالة "/" من النهاية إذا كان موجوداً
                    approvalBaseUrl = approvalBaseUrl.TrimEnd('/');
                    var approvalLink = $"{approvalBaseUrl}/approve-expense.html?id={expense.ExpenseId}";
                    
                    _logger.LogInformation("🔗 Approval link generated: {ApprovalLink} (BaseUrl: {BaseUrl})", approvalLink, approvalBaseUrl);
                    
                    // ✅ إرجاع كائن مخصص يحتوي على approvalLink
                    var responseObject = new
                    {
                        expense.ExpenseId,
                        expense.HotelId,
                        expense.DateTime,
                        expense.Comment,
                        expense.ExpenseCategoryId,
                        expenseCategoryName = expense.ExpenseCategoryName,
                        expense.TaxRate,
                        expense.TaxAmount,
                        expense.TotalAmount,
                        expense.CreatedAt,
                        expense.UpdatedAt,
                        expense.ApprovalStatus,
                        expense.ApprovedBy,
                        expense.ApprovedAt,
                        expense.HotelName,
                        approvalLink = approvalLink, // ✅ رابط الموافقة
                        expense.ExpenseRooms
                    };
                    
                    _logger.LogInformation("📤 Returning response with approvalLink: {ApprovalLink}", approvalLink);
                    return CreatedAtAction(nameof(GetById), new { id = expense.ExpenseId }, responseObject);
                }

                _logger.LogInformation("✅ Expense auto-approved (amount <= 50), no approval link needed");
                return CreatedAtAction(nameof(GetById), new { id = expense.ExpenseId }, expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating expense: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to create expense", details = ex.Message });
            }
        }

        /// <summary>
        /// تحديث نفقة موجودة
        /// </summary>
        /// <param name="id">معرف النفقة</param>
        /// <param name="dto">بيانات التحديث</param>
        /// <returns>النفقة المُحدّثة</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExpenseResponseDto>> Update(int id, [FromBody] UpdateExpenseDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    // Get HotelId from tenant service (from X-Hotel-Code header)
                    var tenantService = HttpContext.RequestServices.GetRequiredService<ITenantService>();
                    var tenant = tenantService.GetTenant();
                    
                    var q = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = $"/api/expenses/{id}",
                        OperationKey = "Expense.UpdateById",
                        TargetId = id,
                        PayloadType = nameof(UpdateExpenseDto),
                        PayloadJson = JsonSerializer.Serialize(dto),
                        HotelId = tenant?.Id // Use tenant ID from X-Hotel-Code header
                    };
                    await _queueService.EnqueueAsync(q);
                    return Accepted(new { queued = true, requestRef = q.RequestRef });
                }

                _logger.LogInformation("✏️ Updating expense with id: {ExpenseId}", id);

                var expense = await _expenseService.UpdateAsync(id, dto);

                if (expense == null)
                {
                    _logger.LogWarning("⚠️ Expense not found with id: {ExpenseId}", id);
                    return NotFound(new { error = $"Expense with id {id} not found" });
                }

                _logger.LogInformation("✅ Expense updated successfully: ExpenseId={ExpenseId}", id);

                return Ok(expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating expense: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to update expense", details = ex.Message });
            }
        }

        /// <summary>
        /// حذف نفقة
        /// </summary>
        /// <param name="id">معرف النفقة</param>
        /// <returns>نتيجة الحذف</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    // Get HotelId from tenant service (from X-Hotel-Code header)
                    var tenantService = HttpContext.RequestServices.GetRequiredService<ITenantService>();
                    var tenant = tenantService.GetTenant();
                    
                    var q = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = $"/api/expenses/{id}",
                        OperationKey = "Expense.Delete",
                        TargetId = id,
                        PayloadType = nameof(Delete),
                        PayloadJson = "{}",
                        HotelId = tenant?.Id // Use tenant ID from X-Hotel-Code header
                    };
                    await _queueService.EnqueueAsync(q);
                    return Accepted(new { queued = true, requestRef = q.RequestRef });
                }

                _logger.LogInformation("🗑️ Deleting expense with id: {ExpenseId}", id);

                var deleted = await _expenseService.DeleteAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("⚠️ Expense not found with id: {ExpenseId}", id);
                    return NotFound(new { error = $"Expense with id {id} not found" });
                }

                _logger.LogInformation("✅ Expense deleted successfully: ExpenseId={ExpenseId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting expense: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to delete expense", details = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على جميع expense_rooms لنفقة محددة
        /// </summary>
        /// <param name="expenseId">معرف النفقة</param>
        /// <returns>قائمة expense_rooms</returns>
        [HttpGet("{expenseId}/rooms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ExpenseRoomResponseDto>>> GetExpenseRooms(int expenseId)
        {
            try
            {
                _logger.LogInformation("🔍 Fetching expense rooms for expense: {ExpenseId}", expenseId);

                var expenseRooms = await _expenseService.GetExpenseRoomsAsync(expenseId);

                return Ok(expenseRooms);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "⚠️ Expense not found: {Message}", ex.Message);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching expense rooms: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch expense rooms", details = ex.Message });
            }
        }

        /// <summary>
        /// إضافة غرفة إلى نفقة
        /// </summary>
        /// <param name="expenseId">معرف النفقة</param>
        /// <param name="dto">بيانات expense_room</param>
        /// <returns>expense_room المُنشأ</returns>
        [HttpPost("{expenseId}/rooms")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExpenseRoomResponseDto>> AddExpenseRoom(int expenseId, [FromBody] CreateExpenseRoomDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    // Get HotelId from tenant service (from X-Hotel-Code header)
                    var tenantService = HttpContext.RequestServices.GetRequiredService<ITenantService>();
                    var tenant = tenantService.GetTenant();
                    
                    var q = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = $"/api/expenses/{expenseId}/rooms",
                        OperationKey = "Expense.Room.Add",
                        TargetId = expenseId,
                        PayloadType = nameof(CreateExpenseRoomDto),
                        PayloadJson = JsonSerializer.Serialize(dto),
                        HotelId = tenant?.Id // Use tenant ID from X-Hotel-Code header
                    };
                    await _queueService.EnqueueAsync(q);
                    return Accepted(new { queued = true, requestRef = q.RequestRef });
                }

                _logger.LogInformation("➕ Adding room to expense: ExpenseId={ExpenseId}, ApartmentId={ApartmentId}", 
                    expenseId, dto.ApartmentId);

                var expenseRoom = await _expenseService.AddExpenseRoomAsync(expenseId, dto);

                _logger.LogInformation("✅ ExpenseRoom added successfully: ExpenseRoomId={ExpenseRoomId}", 
                    expenseRoom.ExpenseRoomId);

                return CreatedAtAction(nameof(GetExpenseRooms), new { expenseId }, expenseRoom);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "⚠️ Resource not found: {Message}", ex.Message);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error adding expense room: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to add expense room", details = ex.Message });
            }
        }

        /// <summary>
        /// تحديث expense_room
        /// </summary>
        /// <param name="expenseId">معرف النفقة</param>
        /// <param name="roomId">معرف expense_room</param>
        /// <param name="dto">بيانات التحديث</param>
        /// <returns>expense_room المُحدّث</returns>
        [HttpPut("{expenseId}/rooms/{roomId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExpenseRoomResponseDto>> UpdateExpenseRoom(int expenseId, int roomId, [FromBody] UpdateExpenseRoomDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    // Get HotelId from tenant service (from X-Hotel-Code header)
                    var tenantService = HttpContext.RequestServices.GetRequiredService<ITenantService>();
                    var tenant = tenantService.GetTenant();
                    
                    var q = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = $"/api/expenses/{expenseId}/rooms/{roomId}",
                        OperationKey = "Expense.Room.Update",
                        TargetId = roomId,
                        PayloadType = nameof(UpdateExpenseRoomDto),
                        PayloadJson = JsonSerializer.Serialize(dto),
                        HotelId = tenant?.Id // Use tenant ID from X-Hotel-Code header
                    };
                    await _queueService.EnqueueAsync(q);
                    return Accepted(new { queued = true, requestRef = q.RequestRef });
                }

                _logger.LogInformation("✏️ Updating expense room: ExpenseRoomId={ExpenseRoomId}", roomId);

                var expenseRoom = await _expenseService.UpdateExpenseRoomAsync(roomId, dto);

                if (expenseRoom == null)
                {
                    _logger.LogWarning("⚠️ ExpenseRoom not found with id: {ExpenseRoomId}", roomId);
                    return NotFound(new { error = $"ExpenseRoom with id {roomId} not found" });
                }

                _logger.LogInformation("✅ ExpenseRoom updated successfully: ExpenseRoomId={ExpenseRoomId}", roomId);

                return Ok(expenseRoom);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "⚠️ Resource not found: {Message}", ex.Message);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating expense room: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to update expense room", details = ex.Message });
            }
        }

        /// <summary>
        /// حذف expense_room
        /// </summary>
        /// <param name="expenseId">معرف النفقة</param>
        /// <param name="roomId">معرف expense_room</param>
        /// <returns>نتيجة الحذف</returns>
        [HttpDelete("{expenseId}/rooms/{roomId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteExpenseRoom(int expenseId, int roomId)
        {
            try
            {
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    // Get HotelId from tenant service (from X-Hotel-Code header)
                    var tenantService = HttpContext.RequestServices.GetRequiredService<ITenantService>();
                    var tenant = tenantService.GetTenant();
                    
                    var q = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = $"/api/expenses/{expenseId}/rooms/{roomId}",
                        OperationKey = "Expense.Room.Delete",
                        TargetId = roomId,
                        PayloadType = nameof(DeleteExpenseRoom),
                        PayloadJson = "{}",
                        HotelId = tenant?.Id // Use tenant ID from X-Hotel-Code header
                    };
                    await _queueService.EnqueueAsync(q);
                    return Accepted(new { queued = true, requestRef = q.RequestRef });
                }

                _logger.LogInformation("🗑️ Deleting expense room: ExpenseRoomId={ExpenseRoomId}", roomId);

                var deleted = await _expenseService.DeleteExpenseRoomAsync(roomId);

                if (!deleted)
                {
                    _logger.LogWarning("⚠️ ExpenseRoom not found with id: {ExpenseRoomId}", roomId);
                    return NotFound(new { error = $"ExpenseRoom with id {roomId} not found" });
                }

                _logger.LogInformation("✅ ExpenseRoom deleted successfully: ExpenseRoomId={ExpenseRoomId}", roomId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting expense room: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to delete expense room", details = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على جميع فئات المصروفات من Master DB
        /// Get all expense categories from Master DB (ignoring tenant DB expense_categories table)
        /// </summary>
        /// <returns>قائمة فئات المصروفات</returns>
        [HttpGet("categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<object>>> GetExpenseCategories()
        {
            try
            {
                _logger.LogInformation("📋 [GetExpenseCategories] Fetching expense categories from Master DB");

                // ✅ Get categories from Master DB (not tenant DB)
                var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                
                var categories = await masterDb.ExpenseCategories
                    .AsNoTracking()
                    .Where(ec => ec.IsActive)
                    .OrderBy(ec => ec.Id)
                    .Select(ec => new
                    {
                        id = ec.Id,
                        expenseCategoryId = ec.Id, // ✅ For backward compatibility
                        categoryName = ec.MainCategory,
                        mainCategory = ec.MainCategory,
                        details = ec.Details,
                        categoryCode = ec.CategoryCode,
                        isActive = ec.IsActive
                    })
                    .ToListAsync<object>();

                _logger.LogInformation("✅ [GetExpenseCategories] Successfully retrieved {Count} expense categories from Master DB", categories.Count);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching expense categories: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch expense categories", details = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على نسبة الضريبة للفندق الحالي
        /// Get tax rate for current hotel
        /// </summary>
        /// <returns>نسبة الضريبة</returns>
        [HttpGet("tax-rate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetTaxRate()
        {
            try
            {
                _logger.LogInformation("📊 Fetching tax rate for current hotel");

                var tenant = _tenantService.GetTenant();
                if (tenant == null)
                {
                    return Unauthorized(new { error = "Tenant not resolved. Please provide X-Hotel-Code header." });
                }

                var dbContext = _dbContextResolver.GetCurrentDbContext();

                // Get all hotel settings with the same HotelCode (case-insensitive)
                var allHotelSettings = await dbContext.HotelSettings
                    .AsNoTracking()
                    .Where(h => h.HotelCode != null && h.HotelCode.ToLower() == tenant.Code.ToLower())
                    .Select(h => h.HotelId)
                    .ToListAsync();

                if (allHotelSettings == null || allHotelSettings.Count == 0)
                {
                    _logger.LogWarning("⚠️ No HotelSettings found for hotel code: {HotelCode}", tenant.Code);
                    return NotFound(new { error = $"HotelSettings not found for hotel code: {tenant.Code}" });
                }

                _logger.LogInformation("🔍 Found {Count} HotelSettings with HotelCode '{HotelCode}': HotelIds = {HotelIds}", 
                    allHotelSettings.Count, tenant.Code, string.Join(", ", allHotelSettings));

                // Get enabled tax for any of these hotels (prefer VAT type, or first enabled tax)
                // Search across all HotelIds with the same HotelCode
                var tax = await dbContext.Taxes
                    .AsNoTracking()
                    .Where(t => allHotelSettings.Contains(t.HotelId) && t.Enabled)
                    .OrderByDescending(t => t.TaxType == "VAT" || t.TaxType == "vat")
                    .ThenBy(t => t.Id)
                    .FirstOrDefaultAsync();

                if (tax == null)
                {
                    // Log all available taxes for debugging
                    var allTaxes = await dbContext.Taxes
                        .AsNoTracking()
                        .Where(t => allHotelSettings.Contains(t.HotelId))
                        .Select(t => new { t.Id, t.HotelId, t.TaxName, t.TaxRate, t.Enabled, t.TaxType })
                        .ToListAsync();
                    
                    _logger.LogWarning("⚠️ No enabled tax found for HotelIds: {HotelIds}. Available taxes: {Taxes}", 
                        string.Join(", ", allHotelSettings),
                        string.Join("; ", allTaxes.Select(t => $"Id={t.Id}, HotelId={t.HotelId}, Name={t.TaxName}, Rate={t.TaxRate}, Enabled={t.Enabled}, Type={t.TaxType}")));
                    
                    return Ok(new { taxRate = 0m, hasTax = false });
                }

                _logger.LogInformation("✅ Tax rate found: {TaxRate}% for HotelId: {HotelId} (TaxId: {TaxId}, Name: {TaxName}, Type: {TaxType})", 
                    tax.TaxRate, tax.HotelId, tax.Id, tax.TaxName, tax.TaxType);

                return Ok(new { 
                    taxRate = tax.TaxRate, 
                    hasTax = true,
                    taxName = tax.TaxName,
                    taxType = tax.TaxType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching tax rate: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch tax rate", details = ex.Message });
            }
        }

        /// <summary>
        /// رفع صور لنفقة موجودة
        /// Upload images for an existing expense
        /// </summary>
        /// <param name="expenseId">معرف النفقة</param>
        /// <param name="images">الصور المرفوعة</param>
        /// <returns>قائمة الصور المُرفوعة</returns>
        [HttpPost("{expenseId}/images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<object>>> UploadImages(int expenseId, [FromForm] List<IFormFile> images)
        {
            try
            {
                _logger.LogInformation("📸 Uploading images for expense: ExpenseId={ExpenseId}, ImageCount={ImageCount}", expenseId, images?.Count ?? 0);

                if (images == null || images.Count == 0)
                {
                    return BadRequest(new { error = "No images provided" });
                }

                var tenant = _tenantService.GetTenant();
                if (tenant == null)
                {
                    return Unauthorized(new { error = "Tenant not resolved. Please provide X-Hotel-Code header." });
                }

                var dbContext = _dbContextResolver.GetCurrentDbContext();

                // Verify expense exists and belongs to current hotel
                var hotelSettings = await dbContext.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelCode == tenant.Code);

                if (hotelSettings == null)
                {
                    return NotFound(new { error = $"HotelSettings not found for hotel code: {tenant.Code}" });
                }

                var expense = await dbContext.Expenses
                    .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelSettings.HotelId);

                if (expense == null)
                {
                    return NotFound(new { error = $"Expense with id {expenseId} not found" });
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "expenses");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var uploadedImages = new List<object>();
                var displayOrder = await dbContext.ExpenseImages
                    .Where(ei => ei.ExpenseId == expenseId)
                    .OrderByDescending(ei => ei.DisplayOrder)
                    .Select(ei => ei.DisplayOrder)
                    .FirstOrDefaultAsync();

                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        // Generate unique filename
                        var fileName = $"{expenseId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        var filePath = Path.Combine(uploadsPath, fileName);
                        var relativePath = $"/uploads/expenses/{fileName}";

                        // Save file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Save image record to database
                        var expenseImage = new ExpenseImage
                        {
                            ExpenseId = expenseId,
                            ImagePath = relativePath,
                            OriginalFilename = image.FileName,
                            FileSize = image.Length,
                            ContentType = image.ContentType,
                            DisplayOrder = displayOrder + 1,
                            CreatedAt = DateTime.Now
                        };

                        dbContext.ExpenseImages.Add(expenseImage);
                        await dbContext.SaveChangesAsync();

                        displayOrder++;

                        uploadedImages.Add(new
                        {
                            expenseImageId = expenseImage.ExpenseImageId,
                            imagePath = expenseImage.ImagePath,
                            originalFilename = expenseImage.OriginalFilename,
                            fileSize = expenseImage.FileSize,
                            contentType = expenseImage.ContentType,
                            displayOrder = expenseImage.DisplayOrder
                        });
                    }
                }

                _logger.LogInformation("✅ Successfully uploaded {Count} images for expense: ExpenseId={ExpenseId}", uploadedImages.Count, expenseId);

                return Ok(uploadedImages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error uploading images: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to upload images", details = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على صور نفقة محددة
        /// Get images for a specific expense
        /// </summary>
        /// <param name="expenseId">معرف النفقة</param>
        /// <returns>قائمة الصور</returns>
        [HttpGet("{expenseId}/images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<object>>> GetExpenseImages(int expenseId)
        {
            try
            {
                _logger.LogInformation("📸 Fetching images for expense: ExpenseId={ExpenseId}", expenseId);

                // ✅ الحصول على X-Hotel-Code header إذا كان موجوداً (للمشرفين)
                string? hotelCode = null;
                if (HttpContext.Request.Headers.TryGetValue("X-Hotel-Code", out var hotelCodeValues) && 
                    !string.IsNullOrWhiteSpace(hotelCodeValues))
                {
                    hotelCode = hotelCodeValues.ToString().Trim();
                    _logger.LogInformation("✅ [GetExpenseImages] X-Hotel-Code header found: {HotelCode}", hotelCode);
                }

                // ✅ إذا كان هناك X-Hotel-Code header، نستخدمه لتحديد قاعدة البيانات الصحيحة
                if (!string.IsNullOrWhiteSpace(hotelCode))
                {
                    // ✅ للمشرفين: البحث في قاعدة البيانات الصحيحة بناءً على HotelCode
                    var supervisorImages = await GetExpenseImagesForSupervisorAsync(expenseId, hotelCode);
                    if (supervisorImages != null)
                    {
                        return Ok(supervisorImages);
                    }
                    // If not found, return NotFound
                    return NotFound(new { error = $"Expense with id {expenseId} not found in tenant: {hotelCode}" });
                }

                // ✅ للمستخدمين العاديين: استخدام الطريقة العادية
                var tenant = _tenantService.GetTenant();
                if (tenant == null)
                {
                    return Unauthorized(new { error = "Tenant not resolved. Please provide X-Hotel-Code header." });
                }

                var dbContext = _dbContextResolver.GetCurrentDbContext();

                // Verify expense exists and belongs to current hotel
                var hotelSettings = await dbContext.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelCode == tenant.Code);

                if (hotelSettings == null)
                {
                    return NotFound(new { error = $"HotelSettings not found for hotel code: {tenant.Code}" });
                }

                var expense = await dbContext.Expenses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelSettings.HotelId);

                if (expense == null)
                {
                    return NotFound(new { error = $"Expense with id {expenseId} not found" });
                }

                // Get all images for this expense
                var images = await dbContext.ExpenseImages
                    .AsNoTracking()
                    .Where(ei => ei.ExpenseId == expenseId)
                    .OrderBy(ei => ei.DisplayOrder)
                    .ThenBy(ei => ei.CreatedAt)
                    .Select(ei => new
                    {
                        expenseImageId = ei.ExpenseImageId,
                        imageUrl = ei.ImagePath.StartsWith("http") ? ei.ImagePath : $"{Request.Scheme}://{Request.Host}{ei.ImagePath}",
                        imagePath = ei.ImagePath,
                        originalFilename = ei.OriginalFilename,
                        fileSize = ei.FileSize,
                        contentType = ei.ContentType,
                        displayOrder = ei.DisplayOrder,
                        createdAt = ei.CreatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("✅ Successfully retrieved {Count} images for expense: ExpenseId={ExpenseId}", images.Count, expenseId);

                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching expense images: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch expense images", details = ex.Message });
            }
        }

        /// <summary>
        /// الموافقة أو الرفض على مصروف
        /// Approve or reject an expense
        /// </summary>
        /// <param name="id">معرف المصروف</param>
        /// <param name="status">حالة الموافقة (accepted, rejected, awaiting-manager, awaiting-accountant, أو awaiting-admin)</param>
        /// <param name="rejectionReason">سبب الرفض (اختياري، يُستخدم فقط في حالة rejected)</param>
        /// <returns>نتيجة العملية</returns>
        [HttpPut("approve/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveExpense(int id, [FromQuery] string status, [FromQuery] string? rejectionReason = null)
        {
            try
            {
                _logger.LogInformation("🔐 Approving/Rejecting expense: ExpenseId={ExpenseId}, Status={Status}", id, status);

                // التحقق من صحة الحالة
                if (status != "accepted" && status != "rejected" && status != "awaiting-manager" && status != "awaiting-accountant" && status != "awaiting-admin")
                {
                    return BadRequest(new { error = "Invalid status. Must be 'accepted', 'rejected', 'awaiting-manager', 'awaiting-accountant', or 'awaiting-admin'" });
                }

                // ✅ الحصول على X-Hotel-Code header إذا كان موجوداً (للمشرفين)
                string? hotelCode = null;
                if (HttpContext.Request.Headers.TryGetValue("X-Hotel-Code", out var hotelCodeValues) && 
                    !string.IsNullOrWhiteSpace(hotelCodeValues))
                {
                    hotelCode = hotelCodeValues.ToString().Trim();
                    _logger.LogInformation("✅ X-Hotel-Code header found: {HotelCode}", hotelCode);
                }

                // الحصول على UserId من JWT Token
                int? userId = null;
                if (HttpContext.Items.TryGetValue("UserId", out var userIdObj) && userIdObj != null)
                {
                    if (int.TryParse(userIdObj.ToString(), out int parsedUserId))
                    {
                        userId = parsedUserId;
                        _logger.LogInformation("✅ UserId from JWT Token: {UserId}", userId);
                    }
                }

                if (!userId.HasValue)
                {
                    _logger.LogWarning("⚠️ UserId not found in JWT Token - using default value 0");
                    userId = 0; // Default value if not found
                }

                // ✅ Check if user is supervisor/manager/accountant/admin
                var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                var rolesList = await masterDb.UserRoles
                    .AsNoTracking()
                    .Include(ur => ur.Role)
                    .Where(ur => ur.UserId == userId.Value)
                    .Select(ur => ur.Role!.Code.ToLower())
                    .ToListAsync();

                var isSupervisorOrManagerOrAdminOrAccountant = rolesList.Contains("supervisor") || 
                                                               rolesList.Contains("manager") || 
                                                               rolesList.Contains("admin") || 
                                                               rolesList.Contains("accountant");

                ExpenseResponseDto? expense = null;
                
                if (isSupervisorOrManagerOrAdminOrAccountant)
                {
                    // ✅ For supervisors/managers/admins/accountants: search across all accessible hotels
                if (!string.IsNullOrWhiteSpace(hotelCode))
                {
                        // ✅ If X-Hotel-Code header is provided, use it to target specific hotel
                        _logger.LogInformation("✅ [ApproveExpense] Supervisor/Manager/Admin/Accountant with X-Hotel-Code header: {HotelCode}", hotelCode);
                    expense = await ApproveExpenseForSupervisorAsync(id, status, userId.Value, rejectionReason, hotelCode);
                }
                else
                {
                        // ✅ Search across all accessible hotels
                        _logger.LogInformation("✅ [ApproveExpense] Supervisor/Manager/Admin/Accountant - searching across all accessible hotels");
                        expense = await ApproveExpenseForSupervisorAcrossAllHotelsAsync(id, status, userId.Value, rejectionReason);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(hotelCode))
                {
                    // ✅ Regular user with X-Hotel-Code header (for supervisors accessing specific hotel)
                    expense = await ApproveExpenseForSupervisorAsync(id, status, userId.Value, rejectionReason, hotelCode);
                }
                else
                {
                    // ✅ Regular user - use standard service method
                    expense = await _expenseService.ApproveExpenseAsync(id, status, userId.Value, rejectionReason);
                }

                if (expense == null)
                {
                    _logger.LogWarning("⚠️ Expense not found with id: {ExpenseId}", id);
                    return NotFound(new { error = $"Expense with id {id} not found" });
                }

                _logger.LogInformation("✅ Expense approval updated successfully: ExpenseId={ExpenseId}, Status={Status}, ApprovedBy={ApprovedBy}", 
                    id, status, userId);

                return Ok(new { 
                    message = "Expense status updated successfully", 
                    expenseId = expense.ExpenseId,
                    status = expense.ApprovalStatus,
                    approvedBy = expense.ApprovedBy,
                    approvedAt = expense.ApprovedAt,
                    rejectionReason = expense.RejectionReason
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error approving/rejecting expense: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to update expense status", details = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على سجل موافقات المصروف
        /// Get expense approval history
        /// ✅ Supports supervisors/managers/accountants/admins accessing history from any hotel
        /// </summary>
        /// <param name="id">معرف المصروف</param>
        /// <returns>قائمة سجلات الموافقات</returns>
        [HttpGet("{id}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetApprovalHistory(int id)
        {
            try
            {
                _logger.LogInformation("📋 Fetching approval history for expense: ExpenseId={ExpenseId}", id);

                // ✅ Check if user is supervisor/manager/accountant/admin
                var userIdClaim = HttpContext.Items["UserId"]?.ToString();
                if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    // Regular user - use standard service method
                var history = await _expenseService.GetApprovalHistoryAsync(id);
                _logger.LogInformation("✅ Approval history fetched successfully: ExpenseId={ExpenseId}, Count={Count}", 
                    id, history.Count());
                    return Ok(history);
                }

                // ✅ Get user roles
                var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                var rolesList = await masterDb.UserRoles
                    .AsNoTracking()
                    .Include(ur => ur.Role)
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.Role!.Code.ToLower())
                    .ToListAsync();

                var isSupervisorOrManagerOrAdminOrAccountant = rolesList.Contains("supervisor") || 
                                                               rolesList.Contains("manager") || 
                                                               rolesList.Contains("admin") || 
                                                               rolesList.Contains("accountant");

                if (isSupervisorOrManagerOrAdminOrAccountant)
                {
                    // ✅ For supervisors/managers/admins/accountants: search across all tenant databases
                    _logger.LogInformation("✅ [GetApprovalHistory] Supervisor/Manager/Admin/Accountant detected - searching across all hotels");
                    var history = await GetApprovalHistoryForSupervisorAsync(id, userId);
                    if (history != null)
                    {
                        _logger.LogInformation("✅ Approval history fetched successfully: ExpenseId={ExpenseId}, Count={Count}", 
                            id, history.Count());
                return Ok(history);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Approval history not found for expense: ExpenseId={ExpenseId}", id);
                        return NotFound(new { error = $"Approval history not found for expense {id}" });
                    }
                }
                else
                {
                    // Regular user - use standard service method
                    var history = await _expenseService.GetApprovalHistoryAsync(id);
                    _logger.LogInformation("✅ Approval history fetched successfully: ExpenseId={ExpenseId}, Count={Count}", 
                        id, history.Count());
                    return Ok(history);
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("⚠️ Expense not found: ExpenseId={ExpenseId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching approval history: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch approval history", details = ex.Message });
            }
        }

        /// <summary>
        /// Get approval history for supervisor (searching across all tenant databases)
        /// </summary>
        private async Task<List<ExpenseApprovalHistoryDto>?> GetApprovalHistoryForSupervisorAsync(int expenseId, int userId)
        {
            try
            {
                _logger.LogInformation("🔍 [GetApprovalHistoryForSupervisor] Searching for expense history: ExpenseId={ExpenseId}, UserId={UserId}", 
                    expenseId, userId);

                // ✅ Get all tenants the user has access to
                var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                var userTenants = await masterDb.UserTenants
                    .AsNoTracking()
                    .Include(ut => ut.Tenant)
                    .Where(ut => ut.UserId == userId)
                    .Select(ut => new { ut.TenantId, ut.Tenant!.Code, ut.Tenant.DatabaseName, ut.Tenant.Name })
                    .ToListAsync();

                // ✅ Get user roles to check if manager/admin/accountant (should see all tenants)
                var rolesList = await masterDb.UserRoles
                    .AsNoTracking()
                    .Include(ur => ur.Role)
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.Role!.Code.ToLower())
                    .ToListAsync();

                var isManagerOrAdminOrAccountant = rolesList.Contains("manager") || 
                                                   rolesList.Contains("admin") || 
                                                   rolesList.Contains("accountant");

                if (isManagerOrAdminOrAccountant)
                {
                    _logger.LogInformation("✅ [GetApprovalHistoryForSupervisor] Manager/Admin/Accountant - loading all tenants");
                    userTenants = await masterDb.Tenants
                        .AsNoTracking()
                        .Select(t => new { TenantId = t.Id, Code = t.Code, DatabaseName = t.DatabaseName, Name = t.Name })
                        .ToListAsync();
                }

                if (!userTenants.Any())
                {
                    _logger.LogWarning("⚠️ [GetApprovalHistoryForSupervisor] No tenants found for user: UserId={UserId}", userId);
                    return null;
                }

                // ✅ Get configuration
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var server = configuration["TenantDatabase:Server"]?.Trim();
                var dbUserId = configuration["TenantDatabase:UserId"]?.Trim();
                var password = configuration["TenantDatabase:Password"]?.Trim();

                if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(dbUserId) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogError("❌ [GetApprovalHistoryForSupervisor] TenantDatabase settings not found");
                    return null;
                }

                // ✅ Search across all tenant databases
                foreach (var userTenant in userTenants)
                {
                    try
                    {
                        var connectionString = $"Server={server}; Database={userTenant.DatabaseName}; User Id={dbUserId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

                        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                        optionsBuilder.UseSqlServer(connectionString);
                        using var tenantContext = new ApplicationDbContext(optionsBuilder.Options);

                        // ✅ Check if expense exists in this tenant database
                        var expense = await tenantContext.Expenses
                            .AsNoTracking()
                            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);

                        if (expense != null)
                        {
                            // ✅ Found the expense - get its history
                            _logger.LogInformation("✅ [GetApprovalHistoryForSupervisor] Found expense in tenant: {Code}", userTenant.Code);
                            
                            var history = await tenantContext.ExpenseApprovalHistories
                                .AsNoTracking()
                                .Where(h => h.ExpenseId == expenseId)
                                .OrderBy(h => h.ActionAt)
                                .ToListAsync();

                            // Get unique user IDs to fetch role and tenant info
                            var userIds = history.Where(h => h.ActionBy.HasValue).Select(h => h.ActionBy!.Value).Distinct().ToList();
                            var userInfoDict = new Dictionary<int, (string? role, string? tenantName)>();
                            
                            if (userIds.Any())
                            {
                                var users = await masterDb.MasterUsers
                                    .AsNoTracking()
                                    .Include(u => u.UserRoles)
                                        .ThenInclude(ur => ur.Role)
                                    .Include(u => u.Tenant)
                                    .Where(u => userIds.Contains(u.Id))
                                    .ToListAsync();

                                foreach (var user in users)
                                {
                                    var primaryRole = user.UserRoles?.FirstOrDefault()?.Role;
                                    var roleName = GetRoleDisplayName(primaryRole?.Code);
                                    var tenantName = user.Tenant?.Name;
                                    userInfoDict[user.Id] = (roleName, tenantName);
                                }
                            }

                            return history.Select(h =>
                            {
                                var dto = new ExpenseApprovalHistoryDto
                                {
                                    Id = h.Id,
                                    ExpenseId = h.ExpenseId,
                                    Action = h.Action,
                                    ActionBy = h.ActionBy,
                                    ActionByFullName = h.ActionByFullName,
                                    ActionAt = h.ActionAt,
                                    Status = h.Status,
                                    RejectionReason = h.RejectionReason,
                                    Comments = h.Comments
                                };

                                if (h.ActionBy.HasValue && userInfoDict.TryGetValue(h.ActionBy.Value, out var userInfo))
                                {
                                    dto.ActionByRole = userInfo.role;
                                    dto.ActionByTenantName = userInfo.tenantName;
                                }

                                return dto;
                            }).ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ [GetApprovalHistoryForSupervisor] Error searching tenant {Code}: {Message}", 
                            userTenant.Code, ex.Message);
                        // Continue searching other tenants
                    }
                }

                _logger.LogWarning("⚠️ [GetApprovalHistoryForSupervisor] Expense not found in any tenant database: ExpenseId={ExpenseId}", expenseId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetApprovalHistoryForSupervisor] Error: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// الموافقة/الرفض على مصروف للمشرف (مع تحديد قاعدة البيانات الصحيحة)
        /// Approve/Reject expense for supervisor (with correct database identification)
        /// </summary>
        private async Task<ExpenseResponseDto?> ApproveExpenseForSupervisorAsync(int expenseId, string status, int approvedBy, string? rejectionReason, string hotelCode)
        {
            try
            {
                _logger.LogInformation("🔐 [ApproveExpenseForSupervisor] Approving expense: ExpenseId={ExpenseId}, Status={Status}, HotelCode={HotelCode}", 
                    expenseId, status, hotelCode);

                // ✅ الحصول على معلومات Tenant من Master DB
                var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                var tenant = await masterDb.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Code.ToLower() == hotelCode.ToLower());

                if (tenant == null)
                {
                    _logger.LogError("❌ [ApproveExpenseForSupervisor] Tenant not found for HotelCode: {HotelCode}", hotelCode);
                    throw new InvalidOperationException($"Tenant not found for hotel code: {hotelCode}");
                }

                if (string.IsNullOrWhiteSpace(tenant.DatabaseName))
                {
                    _logger.LogError("❌ [ApproveExpenseForSupervisor] DatabaseName not set for Tenant: {Code}", tenant.Code);
                    throw new InvalidOperationException($"DatabaseName not configured for tenant: {tenant.Code}");
                }

                // ✅ بناء connection string للـ tenant
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var server = configuration["TenantDatabase:Server"]?.Trim();
                var dbUserId = configuration["TenantDatabase:UserId"]?.Trim();
                var password = configuration["TenantDatabase:Password"]?.Trim();

                if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(dbUserId) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogError("❌ [ApproveExpenseForSupervisor] TenantDatabase settings not found in configuration");
                    throw new InvalidOperationException("TenantDatabase settings not found in configuration");
                }

                var connectionString = $"Server={server}; Database={tenant.DatabaseName}; User Id={dbUserId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

                // ✅ إنشاء DbContext للـ tenant
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(connectionString);
                using var tenantContext = new ApplicationDbContext(optionsBuilder.Options);

                // ✅ الحصول على HotelId من HotelSettings
                var hotelSettings = await tenantContext.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelCode != null && h.HotelCode.ToLower() == hotelCode.ToLower());

                if (hotelSettings == null)
                {
                    _logger.LogError("❌ [ApproveExpenseForSupervisor] HotelSettings not found for HotelCode: {HotelCode}", hotelCode);
                    throw new InvalidOperationException($"HotelSettings not found for hotel code: {hotelCode}");
                }

                // ✅ البحث عن المصروف في قاعدة البيانات الصحيحة
                // ✅ محاولة البحث أولاً مع HotelId filter
                var expense = await tenantContext.Expenses
                    .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelSettings.HotelId);

                // ✅ إذا لم يتم العثور عليه، نبحث بدون HotelId filter (في حالة وجود مشكلة في التطابق)
                if (expense == null)
                {
                    _logger.LogWarning("⚠️ [ApproveExpenseForSupervisor] Expense not found with HotelId filter. Trying without filter: ExpenseId={ExpenseId}, HotelId={HotelId}, HotelCode={HotelCode}", 
                        expenseId, hotelSettings.HotelId, hotelCode);
                    
                    expense = await tenantContext.Expenses
                        .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);
                    
                    if (expense != null)
                    {
                        _logger.LogInformation("✅ [ApproveExpenseForSupervisor] Expense found without HotelId filter: ExpenseId={ExpenseId}, ActualHotelId={ActualHotelId}, ExpectedHotelId={ExpectedHotelId}", 
                            expenseId, expense.HotelId, hotelSettings.HotelId);
                    }
                }

                if (expense == null)
                {
                    _logger.LogError("❌ [ApproveExpenseForSupervisor] Expense not found: ExpenseId={ExpenseId}, HotelId={HotelId}, HotelCode={HotelCode}", 
                        expenseId, hotelSettings.HotelId, hotelCode);
                    throw new InvalidOperationException($"Expense with id {expenseId} not found in tenant database for hotel code {hotelCode}");
                }

                // ✅ تحديث حالة الموافقة
                expense.ApprovalStatus = status;

                bool awaitingNextLevel = status == "awaiting-manager" || status == "awaiting-accountant" || status == "awaiting-admin";
                if (awaitingNextLevel)
                {
                    expense.ApprovedBy = null;
                    expense.ApprovedAt = null;
                }
                else
                {
                    expense.ApprovedBy = approvedBy;
                    expense.ApprovedAt = DateTime.Now;
                }
                expense.UpdatedAt = DateTime.Now;

                // ✅ تحديث سبب الرفض إذا كان موجوداً
                if (status == "rejected" && !string.IsNullOrWhiteSpace(rejectionReason))
                {
                    expense.RejectionReason = rejectionReason;
                }
                else if (status != "rejected")
                {
                    // ✅ مسح سبب الرفض إذا تمت الموافقة
                    expense.RejectionReason = null;
                }

                await tenantContext.SaveChangesAsync();

                // حفظ سجل الموافقة/الرفض في ExpenseApprovalHistory
                string? actionByFullName = null;
                if (approvedBy > 0)
                {
                    var masterUser = await masterDb.MasterUsers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == approvedBy);
                    actionByFullName = masterUser?.FullName ?? masterUser?.Username;
                }

                string action = status switch
                {
                    "accepted" => "approved",
                    "rejected" => "rejected",
                    "awaiting-manager" => "awaiting-manager",
                    "awaiting-accountant" => "awaiting-accountant",
                    "awaiting-admin" => "awaiting-admin",
                    _ => "updated"
                };

                string comments = status switch
                {
                    "accepted" => "تم الموافقة على المصروف",
                    "rejected" => $"تم رفض المصروف{(string.IsNullOrWhiteSpace(rejectionReason) ? "" : $": {rejectionReason}")}",
                    "awaiting-manager" => "في انتظار موافقة مدير العمليات",
                    "awaiting-accountant" => "في انتظار موافقة المحاسب",
                    "awaiting-admin" => "في انتظار موافقة المدير العام",
                    _ => "تم تحديث حالة المصروف"
                };

                var history = new FinanceLedgerAPI.Models.ExpenseApprovalHistory
                {
                    ExpenseId = expense.ExpenseId,
                    Action = action,
                    ActionBy = approvedBy > 0 ? approvedBy : null,
                    ActionByFullName = actionByFullName,
                    ActionAt = DateTime.UtcNow,
                    Status = status,
                    RejectionReason = status == "rejected" ? rejectionReason : null,
                    Comments = comments
                };
                await tenantContext.ExpenseApprovalHistories.AddAsync(history);
                await tenantContext.SaveChangesAsync();
                _logger.LogInformation("✅ [ApproveExpenseForSupervisor] Expense approval history saved: ExpenseId={ExpenseId}, Action={Action}, Status={Status}, ActionBy={ActionBy}", 
                    expense.ExpenseId, action, status, approvedBy);

                _logger.LogInformation("✅ [ApproveExpenseForSupervisor] Expense approval updated: ExpenseId={ExpenseId}, Status={Status}, ApprovedBy={ApprovedBy}, HotelCode={HotelCode}", 
                    expenseId, status, approvedBy, hotelCode);

                // ✅ تحميل المصروف مع العلاقات لعرضه
                var updatedExpense = await tenantContext.Expenses
                    .AsNoTracking()
                    .Include(e => e.HotelSettings)
                    .Include(e => e.ExpenseRooms)
                        .ThenInclude(er => er.Apartment)
                    .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelSettings.HotelId);

                // ✅ إذا لم يتم العثور عليه، نبحث بدون HotelId filter
                if (updatedExpense == null)
                {
                    _logger.LogWarning("⚠️ [ApproveExpenseForSupervisor] Updated expense not found with HotelId filter. Trying without filter: ExpenseId={ExpenseId}", expenseId);
                    updatedExpense = await tenantContext.Expenses
                        .AsNoTracking()
                        .Include(e => e.HotelSettings)
                        .Include(e => e.ExpenseRooms)
                            .ThenInclude(er => er.Apartment)
                        .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);
                }

                if (updatedExpense == null)
                {
                    _logger.LogError("❌ [ApproveExpenseForSupervisor] Updated expense not found after save: ExpenseId={ExpenseId}", expenseId);
                    throw new InvalidOperationException($"Failed to retrieve updated expense with id {expenseId}");
                }

                // ✅ Get category name from Master DB
                string? categoryName = null;
                if (updatedExpense.ExpenseCategoryId.HasValue)
                {
                    var masterCategory = await masterDb.ExpenseCategories
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ec => ec.Id == updatedExpense.ExpenseCategoryId.Value);
                    categoryName = masterCategory?.MainCategory;
                }

                // ✅ Get approved by user info (full name, role, tenant) from Master DB
                string? approvedByFullName = actionByFullName; // Already fetched above
                string? approvedByRole = null;
                string? approvedByTenantName = null;
                if (approvedBy > 0)
                {
                    var masterUser = await masterDb.MasterUsers
                        .AsNoTracking()
                        .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                        .Include(u => u.Tenant)
                        .FirstOrDefaultAsync(u => u.Id == approvedBy);
                    
                    if (masterUser != null)
                    {
                        var primaryRole = masterUser.UserRoles?.FirstOrDefault()?.Role;
                        approvedByRole = GetRoleDisplayName(primaryRole?.Code);
                        approvedByTenantName = masterUser.Tenant?.Name;
                    }
                }

                // ✅ تحويل إلى DTO
                var expenseRooms = updatedExpense.ExpenseRooms.Select(er => new ExpenseRoomResponseDto
                {
                    ExpenseRoomId = er.ExpenseRoomId,
                    ExpenseId = er.ExpenseId,
                    ZaaerId = er.ZaaerId,
                    Purpose = er.Purpose,
                    Amount = er.Amount,
                    CreatedAt = er.CreatedAt,
                    ApartmentId = er.Apartment?.ApartmentId,
                    ApartmentCode = er.Apartment?.ApartmentCode,
                    ApartmentName = er.Apartment?.ApartmentName
                }).ToList();

                return new ExpenseResponseDto
                {
                    ExpenseId = updatedExpense.ExpenseId,
                    HotelId = updatedExpense.HotelId,
                    HotelName = updatedExpense.HotelSettings?.HotelName,
                    HotelCode = hotelCode,
                    DateTime = updatedExpense.DateTime,
                    DueDate = updatedExpense.DueDate,
                    Comment = updatedExpense.Comment,
                    ExpenseCategoryId = updatedExpense.ExpenseCategoryId,
                    ExpenseCategoryName = categoryName, // ✅ From Master DB
                    TaxRate = updatedExpense.TaxRate,
                    TaxAmount = updatedExpense.TaxAmount,
                    TotalAmount = updatedExpense.TotalAmount,
                    CreatedAt = updatedExpense.CreatedAt,
                    UpdatedAt = updatedExpense.UpdatedAt,
                    ApprovalStatus = updatedExpense.ApprovalStatus,
                    ApprovedBy = updatedExpense.ApprovedBy,
                    ApprovedByFullName = approvedByFullName,
                    ApprovedByRole = approvedByRole,
                    ApprovedByTenantName = approvedByTenantName,
                    ApprovedAt = updatedExpense.ApprovedAt,
                    RejectionReason = updatedExpense.RejectionReason,
                    ExpenseRooms = expenseRooms
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [ApproveExpenseForSupervisor] Error approving expense: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Approve/Reject expense for supervisor across all accessible hotels (searches all tenant databases)
        /// </summary>
        private async Task<ExpenseResponseDto?> ApproveExpenseForSupervisorAcrossAllHotelsAsync(int expenseId, string status, int approvedBy, string? rejectionReason)
        {
            try
            {
                _logger.LogInformation("🔐 [ApproveExpenseForSupervisorAcrossAllHotels] Approving expense: ExpenseId={ExpenseId}, Status={Status}, UserId={UserId}", 
                    expenseId, status, approvedBy);

                // ✅ Get all tenants the user has access to
                var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                var userTenants = await masterDb.UserTenants
                    .AsNoTracking()
                    .Include(ut => ut.Tenant)
                    .Where(ut => ut.UserId == approvedBy)
                    .Select(ut => new { ut.TenantId, ut.Tenant!.Code, ut.Tenant.DatabaseName, ut.Tenant.Name })
                    .ToListAsync();

                // ✅ Get user roles to check if manager/admin/accountant (should see all tenants)
                var rolesList = await masterDb.UserRoles
                    .AsNoTracking()
                    .Include(ur => ur.Role)
                    .Where(ur => ur.UserId == approvedBy)
                    .Select(ur => ur.Role!.Code.ToLower())
                    .ToListAsync();

                var isManagerOrAdminOrAccountant = rolesList.Contains("manager") || 
                                                   rolesList.Contains("admin") || 
                                                   rolesList.Contains("accountant");

                if (isManagerOrAdminOrAccountant)
                {
                    _logger.LogInformation("✅ [ApproveExpenseForSupervisorAcrossAllHotels] Manager/Admin/Accountant - loading all tenants");
                    userTenants = await masterDb.Tenants
                        .AsNoTracking()
                        .Select(t => new { TenantId = t.Id, Code = t.Code, DatabaseName = t.DatabaseName, Name = t.Name })
                        .ToListAsync();
                }

                if (!userTenants.Any())
                {
                    _logger.LogWarning("⚠️ [ApproveExpenseForSupervisorAcrossAllHotels] No tenants found for user: UserId={UserId}", approvedBy);
                    return null;
                }

                // ✅ Get configuration
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var server = configuration["TenantDatabase:Server"]?.Trim();
                var dbUserId = configuration["TenantDatabase:UserId"]?.Trim();
                var password = configuration["TenantDatabase:Password"]?.Trim();

                if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(dbUserId) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogError("❌ [ApproveExpenseForSupervisorAcrossAllHotels] TenantDatabase settings not found");
                    return null;
                }

                // ✅ Search across all tenant databases
                foreach (var userTenant in userTenants)
                {
                    try
                    {
                        var connectionString = $"Server={server}; Database={userTenant.DatabaseName}; User Id={dbUserId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

                        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                        optionsBuilder.UseSqlServer(connectionString);
                        using var tenantContext = new ApplicationDbContext(optionsBuilder.Options);

                        // ✅ Check if expense exists in this tenant database
                        var expense = await tenantContext.Expenses
                            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);

                        if (expense != null)
                        {
                            // ✅ Found the expense - approve/reject it
                            _logger.LogInformation("✅ [ApproveExpenseForSupervisorAcrossAllHotels] Found expense in tenant: {Code}", userTenant.Code);

                            // ✅ Update approval status
                            expense.ApprovalStatus = status;

                            bool awaitingNextLevel = status == "awaiting-manager" || status == "awaiting-accountant" || status == "awaiting-admin";
                            if (awaitingNextLevel)
                            {
                                expense.ApprovedBy = null;
                                expense.ApprovedAt = null;
                            }
                            else
                            {
                                expense.ApprovedBy = approvedBy;
                                expense.ApprovedAt = DateTime.Now;
                            }
                            expense.UpdatedAt = DateTime.Now;

                            // ✅ Update rejection reason if provided
                            if (status == "rejected" && !string.IsNullOrWhiteSpace(rejectionReason))
                            {
                                expense.RejectionReason = rejectionReason;
                            }
                            else if (status != "rejected")
                            {
                                expense.RejectionReason = null;
                            }

                            await tenantContext.SaveChangesAsync();

                            // ✅ Save approval history
                            string? actionByFullName = null;
                            if (approvedBy > 0)
                            {
                                var masterUser = await masterDb.MasterUsers
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(u => u.Id == approvedBy);
                                actionByFullName = masterUser?.FullName ?? masterUser?.Username;
                            }

                            string action = status switch
                            {
                                "accepted" => "approved",
                                "rejected" => "rejected",
                                "awaiting-manager" => "awaiting-manager",
                                "awaiting-accountant" => "awaiting-accountant",
                                "awaiting-admin" => "awaiting-admin",
                                _ => "updated"
                            };

                            string comments = status switch
                            {
                                "accepted" => "تم الموافقة على المصروف",
                                "rejected" => $"تم رفض المصروف{(string.IsNullOrWhiteSpace(rejectionReason) ? "" : $": {rejectionReason}")}",
                                "awaiting-manager" => "في انتظار موافقة مدير العمليات",
                                "awaiting-accountant" => "في انتظار موافقة المحاسب",
                                "awaiting-admin" => "في انتظار موافقة المدير العام",
                                _ => "تم تحديث حالة المصروف"
                            };

                            var history = new FinanceLedgerAPI.Models.ExpenseApprovalHistory
                            {
                                ExpenseId = expense.ExpenseId,
                                Action = action,
                                ActionBy = approvedBy > 0 ? approvedBy : null,
                                ActionByFullName = actionByFullName,
                                ActionAt = DateTime.UtcNow,
                                Status = status,
                                RejectionReason = status == "rejected" ? rejectionReason : null,
                                Comments = comments
                            };
                            await tenantContext.ExpenseApprovalHistories.AddAsync(history);
                            await tenantContext.SaveChangesAsync();

                            // ✅ Load updated expense with relationships
                            var updatedExpense = await tenantContext.Expenses
                                .AsNoTracking()
                                .Include(e => e.HotelSettings)
                                .Include(e => e.ExpenseRooms)
                                    .ThenInclude(er => er.Apartment)
                                .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);

                            if (updatedExpense == null)
                            {
                                _logger.LogError("❌ [ApproveExpenseForSupervisorAcrossAllHotels] Updated expense not found after save: ExpenseId={ExpenseId}", expenseId);
                                return null;
                            }

                            // ✅ Get category name from Master DB
                            string? categoryName = null;
                            if (updatedExpense.ExpenseCategoryId.HasValue)
                            {
                                var masterCategory = await masterDb.ExpenseCategories
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(ec => ec.Id == updatedExpense.ExpenseCategoryId.Value);
                                categoryName = masterCategory?.MainCategory;
                            }

                            // ✅ Get approved by user info (full name, role, tenant)
                            string? approvedByFullName = actionByFullName;
                            string? approvedByRole = null;
                            string? approvedByTenantName = null;
                            if (approvedBy > 0)
                            {
                                var masterUser = await masterDb.MasterUsers
                                    .AsNoTracking()
                                    .Include(u => u.UserRoles)
                                        .ThenInclude(ur => ur.Role)
                                    .Include(u => u.Tenant)
                                    .FirstOrDefaultAsync(u => u.Id == approvedBy);
                                
                                if (masterUser != null)
                                {
                                    var primaryRole = masterUser.UserRoles?.FirstOrDefault()?.Role;
                                    approvedByRole = GetRoleDisplayName(primaryRole?.Code);
                                    approvedByTenantName = masterUser.Tenant?.Name;
                                }
                            }

                            // ✅ Convert to DTO
                            var expenseRooms = updatedExpense.ExpenseRooms.Select(er => new ExpenseRoomResponseDto
                            {
                                ExpenseRoomId = er.ExpenseRoomId,
                                ExpenseId = er.ExpenseId,
                                ZaaerId = er.ZaaerId,
                                Purpose = er.Purpose,
                                Amount = er.Amount,
                                CreatedAt = er.CreatedAt,
                                ApartmentId = er.Apartment?.ApartmentId,
                                ApartmentCode = er.Apartment?.ApartmentCode,
                                ApartmentName = er.Apartment?.ApartmentName
                            }).ToList();

                            _logger.LogInformation("✅ [ApproveExpenseForSupervisorAcrossAllHotels] Expense approved successfully: ExpenseId={ExpenseId}, Status={Status}, Tenant={Code}", 
                                expenseId, status, userTenant.Code);

                            return new ExpenseResponseDto
                            {
                                ExpenseId = updatedExpense.ExpenseId,
                                HotelId = updatedExpense.HotelId,
                                HotelName = updatedExpense.HotelSettings?.HotelName,
                                HotelCode = userTenant.Code,
                                DateTime = updatedExpense.DateTime,
                                DueDate = updatedExpense.DueDate,
                                Comment = updatedExpense.Comment,
                                ExpenseCategoryId = updatedExpense.ExpenseCategoryId,
                                ExpenseCategoryName = categoryName, // ✅ From Master DB
                                TaxRate = updatedExpense.TaxRate,
                                TaxAmount = updatedExpense.TaxAmount,
                                TotalAmount = updatedExpense.TotalAmount,
                                CreatedAt = updatedExpense.CreatedAt,
                                UpdatedAt = updatedExpense.UpdatedAt,
                                ApprovalStatus = updatedExpense.ApprovalStatus,
                                ApprovedBy = updatedExpense.ApprovedBy,
                                ApprovedByFullName = approvedByFullName,
                                ApprovedByRole = approvedByRole,
                                ApprovedByTenantName = approvedByTenantName,
                                ApprovedAt = updatedExpense.ApprovedAt,
                                RejectionReason = updatedExpense.RejectionReason,
                                ExpenseRooms = expenseRooms
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ [ApproveExpenseForSupervisorAcrossAllHotels] Error searching tenant {Code}: {Message}", 
                            userTenant.Code, ex.Message);
                        // Continue searching other tenants
                    }
                }

                _logger.LogWarning("⚠️ [ApproveExpenseForSupervisorAcrossAllHotels] Expense not found in any tenant database: ExpenseId={ExpenseId}", expenseId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [ApproveExpenseForSupervisorAcrossAllHotels] Error: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// الحصول على صور مصروف للمشرف (مع تحديد قاعدة البيانات الصحيحة)
        /// Get expense images for supervisor (with correct database identification)
        /// </summary>
        private async Task<List<object>?> GetExpenseImagesForSupervisorAsync(int expenseId, string hotelCode)
        {
            try
            {
                _logger.LogInformation("📸 [GetExpenseImagesForSupervisor] Fetching images for expense: ExpenseId={ExpenseId}, HotelCode={HotelCode}", 
                    expenseId, hotelCode);

                // ✅ الحصول على معلومات Tenant من Master DB
                var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                var tenant = await masterDb.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Code.ToLower() == hotelCode.ToLower());

                if (tenant == null)
                {
                    _logger.LogError("❌ [GetExpenseImagesForSupervisor] Tenant not found for HotelCode: {HotelCode}", hotelCode);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(tenant.DatabaseName))
                {
                    _logger.LogError("❌ [GetExpenseImagesForSupervisor] DatabaseName not set for Tenant: {Code}", tenant.Code);
                    return null;
                }

                // ✅ بناء connection string للـ tenant
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var server = configuration["TenantDatabase:Server"]?.Trim();
                var dbUserId = configuration["TenantDatabase:UserId"]?.Trim();
                var password = configuration["TenantDatabase:Password"]?.Trim();

                if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(dbUserId) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogError("❌ [GetExpenseImagesForSupervisor] TenantDatabase settings not found in configuration");
                    return null;
                }

                var connectionString = $"Server={server}; Database={tenant.DatabaseName}; User Id={dbUserId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

                // ✅ إنشاء DbContext للـ tenant
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(connectionString);
                using var tenantContext = new ApplicationDbContext(optionsBuilder.Options);

                // ✅ الحصول على HotelId من HotelSettings
                var hotelSettings = await tenantContext.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelCode != null && h.HotelCode.ToLower() == hotelCode.ToLower());

                if (hotelSettings == null)
                {
                    _logger.LogError("❌ [GetExpenseImagesForSupervisor] HotelSettings not found for HotelCode: {HotelCode}", hotelCode);
                    return null;
                }

                // ✅ التحقق من وجود المصروف في قاعدة البيانات الصحيحة
                var expense = await tenantContext.Expenses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelSettings.HotelId);

                if (expense == null)
                {
                    _logger.LogWarning("⚠️ [GetExpenseImagesForSupervisor] Expense not found: ExpenseId={ExpenseId}, HotelId={HotelId}, HotelCode={HotelCode}", 
                        expenseId, hotelSettings.HotelId, hotelCode);
                    return null;
                }

                // ✅ الحصول على الصور
                var images = await tenantContext.ExpenseImages
                    .AsNoTracking()
                    .Where(ei => ei.ExpenseId == expenseId)
                    .OrderBy(ei => ei.DisplayOrder)
                    .ThenBy(ei => ei.CreatedAt)
                    .Select(ei => new
                    {
                        expenseImageId = ei.ExpenseImageId,
                        imageUrl = ei.ImagePath.StartsWith("http") ? ei.ImagePath : $"{Request.Scheme}://{Request.Host}{ei.ImagePath}",
                        imagePath = ei.ImagePath,
                        originalFilename = ei.OriginalFilename,
                        fileSize = ei.FileSize,
                        contentType = ei.ContentType,
                        displayOrder = ei.DisplayOrder,
                        createdAt = ei.CreatedAt
                    })
                    .ToListAsync<object>();

                _logger.LogInformation("✅ [GetExpenseImagesForSupervisor] Successfully retrieved {Count} images for expense: ExpenseId={ExpenseId}, HotelCode={HotelCode}", 
                    images.Count, expenseId, hotelCode);

                return images;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetExpenseImagesForSupervisor] Error fetching expense images: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// الحصول على جميع المصروفات من عدة tenants للمشرف
        /// Get all expenses from multiple tenants for supervisor
        /// </summary>
        /// <returns>قائمة المصروفات من جميع الفنادق التابعة للمشرف</returns>
        [HttpGet("supervisor/all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetSupervisorExpenses()
        {
            try
            {
                // ✅ استخراج معلومات المستخدم من JWT Token
                var userIdClaim = HttpContext.Items["UserId"]?.ToString();
                if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("⚠️ [GetSupervisorExpenses] UserId not found in JWT token");
                    return Unauthorized(new { error = "User information not found in token" });
                }

                _logger.LogInformation("📋 [GetSupervisorExpenses] Fetching expenses for supervisor UserId: {UserId}", userId);

                // ✅ الحصول على قائمة الفنادق التابعة للمشرف من UserTenants
                var masterDb = HttpContext.RequestServices.GetRequiredService<MasterDbContext>();
                
                // ✅ محاولة استخراج الأدوار من HttpContext أولاً
                var roleCsv = HttpContext.Items["Roles"]?.ToString() ?? string.Empty;
                _logger.LogInformation("🔍 [GetSupervisorExpenses] Raw roles CSV from HttpContext for UserId {UserId}: '{RoleCsv}'", userId, roleCsv);
                
                var rolesList = roleCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                       .Select(r => r.Trim().ToLower())
                                       .Where(r => !string.IsNullOrWhiteSpace(r))
                                       .ToList();
                
                // ✅ إذا لم تكن الأدوار متوفرة في HttpContext، جلبها مباشرة من قاعدة البيانات
                if (!rolesList.Any())
                {
                    _logger.LogWarning("⚠️ [GetSupervisorExpenses] No roles found in HttpContext for UserId {UserId}. Fetching from database.", userId);
                    var dbRoles = await masterDb.UserRoles
                        .AsNoTracking()
                        .Include(ur => ur.Role)
                        .Where(ur => ur.UserId == userId)
                        .Select(ur => ur.Role!.Code)
                        .ToListAsync();
                    
                    _logger.LogInformation("📋 [GetSupervisorExpenses] Raw roles from database for UserId {UserId}: {RawRoles}", userId, string.Join(", ", dbRoles));
                    
                    rolesList = dbRoles.Where(r => !string.IsNullOrWhiteSpace(r))
                                      .Select(r => r.Trim().ToLower())
                                      .ToList();
                    _logger.LogInformation("📋 [GetSupervisorExpenses] Fetched and normalized roles from database for UserId {UserId}: {Roles}", userId, string.Join(", ", rolesList));
                }
                else
                {
                    _logger.LogInformation("📋 [GetSupervisorExpenses] Roles from HttpContext (normalized) for UserId {UserId}: {Roles}", userId, string.Join(", ", rolesList));
                }
                
                var isManagerOrAdminOrAccountant = rolesList.Contains("manager") || rolesList.Contains("admin") || rolesList.Contains("accountant");
                _logger.LogInformation("🔍 [GetSupervisorExpenses] UserId {UserId} - isManagerOrAdminOrAccountant: {IsManagerOrAdminOrAccountant} (checked for 'manager', 'admin', or 'accountant' in: [{Roles}])", 
                    userId, isManagerOrAdminOrAccountant, string.Join(", ", rolesList));

                var userTenants = await masterDb.UserTenants
                    .AsNoTracking()
                    .Include(ut => ut.Tenant)
                    .Where(ut => ut.UserId == userId)
                    .Select(ut => new { ut.TenantId, ut.Tenant!.Code, ut.Tenant.DatabaseName, ut.Tenant.Name })
                    .ToListAsync();

                _logger.LogInformation("📊 [GetSupervisorExpenses] UserId {UserId} - Found {Count} tenants from UserTenants table", userId, userTenants.Count);

                if (isManagerOrAdminOrAccountant)
                {
                    _logger.LogInformation("✅ [GetSupervisorExpenses] Manager/Admin/Accountant role detected for UserId {UserId}. Loading all tenants.", userId);
                    userTenants = await masterDb.Tenants
                        .AsNoTracking()
                        .Select(t => new { TenantId = t.Id, Code = t.Code, DatabaseName = t.DatabaseName, Name = t.Name })
                        .ToListAsync();
                    _logger.LogInformation("✅ [GetSupervisorExpenses] Loaded {Count} tenants for Manager/Admin/Accountant", userTenants.Count);
                }
                else if (!userTenants.Any())
                {
                    _logger.LogWarning("⚠️ [GetSupervisorExpenses] No tenants linked to user {UserId}. Loading all tenants (fallback).", userId);
                    userTenants = await masterDb.Tenants
                        .AsNoTracking()
                        .Select(t => new { TenantId = t.Id, Code = t.Code, DatabaseName = t.DatabaseName, Name = t.Name })
                        .ToListAsync();

                    if (!userTenants.Any())
                    {
                        return Ok(new List<ExpenseResponseDto>());
                    }
                }

                _logger.LogInformation("✅ [GetSupervisorExpenses] Found {Count} tenants for supervisor", userTenants.Count);

                var allExpenses = new List<ExpenseResponseDto>();

                // ✅ Performance Optimization: Get configuration once
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var server = configuration["TenantDatabase:Server"]?.Trim();
                var dbUserId = configuration["TenantDatabase:UserId"]?.Trim();
                var password = configuration["TenantDatabase:Password"]?.Trim();
                
                if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(dbUserId) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogError("❌ [GetSupervisorExpenses] TenantDatabase settings not found in configuration");
                    return Ok(new List<ExpenseResponseDto>());
                }

                // ✅ Performance Optimization: Use Parallel processing to fetch from all tenants simultaneously
                var tenantExpensesTasks = userTenants.Select(async userTenant =>
                {
                    try
                    {
                        var connectionString = $"Server={server}; Database={userTenant.DatabaseName}; User Id={dbUserId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

                        // ✅ إنشاء DbContext للـ tenant (using for proper disposal)
                        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                        optionsBuilder.UseSqlServer(connectionString);
                        await using var tenantContext = new ApplicationDbContext(optionsBuilder.Options);

                        // ✅ الحصول على HotelIds من هذا Tenant
                        // First try to match by HotelCode == Tenant.Code
                        var hotelSettings = await tenantContext.HotelSettings
                            .AsNoTracking()
                            .Where(h => h.HotelCode != null && h.HotelCode.ToLower() == userTenant.Code.ToLower())
                            .Select(h => h.HotelId)
                            .ToListAsync();

                        // ✅ FALLBACK: If no match found, get ALL HotelIds from this tenant database
                        // This handles cases where hotel_code was changed or doesn't match Tenant.Code
                        if (!hotelSettings.Any())
                        {
                            _logger.LogWarning("⚠️ [GetSupervisorExpenses] No HotelSettings found matching Tenant Code '{Code}'. Getting ALL HotelIds from tenant database as fallback.", userTenant.Code);
                            
                            // Get all HotelIds from this tenant database
                            var allHotelIds = await tenantContext.HotelSettings
                                .AsNoTracking()
                                .Select(h => h.HotelId)
                                .ToListAsync();
                            
                            if (allHotelIds.Any())
                            {
                                hotelSettings = allHotelIds;
                                _logger.LogInformation("✅ [GetSupervisorExpenses] Using {Count} HotelIds from tenant database (fallback mode)", hotelSettings.Count);
                                
                                // Log all hotel_codes for debugging
                                var allHotelCodes = await tenantContext.HotelSettings
                                    .AsNoTracking()
                                    .Select(h => new { h.HotelId, h.HotelCode })
                                    .ToListAsync();
                                _logger.LogInformation("📋 [GetSupervisorExpenses] Available HotelSettings in tenant DB: {HotelSettings}", 
                                    string.Join(", ", allHotelCodes.Select(h => $"HotelId={h.HotelId}, HotelCode='{h.HotelCode}'")));
                            }
                            else
                            {
                                _logger.LogError("❌ [GetSupervisorExpenses] No HotelSettings found at all in tenant database: {DatabaseName}", userTenant.DatabaseName);
                            return new List<ExpenseResponseDto>();
                        }
                        }
                        else
                        {
                            _logger.LogInformation("✅ [GetSupervisorExpenses] Found {Count} HotelSettings matching Tenant Code '{Code}': HotelIds = {HotelIds}", 
                                hotelSettings.Count, userTenant.Code, string.Join(", ", hotelSettings));
                        }

                        // ✅ DIAGNOSTIC: Check what hotel_ids are actually in expenses table
                        var expenseHotelIds = await tenantContext.Expenses
                            .AsNoTracking()
                            .Select(e => e.HotelId)
                            .Distinct()
                            .ToListAsync();
                        _logger.LogInformation("🔍 [GetSupervisorExpenses] Tenant '{Code}' - Expenses table contains HotelIds: {ExpenseHotelIds}, Expected HotelIds: {ExpectedHotelIds}", 
                            userTenant.Code, string.Join(", ", expenseHotelIds), string.Join(", ", hotelSettings));

                        // ✅ الحصول على المصروفات من هذا Tenant (optimized query)
                        // ✅ CRITICAL FIX: Get ALL expenses from tenant database, regardless of hotel_id
                        // Each tenant database should only contain expenses for that tenant anyway
                        // Filtering by hotel_id can cause issues if hotel_code was changed or expenses have wrong hotel_id
                        var tenantExpenses = await tenantContext.Expenses
                            .AsNoTracking()
                            .Include(e => e.HotelSettings)
                            .Include(e => e.ExpenseRooms)
                                .ThenInclude(er => er.Apartment)
                            // ✅ Removed hotel_id filter - get ALL expenses from this tenant database
                            .OrderByDescending(e => e.DateTime)
                            .Select(e => new
                            {
                                Expense = e,
                                HotelName = e.HotelSettings != null ? e.HotelSettings.HotelName : null,
                                ExpenseRooms = e.ExpenseRooms.Select(er => new
                                {
                                    ExpenseRoomId = er.ExpenseRoomId,
                                    ExpenseId = er.ExpenseId,
                                    ZaaerId = er.ZaaerId,
                                    Purpose = er.Purpose,
                                    Amount = er.Amount,
                                    CreatedAt = er.CreatedAt,
                                    Apartment = er.Apartment != null ? new
                                    {
                                        ApartmentId = er.Apartment.ApartmentId,
                                        ApartmentCode = er.Apartment.ApartmentCode,
                                        ApartmentName = er.Apartment.ApartmentName
                                    } : null
                                }).ToList()
                            })
                            .ToListAsync();

                        _logger.LogInformation("📊 [GetSupervisorExpenses] Tenant '{Code}' - Found {Count} expenses (all expenses from database, not filtered by hotel_id)", 
                            userTenant.Code, tenantExpenses.Count);

                        // ✅ Get all unique category IDs from expenses
                        var categoryIds = tenantExpenses
                            .Where(e => e.Expense.ExpenseCategoryId.HasValue)
                            .Select(e => e.Expense.ExpenseCategoryId!.Value)
                            .Distinct()
                            .ToList();

                        // ✅ Load category names from Master DB using a NEW scope for this task
                        // CRITICAL: Each parallel task needs its own DbContext instance to avoid concurrency issues
                        Dictionary<int, string> masterCategories;
                        if (categoryIds.Any())
                        {
                            // Create a new scope for this task to get a fresh DbContext instance
                            using var scope = HttpContext.RequestServices.CreateScope();
                            var masterDbForTask = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
                            masterCategories = await masterDbForTask.ExpenseCategories
                                .AsNoTracking()
                                .Where(ec => categoryIds.Contains(ec.Id))
                                .ToDictionaryAsync(ec => ec.Id, ec => ec.MainCategory);
                        }
                        else
                        {
                            masterCategories = new Dictionary<int, string>();
                        }

                        // ✅ Get all unique ApprovedBy user IDs from expenses
                        var approvedByUserIds = tenantExpenses
                            .Where(e => e.Expense.ApprovedBy.HasValue)
                            .Select(e => e.Expense.ApprovedBy!.Value)
                            .Distinct()
                            .ToList();

                        // ✅ Load all approved by user info (full name, role, tenant) from Master DB using a NEW scope
                        Dictionary<int, (string fullName, string? role, string? tenantName)> approvedByUsersDict;
                        if (approvedByUserIds.Any())
                        {
                            using var scope = HttpContext.RequestServices.CreateScope();
                            var masterDbForTask = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
                            var users = await masterDbForTask.MasterUsers
                                .AsNoTracking()
                                .Include(u => u.UserRoles)
                                    .ThenInclude(ur => ur.Role)
                                .Include(u => u.Tenant)
                                .Where(u => approvedByUserIds.Contains(u.Id))
                                .ToListAsync();
                            
                            approvedByUsersDict = users.ToDictionary(
                                u => u.Id,
                                u =>
                                {
                                    var fullName = u.FullName ?? u.Username;
                                    var primaryRole = u.UserRoles?.FirstOrDefault()?.Role;
                                    var roleName = GetRoleDisplayName(primaryRole?.Code);
                                    var tenantName = u.Tenant?.Name;
                                    return (fullName, roleName, tenantName);
                                }
                            );
                        }
                        else
                        {
                            approvedByUsersDict = new Dictionary<int, (string, string?, string?)>();
                        }

                        // ✅ تحويل إلى DTOs
                        var tenantExpenseDtos = tenantExpenses.Select(item =>
                        {
                            var expense = item.Expense;
                            
                            // ✅ Get category name from Master DB
                            string? categoryName = null;
                            if (expense.ExpenseCategoryId.HasValue && masterCategories.TryGetValue(expense.ExpenseCategoryId.Value, out var catName))
                            {
                                categoryName = catName;
                            }
                            
                            // ✅ Get approved by user info from dictionary
                            string? approvedByFullName = null;
                            string? approvedByRole = null;
                            string? approvedByTenantName = null;
                            if (expense.ApprovedBy.HasValue && approvedByUsersDict.TryGetValue(expense.ApprovedBy.Value, out var userInfo))
                            {
                                approvedByFullName = userInfo.fullName;
                                approvedByRole = userInfo.role;
                                approvedByTenantName = userInfo.tenantName;
                            }
                            
                            var expenseRooms = item.ExpenseRooms.Select(er => new ExpenseRoomResponseDto
                            {
                                ExpenseRoomId = er.ExpenseRoomId,
                                ExpenseId = er.ExpenseId,
                                ZaaerId = er.ZaaerId,
                                Purpose = er.Purpose,
                                Amount = er.Amount,
                                CreatedAt = er.CreatedAt,
                                ApartmentId = er.Apartment?.ApartmentId,
                                ApartmentCode = er.Apartment?.ApartmentCode,
                                ApartmentName = er.Apartment?.ApartmentName
                            }).ToList();

                            return new ExpenseResponseDto
                            {
                                ExpenseId = expense.ExpenseId,
                                HotelId = expense.HotelId,
                                HotelName = item.HotelName ?? userTenant.Name,
                                HotelCode = userTenant.Code,
                                DateTime = expense.DateTime,
                                DueDate = expense.DueDate,
                                Comment = expense.Comment,
                                ExpenseCategoryId = expense.ExpenseCategoryId,
                                ExpenseCategoryName = categoryName, // ✅ From Master DB
                                TaxRate = expense.TaxRate,
                                TaxAmount = expense.TaxAmount,
                                TotalAmount = expense.TotalAmount,
                                CreatedAt = expense.CreatedAt,
                                UpdatedAt = expense.UpdatedAt,
                                ApprovalStatus = expense.ApprovalStatus,
                                ApprovedBy = expense.ApprovedBy,
                                ApprovedByFullName = approvedByFullName,
                                ApprovedAt = expense.ApprovedAt,
                                RejectionReason = expense.RejectionReason,
                                ExpenseRooms = expenseRooms
                            };
                        }).ToList();

                        _logger.LogInformation("✅ [GetSupervisorExpenses] Retrieved {Count} expenses from Tenant: {Code}", 
                            tenantExpenseDtos.Count, userTenant.Code);
                        
                        return tenantExpenseDtos;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ [GetSupervisorExpenses] Error fetching expenses from Tenant: {Code}, Error: {Message}", 
                            userTenant.Code, ex.Message);
                        return new List<ExpenseResponseDto>(); // Return empty list on error
                    }
                });

                // ✅ Wait for all tenants to complete in parallel (Performance Optimization)
                var allTenantResults = await Task.WhenAll(tenantExpensesTasks);
                
                // ✅ Flatten results into single list
                allExpenses = allTenantResults.SelectMany(x => x).ToList();

                _logger.LogInformation("✅ [GetSupervisorExpenses] Successfully retrieved {Count} total expenses for supervisor", allExpenses.Count);

                return Ok(allExpenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetSupervisorExpenses] Error fetching supervisor expenses: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch supervisor expenses", details = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على المصروفات المعلقة للموافقة للمشرف
        /// Get pending expenses for supervisor approval
        /// </summary>
        /// <returns>قائمة المصروفات المعلقة</returns>
        [HttpGet("supervisor/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetSupervisorPendingExpenses()
        {
            try
            {
                var allExpenses = await GetSupervisorExpenses();
                if (allExpenses.Result is OkObjectResult okResult && okResult.Value is IEnumerable<ExpenseResponseDto> expenses)
                {
                    // Filter for pending expenses only (including awaiting-manager)
                    var pendingExpenses = expenses.Where(e => 
                        e.ApprovalStatus?.ToLower() == "pending" || 
                        e.ApprovalStatus?.ToLower() == "awaiting-manager"
                    ).ToList();
                    _logger.LogInformation("✅ [GetSupervisorPendingExpenses] Found {Count} pending expenses", pendingExpenses.Count);
                    return Ok(pendingExpenses);
                }
                return Ok(new List<ExpenseResponseDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetSupervisorPendingExpenses] Error: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch pending expenses", details = ex.Message });
            }
        }

        /// <summary>
        /// تحويل Role Code إلى اسم عربي للعرض
        /// Convert Role Code to Arabic display name
        /// </summary>
        private string? GetRoleDisplayName(string? roleCode)
        {
            if (string.IsNullOrWhiteSpace(roleCode))
                return null;

            return roleCode.ToLower() switch
            {
                "staff" or "reception staff" => "موظف",
                "supervisor" => "مشرف فرع",
                "manager" => "مدير العمليات",
                "accountant" => "المحاسب",
                "admin" or "administrator" => "المدير العام",
                _ => roleCode
            };
        }
    }
}

