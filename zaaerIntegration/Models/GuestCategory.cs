using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// Guest Category Model
    /// نموذج فئة الضيوف
    /// 
    /// Purpose: Defines customer categories (Frequent, VIP, Regular, etc.)
    /// Used for: Customer classification, loyalty programs, special treatment
    /// </summary>
    [Table("guestcategory")]
    public class GuestCategory
    {
        [Key]
        [Column("gc_id")]
        public int GcId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("gc_name")]
        public string GcName { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("gc_name_ar")]
        public string? GcNameAr { get; set; }

        [Column("gc_active")]
        public bool GcActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = KsaTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
