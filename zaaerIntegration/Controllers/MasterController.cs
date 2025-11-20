using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using zaaerIntegration.Data;
using Microsoft.EntityFrameworkCore;
using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for managing Master DB data (static/shared data)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MasterController : ControllerBase
    {
        private readonly MasterDbContext _masterDbContext;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MasterController> _logger;
        private const string EXPENSE_CATEGORIES_CACHE_KEY = "Master_ExpenseCategories";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(24); // Cache for 24 hours (static data)

        public MasterController(
            MasterDbContext masterDbContext, 
            IMemoryCache cache,
            ILogger<MasterController> logger)
        {
            _masterDbContext = masterDbContext ?? throw new ArgumentNullException(nameof(masterDbContext));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all expense categories from Master DB (cached for 24 hours)
        /// الحصول على جميع فئات المصروفات من قاعدة البيانات المركزية (مخزنة مؤقتاً لمدة 24 ساعة)
        /// </summary>
        /// <returns>List of expense categories</returns>
        [HttpGet("ExpenseCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<object>>> GetExpenseCategories()
        {
            try
            {
                _logger.LogInformation("📋 [Master/ExpenseCategories] Fetching expense categories from Master DB (with caching)");

                // Try to get from cache first
                if (_cache.TryGetValue(EXPENSE_CATEGORIES_CACHE_KEY, out List<object>? cachedCategories))
                {
                    _logger.LogInformation("✅ [Master/ExpenseCategories] Returning {Count} categories from cache", cachedCategories?.Count ?? 0);
                    return Ok(cachedCategories);
                }

                // Cache miss - fetch from database
                _logger.LogInformation("🔄 [Master/ExpenseCategories] Cache miss - fetching from database");

                var categories = await _masterDbContext.ExpenseCategories
                    .AsNoTracking()
                    .Where(ec => ec.IsActive)
                    .OrderBy(ec => ec.Id)
                    .Select(ec => new
                    {
                        id = ec.Id,
                        mainCategory = ec.MainCategory,
                        details = ec.Details,
                        categoryCode = ec.CategoryCode, // ✅ إضافة CategoryCode
                        isActive = ec.IsActive
                    })
                    .ToListAsync<object>();

                _logger.LogInformation("✅ [Master/ExpenseCategories] Successfully retrieved {Count} categories from database", categories.Count);

                // Store in cache with expiration
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CACHE_DURATION,
                    Priority = CacheItemPriority.High, // High priority since this is static data
                    SlidingExpiration = null // Use absolute expiration only (static data doesn't need sliding)
                };

                _cache.Set(EXPENSE_CATEGORIES_CACHE_KEY, categories, cacheOptions);
                _logger.LogInformation("💾 [Master/ExpenseCategories] Categories cached for {Hours} hours", CACHE_DURATION.TotalHours);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [Master/ExpenseCategories] Error fetching expense categories: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to fetch expense categories", details = ex.Message });
            }
        }

        /// <summary>
        /// Clear expense categories cache (admin function)
        /// مسح ذاكرة التخزين المؤقت لفئات المصروفات (وظيفة إدارية)
        /// </summary>
        [HttpDelete("ExpenseCategories/Cache")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ClearExpenseCategoriesCache()
        {
            try
            {
                _cache.Remove(EXPENSE_CATEGORIES_CACHE_KEY);
                _logger.LogInformation("🗑️ [Master/ExpenseCategories] Cache cleared successfully");
                return Ok(new { message = "Cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [Master/ExpenseCategories] Error clearing cache: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to clear cache", details = ex.Message });
            }
        }
    }
}

