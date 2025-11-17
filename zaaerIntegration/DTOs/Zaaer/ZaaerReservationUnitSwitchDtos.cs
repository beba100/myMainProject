using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    public class ZaaerCreateReservationUnitSwitchDto
    {
        [Required]
        public int ReservationId { get; set; }
        [Required]
        public int UnitId { get; set; }              // current reservation_unit (unit_id)
        [Required]
        public int FromApartmentId { get; set; }     // current apartment
        [Required]
        public int ToApartmentId { get; set; }       // target apartment
        [Required]
        [MaxLength(30)]
        public string ApplyMode { get; set; } = "SamePrice"; // SamePrice | NewFromToday | NewForAllDays
        public DateTime? EffectiveDate { get; set; }
        [MaxLength(500)]
        public string? Comment { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }

    public class ZaaerReservationUnitSwitchResponseDto
    {
        public int SwitchId { get; set; }
        public int ReservationId { get; set; }
        public int UnitId { get; set; }
        public int FromApartmentId { get; set; }
        public int ToApartmentId { get; set; }
        public string ApplyMode { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}


