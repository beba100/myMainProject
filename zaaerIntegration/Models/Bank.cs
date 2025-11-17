using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Bank Entity - البنوك
	/// Used for tracking payment methods and bank transfers
	/// </summary>
	[Table("banks")]
	public class Bank
	{
		[Key]
		[Column("bank_id")]
		public int BankId { get; set; }

		[Column("bank_code")]
		[MaxLength(50)]
		public string? BankCode { get; set; }

		[Column("bank_name_ar")]
		[Required]
		[MaxLength(200)]
		public string BankNameAr { get; set; } = string.Empty;

		[Column("bank_name_en")]
		[Required]
		[MaxLength(200)]
		public string BankNameEn { get; set; } = string.Empty;

		[Column("is_active")]
		public bool IsActive { get; set; } = true;

		[Column("is_default")]
		public bool IsDefault { get; set; } = false;

		/// <summary>
		/// SWIFT/BIC Code for international transfers
		/// </summary>
		[Column("swift_code")]
		[MaxLength(20)]
		public string? SwiftCode { get; set; }

		[Column("sort_order")]
		public int SortOrder { get; set; } = 0;

		// New fields required by Zaaer integration UI (Bank Create form)
		[Column("account_number")]
		[MaxLength(50)]
		public string? AccountNumber { get; set; }

		[Column("iban")]
		[MaxLength(50)]
		public string? Iban { get; set; }

		[Column("currency_code")]
		[MaxLength(10)]
		public string? CurrencyCode { get; set; }

		[Column("description")]
		[MaxLength(500)]
		public string? Description { get; set; }

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		[InverseProperty("BankNavigation")]
		public ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();

		[InverseProperty("BankNavigation")]
		public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
	}
}

