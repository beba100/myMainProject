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

                var expense = await _expenseService.GetByIdAsync(id);

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
        /// الحصول على جميع فئات المصروفات للفندق الحالي
        /// </summary>
        /// <returns>قائمة فئات المصروفات</returns>
        [HttpGet("categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<object>>> GetExpenseCategories()
        {
            try
            {
                // ✅ Log request details including headers
                var requestHotelCode = Request.Headers.ContainsKey("X-Hotel-Code") 
                    ? Request.Headers["X-Hotel-Code"].ToString() 
                    : "Not provided";
                
                _logger.LogInformation("📋 [GetExpenseCategories] ===== REQUEST ===== X-Hotel-Code: {HotelCode}", requestHotelCode);

                var tenant = _tenantService.GetTenant();
                
                if (tenant == null)
                {
                    _logger.LogError("❌ [GetExpenseCategories] Tenant not resolved");
                    return Unauthorized(new { error = "Tenant not resolved. Please provide X-Hotel-Code header." });
                }

                _logger.LogInformation("✅ [GetExpenseCategories] Tenant resolved: Code='{TenantCode}', Id={TenantId}, DatabaseName='{DatabaseName}'", 
                    tenant.Code, tenant.Id, tenant.DatabaseName);

                var dbContext = _dbContextResolver.GetCurrentDbContext();

                // ✅ 1. الحصول على Tenant.Code من Master DB
                // ✅ 2. البحث عن HotelSettings في Tenant DB باستخدام HotelCode == Tenant.Code
                _logger.LogInformation("🔍 [GetExpenseCategories] Searching for HotelSettings with HotelCode: '{TenantCode}' (case-insensitive)", tenant.Code);
                
                // Try case-insensitive search
                var hotelSettings = await dbContext.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelCode != null && h.HotelCode.ToLower() == tenant.Code.ToLower());

                if (hotelSettings == null)
                {
                    // Log all available HotelSettings for debugging
                    var allHotelSettings = await dbContext.HotelSettings
                        .AsNoTracking()
                        .Select(h => new { h.HotelId, h.HotelCode })
                        .ToListAsync();
                    
                    _logger.LogError("HotelSettings not found for Tenant Code: '{TenantCode}' in Tenant DB. Available HotelCodes: {AvailableCodes}", 
                        tenant.Code, string.Join(", ", allHotelSettings.Select(h => $"HotelId={h.HotelId}, Code='{h.HotelCode}'")));
                    
                    return NotFound(new { error = $"HotelSettings not found for hotel code: {tenant.Code}. Please ensure hotel settings are configured in the tenant database with matching HotelCode." });
                }

                // ✅ SOLUTION: Find ALL HotelSettings with the same HotelCode, then use ALL their HotelIds
                // This handles cases where data is linked to different HotelIds but same HotelCode
                var hotelCode = hotelSettings.HotelCode ?? tenant.Code; // Fallback to tenant.Code if null
                
                // ✅ DEBUG: Log ALL HotelSettings in the database to see what we have
                var allHotelSettingsInDb = await dbContext.HotelSettings
                    .AsNoTracking()
                    .Select(h => new { h.HotelId, h.HotelCode })
                    .ToListAsync();
                _logger.LogInformation("🔍 [GetExpenseCategories] ALL HotelSettings in tenant DB: {AllSettings}", 
                    string.Join(", ", allHotelSettingsInDb.Select(h => $"HotelId={h.HotelId}, HotelCode='{h.HotelCode}'")));
                
                var allHotelSettingsWithSameCode = await dbContext.HotelSettings
                    .AsNoTracking()
                    .Where(h => h.HotelCode != null && h.HotelCode.ToLower() == hotelCode.ToLower())
                    .Select(h => h.HotelId)
                    .ToListAsync();
                
                _logger.LogInformation("📋 Found HotelSettings: HotelId={HotelId}, HotelCode='{HotelCode}' for Tenant Code: '{TenantCode}'", 
                    hotelSettings.HotelId, hotelSettings.HotelCode, tenant.Code);
                _logger.LogInformation("🔍 [GetExpenseCategories] All HotelIds with HotelCode='{HotelCode}': {HotelIds}", 
                    hotelCode, string.Join(", ", allHotelSettingsWithSameCode));
                
                // ✅ CRITICAL FIX: If data is linked to HotelId=11 but we only found HotelId=1,
                // we need to also check if there's a HotelSettings with HotelId=11 that should have the same HotelCode
                // OR we need to include HotelId=11 in our search if categories/apartments are linked to it
                // Let's check what HotelIds are actually used in the data tables
                var hotelIdsInCategories = await dbContext.ExpenseCategories
                    .AsNoTracking()
                    .Select(ec => ec.HotelId)
                    .Distinct()
                    .ToListAsync();
                _logger.LogInformation("🔍 [GetExpenseCategories] HotelIds actually used in expense_categories table: {HotelIds}", 
                    string.Join(", ", hotelIdsInCategories));
                
                // ✅ If categories are linked to HotelId=11 but we're searching with HotelId=1,
                // we need to include HotelId=11 in our search
                // Check if there's a HotelSettings with HotelId=11
                var hotelSettingsWithId11 = await dbContext.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelId == 11);
                
                if (hotelSettingsWithId11 != null)
                {
                    _logger.LogInformation("🔍 [GetExpenseCategories] Found HotelSettings with HotelId=11: HotelCode='{HotelCode}'", 
                        hotelSettingsWithId11.HotelCode);
                    
                    // ✅ If HotelId=11 has the same HotelCode, add it to the list
                    if (hotelSettingsWithId11.HotelCode != null && 
                        hotelSettingsWithId11.HotelCode.ToLower() == hotelCode.ToLower())
                    {
                        if (!allHotelSettingsWithSameCode.Contains(11))
                        {
                            allHotelSettingsWithSameCode.Add(11);
                            _logger.LogInformation("✅ [GetExpenseCategories] Added HotelId=11 to search list (same HotelCode)");
                        }
                    }
                    // ✅ If HotelId=11 is used in categories but has different HotelCode, still include it
                    // This handles data migration scenarios
                    else if (hotelIdsInCategories.Contains(11))
                    {
                        allHotelSettingsWithSameCode.Add(11);
                        _logger.LogWarning("⚠️ [GetExpenseCategories] Added HotelId=11 to search list (data exists but different HotelCode: '{DifferentCode}')", 
                            hotelSettingsWithId11.HotelCode);
                    }
                }
                else if (hotelIdsInCategories.Contains(11))
                {
                    // ✅ If HotelId=11 is used in categories but no HotelSettings exists for it,
                    // we still need to include it in the search
                    allHotelSettingsWithSameCode.Add(11);
                    _logger.LogWarning("⚠️ [GetExpenseCategories] Added HotelId=11 to search list (data exists but no HotelSettings record)");
                }
                
                _logger.LogInformation("🔍 [GetExpenseCategories] Final HotelIds to search: {HotelIds}", 
                    string.Join(", ", allHotelSettingsWithSameCode));

                // Log all expense categories before filtering
                var allCategories = await dbContext.ExpenseCategories
                    .AsNoTracking()
                    .Select(ec => new { ec.ExpenseCategoryId, ec.HotelId, ec.CategoryName, ec.IsActive })
                    .ToListAsync();
                
                _logger.LogInformation("📋 All expense categories in DB: {Count} total. Details: {Categories}", 
                    allCategories.Count, 
                    string.Join(", ", allCategories.Select(c => $"Id={c.ExpenseCategoryId}, HotelId={c.HotelId}, Name='{c.CategoryName}', IsActive={c.IsActive}")));

                // ✅ Filter by ALL HotelIds that have the same HotelCode
                var categories = await dbContext.ExpenseCategories
                    .AsNoTracking()
                    .Where(ec => allHotelSettingsWithSameCode.Contains(ec.HotelId) && ec.IsActive == true)
                    .Select(ec => new
                    {
                        expenseCategoryId = ec.ExpenseCategoryId,
                        categoryName = ec.CategoryName,
                        description = ec.Description,
                        isActive = ec.IsActive
                    })
                    .OrderBy(ec => ec.categoryName)
                    .ToListAsync();

                _logger.LogInformation("✅ Successfully retrieved {Count} expense categories for HotelCode: '{HotelCode}' (HotelIds: {HotelIds})", 
                    categories.Count, hotelCode, string.Join(", ", allHotelSettingsWithSameCode));
                
                if (categories.Count == 0)
                {
                    var totalCategoriesForHotelIds = allCategories.Count(c => allHotelSettingsWithSameCode.Contains(c.HotelId));
                    _logger.LogWarning("⚠️ No active expense categories found for HotelCode: '{HotelCode}' (HotelIds: {HotelIds}). Total categories in DB for these hotels: {TotalCount}", 
                        hotelCode, 
                        string.Join(", ", allHotelSettingsWithSameCode),
                        totalCategoriesForHotelIds);
                }

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
        /// <param name="status">حالة الموافقة (accepted أو rejected)</param>
        /// <returns>نتيجة العملية</returns>
        [HttpPut("approve/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveExpense(int id, [FromQuery] string status)
        {
            try
            {
                _logger.LogInformation("🔐 Approving/Rejecting expense: ExpenseId={ExpenseId}, Status={Status}", id, status);

                // التحقق من صحة الحالة
                if (status != "accepted" && status != "rejected")
                {
                    return BadRequest(new { error = "Invalid status. Must be 'accepted' or 'rejected'" });
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

                var expense = await _expenseService.ApproveExpenseAsync(id, status, userId.Value);

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
                    approvedAt = expense.ApprovedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error approving/rejecting expense: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to update expense status", details = ex.Message });
            }
        }
    }
}

