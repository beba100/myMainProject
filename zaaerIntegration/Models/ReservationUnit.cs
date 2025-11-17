using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinanceLedgerAPI.Enums;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول وحدات الحجز - Reservation units (for multi-apartment bookings)
	/// </summary>
	[Table("reservation_units")]
	public class ReservationUnit
	{
		[Key]
		[Column("unit_id")]
		public int UnitId { get; set; }

		[Column("reservation_id")]
		[Required]
		public int ReservationId { get; set; }

		[Column("apartment_id")]
		[Required]
		public int ApartmentId { get; set; }

		[Column("check_in_date")]
		[Required]
		public DateTime CheckInDate { get; set; }

		[Column("check_out_date")]
		[Required]
		public DateTime CheckOutDate { get; set; }

	[Column("departure_date")]
	public DateTime? DepartureDate { get; set; }

		[Column("number_of_nights")]
		public int? NumberOfNights { get; set; }

		[Column("rent_amount", TypeName = "decimal(12,2)")]
		[Required]
		public decimal RentAmount { get; set; }

		[Column("vat_rate", TypeName = "decimal(5,2)")]
		public decimal VatRate { get; set; } = 15.00M;

		[Column("vat_amount", TypeName = "decimal(12,2)")]
		public decimal? VatAmount { get; set; }

		[Column("lodging_tax_rate", TypeName = "decimal(5,2)")]
		public decimal LodgingTaxRate { get; set; } = 2.50M;

		[Column("lodging_tax_amount", TypeName = "decimal(12,2)")]
		public decimal? LodgingTaxAmount { get; set; }

		[Column("total_amount", TypeName = "decimal(12,2)")]
		[Required]
		public decimal TotalAmount { get; set; }

		[Column("status")]
		[MaxLength(50)]
		public string Status { get; set; } = "Reserved";

		/// <summary>
		/// Reservation Unit Status as Enum
		/// حالة وحدة الحجز كـ Enum
		/// </summary>
		[NotMapped]
		public ReservationUnitStatus StatusEnum
		{
			get => Enum.TryParse<ReservationUnitStatus>(Status, true, out var result) ? result : ReservationUnitStatus.Reserved;
			set => Status = value.ToString();
		}

		/// <summary>
		/// Status as English Word (for API responses)
		/// الحالة بالكلمة الإنجليزية (للاستجابات)
		/// </summary>
		[NotMapped]
		public string StatusWord => StatusEnum.ToString();

		/// <summary>
		/// Status Display Name
		/// اسم الحالة للعرض
		/// </summary>
		[NotMapped]
		public string StatusDisplayName => ReservationUnitStatusHelper.GetDisplayName(StatusEnum);

		/// <summary>
		/// Status Display Name in Arabic
		/// اسم الحالة بالعربية
		/// </summary>
		[NotMapped]
		public string StatusDisplayNameAr => ReservationUnitStatusHelper.GetDisplayNameAr(StatusEnum);

		/// <summary>
		/// Status Color for UI
		/// لون الحالة للواجهة
		/// </summary>
		[NotMapped]
		public string StatusColor => ReservationUnitStatusHelper.GetStatusColor(StatusEnum);

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }

		// Navigation properties
		public Reservation Reservation { get; set; }
		public Apartment Apartment { get; set; }
		public ICollection<Invoice> Invoices { get; set; }
		public ICollection<PaymentReceipt> PaymentReceipts { get; set; }
		public ICollection<Refund> Refunds { get; set; }
	}
}

