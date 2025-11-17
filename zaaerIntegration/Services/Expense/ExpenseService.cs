using FinanceLedgerAPI.Models;
using ExpenseModel = FinanceLedgerAPI.Models.Expense;
using ExpenseRoomModel = FinanceLedgerAPI.Models.ExpenseRoom;
using ExpenseCategoryModel = FinanceLedgerAPI.Models.ExpenseCategory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Expense;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Expense
{
    /// <summary>
    /// Service لإدارة النفقات (Expenses)
    /// يستخدم ITenantService للحصول على HotelId من X-Hotel-Code header
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly ILogger<ExpenseService> _logger;

        /// <summary>
        /// Constructor for ExpenseService
        /// </summary>
        /// <param name="context">Application database context</param>
        /// <param name="tenantService">Tenant service for getting current hotel</param>
        /// <param name="logger">Logger</param>
        public ExpenseService(
            ApplicationDbContext context,
            ITenantService tenantService,
            ILogger<ExpenseService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على HotelId من Tenant (يُقرأ من X-Hotel-Code header)
        /// </summary>
        private async Task<int> GetCurrentHotelIdAsync()
        {
            var tenant = _tenantService.GetTenant();
            if (tenant == null)
            {
                throw new InvalidOperationException("Tenant not resolved. Cannot get hotel ID.");
            }

            // البحث عن HotelSettings في Tenant DB باستخدام HotelCode
            var hotelSettings = await _context.HotelSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.HotelCode == tenant.Code);

            if (hotelSettings == null)
            {
                _logger.LogError("HotelSettings not found for tenant code: {TenantCode} in Tenant DB", tenant.Code);
                throw new InvalidOperationException(
                    $"HotelSettings not found for hotel code: {tenant.Code}. " +
                    "Please ensure hotel settings are configured in the tenant database.");
            }

            return hotelSettings.HotelId;
        }

        /// <summary>
        /// الحصول على جميع النفقات للفندق الحالي
        /// </summary>
        public async Task<IEnumerable<ExpenseResponseDto>> GetAllAsync()
        {
            var hotelId = await GetCurrentHotelIdAsync();

            var expenses = await _context.Expenses
                .AsNoTracking()
                .Include(e => e.ExpenseCategory)
                .Include(e => e.ExpenseRooms)
                    .ThenInclude(er => er.Apartment)
                .Where(e => e.HotelId == hotelId)
                .OrderByDescending(e => e.DateTime)
                .ToListAsync();

            return expenses.Select(e => MapToDto(e));
        }

        /// <summary>
        /// الحصول على نفقة محددة بالمعرف
        /// </summary>
        public async Task<ExpenseResponseDto?> GetByIdAsync(int id)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            var expense = await _context.Expenses
                .AsNoTracking()
                .Include(e => e.ExpenseCategory)
                .Include(e => e.ExpenseRooms)
                    .ThenInclude(er => er.Apartment)
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.HotelId == hotelId);

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

            var expense = new ExpenseModel
            {
                HotelId = hotelId,
                DateTime = dto.DateTime,
                Comment = dto.Comment,
                ExpenseCategoryId = dto.ExpenseCategoryId,
                TaxRate = dto.TaxRate,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                CreatedAt = DateTime.Now
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            // إضافة expense_rooms إذا وُجدت
            if (dto.ExpenseRooms != null && dto.ExpenseRooms.Any())
            {
                foreach (var roomDto in dto.ExpenseRooms)
                {
                    // التحقق من أن Apartment موجود في نفس الفندق
                    var apartment = await _context.Apartments
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.ApartmentId == roomDto.ApartmentId && a.HotelId == hotelId);

                    if (apartment == null)
                    {
                        _logger.LogWarning("Apartment {ApartmentId} not found for hotel {HotelId}", 
                            roomDto.ApartmentId, hotelId);
                        continue; // Skip invalid apartment
                    }

                    var expenseRoom = new ExpenseRoomModel
                    {
                        ExpenseId = expense.ExpenseId,
                        ApartmentId = roomDto.ApartmentId,
                        Purpose = roomDto.Purpose,
                        CreatedAt = DateTime.Now
                    };

                    _context.ExpenseRooms.Add(expenseRoom);
                }

                await _context.SaveChangesAsync();
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

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.HotelId == hotelId);

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
            if (dto.TaxRate.HasValue)
                expense.TaxRate = dto.TaxRate;
            if (dto.TaxAmount.HasValue)
                expense.TaxAmount = dto.TaxAmount;
            if (dto.TotalAmount.HasValue)
                expense.TotalAmount = dto.TotalAmount.Value;

            expense.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

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
                _context.ExpenseRooms.RemoveRange(expense.ExpenseRooms);
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

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
            var expense = await _context.Expenses
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelId);

            if (expense == null)
            {
                throw new KeyNotFoundException($"Expense with id {expenseId} not found");
            }

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
            var expense = await _context.Expenses
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.HotelId == hotelId);

            if (expense == null)
            {
                throw new KeyNotFoundException($"Expense with id {expenseId} not found");
            }

            // التحقق من أن Apartment موجود في نفس الفندق
            var apartment = await _context.Apartments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ApartmentId == dto.ApartmentId && a.HotelId == hotelId);

            if (apartment == null)
            {
                throw new KeyNotFoundException($"Apartment with id {dto.ApartmentId} not found");
            }

            var expenseRoom = new ExpenseRoomModel
            {
                ExpenseId = expenseId,
                ApartmentId = dto.ApartmentId,
                Purpose = dto.Purpose,
                CreatedAt = DateTime.Now
            };

            _context.ExpenseRooms.Add(expenseRoom);
            await _context.SaveChangesAsync();

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
                var apartment = await _context.Apartments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ApartmentId == dto.ApartmentId.Value && a.HotelId == hotelId);

                if (apartment == null)
                {
                    throw new KeyNotFoundException($"Apartment with id {dto.ApartmentId.Value} not found");
                }

                expenseRoom.ApartmentId = dto.ApartmentId.Value;
            }

            if (dto.Purpose != null)
                expenseRoom.Purpose = dto.Purpose;

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ ExpenseRoom updated successfully: ExpenseRoomId={ExpenseRoomId}", expenseRoomId);

            return await MapExpenseRoomToDtoWithLoadAsync(expenseRoomId);
        }

        /// <summary>
        /// حذف expense_room
        /// </summary>
        public async Task<bool> DeleteExpenseRoomAsync(int expenseRoomId)
        {
            var hotelId = await GetCurrentHotelIdAsync();

            var expenseRoom = await _context.ExpenseRooms
                .Include(er => er.Expense)
                .FirstOrDefaultAsync(er => er.ExpenseRoomId == expenseRoomId);

            if (expenseRoom == null || expenseRoom.Expense.HotelId != hotelId)
            {
                return false;
            }

            _context.ExpenseRooms.Remove(expenseRoom);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ ExpenseRoom deleted successfully: ExpenseRoomId={ExpenseRoomId}", expenseRoomId);

            return true;
        }

        /// <summary>
        /// تحويل Expense إلى ExpenseResponseDto
        /// </summary>
        private ExpenseResponseDto MapToDto(ExpenseModel expense)
        {
            return new ExpenseResponseDto
            {
                ExpenseId = expense.ExpenseId,
                HotelId = expense.HotelId,
                DateTime = expense.DateTime,
                Comment = expense.Comment,
                ExpenseCategoryId = expense.ExpenseCategoryId,
                ExpenseCategoryName = expense.ExpenseCategory?.CategoryName,
                TaxRate = expense.TaxRate,
                TaxAmount = expense.TaxAmount,
                TotalAmount = expense.TotalAmount,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt,
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

