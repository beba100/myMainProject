using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Auth
{
    /// <summary>
    /// DTO لطلب تسجيل الدخول
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// اسم المستخدم
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// كلمة المرور
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}

