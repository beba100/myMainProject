using FinanceLedgerAPI.Models;
using ExpenseModel = FinanceLedgerAPI.Models.Expense;
using ExpenseRoomModel = FinanceLedgerAPI.Models.ExpenseRoom;
using ExpenseCategoryModel = FinanceLedgerAPI.Models.ExpenseCategory;
using ExpenseApprovalHistoryModel = FinanceLedgerAPI.Models.ExpenseApprovalHistory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Expense;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Expense
{
    /// <summary>
    /// Service لإدارة النفقات (Expenses)
    /// يستخدم ITenantService للحصول على HotelId من X-Hotel-Code header
    /// يستخدم Unit of Work pattern للوصول إلى قاعدة البيانات
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context; // For complex queries with Include
        private readonly ITenantService _tenantService;
        private readonly ILogger<ExpenseService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MasterDbContext _masterDbContext;

        /// <summary>
        /// Constructor for ExpenseService
        /// </summary>
        /// <param name="unitOfWork">Unit of Work for database operations</param>
        /// <param name="context">Application database context (for complex queries with Include)</param>
        /// <param name="tenantService">Tenant service for getting current hotel</param>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration for reading app settings</param>
        /// <param name="httpContextAccessor">HTTP context accessor for getting current user</param>
        /// <param name="masterDbContext">Master database context for getting user info</param>
        public ExpenseService(
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            ITenantService tenantService,
            ILogger<ExpenseService> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            MasterDbContext masterDbContext)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _masterDbContext = masterDbContext ?? throw new ArgumentNullException(nameof(masterDbContext));
        }

        /// <summary>
        /// الحصول على HotelId من Tenant (يُقرأ من X-Hotel-Code header)
        /// 1. يحصل على Tenant.Code من Master DB
        /// 2. يبحث عن HotelSettings في Tenant DB باستخدام HotelCode == Tenant.Code
        /// 3. يستخدم HotelSettings.HotelId في الاستعلامات
        /// </summary>
        private async Task<int> GetCurrentHotelIdAsync()
        {
            var tenant = _tenantService.GetTenant();
            if (tenant == null)
            {
                throw new InvalidOperationException("Tenant not resolved. Cannot get hotel ID.");
            }

            // ✅ البحث عن HotelSettings في Tenant DB باستخدام Tenant.Code من Master DB
            var hotelSettings = await _unitOfWork.HotelSettings
                .FindSingleAsync(h => h.HotelCode == tenant.Code);

            if (hotelSettings == null)
            {
                _logger.LogError("HotelSettings not found for Tenant Code: {TenantCode} in Tenant DB", tenant.Code);
                throw new InvalidOperationException(
                    $"HotelSettings not found for hotel code: {tenant.Code}. " +
                    "Please ensure hotel settings are configured in the tenant database with matching HotelCode.");
            }

            _logger.LogDebug("Using HotelId: {HotelId} for Tenant Code: {TenantCode} from Master DB (HotelSettings.HotelCode: {HotelCode})", 
                hotelSettings.HotelId, tenant.Code, hotelSettings.HotelCode);
            
            return hotelSettings.HotelId;
        }

        /// <summary>
        /// الحصول على جميع النفقات للفندق الحالي
        /// </summary>
        public async Task<IEnumerable<ExpenseResponseDto>> GetAllAsync()
        {
            var tenant = _tenantService.GetTenant();
            var hotelId = await GetCurrentHotelIdAsync();
            _logger.LogInformation("Fetching expenses for Tenant Code: {TenantCode} (HotelId: {HotelId}) from Master DB", 
                tenant?.Code ?? "Unknown", hotelId);

            try
            {
                // ✅ PERFORMANCE OPTIMIZATION: Use Select projection to only load needed fields
                // This avoids loading full entity graphs and reduces memory usage
                var expenseData = await _context.Expenses
                    .AsNoTracking()
                    .Where(e => e.HotelId == hotelId)
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
                            CreatedAt = er.CreatedAt
                        }).ToList()
                    })
                    .ToListAsync();

                // ✅ Get all unique ExpenseCategoryIds from expenses
                var categoryIds = expenseData
                    .Where(e => e.Expense.ExpenseCategoryId.HasValue)
                    .Select(e => e.Expense.ExpenseCategoryId!.Value)
                    .Distinct()
                    .ToList();

                // ✅ Load category names from Master DB
                var masterCategories = categoryIds.Any()
                    ? await _masterDbContext.ExpenseCategories
                        .AsNoTracking()
                        .Where(ec => categoryIds.Contains(ec.Id))
                        .ToDictionaryAsync(ec => ec.Id, ec => ec.MainCategory)
                    : new Dictionary<int, string>();

                // ✅ Add category names to expense data
                var expenseDataWithCategories = expenseData.Select(e => new
                {
                    e.Expense,
                    ExpenseCategoryName = e.Expense.ExpenseCategoryId.HasValue && masterCategories.TryGetValue(e.Expense.ExpenseCategoryId.Value, out var categoryName)
                        ? categoryName
                        : null,
                    e.HotelName,
                    e.ExpenseRooms
                }).ToList();

                // ✅ PERFORMANCE OPTIMIZATION: Load all apartments in one query using dictionary for O(1) lookup
                var allZaaerIds = expenseDataWithCategories
                    .SelectMany(e => e.ExpenseRooms)
                    .Where(er => er.ZaaerId.HasValue)
                    .Select(er => er.ZaaerId!.Value)
                    .Distinct()
                    .ToList();

                var apartmentsDict = allZaaerIds.Any()
                    ? await _context.Apartments
                        .AsNoTracking()
                        .Where(a => allZaaerIds.Contains(a.ZaaerId ?? 0))
                        .ToDictionaryAsync(a => a.ZaaerId!.Value, a => a)
                    : new Dictionary<int, Apartment>();

                // ✅ PERFORMANCE OPTIMIZATION: Get all unique ApprovedBy user IDs
                var approvedByUserIds = expenseDataWithCategories
                    .Where(e => e.Expense.ApprovedBy.HasValue)
                    .Select(e => e.Expense.ApprovedBy!.Value)
                    .Distinct()
                    .ToList();

                // ✅ Load all approved by user info (full name, role, tenant) from Master DB in one query
                var approvedByUsersDict = new Dictionary<int, (string fullName, string? role, string? tenantName)>();
                if (approvedByUserIds.Any())
                {
                    var users = await _masterDbContext.MasterUsers
                        .AsNoTracking()
                        .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                        .Include(u => u.Tenant)
                        .Where(u => approvedByUserIds.Contains(u.Id))
                        .ToListAsync();

                    foreach (var user in users)
                    {
                        var fullName = user.FullName ?? user.Username;
                        var primaryRole = user.UserRoles?.FirstOrDefault()?.Role;
                        var roleName = GetRoleDisplayName(primaryRole?.Code);
                        var tenantName = user.Tenant?.Name;
                        approvedByUsersDict[user.Id] = (fullName, roleName, tenantName);
                    }
                }

                // ✅ PERFORMANCE OPTIMIZATION: Map to DTOs efficiently without nested loops
                var result = new List<ExpenseResponseDto>();
                foreach (var item in expenseDataWithCategories)
                {
                    var expense = item.Expense;
                    
                    // ✅ Create approval link only for pending expenses
                    string? approvalLink = null;
                    if (expense.ApprovalStatus == "pending")
                    {
                        var approvalBaseUrl = _configuration["AppSettings:ApprovalBaseUrl"] ?? "https://aleery.tryasp.net";
                        approvalBaseUrl = approvalBaseUrl.TrimEnd('/');
                        approvalLink = $"{approvalBaseUrl}/approve-expense.html?id={expense.ExpenseId}";
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

                    // ✅ Map expense rooms efficiently
                    var expenseRooms = item.ExpenseRooms.Select(er =>
                    {
                        // ✅ Extract category code from purpose if it exists
                        string? categoryCode = null;
                        string? actualPurpose = er.Purpose;
                        
                        if (er.ZaaerId == null || (!string.IsNullOrEmpty(er.Purpose) && er.Purpose.StartsWith("CAT_")))
                        {
                            if (!string.IsNullOrEmpty(er.Purpose) && er.Purpose.StartsWith("CAT_"))
                            {
                                var parts = er.Purpose.Split(new[] { " - " }, 2, StringSplitOptions.None);
                                if (parts.Length > 0)
                                {
                                    categoryCode = parts[0];
                                    actualPurpose = parts.Length > 1 ? parts[1] : null;
                                }
                            }
                        }

                        // ✅ Get apartment name if ZaaerId exists (O(1) dictionary lookup)
                        string? apartmentName = null;
                        if (er.ZaaerId.HasValue && apartmentsDict.TryGetValue(er.ZaaerId.Value, out var apartment))
                        {
                            apartmentName = apartment.ApartmentName;
                        }

                        return new ExpenseRoomResponseDto
                        {
                            ExpenseRoomId = er.ExpenseRoomId,
                            ExpenseId = er.ExpenseId,
                            ZaaerId = er.ZaaerId,
                            CategoryCode = categoryCode,
                            Purpose = actualPurpose,
                            Amount = er.Amount,
                            ApartmentName = apartmentName,
                            CreatedAt = er.CreatedAt
                        };
                    }).ToList();

                    result.Add(new ExpenseResponseDto
                    {
                        ExpenseId = expense.ExpenseId,
                        HotelId = expense.HotelId,
                        HotelName = item.HotelName,
                        DateTime = expense.DateTime,
                        DueDate = expense.DueDate,
                        Comment = expense.Comment,
                        ExpenseCategoryId = expense.ExpenseCategoryId,
                        ExpenseCategoryName = item.ExpenseCategoryName,
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
                        ApprovalLink = approvalLink,
                        ExpenseRooms = expenseRooms
                    });
                }

                _logger.LogInformation("✅ Successfully loaded {Count} expenses with optimized query", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GetAllAsync: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// الحصول على نفقة محددة بالمعرف
        /// ✅ يسمح بالوصول بدون X-Hotel-Code header للسماح للمشرفين بالوصول المباشر
        /// </summary>
        public async Task<ExpenseResponseDto?> GetByIdAsync(int id)
        {
            // ✅ محاولة الحصول على hotelId، لكن إذا فشل، نبحث بدون filter
            int? hotelId = null;
            try
            {
                hotelId = await GetCurrentHotelIdAsync();
            }
            catch (InvalidOperationException)
            {
                // ✅ إذا لم يكن هناك X-Hotel-Code header، نبحث بدون hotel filter
                // هذا يسمح للمشرفين بالوصول المباشر عبر رابط الموافقة
                _logger.LogInformation("⚠️ No X-Hotel-Code header found, searching expense without hotel filter (for public approval access)");
            }

            var expense = hotelId.HasValue
                ? await _context.Expenses
                    .AsNoTracking()
                    .Include(e => e.HotelSettings) // ✅ تحميل HotelSettings للحصول على اسم الفندق
                    .Include(e => e.ExpenseRooms)
                        .ThenInclude(er => er.Apartment)
                    .FirstOrDefaultAsync(e => e.ExpenseId == id && e.HotelId == hotelId.Value)
                : await _context.Expenses
                    .AsNoTracking()
                    .Include(e => e.HotelSettings) // ✅ تحميل HotelSettings للحصول على اسم الفندق
                    .Include(e => e.ExpenseRooms)
                        .ThenInclude(er => er.Apartment)
                    .FirstOrDefaultAsync(e => e.ExpenseId == id);

            if (expense == null)
            {
                return null;
            }

            // ✅ Get category name from Master DB
            string? categoryName = null;
            if (expense.ExpenseCategoryId.HasValue)
            {
                var masterCategory = await _masterDbContext.ExpenseCategories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ec => ec.Id == expense.ExpenseCategoryId.Value);
                categoryName = masterCategory?.MainCategory;
            }

            return await MapToDtoAsync(expense, categoryName);
        }

        /// <summary>
        /// إنشاء نفقة جديدة
        /// </summary>
        public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            // ✅ Always set approval status to pending after creation
            string approvalStatus = "pending";
            _logger.LogInformation("⏳ Setting expense status to pending (requires supervisor approval)");

            // ✅ Set DueDate to today if not provided
            DateTime? dueDate = dto.DueDate ?? DateTime.Today;

            // الحصول على UserId من JWT Token
            int? createdBy = null;
            string? createdByFullName = null;
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue("UserId", out var userIdObj) == true && userIdObj != null)
            {
                if (int.TryParse(userIdObj.ToString(), out int userId))
                {
                    createdBy = userId;
                    // الحصول على FullName من Master DB
                    var masterUser = await _masterDbContext.MasterUsers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    createdByFullName = masterUser?.FullName ?? masterUser?.Username;
                }
            }

            // ✅ Get tax rate from taxes table if not provided in DTO
            decimal? taxRate = dto.TaxRate;
            if (!taxRate.HasValue)
            {
                // Get all hotel settings with the same HotelCode
                var tenant = _tenantService.GetTenant();
                if (tenant != null)
                {
                    var allHotelSettings = await _context.HotelSettings
                        .AsNoTracking()
                        .Where(h => h.HotelCode != null && h.HotelCode.ToLower() == tenant.Code.ToLower())
                        .Select(h => h.HotelId)
                        .ToListAsync();

                    if (allHotelSettings.Any())
                    {
                        // Get enabled tax for any of these hotels (prefer VAT type, or first enabled tax)
                        var tax = await _context.Taxes
                            .AsNoTracking()
                            .Where(t => allHotelSettings.Contains(t.HotelId) && t.Enabled)
                            .OrderByDescending(t => t.TaxType == "VAT" || t.TaxType == "vat")
                            .ThenBy(t => t.Id)
                            .FirstOrDefaultAsync();

                        if (tax != null)
                        {
                            taxRate = tax.TaxRate;
                            _logger.LogInformation("✅ Tax rate retrieved from taxes table: {TaxRate}% for HotelId: {HotelId}", taxRate, tax.HotelId);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ No enabled tax found for HotelIds: {HotelIds}", string.Join(", ", allHotelSettings));
                        }
                    }
                }
            }

            // ✅ Store Master DB ExpenseCategory ID (dto.ExpenseCategoryId is from Master DB)
            var expense = new ExpenseModel
            {
                HotelId = hotelId,
                DateTime = dto.DateTime,
                DueDate = dueDate,
                Comment = dto.Comment,
                ExpenseCategoryId = dto.ExpenseCategoryId, // ✅ This is Master DB ExpenseCategory ID
                TaxRate = taxRate,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                ApprovalStatus = approvalStatus,
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy
            };

            await _unitOfWork.Expenses.AddAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            // حفظ سجل الإنشاء في ExpenseApprovalHistory
            if (createdBy.HasValue)
            {
                var history = new ExpenseApprovalHistoryModel
                {
                    ExpenseId = expense.ExpenseId,
                    Action = "created",
                    ActionBy = createdBy.Value,
                    ActionByFullName = createdByFullName,
                    ActionAt = DateTime.UtcNow,
                    Status = approvalStatus,
                    Comments = "تم إنشاء طلب المصروف"
                };
                await _context.ExpenseApprovalHistories.AddAsync(history);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Expense approval history saved: ExpenseId={ExpenseId}, Action=created, ActionBy={ActionBy}", 
                    expense.ExpenseId, createdBy.Value);
            }

            // إضافة expense_rooms إذا وُجدت
            if (dto.ExpenseRooms != null && dto.ExpenseRooms.Any())
            {
                // ✅ CRITICAL FIX: Get all HotelIds with the same HotelCode (like in ApartmentService)
                // This handles cases where data is linked to different HotelIds but same HotelCode
                var tenant = _tenantService.GetTenant();
                if (tenant == null)
                {
                    throw new InvalidOperationException("Tenant not resolved. Cannot create expense rooms.");
                }
                
                var hotelSettings = await _unitOfWork.HotelSettings
                    .FindSingleAsync(h => h.HotelCode != null && h.HotelCode.ToLower() == tenant.Code.ToLower());
                
                var hotelCode = hotelSettings?.HotelCode ?? tenant.Code;
                
                // Get all HotelIds with the same HotelCode
                var allHotelIdsWithSameCode = await _context.HotelSettings
                    .AsNoTracking()
                    .Where(h => h.HotelCode != null && h.HotelCode.ToLower() == hotelCode.ToLower())
                    .Select(h => h.HotelId)
                    .ToListAsync();
                
                // Check what HotelIds are actually used in apartments table
                var hotelIdsInApartments = await _context.Apartments
                    .AsNoTracking()
                    .Select(a => a.HotelId)
                    .Distinct()
                    .ToListAsync();
                
                // If apartments are linked to HotelId=11 but we're searching with HotelId=1, include HotelId=11
                var hotelSettingsWithId11 = await _context.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelId == 11);
                
                if (hotelSettingsWithId11 != null && hotelIdsInApartments.Contains(11))
                {
                    if (!allHotelIdsWithSameCode.Contains(11))
                    {
                        allHotelIdsWithSameCode.Add(11);
                        _logger.LogWarning("⚠️ [CreateAsync] Added HotelId=11 to search list (data exists but different HotelCode: '{DifferentCode}')", 
                            hotelSettingsWithId11.HotelCode);
                    }
                }
                else if (hotelIdsInApartments.Contains(11))
                {
                    allHotelIdsWithSameCode.Add(11);
                    _logger.LogWarning("⚠️ [CreateAsync] Added HotelId=11 to search list (data exists but no HotelSettings record)");
                }
                
                _logger.LogInformation("🔍 [CreateAsync] Final HotelIds to search for apartments: {HotelIds}", 
                    string.Join(", ", allHotelIdsWithSameCode));

                foreach (var roomDto in dto.ExpenseRooms)
                {
                    // ✅ Check if it's a category (CAT_BUILDING, CAT_RECEPTION, CAT_CORRIDORS) or actual room
                    if (!string.IsNullOrEmpty(roomDto.CategoryCode) && roomDto.CategoryCode.StartsWith("CAT_"))
                    {
                        // ✅ It's a room category (مبنى كامل, الاستقبال, الممرات)
                        // For categories, we don't need to find an apartment - just save the category code
                        // We'll use ApartmentId = 0 or a special value, but store categoryCode in Purpose field
                        // Or we need to add category_code column to expense_rooms table
                        var categoryRoom = new ExpenseRoomModel
                        {
                            ExpenseId = expense.ExpenseId,
                            ZaaerId = null, // ✅ Use null for categories (ZaaerId is nullable)
                            Purpose = roomDto.CategoryCode + (string.IsNullOrEmpty(roomDto.Purpose) ? "" : " - " + roomDto.Purpose), // ✅ Store category code in purpose
                            Amount = roomDto.Amount,
                            CreatedAt = DateTime.Now
                        };

                        await _unitOfWork.ExpenseRooms.AddAsync(categoryRoom);
                        _logger.LogInformation("✅ [CreateAsync] Added ExpenseRoom with Category: ExpenseId={ExpenseId}, CategoryCode={CategoryCode}, Purpose={Purpose}, Amount={Amount}", 
                            expense.ExpenseId, roomDto.CategoryCode, roomDto.Purpose, roomDto.Amount);
                        continue;
                    }

                    // ✅ البحث عن Apartment باستخدام ApartmentId أو ZaaerId مع جميع HotelIds المرتبطة بنفس HotelCode
                    Apartment? apartment = null;
                    
                    if (roomDto.ApartmentId.HasValue)
                    {
                        // البحث باستخدام ApartmentId مع جميع HotelIds
                        apartment = await _context.Apartments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.ApartmentId == roomDto.ApartmentId.Value && allHotelIdsWithSameCode.Contains(a.HotelId));
                    }
                    else if (roomDto.ZaaerId.HasValue)
                    {
                        // ✅ البحث باستخدام ZaaerId مع جميع HotelIds المرتبطة بنفس HotelCode
                        _logger.LogInformation("🔍 [CreateAsync] Searching for apartment with ZaaerId={ZaaerId}, HotelIds={HotelIds}", 
                            roomDto.ZaaerId.Value, string.Join(", ", allHotelIdsWithSameCode));
                        
                        apartment = await _context.Apartments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.ZaaerId == roomDto.ZaaerId.Value && allHotelIdsWithSameCode.Contains(a.HotelId));
                        
                        if (apartment == null)
                        {
                            // ✅ Try searching without HotelId filter as fallback
                            _logger.LogWarning("⚠️ [CreateAsync] Apartment not found with HotelId filter, trying without filter...");
                            apartment = await _context.Apartments
                                .AsNoTracking()
                                .FirstOrDefaultAsync(a => a.ZaaerId == roomDto.ZaaerId.Value);
                        }
                    }

                    if (apartment == null)
                    {
                        _logger.LogError("❌ [CreateAsync] Apartment not found: ApartmentId={ApartmentId}, ZaaerId={ZaaerId}, HotelIds={HotelIds}", 
                            roomDto.ApartmentId, roomDto.ZaaerId, string.Join(", ", allHotelIdsWithSameCode));
                        continue; // Skip invalid apartment
                    }

                    _logger.LogInformation("✅ [CreateAsync] Found apartment: ApartmentId={ApartmentId}, ZaaerId={ZaaerId}, Name={Name}, HotelId={HotelId}", 
                        apartment.ApartmentId, apartment.ZaaerId, apartment.ApartmentName, apartment.HotelId);

                    // ✅ Save zaaerId directly (Foreign Key to apartments.zaaer_id)
                    if (!apartment.ZaaerId.HasValue)
                    {
                        _logger.LogWarning("⚠️ [CreateAsync] Apartment found but ZaaerId is null: ApartmentId={ApartmentId}, Name={Name}", 
                            apartment.ApartmentId, apartment.ApartmentName);
                        continue; // Skip if apartment doesn't have zaaerId
                    }

                    var expenseRoom = new ExpenseRoomModel
                    {
                        ExpenseId = expense.ExpenseId,
                        ZaaerId = apartment.ZaaerId.Value, // ✅ حفظ zaaerId مباشرة (Foreign Key to apartments.zaaer_id)
                        Purpose = roomDto.Purpose,
                        Amount = roomDto.Amount,
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.ExpenseRooms.AddAsync(expenseRoom);
                    _logger.LogInformation("✅ [CreateAsync] Added ExpenseRoom: ExpenseId={ExpenseId}, ZaaerId={ZaaerId}, Purpose={Purpose}, Amount={Amount}", 
                        expense.ExpenseId, apartment.ZaaerId.Value, roomDto.Purpose, roomDto.Amount);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("✅ [CreateAsync] Saved {Count} expense rooms to database", dto.ExpenseRooms.Count);
            }

            _logger.LogInformation("✅ Expense created successfully: ExpenseId={ExpenseId}, HotelId={HotelId}", 
                expense.ExpenseId, hotelId);

            // ✅ Get category name from Master DB for response
            string? categoryName = null;
            if (expense.ExpenseCategoryId.HasValue)
            {
                var masterCategory = await _masterDbContext.ExpenseCategories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ec => ec.Id == expense.ExpenseCategoryId.Value);
                categoryName = masterCategory?.MainCategory;
            }

            return await GetByIdAsync(expense.ExpenseId) ?? await MapToDtoAsync(expense, categoryName);
        }

        /// <summary>
        /// تحديث نفقة موجودة
        /// </summary>
        public async Task<ExpenseResponseDto?> UpdateAsync(int id, UpdateExpenseDto dto)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            var expense = await _unitOfWork.Expenses
                .FindSingleAsync(e => e.ExpenseId == id && e.HotelId == hotelId);

            if (expense == null)
            {
                return null;
            }

            // تحديث الحقول
            if (dto.DateTime.HasValue)
                expense.DateTime = dto.DateTime.Value;
            if (dto.DueDate.HasValue)
                expense.DueDate = dto.DueDate.Value;
            if (dto.Comment != null)
                expense.Comment = dto.Comment;
            if (dto.ExpenseCategoryId.HasValue)
                expense.ExpenseCategoryId = dto.ExpenseCategoryId;
            
            // Handle tax fields - update if provided
            // Note: If both are null, we keep existing values (don't clear)
            // To clear tax, explicitly set both to 0 or handle separately
            if (dto.TaxRate.HasValue)
                expense.TaxRate = dto.TaxRate.Value;
            else if (dto.TaxRate == null && !dto.TaxAmount.HasValue)
            {
                // If TaxRate is explicitly null and TaxAmount is also null/not provided, clear tax
                // This handles the case when checkbox is unchecked
                expense.TaxRate = null;
            }
            
            if (dto.TaxAmount.HasValue)
                expense.TaxAmount = dto.TaxAmount.Value;
            else if (dto.TaxAmount == null && !dto.TaxRate.HasValue)
            {
                // If TaxAmount is explicitly null and TaxRate is also null/not provided, clear tax
                expense.TaxAmount = null;
            }
            
            if (dto.TotalAmount.HasValue)
                expense.TotalAmount = dto.TotalAmount.Value;

            expense.UpdatedAt = DateTime.Now;

            await _unitOfWork.Expenses.UpdateAsync(expense);

            // ✅ Update expense rooms if provided (same logic as CreateAsync)
            if (dto.ExpenseRooms != null && dto.ExpenseRooms.Any())
            {
                // Delete existing expense rooms first
                var existingRooms = await _context.ExpenseRooms
                    .Where(er => er.ExpenseId == expense.ExpenseId)
                    .ToListAsync();

                if (existingRooms.Any())
                {
                    foreach (var existingRoom in existingRooms)
                    {
                        await _unitOfWork.ExpenseRooms.DeleteAsync(existingRoom);
                    }
                    // ✅ Save changes after deleting old rooms before adding new ones
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("✅ [UpdateAsync] Deleted {Count} existing expense rooms", existingRooms.Count);
                }

                // ✅ Get all HotelIds with the same HotelCode (like in CreateAsync)
                var tenant = _tenantService.GetTenant();
                if (tenant == null)
                {
                    throw new InvalidOperationException("Tenant not resolved. Cannot update expense rooms.");
                }
                
                var hotelSettings = await _unitOfWork.HotelSettings
                    .FindSingleAsync(h => h.HotelCode != null && h.HotelCode.ToLower() == tenant.Code.ToLower());
                
                var hotelCode = hotelSettings?.HotelCode ?? tenant.Code;
                
                // Get all HotelIds with the same HotelCode
                var allHotelIdsWithSameCode = await _context.HotelSettings
                    .AsNoTracking()
                    .Where(h => h.HotelCode != null && h.HotelCode.ToLower() == hotelCode.ToLower())
                    .Select(h => h.HotelId)
                    .ToListAsync();

                // Add new expense rooms (same logic as CreateAsync)
                foreach (var roomDto in dto.ExpenseRooms)
                {
                    // ✅ Check if it's a category (CAT_BUILDING, CAT_RECEPTION, CAT_CORRIDORS) or actual room
                    if (!string.IsNullOrEmpty(roomDto.CategoryCode) && roomDto.CategoryCode.StartsWith("CAT_"))
                    {
                        // ✅ It's a room category
                        var categoryExpenseRoom = new ExpenseRoomModel
                        {
                            ExpenseId = expense.ExpenseId,
                            ZaaerId = null, // ✅ Use null for categories (ZaaerId is nullable)
                            Purpose = roomDto.CategoryCode + (string.IsNullOrEmpty(roomDto.Purpose) ? "" : " - " + roomDto.Purpose),
                            Amount = roomDto.Amount,
                            CreatedAt = DateTime.Now
                        };

                        await _unitOfWork.ExpenseRooms.AddAsync(categoryExpenseRoom);
                        _logger.LogInformation("✅ [UpdateAsync] Added ExpenseRoom with Category: ExpenseId={ExpenseId}, CategoryCode={CategoryCode}, Purpose={Purpose}, Amount={Amount}", 
                            expense.ExpenseId, roomDto.CategoryCode, roomDto.Purpose, roomDto.Amount);
                        continue;
                    }

                    // ✅ Search for Apartment using ApartmentId or ZaaerId
                    Apartment? apartment = null;
                    
                    if (roomDto.ApartmentId.HasValue)
                    {
                        apartment = await _context.Apartments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.ApartmentId == roomDto.ApartmentId.Value && allHotelIdsWithSameCode.Contains(a.HotelId));
                    }
                    else if (roomDto.ZaaerId.HasValue)
                    {
                        _logger.LogInformation("🔍 [UpdateAsync] Searching for apartment with ZaaerId={ZaaerId}, HotelIds={HotelIds}", 
                            roomDto.ZaaerId.Value, string.Join(", ", allHotelIdsWithSameCode));
                        
                        apartment = await _context.Apartments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.ZaaerId == roomDto.ZaaerId.Value && allHotelIdsWithSameCode.Contains(a.HotelId));
                        
                        if (apartment == null)
                        {
                            // ✅ Try searching without HotelId filter as fallback
                            _logger.LogWarning("⚠️ [UpdateAsync] Apartment not found with HotelId filter, trying without filter...");
                            apartment = await _context.Apartments
                                .AsNoTracking()
                                .FirstOrDefaultAsync(a => a.ZaaerId == roomDto.ZaaerId.Value);
                        }
                    }

                    if (apartment == null)
                    {
                        _logger.LogError("❌ [UpdateAsync] Apartment not found: ApartmentId={ApartmentId}, ZaaerId={ZaaerId}, HotelIds={HotelIds}", 
                            roomDto.ApartmentId, roomDto.ZaaerId, string.Join(", ", allHotelIdsWithSameCode));
                        continue;
                    }

                    _logger.LogInformation("✅ [UpdateAsync] Found apartment: ApartmentId={ApartmentId}, ZaaerId={ZaaerId}, Name={Name}, HotelId={HotelId}", 
                        apartment.ApartmentId, apartment.ZaaerId, apartment.ApartmentName, apartment.HotelId);

                    // ✅ Save zaaerId directly (Foreign Key to apartments.zaaer_id)
                    if (!apartment.ZaaerId.HasValue)
                    {
                        _logger.LogWarning("⚠️ [UpdateAsync] Apartment found but ZaaerId is null: ApartmentId={ApartmentId}, Name={Name}", 
                            apartment.ApartmentId, apartment.ApartmentName);
                        continue; // Skip if apartment doesn't have zaaerId
                    }

                    var roomExpenseRoom = new ExpenseRoomModel
                    {
                        ExpenseId = expense.ExpenseId,
                        ZaaerId = apartment.ZaaerId.Value, // ✅ حفظ zaaerId مباشرة (Foreign Key to apartments.zaaer_id)
                        Purpose = roomDto.Purpose,
                        Amount = roomDto.Amount,
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.ExpenseRooms.AddAsync(roomExpenseRoom);
                    _logger.LogInformation("✅ [UpdateAsync] Added ExpenseRoom: ExpenseId={ExpenseId}, ZaaerId={ZaaerId}, Purpose={Purpose}, Amount={Amount}", 
                        expense.ExpenseId, apartment.ZaaerId.Value, roomDto.Purpose, roomDto.Amount);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("✅ Expense updated successfully: ExpenseId={ExpenseId}", expense.ExpenseId);

            // ✅ Get category name from Master DB for response
            string? categoryName = null;
            if (expense.ExpenseCategoryId.HasValue)
            {
                var masterCategory = await _masterDbContext.ExpenseCategories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ec => ec.Id == expense.ExpenseCategoryId.Value);
                categoryName = masterCategory?.MainCategory;
            }

            return await GetByIdAsync(expense.ExpenseId);
        }

        /// <summary>
        /// حذف نفقة
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            var expense = await _context.Expenses
                .Include(e => e.ExpenseRooms)
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.HotelId == hotelId);

            if (expense == null)
            {
                return false;
            }

            // حذف expense_rooms أولاً (Cascade delete)
            if (expense.ExpenseRooms.Any())
            {
                foreach (var expenseRoom in expense.ExpenseRooms)
                {
                    await _unitOfWork.ExpenseRooms.DeleteAsync(expenseRoom);
                }
            }

            await _unitOfWork.Expenses.DeleteAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("✅ Expense deleted successfully: ExpenseId={ExpenseId}", id);

            return true;
        }

        /// <summary>
        /// الحصول على جميع expense_rooms لنفقة محددة
        /// </summary>
        public async Task<IEnumerable<ExpenseRoomResponseDto>> GetExpenseRoomsAsync(int expenseId)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            // التحقق من أن Expense موجود في نفس الفندق
            var expense = await _unitOfWork.Expenses
                .FindSingleAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelId);

            if (expense == null)
            {
                throw new KeyNotFoundException($"Expense with id {expenseId} not found");
            }

            // Use context for complex query with Include
            var expenseRooms = await _context.ExpenseRooms
                .AsNoTracking()
                .Include(er => er.Apartment)
                .Where(er => er.ExpenseId == expenseId)
                .OrderBy(er => er.CreatedAt)
                .ToListAsync();

            return expenseRooms.Select(MapExpenseRoomToDto);
        }

        /// <summary>
        /// إضافة غرفة إلى نفقة
        /// </summary>
        public async Task<ExpenseRoomResponseDto> AddExpenseRoomAsync(int expenseId, CreateExpenseRoomDto dto)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            // التحقق من أن Expense موجود في نفس الفندق
            var expense = await _unitOfWork.Expenses
                .FindSingleAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelId);

            if (expense == null)
            {
                throw new KeyNotFoundException($"Expense with id {expenseId} not found");
            }

            // ✅ البحث عن Apartment باستخدام ApartmentId أو ZaaerId
            Apartment? apartment = null;
            
            if (dto.ApartmentId.HasValue)
            {
                // البحث باستخدام ApartmentId
                apartment = await _unitOfWork.Apartments
                    .FindSingleAsync(a => a.ApartmentId == dto.ApartmentId.Value && a.HotelId == hotelId);
            }
            else if (dto.ZaaerId.HasValue)
            {
                // ✅ البحث باستخدام ZaaerId (من الـ frontend)
                apartment = await _unitOfWork.Apartments
                    .FindSingleAsync(a => a.ZaaerId == dto.ZaaerId.Value && a.HotelId == hotelId);
                
                _logger.LogInformation("🔍 [AddExpenseRoomAsync] Searching for apartment with ZaaerId={ZaaerId}, HotelId={HotelId}", 
                    dto.ZaaerId.Value, hotelId);
            }

            if (apartment == null)
            {
                throw new KeyNotFoundException($"Apartment not found: ApartmentId={dto.ApartmentId}, ZaaerId={dto.ZaaerId}, HotelId={hotelId}");
            }

            _logger.LogInformation("✅ [AddExpenseRoomAsync] Found apartment: ApartmentId={ApartmentId}, ZaaerId={ZaaerId}, Name={Name}", 
                apartment.ApartmentId, apartment.ZaaerId, apartment.ApartmentName);

            // ✅ Save zaaerId directly (Foreign Key to apartments.zaaer_id)
            if (!apartment.ZaaerId.HasValue)
            {
                throw new InvalidOperationException($"Apartment found but ZaaerId is null: ApartmentId={apartment.ApartmentId}, Name={apartment.ApartmentName}");
            }

            var expenseRoom = new ExpenseRoomModel
            {
                ExpenseId = expenseId,
                ZaaerId = apartment.ZaaerId.Value, // ✅ حفظ zaaerId مباشرة (Foreign Key to apartments.zaaer_id)
                Purpose = dto.Purpose,
                Amount = dto.Amount,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.ExpenseRooms.AddAsync(expenseRoom);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("✅ ExpenseRoom added successfully: ExpenseRoomId={ExpenseRoomId}, ExpenseId={ExpenseId}, ZaaerId={ZaaerId}", 
                expenseRoom.ExpenseRoomId, expenseId, apartment.ZaaerId.Value);

            return await MapExpenseRoomToDtoWithLoadAsync(expenseRoom.ExpenseRoomId);
        }

        /// <summary>
        /// تحديث expense_room
        /// </summary>
        public async Task<ExpenseRoomResponseDto?> UpdateExpenseRoomAsync(int expenseRoomId, UpdateExpenseRoomDto dto)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            // Use context for complex query with Include
            var expenseRoom = await _context.ExpenseRooms
                .Include(er => er.Expense)
                .FirstOrDefaultAsync(er => er.ExpenseRoomId == expenseRoomId);

            if (expenseRoom == null || expenseRoom.Expense.HotelId != hotelId)
            {
                return null;
            }

            // ✅ التحقق من Apartment إذا تم تحديثه (باستخدام ZaaerId)
            if (dto.ZaaerId.HasValue)
            {
                var apartment = await _unitOfWork.Apartments
                    .FindSingleAsync(a => a.ZaaerId == dto.ZaaerId.Value && a.HotelId == hotelId);

                if (apartment == null)
                {
                    throw new KeyNotFoundException($"Apartment with ZaaerId {dto.ZaaerId.Value} not found");
                }

                if (!apartment.ZaaerId.HasValue)
                {
                    throw new InvalidOperationException($"Apartment found but ZaaerId is null: ApartmentId={apartment.ApartmentId}");
                }

                expenseRoom.ZaaerId = apartment.ZaaerId.Value; // ✅ تحديث zaaerId (Foreign Key to apartments.zaaer_id)
            }
            else if (dto.ApartmentId.HasValue)
            {
                // ✅ Fallback: البحث باستخدام ApartmentId ثم استخدام ZaaerId
                var apartment = await _unitOfWork.Apartments
                    .FindSingleAsync(a => a.ApartmentId == dto.ApartmentId.Value && a.HotelId == hotelId);

                if (apartment == null)
                {
                    throw new KeyNotFoundException($"Apartment with id {dto.ApartmentId.Value} not found");
                }

                if (!apartment.ZaaerId.HasValue)
                {
                    throw new InvalidOperationException($"Apartment found but ZaaerId is null: ApartmentId={apartment.ApartmentId}");
                }

                expenseRoom.ZaaerId = apartment.ZaaerId.Value; // ✅ تحديث zaaerId (Foreign Key to apartments.zaaer_id)
            }

            if (dto.Purpose != null)
                expenseRoom.Purpose = dto.Purpose;

            // ✅ تحديث Amount إذا كان موجوداً
            if (dto.Amount.HasValue)
                expenseRoom.Amount = dto.Amount.Value;

            await _unitOfWork.ExpenseRooms.UpdateAsync(expenseRoom);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("✅ ExpenseRoom updated successfully: ExpenseRoomId={ExpenseRoomId}", expenseRoomId);

            return await MapExpenseRoomToDtoWithLoadAsync(expenseRoomId);
        }

        /// <summary>
        /// حذف expense_room
        /// </summary>
        public async Task<bool> DeleteExpenseRoomAsync(int expenseRoomId)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            // Use context for complex query with Include
            var expenseRoom = await _context.ExpenseRooms
                .Include(er => er.Expense)
                .FirstOrDefaultAsync(er => er.ExpenseRoomId == expenseRoomId);

            if (expenseRoom == null || expenseRoom.Expense.HotelId != hotelId)
            {
                return false;
            }

            await _unitOfWork.ExpenseRooms.DeleteAsync(expenseRoom);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("✅ ExpenseRoom deleted successfully: ExpenseRoomId={ExpenseRoomId}", expenseRoomId);

            return true;
        }

        /// <summary>
        /// الموافقة أو الرفض على مصروف
        /// Approve or reject an expense
        /// </summary>
        /// <param name="id">معرف المصروف</param>
        /// <param name="status">حالة الموافقة (accepted أو rejected)</param>
        /// <param name="approvedBy">معرف المستخدم الذي وافق/رفض</param>
        /// <param name="rejectionReason">سبب الرفض (في حالة الرفض)</param>
        /// <returns>المصروف المُحدّث</returns>
        public async Task<ExpenseResponseDto?> ApproveExpenseAsync(int id, string status, int approvedBy, string? rejectionReason = null)
        {
            // ✅ محاولة الحصول على hotelId، لكن إذا فشل، نبحث بدون filter
            // هذا يسمح للمشرفين بالموافقة/الرفض بدون تسجيل دخول
            int? hotelId = null;
            try
            {
                hotelId = await GetCurrentHotelIdAsync();
            }
            catch (InvalidOperationException)
            {
                // ✅ إذا لم يكن هناك X-Hotel-Code header، نبحث بدون hotel filter
                _logger.LogInformation("⚠️ No X-Hotel-Code header found for approval, searching expense without hotel filter (for public approval access)");
            }

            var expense = hotelId.HasValue
                ? await _unitOfWork.Expenses
                    .FindSingleAsync(e => e.ExpenseId == id && e.HotelId == hotelId.Value)
                : await _unitOfWork.Expenses
                    .FindSingleAsync(e => e.ExpenseId == id);

            if (expense == null)
            {
                _logger.LogWarning("⚠️ Expense not found: ExpenseId={ExpenseId}, HotelId={HotelId}", id, hotelId);
                return null;
            }

            // تحديث حالة الموافقة
            expense.ApprovalStatus = status;

            bool awaitingNextLevel = status == "awaiting-manager" || status == "awaiting-accountant" || status == "awaiting-admin";
            if (awaitingNextLevel)
            {
                // لا يتم تعيين بيانات الموافقة عند الانتقال لمستوى أعلى
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

            await _unitOfWork.Expenses.UpdateAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            // حفظ سجل الموافقة/الرفض في ExpenseApprovalHistory
            string? actionByFullName = null;
            if (approvedBy > 0)
            {
                var masterUser = await _masterDbContext.MasterUsers
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

            var history = new ExpenseApprovalHistoryModel
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
            await _context.ExpenseApprovalHistories.AddAsync(history);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Expense approval history saved: ExpenseId={ExpenseId}, Action={Action}, Status={Status}, ActionBy={ActionBy}", 
                expense.ExpenseId, action, status, approvedBy);

            _logger.LogInformation("✅ Expense approval updated: ExpenseId={ExpenseId}, Status={Status}, ApprovedBy={ApprovedBy}, ApprovedAt={ApprovedAt}", 
                id, status, approvedBy, expense.ApprovedAt);

            return await GetByIdAsync(expense.ExpenseId);
        }

        /// <summary>
        /// الحصول على سجل موافقات المصروف
        /// Get expense approval history
        /// </summary>
        /// <param name="expenseId">معرف المصروف</param>
        /// <returns>قائمة سجلات الموافقات</returns>
        public async Task<IEnumerable<ExpenseApprovalHistoryDto>> GetApprovalHistoryAsync(int expenseId)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            // التحقق من أن Expense موجود في نفس الفندق
            var expense = await _unitOfWork.Expenses
                .FindSingleAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelId);

            if (expense == null)
            {
                throw new KeyNotFoundException($"Expense with id {expenseId} not found");
            }

            var history = await _context.ExpenseApprovalHistories
                .AsNoTracking()
                .Where(h => h.ExpenseId == expenseId)
                .OrderBy(h => h.ActionAt)
                .ToListAsync();

            // Get unique user IDs to fetch role and tenant info
            var userIds = history.Where(h => h.ActionBy.HasValue).Select(h => h.ActionBy!.Value).Distinct().ToList();
            var userInfoDict = new Dictionary<int, (string? role, string? tenantName)>();
            
            if (userIds.Any())
            {
                var users = await _masterDbContext.MasterUsers
                    .AsNoTracking()
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .Include(u => u.Tenant)
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                foreach (var user in users)
                {
                    // Get primary role (first role or most relevant)
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
            });
        }

        /// <summary>
        /// تحويل Expense إلى ExpenseResponseDto
        /// </summary>
        private async Task<ExpenseResponseDto> MapToDtoAsync(ExpenseModel expense, string? categoryName = null)
        {
            // ✅ الحصول على اسم الفندق من HotelSettings
            string? hotelName = null;
            if (expense.HotelSettings != null)
            {
                hotelName = expense.HotelSettings.HotelName;
            }
            else if (expense.HotelId > 0)
            {
                // محاولة تحميل HotelSettings إذا لم تكن محملة
                var hotelSettings = _context.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefault(h => h.HotelId == expense.HotelId);
                hotelName = hotelSettings?.HotelName;
            }

            // ✅ إنشاء رابط الموافقة فقط للمصروفات في حالة pending
            string? approvalLink = null;
            if (expense.ApprovalStatus == "pending")
            {
                // ✅ استخدام ApprovalBaseUrl من appsettings.json
                var approvalBaseUrl = _configuration["AppSettings:ApprovalBaseUrl"] ?? "https://aleery.tryasp.net";
                // إزالة "/" من النهاية إذا كان موجوداً
                approvalBaseUrl = approvalBaseUrl.TrimEnd('/');
                approvalLink = $"{approvalBaseUrl}/approve-expense.html?id={expense.ExpenseId}";
            }

            // ✅ Get approved by user full name, role, and tenant from Master DB
            string? approvedByFullName = null;
            string? approvedByRole = null;
            string? approvedByTenantName = null;
            if (expense.ApprovedBy.HasValue)
            {
                try
                {
                    var masterUser = await _masterDbContext.MasterUsers
                        .AsNoTracking()
                        .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                        .Include(u => u.Tenant)
                        .FirstOrDefaultAsync(u => u.Id == expense.ApprovedBy.Value);
                    
                    if (masterUser != null)
                    {
                        approvedByFullName = masterUser.FullName ?? masterUser.Username;
                        
                        // Get primary role (first role or most relevant)
                        var primaryRole = masterUser.UserRoles?.FirstOrDefault()?.Role;
                        approvedByRole = GetRoleDisplayName(primaryRole?.Code);
                        
                        approvedByTenantName = masterUser.Tenant?.Name;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch approved by user info for user ID {UserId}", expense.ApprovedBy.Value);
                }
            }

            return new ExpenseResponseDto
            {
                ExpenseId = expense.ExpenseId,
                HotelId = expense.HotelId,
                HotelName = hotelName,
                DateTime = expense.DateTime,
                DueDate = expense.DueDate,
                Comment = expense.Comment,
                ExpenseCategoryId = expense.ExpenseCategoryId,
                ExpenseCategoryName = categoryName, // ✅ Use category name from Master DB
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
                ApprovalLink = approvalLink,
                ExpenseRooms = expense.ExpenseRooms?.Select(MapExpenseRoomToDto).ToList() ?? new List<ExpenseRoomResponseDto>()
            };
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

        /// <summary>
        /// تحويل ExpenseRoom إلى ExpenseRoomResponseDto
        /// </summary>
        private ExpenseRoomResponseDto MapExpenseRoomToDto(ExpenseRoomModel expenseRoom)
        {
            // ✅ Extract category code from purpose if it exists (format: "CAT_XXX - purpose text")
            // أو ZaaerId = null يعني أنه فئة
            string? categoryCode = null;
            string? actualPurpose = expenseRoom.Purpose;
            
            // ✅ Check if ZaaerId is null (for categories) OR purpose starts with CAT_
            if (expenseRoom.ZaaerId == null || (!string.IsNullOrEmpty(expenseRoom.Purpose) && expenseRoom.Purpose.StartsWith("CAT_")))
            {
                // It's a category - extract category code from purpose
                if (!string.IsNullOrEmpty(expenseRoom.Purpose) && expenseRoom.Purpose.StartsWith("CAT_"))
                {
                    var parts = expenseRoom.Purpose.Split(new[] { " - " }, 2, StringSplitOptions.None);
                    if (parts.Length > 0)
                    {
                        categoryCode = parts[0]; // CAT_BUILDING, CAT_RECEPTION, etc.
                        actualPurpose = parts.Length > 1 ? parts[1] : null; // Actual purpose text (after " - ")
                    }
                }
            }
            
            return new ExpenseRoomResponseDto
            {
                ExpenseRoomId = expenseRoom.ExpenseRoomId,
                ExpenseId = expenseRoom.ExpenseId,
                ApartmentId = expenseRoom.Apartment?.ApartmentId, // ✅ For backward compatibility
                ZaaerId = expenseRoom.ZaaerId, // ✅ ZaaerId from expense_rooms.zaaer_id (Foreign Key)
                CategoryCode = categoryCode, // ✅ Category code (null for actual rooms)
                ApartmentCode = expenseRoom.Apartment?.ApartmentCode, // ✅ null for categories
                ApartmentName = expenseRoom.Apartment?.ApartmentName, // ✅ null for categories
                Purpose = actualPurpose, // ✅ Actual purpose without category code
                Amount = expenseRoom.Amount,
                CreatedAt = expenseRoom.CreatedAt
            };
        }

        /// <summary>
        /// تحميل ExpenseRoom من DB وتحويله إلى DTO
        /// </summary>
        private async Task<ExpenseRoomResponseDto> MapExpenseRoomToDtoWithLoadAsync(int expenseRoomId)
        {
            var expenseRoom = await _context.ExpenseRooms
                .AsNoTracking()
                .Include(er => er.Apartment)
                .FirstOrDefaultAsync(er => er.ExpenseRoomId == expenseRoomId);

            if (expenseRoom == null)
            {
                throw new KeyNotFoundException($"ExpenseRoom with id {expenseRoomId} not found");
            }

            return MapExpenseRoomToDto(expenseRoom);
        }
    }
}

