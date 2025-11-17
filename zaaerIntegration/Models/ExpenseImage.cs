using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Models
{
    /// <summary>
    /// جدول صور المصروفات
    /// Expense Images Table
    /// </summary>
    [Table("expense_images")]
    public class ExpenseImage
    {
        [Key]
        [Column("expense_image_id")]
        public int ExpenseImageId { get; set; }

        [Column("expense_id")]
        [Required]
        public int ExpenseId { get; set; }

        /// <summary>
        /// مسار الصورة (URL أو path)
        /// Image path or URL
        /// </summary>
        [Column("image_path")]
        [Required]
        [MaxLength(500)]
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>
        /// اسم الملف الأصلي
        /// Original file name
        /// </summary>
        [Column("original_filename")]
        [MaxLength(255)]
        public string? OriginalFilename { get; set; }

        /// <summary>
        /// حجم الملف بالبايت
        /// File size in bytes
        /// </summary>
        [Column("file_size")]
        public long? FileSize { get; set; }

        /// <summary>
        /// نوع الملف (MIME type)
        /// File MIME type
        /// </summary>
        [Column("content_type")]
        [MaxLength(100)]
        public string? ContentType { get; set; }

        /// <summary>
        /// ترتيب الصورة (للعرض)
        /// Display order
        /// </summary>
        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("ExpenseId")]
        public Expense Expense { get; set; } = null!;
    }
}

