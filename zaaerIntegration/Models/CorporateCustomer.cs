using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Corporate Customer Model
	/// نموذج العملاء من الشركات
	/// </summary>
	[Table("corporate_customers")]
	public class CorporateCustomer
	{
		[Key]
		[Column("corporate_id")]
		public int CorporateId { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("corporate_name")]
		[Required]
		[MaxLength(200)]
		public string CorporateName { get; set; } = string.Empty;

		[Column("corporate_name_ar")]
		[MaxLength(200)]
		public string? CorporateNameAr { get; set; }

		[Column("country")]
		[MaxLength(100)]
		public string? Country { get; set; }

		[Column("country_ar")]
		[MaxLength(100)]
		public string? CountryAr { get; set; }

		[Column("vat_registration_no")]
		[MaxLength(50)]
		public string? VatRegistrationNo { get; set; }

		[Column("commercial_registration_no")]
		[MaxLength(50)]
		public string? CommercialRegistrationNo { get; set; }

		/// <summary>
		/// Discount Method: Amount or Percentage
		/// طريقة الخصم: مبلغ أو نسبة
		/// </summary>
		[Column("discount_method")]
		[MaxLength(20)]
		public string? DiscountMethod { get; set; }

		/// <summary>
		/// Discount Value (amount or percentage based on method)
		/// قيمة الخصم (مبلغ أو نسبة حسب الطريقة)
		/// </summary>
		[Column("discount_value", TypeName = "decimal(10,2)")]
		public decimal? DiscountValue { get; set; }

		[Column("city")]
		[MaxLength(100)]
		public string? City { get; set; }

		[Column("city_ar")]
		[MaxLength(100)]
		public string? CityAr { get; set; }

		[Column("postal_code")]
		[MaxLength(20)]
		public string? PostalCode { get; set; }

		[Column("address")]
		[MaxLength(500)]
		public string? Address { get; set; }

		[Column("address_ar")]
		[MaxLength(500)]
		public string? AddressAr { get; set; }

		[Column("email")]
		[MaxLength(100)]
		public string? Email { get; set; }

		[Column("corporate_phone")]
		[MaxLength(50)]
		public string? CorporatePhone { get; set; }

		[Column("contact_person_name")]
		[MaxLength(100)]
		public string? ContactPersonName { get; set; }

		[Column("contact_person_name_ar")]
		[MaxLength(100)]
		public string? ContactPersonNameAr { get; set; }

		[Column("contact_person_phone")]
		[MaxLength(50)]
		public string? ContactPersonPhone { get; set; }

		[Column("corporate_logo_url")]
		[MaxLength(500)]
		public string? CorporateLogoUrl { get; set; }

		[Column("notes")]
		[MaxLength(1000)]
		public string? Notes { get; set; }

		[Column("is_active")]
		public bool IsActive { get; set; } = true;

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		/// <summary>
		/// Hotel reference
		/// مرجع الفندق
		/// </summary>
		[ForeignKey("HotelId")]
		public virtual HotelSettings? HotelSettings { get; set; }

		/// <summary>
		/// Reservations for this corporate
		/// الحجوزات لهذه الشركة
		/// </summary>
		public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
	}
}

