using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// Shomoos gateway credentials/settings per hotel
    /// </summary>
    [Table("shomoos_details")]
    public class ShomoosDetails
    {
        [Key]
        [Column("details_id")]
        public int DetailsId { get; set; }

        [Required]
        [Column("hotel_id")]
        public int HotelId { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [MaxLength(150)]
        [Column("user_id")]
        public string? UserId { get; set; }

        [MaxLength(100)]
        [Column("branch_code")]
        public string? BranchCode { get; set; }

        [MaxLength(300)]
        [Column("branch_secret")]
        public string? BranchSecret { get; set; }

        [MaxLength(20)]
        [Column("language_code")]
        public string? LanguageCode { get; set; }

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

        [ForeignKey("HotelId")]
        public HotelSettings HotelSettings { get; set; } = null!;

    }
}


