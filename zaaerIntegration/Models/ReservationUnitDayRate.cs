using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    [Table("reservation_unit_day_rates")]
    public class ReservationUnitDayRate
    {
        [Key]
        [Column("rate_id")]
        public int RateId { get; set; }

        [Column("reservation_id")]
        [Required]
        public int ReservationId { get; set; }

        [Column("unit_id")]
        [Required]
        public int UnitId { get; set; }

        [Column("night_date")]
        [Required]
        public DateTime NightDate { get; set; }

        [Column("gross_rate", TypeName = "decimal(12,2)")]
        [Required]
        public decimal GrossRate { get; set; }

        [Column("ewa_amount", TypeName = "decimal(12,2)")]
        public decimal? EwaAmount { get; set; }

        [Column("vat_amount", TypeName = "decimal(12,2)")]
        public decimal? VatAmount { get; set; }

        [Column("net_amount", TypeName = "decimal(12,2)")]
        public decimal? NetAmount { get; set; }

        [Column("is_manual")]
        public bool IsManual { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = KsaTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

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


