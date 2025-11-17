namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning corporate customer data
    /// </summary>
    public class CorporateCustomerResponseDto
    {
        public int CorporateId { get; set; }
        public int HotelId { get; set; }
        public string CorporateName { get; set; } = string.Empty;
        public string? CorporateNameAr { get; set; }
        public string? Country { get; set; }
        public string? CountryAr { get; set; }
        public string? VatRegistrationNo { get; set; }
        public string? CommercialRegistrationNo { get; set; }
        public string? DiscountMethod { get; set; }
        public decimal? DiscountValue { get; set; }
        public string? City { get; set; }
        public string? CityAr { get; set; }
        public string? PostalCode { get; set; }
        public string? Address { get; set; }
        public string? AddressAr { get; set; }
        public string? Email { get; set; }
        public string? CorporatePhone { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonNameAr { get; set; }
        public string? ContactPersonPhone { get; set; }
        public string? CorporateLogoUrl { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Related entity names for display
        public string? HotelName { get; set; }
        public int TotalReservations { get; set; }
    }
}
