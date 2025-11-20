using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// جدول expense_rooms - ربط النفقات بالغرف
    /// Expense Rooms table - Links expenses to apartments/rooms
    /// </summary>
    [Table("expense_rooms")]
    public class ExpenseRoom
    {
        [Key]
        [Column("expense_room_id")]
        public int ExpenseRoomId { get; set; }

        [Column("expense_id")]
        [Required]
        public int ExpenseId { get; set; }

        [Column("zaaer_id")]
        public int? ZaaerId { get; set; } // ✅ Foreign Key to apartments.zaaer_id (nullable for room categories)

        /// <summary>
        /// Purpose - الغرض من ربط النفقة بالغرفة
        /// يتم ملؤه عندما يختار المستخدم غرفة من dropdown
        /// </summary>
        [Column("purpose")]
        [MaxLength(500)]
        public string? Purpose { get; set; }

        /// <summary>
        /// Amount - المبلغ المرتبط بهذه الغرفة
        /// </summary>
        [Column("amount", TypeName = "decimal(12,2)")]
        public decimal? Amount { get; set; }

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ExpenseId")]
        public Expense Expense { get; set; }

        [ForeignKey("ZaaerId")]
        public Apartment? Apartment { get; set; } // ✅ Nullable for room categories - Foreign Key to apartments.zaaer_id
    }
}

