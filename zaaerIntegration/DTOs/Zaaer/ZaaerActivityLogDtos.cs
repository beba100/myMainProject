using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    public class ZaaerCreateActivityLogDto
    {
        [Required]
        public int HotelId { get; set; }
        [Required, MaxLength(100)]
        public string EventKey { get; set; } = string.Empty;
        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
        public int? ReservationId { get; set; }
        public int? UnitId { get; set; }
        [MaxLength(50)] public string? RefType { get; set; }
        public int? RefId { get; set; }
        [MaxLength(100)] public string? RefNo { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        [MaxLength(200)] public string? CreatedBy { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }

    public class ZaaerActivityLogResponseDto
    {
        public int LogId { get; set; }
        public int HotelId { get; set; }
        public string EventKey { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? ReservationId { get; set; }
        public int? UnitId { get; set; }
        public string? RefType { get; set; }
        public int? RefId { get; set; }
        public string? RefNo { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }

    public class ZaaerActivityLogQuery
    {
        public int? HotelId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? ReservationNo { get; set; } // if available from message/ref
        public string? EventKey { get; set; }
        public int? ReservationId { get; set; }
        public int? UnitId { get; set; }
        public int? Take { get; set; }
        public int? Skip { get; set; }
    }
}


