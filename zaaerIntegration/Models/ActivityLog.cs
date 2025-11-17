using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    [Table("activity_logs")]
    public class ActivityLog
    {
        [Key]
        [Column("log_id")]
        public int LogId { get; set; }

        [Required]
        [Column("hotel_id")]
        public int HotelId { get; set; }

        [Column("event_key")]
        [MaxLength(100)]
        public string EventKey { get; set; } = string.Empty; // e.g., ReservationTotalChanged, UnitAdded, InvoiceCreated

        [Column("message")]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Column("reservation_id")]
        public int? ReservationId { get; set; }

        [Column("unit_id")]
        public int? UnitId { get; set; }

        [Column("ref_type")]
        [MaxLength(50)]
        public string? RefType { get; set; } // Invoice, Receipt, Refund, Penalty, etc.

        [Column("ref_id")]
        public int? RefId { get; set; }

        [Column("ref_no")]
        [MaxLength(100)]
        public string? RefNo { get; set; }

        [Column("amount_from", TypeName = "decimal(12,2)")]
        public decimal? AmountFrom { get; set; }

        [Column("amount_to", TypeName = "decimal(12,2)")]
        public decimal? AmountTo { get; set; }

        [Column("created_by")]
        [MaxLength(200)]
        public string? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = KsaTime.Now;

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        [Column("zaaer_id")]
        public int? ZaaerId { get; set; }
    }
}


