using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Expense;
using zaaerIntegration.Services.Expense;
using zaaerIntegration.Services.PartnerQueueing;
using zaaerIntegration.Services.Interfaces;
using zaaerIntegration.Models;
using System.Text.Json;

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

        /// <summary>
        /// Constructor for ExpenseController
        /// </summary>
        /// <param name="expenseService">Expense service</param>
        /// <param name="queueService">Partner queue service</param>
        /// <param name="queueSettings">Queue settings provider</param>
        /// <param name="dbContextResolver">Tenant database context resolver</param>
        /// <param name="tenantService">Tenant service</param>
        /// <param name="logger">Logger</param>
        public ExpenseController(
            IExpenseService expenseService,
            IPartnerQueueService queueService,
            IQueueSettingsProvider queueSettings,
            TenantDbContextResolver dbContextResolver,
            ITenantService tenantService,
            ILogger<ExpenseController> logger)
        {
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
            _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
            _queueSettings = queueSettings ?? throw new ArgumentNullException(nameof(queueSettings));
            _dbContextResolver = dbContextResolver ?? throw new ArgumentNullException(nameof(dbContextResolver));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                _logger.LogInformation("✅ Expense created successfully: ExpenseId={ExpenseId}", expense.ExpenseId);

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
                _logger.LogInformation("📋 Fetching expense categories for current hotel");

                var tenant = _tenantService.GetTenant();
                
                if (tenant == null)
                {
                    return Unauthorized(new { error = "Tenant not resolved. Please provide X-Hotel-Code header." });
                }

                var dbContext = _dbContextResolver.GetCurrentDbContext();

                // Get HotelSettings to get HotelId
                var hotelSettings = await dbContext.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelCode == tenant.Code);

                if (hotelSettings == null)
                {
                    return NotFound(new { error = $"HotelSettings not found for hotel code: {tenant.Code}" });
                }

                var categories = await dbContext.ExpenseCategories
                    .AsNoTracking()
                    .Where(ec => ec.HotelId == hotelSettings.HotelId && ec.IsActive)
                    .Select(ec => new
                    {
                        expenseCategoryId = ec.ExpenseCategoryId,
                        categoryName = ec.CategoryName,
                        description = ec.Description,
                        isActive = ec.IsActive
                    })
                    .OrderBy(ec => ec.categoryName)
                    .ToListAsync();

                _logger.LogInformation("✅ Successfully retrieved {Count} expense categories", categories.Count);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching expense categories: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch expense categories", details = ex.Message });
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
    }
}

