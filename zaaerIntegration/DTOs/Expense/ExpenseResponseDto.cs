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
        /// حالة الموافقة على المصروف
        /// Approval status: auto-approved, pending, accepted, rejected
        /// </summary>
        public string ApprovalStatus { get; set; } = "auto-approved";

        /// <summary>
        /// معرف المستخدم الذي وافق/رفض المصروف
        /// User ID who approved/rejected the expense
        /// </summary>
        public int? ApprovedBy { get; set; }

        /// <summary>
        /// تاريخ ووقت الموافقة/الرفض
        /// Date and time of approval/rejection
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// اسم الفندق
        /// Hotel name
        /// </summary>
        public string? HotelName { get; set; }

        /// <summary>
        /// رابط الموافقة (يُستخدم فقط للمصروفات في حالة pending)
        /// Approval link (only for pending expenses)
        /// </summary>
        public string? ApprovalLink { get; set; }

        /// <summary>
        /// قائمة الغرف المرتبطة بهذه النفقة
        /// List of rooms associated with this expense
        /// </summary>
        public List<ExpenseRoomResponseDto> ExpenseRooms { get; set; } = new List<ExpenseRoomResponseDto>();
    }
}

