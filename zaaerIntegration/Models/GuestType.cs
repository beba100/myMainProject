using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// Guest Type Model
    /// نموذج نوع الضيف
    /// 
    /// Purpose: Defines guest types (Citizen, Resident, Gulf Citizen, Visitor)
    /// Used for: Legal classification, document requirements, pricing rules
    /// </summary>
    [Table("guesttype")]
    public class GuestType
    {
        [Key]
        [Column("gtype_id")]
        public int GtypeId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("gtype_name")]
        public string GtypeName { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("gtype_name_ar")]
        public string? GtypeNameAr { get; set; }

        [Column("gtype_active")]
        public bool GtypeActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = KsaTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
