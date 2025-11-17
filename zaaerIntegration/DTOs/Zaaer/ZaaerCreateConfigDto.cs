using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for creating a config via Zaaer integration
    /// </summary>
    public class ZaaerCreateConfigDto
    {
        /// <summary>
        /// Hotel ID
        /// </summary>
        [Required]
        public int HotelId { get; set; }

        /// <summary>
        /// VAT percentage
        /// </summary>
        public decimal VatPercent { get; set; } = 15.00M;

        /// <summary>
        /// Lodging tax percentage
        /// </summary>
        public decimal LodgingTax { get; set; } = 2.50M;

        /// <summary>
        /// Default currency
        /// </summary>
        [Required]
        [StringLength(10)]
        public string DefaultCurrency { get; set; } = "SAR";

        /// <summary>
        /// Company name
        /// </summary>
        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Company VAT number
        /// </summary>
        [StringLength(50)]
        public string? CompanyVatNo { get; set; }

        /// <summary>
        /// Company CRN (Commercial Registration Number)
        /// </summary>
        [StringLength(50)]
        public string? CompanyCrn { get; set; }

        /// <summary>
        /// Company address
        /// </summary>
        [StringLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Company phone
        /// </summary>
        [StringLength(50)]
        public string? Phone { get; set; }

        /// <summary>
        /// Company email
        /// </summary>
        [StringLength(100)]
        public string? Email { get; set; }
    }
}
