using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Payment Method Model
	/// نموذج طرق الدفع
	/// </summary>
	[Table("payment_methods")]
	public class PaymentMethod
	{
		[Key]
		[Column("payment_method_id")]
		public int PaymentMethodId { get; set; }

		[Column("method_code")]
		[Required]
		[MaxLength(50)]
		public string MethodCode { get; set; } = string.Empty;

		[Column("method_name")]
		[Required]
		[MaxLength(100)]
		public string MethodName { get; set; } = string.Empty;

		[Column("method_name_ar")]
		[MaxLength(100)]
		public string? MethodNameAr { get; set; }

		[Column("description")]
		[MaxLength(500)]
		public string? Description { get; set; }

		[Column("description_ar")]
		[MaxLength(500)]
		public string? DescriptionAr { get; set; }

		/// <summary>
		/// Category: Cash, Card, Bank, Other
		/// الفئة: نقدي، بطاقة، بنك، أخرى
		/// </summary>
		[Column("category")]
		[MaxLength(50)]
		public string? Category { get; set; }

		/// <summary>
		/// Icon name for UI display
		/// اسم الأيقونة للعرض
		/// </summary>
		[Column("icon")]
		[MaxLength(100)]
		public string? Icon { get; set; }

		/// <summary>
		/// If true, requires transaction number
		/// إذا كان صحيح، يتطلب رقم عملية
		/// </summary>
		[Column("requires_transaction_no")]
		public bool RequiresTransactionNo { get; set; } = false;

		/// <summary>
		/// If true, requires approval
		/// إذا كان صحيح، يتطلب موافقة
		/// </summary>
		[Column("requires_approval")]
		public bool RequiresApproval { get; set; } = false;

		[Column("is_active")]
		public bool IsActive { get; set; } = true;

		[Column("sort_order")]
		public int SortOrder { get; set; } = 0;

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		// Navigation Properties
		/// <summary>
		/// Payment receipts using this method
		/// سندات القبض بهذه الطريقة
		/// </summary>
		public virtual ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();

		/// <summary>
		/// Refunds using this method
		/// المرتجعات بهذه الطريقة
		/// </summary>
		public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
	}
}

