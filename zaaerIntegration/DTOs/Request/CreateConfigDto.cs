using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new configuration
    /// </summary>
    public class CreateConfigDto
    {
        [Required]
        public int HotelId { get; set; }

        [Range(0, 100, ErrorMessage = "VAT percentage must be between 0 and 100")]
        public decimal VatPercent { get; set; } = 15.00M;

        [Range(0, 100, ErrorMessage = "Lodging tax must be between 0 and 100")]
        public decimal LodgingTax { get; set; } = 2.50M;

        [Required]
        [StringLength(10)]
        public string DefaultCurrency { get; set; } = "SAR";

        [StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        public string? CompanyVatNo { get; set; }

        [StringLength(50)]
        public string? ZatcaEnvironment { get; set; }

        [StringLength(100)]
        public string? ZatcaDeviceUuid { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
