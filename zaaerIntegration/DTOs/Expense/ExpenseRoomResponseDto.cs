namespace zaaerIntegration.DTOs.Expense
{
    /// <summary>
    /// DTO لعرض expense_room
    /// </summary>
    public class ExpenseRoomResponseDto
    {
        public int ExpenseRoomId { get; set; }
        public int ExpenseId { get; set; }
        public int ApartmentId { get; set; }
        public string? ApartmentCode { get; set; }
        public string? ApartmentName { get; set; }
        public string? Purpose { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

