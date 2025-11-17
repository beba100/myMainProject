using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    [Table("reservation_unit_swaps")]
    public class ReservationUnitSwitch
    {
        [Key]
        [Column("switch_id")]
        public int SwitchId { get; set; }

        [Column("reservation_id")]
        [Required]
        public int ReservationId { get; set; }

        [Column("unit_id")]
        [Required]
        public int UnitId { get; set; }

        [Column("from_apartment_id")]
        [Required]
        public int FromApartmentId { get; set; }

        [Column("to_apartment_id")]
        [Required]
        public int ToApartmentId { get; set; }

        [Column("apply_mode")]
        [MaxLength(30)]
        public string ApplyMode { get; set; } = "SamePrice"; // SamePrice | NewFromToday | NewForAllDays

        [Column("effective_date")]
        public DateTime? EffectiveDate { get; set; }

        [Column("comment")]
        [MaxLength(500)]
        public string? Comment { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = KsaTime.Now;

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        [Column("zaaer_id")]
        public int? ZaaerId { get; set; }

        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; } = null!;

        [ForeignKey("UnitId")]
        public ReservationUnit ReservationUnit { get; set; } = null!;
    }
}


