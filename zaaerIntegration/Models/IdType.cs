using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// ID Type Model
    /// نموذج نوع الهوية
    /// 
    /// Purpose: Defines identification document types (ID Card, Passport, etc.)
    /// Used for: Document validation, legal compliance, guest registration
    /// </summary>
    [Table("idtypes")]
    public class IdType
    {
        [Key]
        [Column("it_id")]
        public int ItId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("it_name")]
        public string ItName { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("it_name_ar")]
        public string? ItNameAr { get; set; }

        [Column("it_active")]
        public bool ItActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = KsaTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
