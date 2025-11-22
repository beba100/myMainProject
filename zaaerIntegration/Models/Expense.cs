using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using zaaerIntegration.Models;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Expense vouchers table
	/// </summary>
	[Table("expenses")]
	public class Expense
	{
		[Key]
		[Column("expense_id")]
		public int ExpenseId { get; set; }

	[Column("date_time")]
	public DateTime DateTime { get; set; }

	[Column("due_date")]
	public DateTime? DueDate { get; set; }

	[Column("comment")]
	[MaxLength(500)]
	public string? Comment { get; set; }

        [Column("hotel_id")]
        public int HotelId { get; set; }

        [Column("expense_category_id")]
        public int? ExpenseCategoryId { get; set; }

        [Column("tax_rate", TypeName = "decimal(5,2)")]
        public decimal? TaxRate { get; set; }

        [Column("tax_amount", TypeName = "decimal(12,2)")]
        public decimal? TaxAmount { get; set; }

        [Column("total_amount", TypeName = "decimal(12,2)")]
        [Required]
        public decimal TotalAmount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// حالة الموافقة على المصروف
        /// Approval status: auto-approved, pending, accepted, rejected, awaiting-manager
        /// </summary>
        [Column("approval_status")]
        [MaxLength(30)]
        public string ApprovalStatus { get; set; } = "auto-approved";

        /// <summary>
        /// معرف المستخدم الذي وافق/رفض المصروف
        /// User ID who approved/rejected the expense
        /// </summary>
        [Column("approved_by")]
        public int? ApprovedBy { get; set; }

        /// <summary>
        /// تاريخ ووقت الموافقة/الرفض
        /// Date and time of approval/rejection
        /// </summary>
        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// سبب الرفض (في حالة رفض المصروف)
        /// Rejection reason (if expense is rejected)
        /// </summary>
        [Column("rejection_reason")]
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        // Navigation properties
        [ForeignKey("HotelId")]
        public HotelSettings? HotelSettings { get; set; }

        [ForeignKey("ExpenseCategoryId")]
        public ExpenseCategory? ExpenseCategory { get; set; }

        /// <summary>
        /// قائمة الغرف المرتبطة بهذه النفقة
        /// List of rooms/apartments associated with this expense
        /// </summary>
        public ICollection<ExpenseRoom> ExpenseRooms { get; set; } = new List<ExpenseRoom>();

        /// <summary>
        /// قائمة الصور المرتبطة بهذه النفقة
        /// List of images associated with this expense
        /// </summary>
        public ICollection<ExpenseImage> ExpenseImages { get; set; } = new List<ExpenseImage>();
    }
}


