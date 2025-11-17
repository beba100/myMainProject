namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning configuration data
    /// </summary>
    public class ConfigResponseDto
    {
        public int ConfigId { get; set; }
        public int HotelId { get; set; }
        public decimal VatPercent { get; set; }
        public decimal LodgingTax { get; set; }
        public string DefaultCurrency { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? CompanyVatNo { get; set; }
        public string? ZatcaEnvironment { get; set; }
        public string? ZatcaDeviceUuid { get; set; }
        public string? LogoUrl { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        // Related entity names for display
        public string? HotelName { get; set; }
    }
}
