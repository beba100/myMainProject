using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for hotel settings response via Zaaer integration
    /// </summary>
    public class ZaaerHotelSettingsResponseDto
    {
        /// <summary>
        /// Hotel ID
        /// </summary>
        public int HotelId { get; set; }

        /// <summary>
        /// Hotel Code
        /// </summary>
        public string? HotelCode { get; set; }

        /// <summary>
        /// Hotel Name
        /// </summary>
        public string? HotelName { get; set; }

        /// <summary>
        /// Default currency
        /// </summary>
        public string? DefaultCurrency { get; set; }

        /// <summary>
        /// Company name
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// Logo URL
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Tax Number (رقم الضريبة)
        /// </summary>
        public string? TaxNumber { get; set; }

        /// <summary>
        /// CR Number (رقم السجل التجاري)
        /// </summary>
        public string? CrNumber { get; set; }

        /// <summary>
        /// Company phone
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Company email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Country Code (كود الدولة)
        /// </summary>
        public string? CountryCode { get; set; }

        /// <summary>
        /// City (المدينة)
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Contact Person (الشخص المسؤول)
        /// </summary>
        public string? ContactPerson { get; set; }

        /// <summary>
        /// Company address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Latitude (خط العرض)
        /// </summary>
        public string? Latitude { get; set; }

        /// <summary>
        /// Longitude (خط الطول)
        /// </summary>
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
        public string? PropertyType { get; set; }

        /// <summary>
        /// Created date
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
