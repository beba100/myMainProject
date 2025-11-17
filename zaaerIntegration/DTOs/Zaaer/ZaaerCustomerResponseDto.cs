namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// Response DTO for customer data via Zaaer integration
    /// </summary>
    public class ZaaerCustomerResponseDto
    {
        /// <summary>
        /// Customer ID
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Customer number
        /// </summary>
        public string CustomerNo { get; set; } = string.Empty;

        /// <summary>
        /// Customer name
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Guest type ID
        /// </summary>
        public int? GtypeId { get; set; }

        /// <summary>
        /// National ID
        /// </summary>
        public int? NId { get; set; }


        /// <summary>
        /// Guest category ID
        /// </summary>
        public int? GuestCategoryId { get; set; }

        /// <summary>
        /// Visa number
        /// </summary>
        public string? VisaNo { get; set; }

        /// <summary>
        /// Mobile number
        /// </summary>
        public string? MobileNo { get; set; }

        /// <summary>
        /// Entered by user ID
        /// </summary>
        public int? EnteredBy { get; set; }

        /// <summary>
        /// Entered at date
        /// </summary>
        public DateTime? EnteredAt { get; set; }

        /// <summary>
        /// Birthdate in Hijri calendar (stored as string in database)
        /// </summary>
        public string? BirthdateHijri { get; set; }

        /// <summary>
        /// Hotel ID
        /// </summary>
        public int HotelId { get; set; }

        /// <summary>
        /// Created at date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated at date
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Birthdate in Gregorian calendar
        /// </summary>
        public DateTime? BirthdateGregorian { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        /// <summary>
        /// List of customer identifications
        /// </summary>
        public List<ZaaerCustomerIdentificationResponseDto> Identifications { get; set; } = new List<ZaaerCustomerIdentificationResponseDto>();
    }
}
