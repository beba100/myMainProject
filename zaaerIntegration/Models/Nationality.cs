using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// Nationality Model
    /// نموذج الجنسية
    /// 
    /// Purpose: Defines nationalities/countries for customer classification
    /// Used for: Customer registration, legal compliance, reporting
    /// </summary>
    [Table("nationality")]
    public class Nationality
    {
        [Key]
        [Column("n_id")]
        public int NId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("n_name")]
        public string NName { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("n_name_ar")]
        public string? NNameAr { get; set; }

        [Column("n_default")]
        public int NDefault { get; set; } = 0;

        [StringLength(20)]
        [Column("nmtp_code")]
        public string? NmtpCode { get; set; }

        [Column("n_gulf")]
        public int? NGulf { get; set; }  // 1 for Gulf countries, 0 for others, NULL for unknown

        [StringLength(10)]
        [Column("code_prefix")]
        public string? CodePrefix { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = KsaTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

        // Computed Properties
        [NotMapped]
        public bool IsGulfCountry => NGulf == 1;

        [NotMapped]
        public string DisplayName => !string.IsNullOrEmpty(NNameAr) ? $"{NName} ({NNameAr})" : NName;
    }
}
