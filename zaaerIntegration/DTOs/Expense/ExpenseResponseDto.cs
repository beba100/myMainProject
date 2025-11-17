namespace zaaerIntegration.DTOs.Expense
{
    /// <summary>
    /// DTO لعرض expense
    /// </summary>
    public class ExpenseResponseDto
    {
        public int ExpenseId { get; set; }
        public int HotelId { get; set; }
        public DateTime DateTime { get; set; }
        public string? Comment { get; set; }
        public int? ExpenseCategoryId { get; set; }
        public string? ExpenseCategoryName { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// قائمة الغرف المرتبطة بهذه النفقة
        /// List of rooms associated with this expense
        /// </summary>
        public List<ExpenseRoomResponseDto> ExpenseRooms { get; set; } = new List<ExpenseRoomResponseDto>();
    }
}

