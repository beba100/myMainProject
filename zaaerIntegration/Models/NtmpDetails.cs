using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// NTMP gateway credentials/settings per hotel
    /// </summary>
    [Table("ntmp_details")]
    public class NtmpDetails
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

        [MaxLength(300)]
        [Column("gateway_api_key")]
        public string? GatewayApiKey { get; set; }

        [MaxLength(150)]
        [Column("user_name")]
        public string? UserName { get; set; }

        [MaxLength(300)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

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


