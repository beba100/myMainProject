using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول حسابات العملاء - Customer accounts (ledger)
	/// </summary>
	[Table("customer_accounts")]
	public class CustomerAccount
	{
		[Key]
		[Column("account_id")]
		public int AccountId { get; set; }

		[Column("customer_id")]
		[Required]
		public int CustomerId { get; set; }

		[Column("reservation_id")]
		public int? ReservationId { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("currency_code")]
		[MaxLength(10)]
		public string CurrencyCode { get; set; } = "SAR";

		[Column("balance", TypeName = "decimal(18,2)")]
		public decimal Balance { get; set; } = 0.00M;

		[Column("total_credit", TypeName = "decimal(18,2)")]
		public decimal TotalCredit { get; set; } = 0.00M;

		[Column("total_debit", TypeName = "decimal(18,2)")]
		public decimal TotalDebit { get; set; } = 0.00M;

		[Column("last_transaction_at")]
		public DateTime? LastTransactionAt { get; set; }

		[Column("status")]
		[MaxLength(20)]
		public string Status { get; set; } = "active";

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		[Column("updated_at")]
		public DateTime UpdatedAt { get; set; } = KsaTime.Now;

		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }

		// Navigation properties
		[ForeignKey(nameof(HotelId))]
		[InverseProperty(nameof(HotelSettings.CustomerAccounts))]
		public HotelSettings HotelSettings { get; set; } = null!;

		public ICollection<CustomerTransaction> CustomerTransactions { get; set; } = new List<CustomerTransaction>();
	}
}

