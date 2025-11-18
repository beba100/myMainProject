using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Middleware
{
    /// <summary>
    /// Middleware لحل المستخدم من JWT Token ووضع TenantId في HttpContext
    /// يتم تشغيله قبل TenantMiddleware
    /// </summary>
    public class MasterUserResolverMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MasterUserResolverMiddleware> _logger;

        public MasterUserResolverMiddleware(RequestDelegate next, ILogger<MasterUserResolverMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IJwtService jwtService)
        {
            // Skip authentication for public endpoints
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.Contains("/swagger") ||
                path.Contains("/health") ||
                path.Contains("/_framework") ||
                path.Contains("/css") ||
                path.Contains("/js") ||
                path.Contains("/api/auth/login") || // Allow login endpoint
                path == "/" ||
                path == "/index.html" ||
                path == "/login.html" ||
                path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                IsStaticFile(path))
            {
                await _next(context);
                return;
            }

            try
            {
                // قراءة Token من Authorization Header
                var token = ExtractTokenFromHeader(context);
                
                if (!string.IsNullOrWhiteSpace(token))
                {
                    // التحقق من صحة Token
                    var principal = jwtService.ValidateToken(token);
                    
                    if (principal != null)
                    {
                        // استخراج Claims
                        var userId = principal.FindFirst("userId")?.Value;
                        var tenantId = principal.FindFirst("tenantId")?.Value;
                        var username = principal.FindFirst("username")?.Value;
                        var roles = principal.FindFirst("roles")?.Value;

                        // وضع البيانات في HttpContext.Items
                        if (!string.IsNullOrWhiteSpace(userId))
                            context.Items["UserId"] = userId;

                        if (!string.IsNullOrWhiteSpace(tenantId))
                        {
                            context.Items["TenantId"] = tenantId;
                            
                            // وضع TenantId في Claims للاستخدام في Authorization
                            var identity = principal.Identity as ClaimsIdentity;
                            if (identity != null && !identity.HasClaim("tenantId", tenantId))
                            {
                                identity.AddClaim(new Claim("tenantId", tenantId));
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(username))
                            context.Items["Username"] = username;

                        if (!string.IsNullOrWhiteSpace(roles))
                            context.Items["Roles"] = roles;

                        // تعيين User في HttpContext للاستخدام في Authorization
                        context.User = principal;

                        _logger.LogDebug("User resolved from JWT: UserId={UserId}, TenantId={TenantId}, Username={Username}", 
                            userId, tenantId, username);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid or expired JWT token");
                    }
                }
                else
                {
                    // لا يوجد Token - سيتم التعامل معه في TenantMiddleware
                    _logger.LogDebug("No JWT token found in request");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MasterUserResolverMiddleware: {Message}", ex.Message);
                // لا نوقف الطلب - نترك TenantMiddleware يتعامل معه
            }

            await _next(context);
        }

        /// <summary>
        /// استخراج Token من Authorization Header
        /// </summary>
        private string? ExtractTokenFromHeader(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                return null;

            var authHeaderValue = authHeader.ToString();
            if (string.IsNullOrWhiteSpace(authHeaderValue))
                return null;

            // Format: "Bearer {token}"
            if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeaderValue.Substring("Bearer ".Length).Trim();
            }

            return null;
        }

        /// <summary>
        /// Check if the request is for a static file
        /// </summary>
        private static bool IsStaticFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var staticExtensions = new[]
            {
                ".zip", ".rar", ".7z", ".tar", ".gz",
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp",
                ".mp4", ".avi", ".mov", ".wmv", ".flv",
                ".mp3", ".wav", ".ogg", ".aac",
                ".txt", ".csv", ".json", ".xml",
                ".exe", ".msi", ".dmg", ".deb", ".rpm"
            };

            return staticExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Extension method لتسجيل Middleware
    /// </summary>
    public static class MasterUserResolverMiddlewareExtensions
    {
        public static IApplicationBuilder UseMasterUserResolverMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MasterUserResolverMiddleware>();
        }
    }
}

