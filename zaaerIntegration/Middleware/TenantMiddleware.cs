using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Middleware
{
    /// <summary>
    /// Middleware ������ �� ���� X-Hotel-Code Header ��������
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            // ���� ������ �� Tenant ��� endpoints �������:
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var pathUpper = context.Request.Path.Value ?? "";
            
            // ������ �� Swagger, Health checks, static files, � Tenant Management ���� tenant
            if (path.Contains("/swagger") || 
                path.Contains("/health") || 
                path.Contains("/_framework") ||
                path.Contains("/css") ||
                path.Contains("/js") ||
                path.Contains("/api/tenant") ||
                path.Contains("/api/auth") ||
                path.Contains("/api/masteruser", StringComparison.OrdinalIgnoreCase) ||  // ✅ Allow MasterUser endpoints without hotel code (uses Master DB)    // ✅ Allow authentication endpoints without hotel code  // ? ������ ���� ����� ������� ���� hotel code
                path == "/" ||
                path == "/index.html" ||
                path == "/queue.html" ||
                path == "/devextreme.html" ||  // ? ������ ������� ��� ���� DevExtreme ���� hotel code
                path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||  // ? ������ ����� ����� HTML
                // ? Allow access to static files (file uploads/downloads) without X-Hotel-Code header
                IsStaticFile(path) ||
                IsFileUploadPath(pathUpper))
            {
                await _next(context);
                return;
            }

            try
            {
                // ������ ������ ��� ��� Tenant
                var tenant = tenantService.GetTenant();
                
                if (tenant != null)
                {
                    _logger.LogInformation("Request authenticated for tenant: {TenantCode}", tenant.Code);
                }
                
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
                
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    message = ex.Message,
                    hint = "Please provide 'X-Hotel-Code' header with a valid hotel code (e.g., Dammam1)"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tenant not found: {Message}", ex.Message);
                
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Not Found",
                    message = ex.Message,
                    hint = "The hotel code you provided does not exist in the Master Database. Please verify the hotel code."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation in TenantMiddleware: {Message}", ex.Message);
                
                // Check if it's a database connection error
                if (ex.Message.Contains("Master Database", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("DatabaseName", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Database Configuration Error",
                        message = ex.Message,
                        hint = "Please check Master Database connection or tenant configuration in Master DB"
                    });
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Invalid Operation",
                        message = ex.Message,
                        hint = "An error occurred while processing your request"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in TenantMiddleware: {Message}", ex.Message);
                
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal Server Error",
                    message = "An unexpected error occurred while processing your request",
                    hint = "Please contact support if this issue persists"
                });
            }
        }

        /// <summary>
        /// Check if the request is for a static file (based on file extension)
        /// </summary>
        private static bool IsStaticFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // Common static file extensions
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

        /// <summary>
        /// Check if the request is for file upload/download paths (e.g., wwwroot subdirectories)
        /// </summary>
        private static bool IsFileUploadPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // Allow access to wwwroot subdirectories
            // This includes paths like: /Old Exe(RHotel_n)/Debug.zip
            var fileUploadPaths = new[]
            {
                "/Exe(RHotel_n)/",
                "/uploads",
                "/files",
                "/downloads",
                "/temp",
                "/images",
                "/documents",
                "/assets"
            };

            // Check if path contains any of the upload paths
            return fileUploadPaths.Any(pattern => path.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Extension method ������ ������� Middleware
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}

