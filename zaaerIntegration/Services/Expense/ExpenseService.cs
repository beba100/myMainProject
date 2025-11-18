using FinanceLedgerAPI.Models;
using ExpenseModel = FinanceLedgerAPI.Models.Expense;
using ExpenseRoomModel = FinanceLedgerAPI.Models.ExpenseRoom;
using ExpenseCategoryModel = FinanceLedgerAPI.Models.ExpenseCategory;
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

        /// <summary>
        /// Constructor for ExpenseService
        /// </summary>
        /// <param name="unitOfWork">Unit of Work for database operations</param>
        /// <param name="context">Application database context (for complex queries with Include)</param>
        /// <param name="tenantService">Tenant service for getting current hotel</param>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration for reading app settings</param>
        public ExpenseService(
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            ITenantService tenantService,
            ILogger<ExpenseService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

            // ✅ إرجاع جميع المصروفات (بما في ذلك pending و rejected) للعرض في الجدول
            // Return all expenses (including pending and rejected) for table display
            var expenses = await _context.Expenses
                .AsNoTracking()
                .Include(e => e.ExpenseCategory)
                .Include(e => e.HotelSettings) // ✅ تحميل HotelSettings للحصول على اسم الفندق
                .Include(e => e.ExpenseRooms)
                    .ThenInclude(er => er.Apartment)
                .Where(e => e.HotelId == hotelId)
                .OrderByDescending(e => e.DateTime)
                .ToListAsync();

            return expenses.Select(e => MapToDto(e));
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
                    .Include(e => e.ExpenseCategory)
                    .Include(e => e.HotelSettings) // ✅ تحميل HotelSettings للحصول على اسم الفندق
                    .Include(e => e.ExpenseRooms)
                        .ThenInclude(er => er.Apartment)
                    .FirstOrDefaultAsync(e => e.ExpenseId == id && e.HotelId == hotelId.Value)
                : await _context.Expenses
                    .AsNoTracking()
                    .Include(e => e.ExpenseCategory)
                    .Include(e => e.HotelSettings) // ✅ تحميل HotelSettings للحصول على اسم الفندق
                    .Include(e => e.ExpenseRooms)
                        .ThenInclude(er => er.Apartment)
                    .FirstOrDefaultAsync(e => e.ExpenseId == id);

            if (expense == null)
            {
                return null;
            }

            return MapToDto(expense);
        }

        /// <summary>
        /// إنشاء نفقة جديدة
        /// </summary>
        public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            // ✅ تحديد حالة الموافقة بناءً على المبلغ
            // Approval status logic: auto-approved if amount <= 50, pending if > 50
            string approvalStatus;
            if (dto.TotalAmount <= 50)
            {
                approvalStatus = "auto-approved";
                _logger.LogInformation("💰 Expense amount ({Amount}) <= 50, setting status to auto-approved", dto.TotalAmount);
            }
            else
            {
                approvalStatus = "pending";
                _logger.LogInformation("⏳ Expense amount ({Amount}) > 50, setting status to pending (requires supervisor approval)", dto.TotalAmount);
            }

            var expense = new ExpenseModel
            {
                HotelId = hotelId,
                DateTime = dto.DateTime,
                Comment = dto.Comment,
                ExpenseCategoryId = dto.ExpenseCategoryId,
                TaxRate = dto.TaxRate,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                ApprovalStatus = approvalStatus,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Expenses.AddAsync(expense);
            await _unitOfWork.SaveChangesAsync();

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
                        apartment = await _context.Apartments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.ZaaerId == roomDto.ZaaerId.Value && allHotelIdsWithSameCode.Contains(a.HotelId));
                        
                        _logger.LogInformation("🔍 [CreateAsync] Searching for apartment with ZaaerId={ZaaerId}, HotelIds={HotelIds}", 
                            roomDto.ZaaerId.Value, string.Join(", ", allHotelIdsWithSameCode));
                    }

                    if (apartment == null)
                    {
                        _logger.LogWarning("⚠️ [CreateAsync] Apartment not found: ApartmentId={ApartmentId}, ZaaerId={ZaaerId}, HotelIds={HotelIds}", 
                            roomDto.ApartmentId, roomDto.ZaaerId, string.Join(", ", allHotelIdsWithSameCode));
                        continue; // Skip invalid apartment
                    }

                    _logger.LogInformation("✅ [CreateAsync] Found apartment: ApartmentId={ApartmentId}, ZaaerId={ZaaerId}, Name={Name}, HotelId={HotelId}", 
                        apartment.ApartmentId, apartment.ZaaerId, apartment.ApartmentName, apartment.HotelId);

                    var expenseRoom = new ExpenseRoomModel
                    {
                        ExpenseId = expense.ExpenseId,
                        ApartmentId = apartment.ApartmentId, // ✅ استخدام ApartmentId من الـ apartment الموجود
                        Purpose = roomDto.Purpose,
                        Amount = roomDto.Amount, // ✅ إضافة Amount
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.ExpenseRooms.AddAsync(expenseRoom);
                    _logger.LogInformation("✅ [CreateAsync] Added ExpenseRoom: ExpenseId={ExpenseId}, ApartmentId={ApartmentId}, Purpose={Purpose}, Amount={Amount}", 
                        expense.ExpenseId, apartment.ApartmentId, roomDto.Purpose, roomDto.Amount);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("✅ [CreateAsync] Saved {Count} expense rooms to database", dto.ExpenseRooms.Count);
            }

            _logger.LogInformation("✅ Expense created successfully: ExpenseId={ExpenseId}, HotelId={HotelId}", 
                expense.ExpenseId, hotelId);

            return await GetByIdAsync(expense.ExpenseId) ?? MapToDto(expense);
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
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("✅ Expense updated successfully: ExpenseId={ExpenseId}", expense.ExpenseId);

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

            var expenseRoom = new ExpenseRoomModel
            {
                ExpenseId = expenseId,
                ApartmentId = apartment.ApartmentId, // ✅ استخدام ApartmentId من الـ apartment الموجود
                Purpose = dto.Purpose,
                Amount = dto.Amount, // ✅ إضافة Amount
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.ExpenseRooms.AddAsync(expenseRoom);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("✅ ExpenseRoom added successfully: ExpenseRoomId={ExpenseRoomId}, ExpenseId={ExpenseId}, ApartmentId={ApartmentId}", 
                expenseRoom.ExpenseRoomId, expenseId, dto.ApartmentId);

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

            // التحقق من Apartment إذا تم تحديثه
            if (dto.ApartmentId.HasValue)
            {
                var apartment = await _unitOfWork.Apartments
                    .FindSingleAsync(a => a.ApartmentId == dto.ApartmentId.Value && a.HotelId == hotelId);

                if (apartment == null)
                {
                    throw new KeyNotFoundException($"Apartment with id {dto.ApartmentId.Value} not found");
                }

                expenseRoom.ApartmentId = dto.ApartmentId.Value;
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
        /// <returns>المصروف المُحدّث</returns>
        public async Task<ExpenseResponseDto?> ApproveExpenseAsync(int id, string status, int approvedBy)
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
            expense.ApprovedBy = approvedBy;
            expense.ApprovedAt = DateTime.Now;
            expense.UpdatedAt = DateTime.Now;

            await _unitOfWork.Expenses.UpdateAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("✅ Expense approval updated: ExpenseId={ExpenseId}, Status={Status}, ApprovedBy={ApprovedBy}, ApprovedAt={ApprovedAt}", 
                id, status, approvedBy, expense.ApprovedAt);

            return await GetByIdAsync(expense.ExpenseId);
        }

        /// <summary>
        /// تحويل Expense إلى ExpenseResponseDto
        /// </summary>
        private ExpenseResponseDto MapToDto(ExpenseModel expense)
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

            return new ExpenseResponseDto
            {
                ExpenseId = expense.ExpenseId,
                HotelId = expense.HotelId,
                HotelName = hotelName,
                DateTime = expense.DateTime,
                Comment = expense.Comment,
                ExpenseCategoryId = expense.ExpenseCategoryId,
                ExpenseCategoryName = expense.ExpenseCategory?.CategoryName,
                TaxRate = expense.TaxRate,
                TaxAmount = expense.TaxAmount,
                TotalAmount = expense.TotalAmount,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt,
                ApprovalStatus = expense.ApprovalStatus,
                ApprovedBy = expense.ApprovedBy,
                ApprovedAt = expense.ApprovedAt,
                ApprovalLink = approvalLink,
                ExpenseRooms = expense.ExpenseRooms?.Select(MapExpenseRoomToDto).ToList() ?? new List<ExpenseRoomResponseDto>()
            };
        }

        /// <summary>
        /// تحويل ExpenseRoom إلى ExpenseRoomResponseDto
        /// </summary>
        private ExpenseRoomResponseDto MapExpenseRoomToDto(ExpenseRoomModel expenseRoom)
        {
            return new ExpenseRoomResponseDto
            {
                ExpenseRoomId = expenseRoom.ExpenseRoomId,
                ExpenseId = expenseRoom.ExpenseId,
                ApartmentId = expenseRoom.ApartmentId,
                ApartmentCode = expenseRoom.Apartment?.ApartmentCode,
                ApartmentName = expenseRoom.Apartment?.ApartmentName,
                Purpose = expenseRoom.Purpose,
                Amount = expenseRoom.Amount, // ✅ إضافة Amount
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

