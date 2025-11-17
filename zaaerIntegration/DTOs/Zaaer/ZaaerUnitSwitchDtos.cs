using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    public class ZaaerSwitchUnitRequestDto
    {
        [Required] public int ReservationId { get; set; }
        [Required] public int UnitId { get; set; }              // ReservationUnit.unit_id
        [Required] public int ToApartmentId { get; set; }       // New apartment to move to
        [Required] [MaxLength(30)] public string ApplyMode { get; set; } = "SamePrice"; // SamePrice | NewFromToday | NewForAllDays
        public DateTime? EffectiveDate { get; set; }
        [MaxLength(500)] public string? Comment { get; set; }
    }

    public class ZaaerUnitSwitchResponseDto
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
    }
}


