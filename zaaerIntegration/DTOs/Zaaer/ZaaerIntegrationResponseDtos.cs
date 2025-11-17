using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    public class ZaaerCreateIntegrationResponseDto
    {
        [Required]
        public int HotelId { get; set; }
        [MaxLength(100)] public string? ResNo { get; set; }
        [Required, MaxLength(50)] public string Service { get; set; } = string.Empty; // NTMP | Shomoos
        [MaxLength(100)] public string? EventType { get; set; }
        [MaxLength(100)] public string? UnitNumber { get; set; }
        [MaxLength(200)] public string? Guest { get; set; }
        [MaxLength(1000)] public string? ErrorMessage { get; set; }
        [Required, MaxLength(20)] public string Status { get; set; } = "Success"; // Success | Error

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }

    public class ZaaerIntegrationResponseDto
    {
        public int HotelId { get; set; }
        public int ResponseId { get; set; }
        public string? ResNo { get; set; }
        public string Service { get; set; } = string.Empty;
        public string? EventType { get; set; }
        public string? UnitNumber { get; set; }
        public string? Guest { get; set; }
        public string? ErrorMessage { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }

    public class ZaaerIntegrationResponseQuery
    {
        public int? HotelId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? ResNo { get; set; }
        public string? Service { get; set; } // NTMP | Shomoos
        public string? EventType { get; set; }
        public string? Status { get; set; } // Success | Error
        public int? Take { get; set; }
        public int? Skip { get; set; }
    }
}


