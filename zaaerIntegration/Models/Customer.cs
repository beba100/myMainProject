using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// Customer Model
    /// نموذج العميل
    /// 
    /// Purpose: Main customer/guest information
    /// Used for: Guest registration, booking management, customer relationships
    /// </summary>
    [Table("customers")]
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [StringLength(50)]
        [Column("customer_no")]
        public string? CustomerNo { get; set; }

        [Required]
        [StringLength(200)]
        [Column("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

        // Foreign Keys
        [Column("gtype_id")]
        public int? GtypeId { get; set; }  // Guest Type
        [Column("n_id")]
        public int? NId { get; set; }     // Nationality ID
        [Column("guest_category_id")]
        public int? GuestCategoryId { get; set; }  // Guest Category

        // Document Information (Simplified - details moved to CustomerIdentifications table)
        [StringLength(50)]
        [Column("visa_no")]
        public string? VisaNo { get; set; }

        // Contact Information
        [StringLength(20)]
        [Column("mobile_no")]
        public string? MobileNo { get; set; }

        [StringLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [StringLength(500)]
        [Column("address")]
        public string? Address { get; set; }

        [StringLength(1000)]
        [Column("comments")]
        public string? Comments { get; set; }

        // System Fields
        [Column("entered_by")]
        public int? EnteredBy { get; set; }
        [Column("entered_at")]
        public DateTime? EnteredAt { get; set; }

        [StringLength(10)]
        [Column("gender")]
        public string? Gender { get; set; }

        // Birthdate Information
        [Column("birthday")]
        public DateTime? Birthday { get; set; }  // Gregorian birthdate (legacy field)

        [StringLength(50)]
        [Column("birthdate_hijri")]
        public string? BirthdateHijri { get; set; }  // Hijri birthdate string

        // Gregorian Birthdate
        [Column("birthdate_gregorian")]
        public DateTime? BirthdateGregorian { get; set; }

        // Multi-tenancy
        [Required]
        [Column("hotel_id")]
        public int HotelId { get; set; }  // REQUIRED: Hotel ID for multi-tenancy

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        [Column("zaaer_id")]
        public int? ZaaerId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = KsaTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual GuestType? GuestType { get; set; }
        public virtual GuestCategory? GuestCategory { get; set; }
        public virtual Nationality? Nationality { get; set; }
        public virtual HotelSettings? HotelSettings { get; set; }
        // Related entities
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        public virtual ICollection<CustomerAccount> CustomerAccounts { get; set; } = new List<CustomerAccount>();
        public virtual ICollection<CustomerTransaction> CustomerTransactions { get; set; } = new List<CustomerTransaction>();
        public virtual ICollection<CustomerIdentification> Identifications { get; set; } = new List<CustomerIdentification>();

        // Computed Properties
        [NotMapped]
        public int Age
        {
            get
            {
                var birthdate = BirthdateGregorian ?? Birthday;
                if (birthdate == null) return 0;
                var today = DateTime.Today;
                var age = today.Year - birthdate.Value.Year;
                if (birthdate.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}
