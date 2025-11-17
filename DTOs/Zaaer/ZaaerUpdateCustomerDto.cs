using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for updating a customer via Zaaer integration
    /// </summary>
    public class ZaaerUpdateCustomerDto
    {
        /// <summary>
        /// Customer number
        /// </summary>
        [StringLength(50)]
        public string? CustomerNo { get; set; }

        /// <summary>
        /// Customer name
        /// </summary>
        [StringLength(100)]
        public string? CustomerName { get; set; }

        /// <summary>
        /// Guest type ID
        /// </summary>
        public int? GtypeId { get; set; }

        /// <summary>
        /// National ID
        /// </summary>
        [StringLength(50)]
        public string? NId { get; set; }

        /// <summary>
        /// ID type
        /// </summary>
        public int? IdType { get; set; }

        /// <summary>
        /// Guest category ID
        /// </summary>
        public int? GuestCategoryId { get; set; }

        /// <summary>
        /// Visa number
        /// </summary>
        [StringLength(50)]
        public string? VisaNo { get; set; }

        /// <summary>
        /// Mobile number
        /// </summary>
        [StringLength(20)]
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
        /// Birthdate in Hijri calendar
        /// </summary>
        public DateTime? BirthdateHijri { get; set; }

        /// <summary>
        /// Birthdate in Gregorian calendar
        /// </summary>
        public DateTime? BirthdateGregorian { get; set; }

        /// <summary>
        /// List of customer identifications
        /// </summary>
        public List<ZaaerCustomerIdentificationDto>? Identifications { get; set; }
    }
}
