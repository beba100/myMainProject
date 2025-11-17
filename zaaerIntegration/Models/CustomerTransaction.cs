using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول معاملات العملاء - Customer transactions (ledger details)
	/// </summary>
	[Table("customer_transactions")]
	public class CustomerTransaction
	{
		[Key]
		[Column("transaction_id")]
		public int TransactionId { get; set; }

		[Column("account_id")]
		[Required]
		public int AccountId { get; set; }

		[Column("customer_id")]
		[Required]
		public int CustomerId { get; set; }

		[Column("reservation_id")]
		public int? ReservationId { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("payment_receipt_id")]
		public int? PaymentReceiptId { get; set; }

		[Column("receipt_no")]
		[MaxLength(50)]
		public string? ReceiptNo { get; set; }

		[Column("voucher_code")]
		[MaxLength(50)]
		public string? VoucherCode { get; set; }

		[Column("receipt_type")]
		[MaxLength(50)]
		public string? ReceiptType { get; set; }

		[Column("zaaer_receipt_id")]
		public int? ZaaerReceiptId { get; set; }

		[Column("transaction_date")]
		public DateTime TransactionDate { get; set; } = KsaTime.Now;

		[Column("transaction_type")]
		[MaxLength(50)]
		public string TransactionType { get; set; } = "receipt";

		[Column("transaction_source")]
		[MaxLength(30)]
		public string TransactionSource { get; set; } = "PaymentReceipt";

		[Column("transaction_status")]
		[MaxLength(20)]
		public string TransactionStatus { get; set; } = "active";

		[Column("credit_amount", TypeName = "decimal(18,2)")]
		public decimal CreditAmount { get; set; } = 0.00M;

		[Column("debit_amount", TypeName = "decimal(18,2)")]
		public decimal DebitAmount { get; set; } = 0.00M;

		[Column("balance_after", TypeName = "decimal(18,2)")]
		public decimal BalanceAfter { get; set; } = 0.00M;

		[Column("payment_method")]
		[MaxLength(50)]
		public string? PaymentMethod { get; set; }

		[Column("related_invoice_id")]
		public int? RelatedInvoiceId { get; set; }

		[Column("description")]
		[MaxLength(255)]
		public string? Description { get; set; }

		[Column("created_by")]
		public int? CreatedBy { get; set; }

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		[Column("updated_by")]
		public int? UpdatedBy { get; set; }

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		[ForeignKey("AccountId")]
		public CustomerAccount CustomerAccount { get; set; } = null!;

		[ForeignKey("RelatedInvoiceId")]
		public Invoice? RelatedInvoice { get; set; }

		[ForeignKey("PaymentReceiptId")]
		public PaymentReceipt? PaymentReceipt { get; set; }
	}
}

