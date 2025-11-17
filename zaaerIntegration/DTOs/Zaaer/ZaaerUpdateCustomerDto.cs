using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using zaaerIntegration.Converters;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for updating a customer via Zaaer integration
    /// </summary>
    public class ZaaerUpdateCustomerDto
    {
        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

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
        public int? NId { get; set; }


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
        [JsonConverter(typeof(NullableIntJsonConverter))]
        public int? EnteredBy { get; set; }

        /// <summary>
        /// Entered at date
        /// </summary>
        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? EnteredAt { get; set; }

        /// <summary>
        /// Birthdate in Hijri calendar (stored as string in database)
        /// </summary>
        public string? BirthdateHijri { get; set; }

        /// <summary>
        /// Birthdate in Gregorian calendar
        /// </summary>
        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? BirthdateGregorian { get; set; }

        /// <summary>
        /// List of customer identifications
        /// </summary>
        public List<ZaaerCustomerIdentificationDto>? Identifications { get; set; }
    }
}
