using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for config response via Zaaer integration
    /// </summary>
    public class ZaaerConfigResponseDto
    {
        /// <summary>
        /// Config ID
        /// </summary>
        public int ConfigId { get; set; }

        /// <summary>
        /// Hotel ID
        /// </summary>
        public int HotelId { get; set; }

        /// <summary>
        /// VAT percentage
        /// </summary>
        public decimal VatPercent { get; set; }

        /// <summary>
        /// Lodging tax percentage
        /// </summary>
        public decimal LodgingTax { get; set; }

        /// <summary>
        /// Default currency
        /// </summary>
        public string DefaultCurrency { get; set; } = string.Empty;

        /// <summary>
        /// Company name
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Company VAT number
        /// </summary>
        public string? CompanyVatNo { get; set; }

        /// <summary>
        /// Company CRN (Commercial Registration Number)
        /// </summary>
        public string? CompanyCrn { get; set; }

        /// <summary>
        /// Company address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Company phone
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Company email
        /// </summary>
        public string? Email { get; set; }
    }
}
