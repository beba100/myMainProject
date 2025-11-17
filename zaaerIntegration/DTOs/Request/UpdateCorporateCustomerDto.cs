using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for updating an existing corporate customer
    /// </summary>
    public class UpdateCorporateCustomerDto
    {
        [Required]
        public int CorporateId { get; set; }

        [Required]
        public int HotelId { get; set; }

        [Required]
        [StringLength(200)]
        public string CorporateName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? CorporateNameAr { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(100)]
        public string? CountryAr { get; set; }

        [StringLength(50)]
        public string? VatRegistrationNo { get; set; }

        [StringLength(50)]
        public string? CommercialRegistrationNo { get; set; }

        [StringLength(20)]
        public string? DiscountMethod { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount value must be a positive number")]
        public decimal? DiscountValue { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? CityAr { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(500)]
        public string? AddressAr { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? CorporatePhone { get; set; }

        [StringLength(100)]
        public string? ContactPersonName { get; set; }

        [StringLength(100)]
        public string? ContactPersonNameAr { get; set; }

        [StringLength(50)]
        public string? ContactPersonPhone { get; set; }

        [StringLength(500)]
        public string? CorporateLogoUrl { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
