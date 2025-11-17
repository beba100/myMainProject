using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for updating a config via Zaaer integration
    /// </summary>
    public class ZaaerUpdateConfigDto
    {
        /// <summary>
        /// VAT percentage
        /// </summary>
        public decimal? VatPercent { get; set; }

        /// <summary>
        /// Lodging tax percentage
        /// </summary>
        public decimal? LodgingTax { get; set; }

        /// <summary>
        /// Default currency
        /// </summary>
        [StringLength(10)]
        public string? DefaultCurrency { get; set; }

        /// <summary>
        /// Company name
        /// </summary>
        [StringLength(200)]
        public string? CompanyName { get; set; }

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
