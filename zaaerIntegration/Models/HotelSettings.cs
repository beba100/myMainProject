using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// جدول إعدادات الفندق - Hotel Settings table
    /// يحتوي على جميع إعدادات الفندق ومعلوماته
    /// </summary>
    [Table("hotel_settings")]
    public class HotelSettings
    {
        [Key]
        [Column("hotel_id")]
        public int HotelId { get; set; }

        [Column("hotel_code")]
        [MaxLength(50)]
        public string? HotelCode { get; set; }

        [Column("hotel_name")]
        [MaxLength(50)]
        public string? HotelName { get; set; }

        [Column("default_currency")]
        [MaxLength(10)]
        public string? DefaultCurrency { get; set; }

        [Column("company_name")]
        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [Column("logo_url")]
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [Column("phone")]
        [MaxLength(50)]
        public string? Phone { get; set; }

        [Column("email")]
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Tax Number (رقم الضريبة)
        /// </summary>
        [Column("tax_number")]
        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        /// <summary>
        /// CR Number (رقم السجل التجاري)
        /// </summary>
        [Column("cr_number")]
        [MaxLength(50)]
        public string? CrNumber { get; set; }

        /// <summary>
        /// Country Code (كود الدولة)
        /// </summary>
        [Column("country_code")]
        [MaxLength(10)]
        public string? CountryCode { get; set; }

        /// <summary>
        /// City (المدينة)
        /// </summary>
        [Column("city")]
        [MaxLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Contact Person (الشخص المسؤول)
        /// </summary>
        [Column("contact_person")]
        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        /// <summary>
        /// Company address
        /// </summary>
        [Column("address")]
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Latitude (خط العرض)
        /// </summary>
        [Column("latitude")]
        [MaxLength(50)]
        public string? Latitude { get; set; }

        /// <summary>
        /// Longitude (خط الطول)
        /// </summary>
        [Column("longitude")]
        [MaxLength(50)]
        public string? Longitude { get; set; }

        /// <summary>
        /// Enabled status (مفعل)
        /// </summary>
        [Column("enabled")]
        public int Enabled { get; set; } = 1;

        /// <summary>
        /// Total Rooms (إجمالي الغرف)
        /// </summary>
        [Column("total_rooms")]
        public int TotalRooms { get; set; } = 0;

        /// <summary>
        /// Property Type (نوع العقار)
        /// </summary>
        [Column("property_type")]
        [MaxLength(50)]
        public string? PropertyType { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        [Column("zaaer_id")]
        public int? ZaaerId { get; set; }

        // Navigation properties
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        [InverseProperty(nameof(PaymentReceipt.HotelSettings))]
        public ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();
        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        [InverseProperty(nameof(CustomerAccount.HotelSettings))]
        public ICollection<CustomerAccount> CustomerAccounts { get; set; } = new List<CustomerAccount>();
        public ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
        public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
        public ICollection<Building> Buildings { get; set; } = new List<Building>();
        public ICollection<Floor> Floors { get; set; } = new List<Floor>();
        public ICollection<CorporateCustomer> CorporateCustomers { get; set; } = new List<CorporateCustomer>();
        public ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();
    }
}
