using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for creating hotel settings via Zaaer integration
    /// </summary>
    public class ZaaerCreateHotelSettingsDto
    {
        /// <summary>
        /// Hotel ID
        /// </summary>
        [Required]
        public int HotelId { get; set; }

        /// <summary>
        /// Hotel Code
        /// </summary>
        [StringLength(50)]
        public string? HotelCode { get; set; }

        /// <summary>
        /// Hotel Name
        /// </summary>
        [StringLength(50)]
        public string? HotelName { get; set; }

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
        /// Logo URL
        /// </summary>
        [StringLength(500)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Tax Number (رقم الضريبة)
        /// </summary>
        [StringLength(50)]
        public string? TaxNumber { get; set; }

        /// <summary>
        /// CR Number (رقم السجل التجاري)
        /// </summary>
        [StringLength(50)]
        public string? CrNumber { get; set; }

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

        /// <summary>
        /// Country Code (كود الدولة)
        /// </summary>
        [StringLength(10)]
        public string? CountryCode { get; set; }

        /// <summary>
        /// City (المدينة)
        /// </summary>
        [StringLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Contact Person (الشخص المسؤول)
        /// </summary>
        [StringLength(100)]
        public string? ContactPerson { get; set; }

        /// <summary>
        /// Company address
        /// </summary>
        [StringLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Latitude (خط العرض)
        /// </summary>
        [StringLength(50)]
        public string? Latitude { get; set; }

        /// <summary>
        /// Longitude (خط الطول)
        /// </summary>
        [StringLength(50)]
        public string? Longitude { get; set; }

        /// <summary>
        /// Enabled status (مفعل)
        /// </summary>
        public int Enabled { get; set; } = 1;

        /// <summary>
        /// Total Rooms (إجمالي الغرف)
        /// </summary>
        public int TotalRooms { get; set; } = 0;

        /// <summary>
        /// Property Type (نوع العقار)
        /// </summary>
        [StringLength(50)]
        public string? PropertyType { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
