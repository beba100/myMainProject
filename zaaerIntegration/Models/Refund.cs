using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول الاستردادات - Refunds table
	/// </summary>
	[Table("refunds")]
	public class Refund
	{
		[Key]
		[Column("refund_id")]
		public int RefundId { get; set; }

		[Column("refund_no")]
		[Required]
		[MaxLength(50)]
		public string RefundNo { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("reservation_id")]
		public int? ReservationId { get; set; }

		[Column("unit_id")]
		public int? UnitId { get; set; }

		[Column("invoice_id")]
		public int? InvoiceId { get; set; }

		[Column("customer_id")]
		[Required]
		public int CustomerId { get; set; }

	[Column("refund_date")]
	public DateTime RefundDate { get; set; } = DateTime.Now;

	/// <summary>
	/// Refund Type: refund | security_deposit_refund
	/// نوع الاسترداد: استرداد عادي | استرداد مبلغ التأمين
	/// </summary>
	[Column("refund_type")]
	[MaxLength(50)]
	public string RefundType { get; set; } = "refund";

	/// <summary>
	/// Paid From: drawer | bank
	/// الدفع من: الصندوق | البنك
	/// </summary>
	[Column("paid_from")]
	[MaxLength(20)]
	public string PaidFrom { get; set; } = "drawer";

	[Column("refund_amount", TypeName = "decimal(12,2)")]
	[Required]
	public decimal RefundAmount { get; set; }

		[Column("refund_reason")]
		[MaxLength(500)]
		public string RefundReason { get; set; }

	[Column("payment_method_id")]
	public int? PaymentMethodId { get; set; }

	/// <summary>
	/// Legacy: Payment Method as string (for backward compatibility)
	/// قديم: طريقة الدفع كنص
	/// </summary>
	[Column("payment_method")]
	[MaxLength(50)]
	public string? PaymentMethod { get; set; }

		[Column("bank_id")]
		public int? BankId { get; set; }

		[Column("transaction_no")]
		[MaxLength(100)]
		public string TransactionNo { get; set; }

		[Column("notes")]
		public string Notes { get; set; }

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
		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;
		public Reservation Reservation { get; set; }
		public ReservationUnit ReservationUnit { get; set; }
		public Invoice Invoice { get; set; }
		public Models.PaymentMethod? PaymentMethodNavigation { get; set; }
		public Bank? BankNavigation { get; set; }
		public ICollection<CustomerTransaction> CustomerTransactions { get; set; } = new List<CustomerTransaction>();
	}
}

