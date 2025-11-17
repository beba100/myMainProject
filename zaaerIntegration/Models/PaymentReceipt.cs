using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول سندات القبض - Payment receipts table
	/// </summary>
	[Table("payment_receipts")]
	public class PaymentReceipt
	{
		[Key]
		[Column("receipt_id")]
		public int ReceiptId { get; set; }

		[Column("receipt_no")]
		[Required]
		[MaxLength(50)]
		public string ReceiptNo { get; set; }

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

	[Column("receipt_date")]
	public DateTime ReceiptDate { get; set; } = DateTime.Now;

	/// <summary>
	/// Receipt Type: receipt | security_deposit
	/// نوع السند: سند قبض عادي | سند قبض تأمين
	/// </summary>
	[Column("receipt_type")]
	[MaxLength(50)]
	public string ReceiptType { get; set; } = "receipt";

	/// <summary>
	/// Voucher code (for discount vouchers)
	/// رمز القسيمة (للخصومات)
	/// </summary>
	[Column("voucher_code")]
	[MaxLength(50)]
	public string? VoucherCode { get; set; }

	[Column("amount_paid", TypeName = "decimal(12,2)")]
	[Required]
	public decimal AmountPaid { get; set; }

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

		/// <summary>
		/// Receipt status: active | cancelled
		/// حالة السند: نشط | ملغي
		/// </summary>
		[Column("receipt_status")]
		[MaxLength(50)]
		public string ReceiptStatus { get; set; } = "active";

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
		[ForeignKey(nameof(HotelId))]
		[InverseProperty(nameof(HotelSettings.PaymentReceipts))]
		public HotelSettings HotelSettings { get; set; } = null!;
		public Reservation Reservation { get; set; }
		public ReservationUnit ReservationUnit { get; set; }
		public Invoice Invoice { get; set; }
		public Models.PaymentMethod? PaymentMethodNavigation { get; set; }
		public Bank? BankNavigation { get; set; }
		public ICollection<CustomerTransaction> CustomerTransactions { get; set; }
	}
}

