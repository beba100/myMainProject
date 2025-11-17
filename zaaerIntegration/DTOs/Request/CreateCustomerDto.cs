using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// Create Customer DTO
    /// نموذج إنشاء العميل
    /// </summary>
    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Customer number cannot exceed 50 characters")]
        public string? CustomerNo { get; set; }

        public int? GtypeId { get; set; }  // Guest Type
        public int? NId { get; set; }     // Nationality ID
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

        /// <summary>
        /// List of customer identifications
        /// </summary>
        public List<CustomerIdentificationDto> Identifications { get; set; } = new List<CustomerIdentificationDto>();
    }

    /// <summary>
    /// Customer Identification DTO
    /// </summary>
    public class CustomerIdentificationDto
    {
        [Required]
        public int IdTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string IdNumber { get; set; } = string.Empty;

        [StringLength(10)]
        public string? VersionNumber { get; set; }

        [StringLength(100)]
        public string? IssuePlace { get; set; }

        [StringLength(100)]
        public string? IssuePlaceAr { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
