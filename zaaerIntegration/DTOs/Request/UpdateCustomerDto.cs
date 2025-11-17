using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// Update Customer DTO
    /// نموذج تحديث العميل
    /// </summary>
    public class UpdateCustomerDto
    {
        [Required(ErrorMessage = "Customer ID is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Customer number cannot exceed 50 characters")]
        public string? CustomerNo { get; set; }

        public int? GtypeId { get; set; }  // Guest Type
        public int? NId { get; set; }     // Nationality ID
        public int? IdType { get; set; }  // ID Type
        public int? GuestCategoryId { get; set; }  // Guest Category

        [StringLength(50, ErrorMessage = "Visa number cannot exceed 50 characters")]
        public string? VisaNo { get; set; }

        [StringLength(20, ErrorMessage = "Mobile number cannot exceed 20 characters")]
        public string? MobileNo { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [StringLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters")]
        public string? Comments { get; set; }

        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        public string? Gender { get; set; }

        public DateTime? Birthday { get; set; }
        public DateTime? BirthdateGregorian { get; set; }
        public DateTime? BirthdateHijri { get; set; }
    }
}
