namespace zaaerIntegration.DTOs.Expense
{
    /// <summary>
    /// DTO لعرض سجل موافقات المصروف
    /// </summary>
    public class ExpenseApprovalHistoryDto
    {
        public int Id { get; set; }
        public int ExpenseId { get; set; }
        public string Action { get; set; } = string.Empty; // created, approved, rejected, awaiting-manager, etc.
        public int? ActionBy { get; set; }
        public string? ActionByFullName { get; set; }
        public string? ActionByRole { get; set; }
        public string? ActionByTenantName { get; set; }
        public DateTime ActionAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public string? Comments { get; set; }
    }
}

