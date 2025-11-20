namespace zaaerIntegration.DTOs.Expense
{
    /// <summary>
    /// DTO لعرض expense_room
    /// </summary>
    public class ExpenseRoomResponseDto
    {
        public int ExpenseRoomId { get; set; }
        public int ExpenseId { get; set; }
        public int? ApartmentId { get; set; } // ✅ Nullable for room categories
        public int? ZaaerId { get; set; }
        public string? CategoryCode { get; set; } // ✅ For room categories (CAT_BUILDING, etc.)
        public string? ApartmentCode { get; set; }
        public string? ApartmentName { get; set; }
        public string? Purpose { get; set; }
        public decimal? Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

