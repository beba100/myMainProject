using System.Security.Claims;

namespace zaaerIntegration.Services.Interfaces
{
    /// <summary>
    /// Interface for JWT Service
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// إنشاء JWT Token
        /// </summary>
        string GenerateToken(int userId, string username, int tenantId, IEnumerable<string> roles);

        /// <summary>
        /// التحقق من صحة Token
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);
    }
}

