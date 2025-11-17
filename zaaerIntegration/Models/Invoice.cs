using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول الفواتير - Invoices table
	/// </summary>
	[Table("invoices")]
	public class Invoice
	{
		[Key]
		[Column("invoice_id")]
		public int InvoiceId { get; set; }

		[Column("invoice_no")]
		[Required]
		[MaxLength(50)]
		public string InvoiceNo { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("reservation_id")]
		public int? ReservationId { get; set; }

		[Column("unit_id")]
		public int? UnitId { get; set; }

		[Column("customer_id")]
		[Required]
		public int CustomerId { get; set; }

	[Column("invoice_date")]
	public DateTime InvoiceDate { get; set; } = DateTime.Now;

	/// <summary>
	/// Invoice Date in Hijri (التاريخ الهجري)
	/// </summary>
	[Column("invoice_date_hijri")]
	[MaxLength(20)]
	public string? InvoiceDateHijri { get; set; }

	/// <summary>
	/// Invoice Period From (مدة الفاتورة من)
	/// </summary>
	[Column("period_from")]
	public DateTime? PeriodFrom { get; set; }

	/// <summary>
	/// Invoice Period To (مدة الفاتورة إلى)
	/// </summary>
	[Column("period_to")]
	public DateTime? PeriodTo { get; set; }

	[Column("invoice_type")]
	[MaxLength(50)]
	public string InvoiceType { get; set; } = "rent";

		[Column("subtotal", TypeName = "decimal(12,2)")]
		public decimal? Subtotal { get; set; }

		[Column("vat_rate", TypeName = "decimal(5,2)")]
		public decimal? VatRate { get; set; }

	[Column("vat_amount", TypeName = "decimal(12,2)")]
	public decimal? VatAmount { get; set; }

	/// <summary>
	/// Lodging Tax Rate (نسبة ضريبة الإقامة)
	/// </summary>
	[Column("lodging_tax_rate", TypeName = "decimal(5,2)")]
	public decimal? LodgingTaxRate { get; set; }

	/// <summary>
	/// Lodging Tax Amount (مبلغ ضريبة الإقامة)
	/// </summary>
	[Column("lodging_tax_amount", TypeName = "decimal(12,2)")]
	public decimal? LodgingTaxAmount { get; set; }

	[Column("total_amount", TypeName = "decimal(12,2)")]
	public decimal? TotalAmount { get; set; }

		[Column("payment_status")]
		[MaxLength(20)]
		public string PaymentStatus { get; set; } = "unpaid";

		[Column("amount_paid", TypeName = "decimal(12,2)")]
		public decimal AmountPaid { get; set; } = 0.00M;

		[Column("amount_remaining", TypeName = "decimal(12,2)")]
		public decimal? AmountRemaining { get; set; }

		[Column("amount_refunded", TypeName = "decimal(12,2)")]
		public decimal? AmountRefunded { get; set; }

		[Column("is_sent_zatca")]
		public bool IsSentZatca { get; set; } = false;

		[Column("zatca_uuid")]
		[MaxLength(255)]
		public string ZatcaUuid { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; } = DateTime.Now;

	/// <summary>
	/// Created By User ID (من أنشأ الفاتورة)
	/// </summary>
	[Column("created_by")]
	public int? CreatedBy { get; set; }

	/// <summary>
	/// Notes (ملاحظات)
	/// </summary>
	[Column("notes")]
	[MaxLength(1000)]
	public string? Notes { get; set; }

	/// <summary>
	/// Zaaer System ID (معرف Zaaer)
	/// External ID from Zaaer integration system
	/// </summary>
	[Column("zaaer_id")]
	public int? ZaaerId { get; set; }

		// Navigation properties
		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;
		public Reservation Reservation { get; set; }
		public ReservationUnit ReservationUnit { get; set; }
		public ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();
		public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
		public ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();
		public ICollection<CustomerTransaction> CustomerTransactions { get; set; } = new List<CustomerTransaction>();
	}
}

