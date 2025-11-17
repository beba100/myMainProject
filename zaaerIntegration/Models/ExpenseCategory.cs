using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// جدول expense_categories - فئات النفقات
    /// Expense Categories table
    /// </summary>
    [Table("expense_categories")]
    public class ExpenseCategory
    {
        [Key]
        [Column("expense_category_id")]
        public int ExpenseCategoryId { get; set; }

        [Column("hotel_id")]
        [Required]
        public int HotelId { get; set; }

        [Column("category_name")]
        [Required]
        [MaxLength(200)]
        public string CategoryName { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("is_active")]
        [Required]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("HotelId")]
        public HotelSettings HotelSettings { get; set; }
    }
}

