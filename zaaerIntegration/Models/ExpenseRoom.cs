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

        [Column("apartment_id")]
        [Required]
        public int ApartmentId { get; set; }

        /// <summary>
        /// Purpose - الغرض من ربط النفقة بالغرفة
        /// يتم ملؤه عندما يختار المستخدم غرفة من dropdown
        /// </summary>
        [Column("purpose")]
        [MaxLength(500)]
        public string? Purpose { get; set; }

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ExpenseId")]
        public Expense Expense { get; set; }

        [ForeignKey("ApartmentId")]
        public Apartment Apartment { get; set; }
    }
}

