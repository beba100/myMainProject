using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Expense
{
    /// <summary>
    /// DTO لتحديث expense
    /// لا يحتاج HotelId - سيُقرأ من X-Hotel-Code header
    /// </summary>
    public class UpdateExpenseDto
    {
        public DateTime? DateTime { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public int? ExpenseCategoryId { get; set; }

        [Range(0, 100)]
        public decimal? TaxRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? TaxAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? TotalAmount { get; set; }
    }
}

