using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Credit Note Entity - إشعار دائن (عكس الفاتورة)
	/// Used to reverse/cancel invoices without actual cash refund
	/// </summary>
	[Table("credit_notes")]
	public class CreditNote
	{
		[Key]
		[Column("credit_note_id")]
		public int CreditNoteId { get; set; }

		[Column("credit_note_no")]
		[Required]
		[MaxLength(50)]
		public string CreditNoteNo { get; set; } = string.Empty;

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		/// <summary>
		/// Original Invoice to be reversed
		/// الفاتورة الأصلية المراد عكسها
		/// </summary>
		[Column("invoice_id")]
		[Required]
		public int InvoiceId { get; set; }

		[Column("reservation_id")]
		public int? ReservationId { get; set; }

		[Column("customer_id")]
		[Required]
		public int CustomerId { get; set; }

	[Column("credit_note_date")]
	public DateTime CreditNoteDate { get; set; } = DateTime.Now;

	/// <summary>
	/// Credit Note Date in Hijri
	/// </summary>
	[Column("credit_note_date_hijri")]
	[MaxLength(20)]
	public string? CreditNoteDateHijri { get; set; }

	/// <summary>
	/// Subtotal amount before tax
	/// المبلغ الإجمالي قبل الضريبة
	/// </summary>
	[Column("subtotal", TypeName = "decimal(12,2)")]
	public decimal? Subtotal { get; set; }

	/// <summary>
	/// VAT Rate percentage
	/// نسبة ضريبة القيمة المضافة
	/// </summary>
	[Column("vat_rate", TypeName = "decimal(12,4)")]
	public decimal? VatRate { get; set; }

	/// <summary>
	/// VAT Amount
	/// مبلغ ضريبة القيمة المضافة
	/// </summary>
	[Column("vat_amount", TypeName = "decimal(12,2)")]
	public decimal? VatAmount { get; set; }

	/// <summary>
	/// Lodging Tax Rate percentage
	/// نسبة ضريبة الإقامة
	/// </summary>
	[Column("lodging_tax_rate", TypeName = "decimal(12,4)")]
	public decimal? LodgingTaxRate { get; set; }

	/// <summary>
	/// Lodging Tax Amount
	/// مبلغ ضريبة الإقامة
	/// </summary>
	[Column("lodging_tax_amount", TypeName = "decimal(12,2)")]
	public decimal? LodgingTaxAmount { get; set; }

	/// <summary>
	/// Amount to be credited (reversed from invoice)
	/// المبلغ المراد عكسه من الفاتورة
	/// </summary>
	[Column("credit_amount", TypeName = "decimal(12,2)")]
	[Required]
	public decimal CreditAmount { get; set; }

		[Column("reason")]
		[Required]
		[MaxLength(500)]
		public string Reason { get; set; } = string.Empty;

		/// <summary>
		/// Credit Note Type (e.g., "refund", "discount", "adjustment", "cancellation")
		/// نوع الإشعار الدائن
		/// </summary>
		[Column("credit_type")]
		[MaxLength(50)]
		public string CreditType { get; set; } = "refund";

		[Column("notes")]
		[MaxLength(1000)]
		public string? Notes { get; set; }

		/// <summary>
		/// ZATCA Integration
		/// </summary>
		[Column("is_sent_zatca")]
		public bool IsSentZatca { get; set; } = false;

		[Column("zatca_uuid")]
		[MaxLength(255)]
		public string? ZatcaUuid { get; set; }

		[Column("created_by")]
		public int? CreatedBy { get; set; }

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }

		// Navigation properties
		public HotelSettings HotelSettings { get; set; } = null!;
		public Invoice Invoice { get; set; } = null!;
		public Reservation? Reservation { get; set; }
		public ICollection<CustomerTransaction> CustomerTransactions { get; set; } = new List<CustomerTransaction>();
	}
}

