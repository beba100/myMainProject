using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Auth;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller للتعامل مع Authentication
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMasterUserService _masterUserService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IMasterUserService masterUserService,
            IJwtService jwtService,
            ILogger<AuthController> logger)
        {
            _masterUserService = masterUserService ?? throw new ArgumentNullException(nameof(masterUserService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// تسجيل الدخول
        /// </summary>
        /// <param name="request">بيانات تسجيل الدخول</param>
        /// <returns>JWT Token وبيانات المستخدم</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Invalid request", details = ModelState });
            }

            try
            {
                // ✅ 1. التحقق من بيانات تسجيل الدخول (يجب أن يكون أول شيء)
                var user = await _masterUserService.ValidateLoginAsync(request.Username, request.Password);

                // ✅ 2. التحقق من المستخدم (بعد التحقق من الباسورد)
                if (user == null)
                {
                    _logger.LogWarning("❌ Login failed: Invalid username or password. Username: {Username}", request.Username);
                    return Unauthorized(new { error = "Invalid username or password" });
                }

                // ✅ 3. التحقق من TenantId موجود قبل إنشاء Token
                if (user.TenantId <= 0)
                {
                    _logger.LogError("❌ Login failed: User has invalid TenantId. Username: {Username}, TenantId: {TenantId}", 
                        user.Username, user.TenantId);
                    return Unauthorized(new { error = "User account configuration error" });
                }

                // ✅ 4. الحصول على أدوار المستخدم (لا يمنع الدخول إذا كان فارغاً)
                var roles = await _masterUserService.GetUserRolesAsync(user.Id);
                var rolesList = roles.ToList();

                // ✅ 5. إنشاء JWT Token مع TenantId (مطلوب)
                var token = _jwtService.GenerateToken(
                    user.Id,
                    user.Username,
                    user.TenantId, // ✅ TenantId من المستخدم نفسه (مطلوب)
                    rolesList
                );

                // إعادة البيانات
                var response = new LoginResponseDto
                {
                    Token = token,
                    UserId = user.Id,
                    Username = user.Username,
                    TenantId = user.TenantId,
                    TenantCode = user.Tenant?.Code ?? "",
                    TenantName = user.Tenant?.Name ?? "",
                    Roles = rolesList,
                    ExpiresAt = DateTime.UtcNow.AddHours(24) // 24 hours
                };

                _logger.LogInformation("✅ Login successful: Username={Username}, TenantId={TenantId}", 
                    user.Username, user.TenantId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
                return StatusCode(500, new { error = "An error occurred during login", message = ex.Message });
            }
        }

        /// <summary>
        /// التحقق من صحة Token (للاختبار)
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ValidateToken()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized(new { error = "No token provided" });
            }

            var principal = _jwtService.ValidateToken(token);
            
            if (principal == null)
            {
                return Unauthorized(new { error = "Invalid or expired token" });
            }

            var userId = principal.FindFirst("userId")?.Value;
            var tenantId = principal.FindFirst("tenantId")?.Value;
            var username = principal.FindFirst("username")?.Value;
            var roles = principal.FindFirst("roles")?.Value;

            return Ok(new
            {
                valid = true,
                userId,
                tenantId,
                username,
                roles = roles?.Split(',') ?? Array.Empty<string>()
            });
        }

        /// <summary>
        /// إنشاء Plain Text Password (للاختبار فقط - Development)
        /// ملاحظة: النظام يستخدم Plain Text passwords الآن (بدون تشفير)
        /// </summary>
        [HttpPost("generate-hash")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GeneratePasswordHash([FromBody] GenerateHashRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Password is required" });
            }

            try
            {
                // ✅ النظام يستخدم Plain Text - إرجاع كلمة المرور كما هي
                var password = _masterUserService.HashPassword(request.Password);
                
                _logger.LogInformation("✅ Password generated (plain text) for password: {Password}", request.Password);
                
                return Ok(new
                {
                    password = request.Password,
                    hash = password, // Plain text (same as password)
                    note = "System uses plain text passwords (no encryption)",
                    sqlUpdate = $"UPDATE MasterUsers SET PasswordHash = '{password}' WHERE Username = '{request.Username ?? "user1"}';"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password");
                return StatusCode(500, new { error = "Error generating password", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO لطلب إنشاء Hash
    /// </summary>
    public class GenerateHashRequestDto
    {
        public string Password { get; set; } = string.Empty;
        public string? Username { get; set; }
    }
}

