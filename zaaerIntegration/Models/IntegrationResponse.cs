using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// Stores external integration responses (e.g., NTMP, Shomoos)
    /// </summary>
    [Table("integration_responses")]
    public class IntegrationResponse
    {
        [Key]
        [Column("response_id")]
        public int ResponseId { get; set; }

        [Required]
        [Column("hotel_id")]
        public int HotelId { get; set; }

        [MaxLength(100)]
        [Column("res_no")]
        public string? ResNo { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("service")] // NTMP | Shomoos
        public string Service { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("event_type")]
        public string? EventType { get; set; }

        [MaxLength(100)]
        [Column("unit_number")]
        public string? UnitNumber { get; set; }

        [MaxLength(200)]
        [Column("guest")]
        public string? Guest { get; set; }

        [MaxLength(1000)]
        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")] // Success | Error
        public string Status { get; set; } = "Success";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = KsaTime.Now;

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        [Column("zaaer_id")]
        public int? ZaaerId { get; set; }

        [ForeignKey("HotelId")]
        public HotelSettings HotelSettings { get; set; } = null!;

    }
}


