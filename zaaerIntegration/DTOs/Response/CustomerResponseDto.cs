namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// Customer Response DTO
    /// äãæĞÌ ÇÓÊÌÇÈÉ ÇáÚãíá
    /// </summary>
    public class CustomerResponseDto
    {
        public int CustomerId { get; set; }
        public string? CustomerNo { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int? GtypeId { get; set; }
        public int? NId { get; set; }
        public int? IdType { get; set; }
        public int? GuestCategoryId { get; set; }
        public string? VisaNo { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Comments { get; set; }
        public int? EnteredBy { get; set; }
        public DateTime? EnteredAt { get; set; }
        public string? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? BirthdateGregorian { get; set; }
        public DateTime? BirthdateHijri { get; set; }

        // Related data
        public string? GuestTypeName { get; set; }
        public string? NationalityName { get; set; }
        public string? IdTypeName { get; set; }
        public string? GuestCategoryName { get; set; }
    }
}
