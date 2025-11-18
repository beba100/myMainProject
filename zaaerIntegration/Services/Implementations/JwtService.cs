using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service for JWT Token operations
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        /// <summary>
        /// Constructor for JwtService
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="logger">Logger</param>
        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _secretKey = _configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";
            _issuer = _configuration["Jwt:Issuer"] ?? "ZaaerIntegration";
            _audience = _configuration["Jwt:Audience"] ?? "ZaaerIntegration";
            _expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "1440"); // 24 hours default

            if (string.IsNullOrWhiteSpace(_secretKey) || _secretKey.Length < 32)
            {
                _logger.LogWarning("JWT SecretKey is too short or empty. Using default key. Please set Jwt:SecretKey in appsettings.json");
            }
        }

        /// <summary>
        /// إنشاء JWT Token
        /// </summary>
        public string GenerateToken(int userId, string username, int tenantId, IEnumerable<string> roles)
        {
            // ✅ التحقق من TenantId (مطلوب)
            if (tenantId <= 0)
            {
                _logger.LogError("❌ Cannot generate token: Invalid TenantId. UserId: {UserId}, TenantId: {TenantId}", 
                    userId, tenantId);
                throw new ArgumentException("TenantId must be greater than 0", nameof(tenantId));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim("userId", userId.ToString()),
                new Claim("tenantId", tenantId.ToString()), // ✅ TenantId مطلوب في Token
                new Claim("username", username)
            };

            // إضافة الأدوار
            if (roles != null && roles.Any())
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                // إضافة roles كـ comma-separated string للسهولة
                claims.Add(new Claim("roles", string.Join(",", roles)));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogDebug("JWT Token generated for user: {Username}, TenantId: {TenantId}", username, tenantId);

            return tokenString;
        }

        /// <summary>
        /// التحقق من صحة Token
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }
    }
}

