using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Expense
{
    /// <summary>
    /// DTO لإنشاء expense جديد
    /// لا يحتاج HotelId - سيُقرأ من X-Hotel-Code header
    /// </summary>
    public class CreateExpenseDto
    {
        [Required]
        public DateTime DateTime { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public int? ExpenseCategoryId { get; set; }

        [Range(0, 100)]
        public decimal? TaxRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? TaxAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// قائمة الغرف المرتبطة بهذه النفقة
        /// List of rooms to associate with this expense
        /// </summary>
        public List<CreateExpenseRoomDto>? ExpenseRooms { get; set; }
    }
}

