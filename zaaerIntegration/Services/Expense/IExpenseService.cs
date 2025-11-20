using zaaerIntegration.DTOs.Expense;

namespace zaaerIntegration.Services.Expense
{
    /// <summary>
    /// Service interface لإدارة النفقات (Expenses)
    /// يستخدم X-Hotel-Code header للحصول على HotelId
    /// </summary>
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseResponseDto>> GetAllAsync();
        Task<ExpenseResponseDto?> GetByIdAsync(int id);
        Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto);
        Task<ExpenseResponseDto?> UpdateAsync(int id, UpdateExpenseDto dto);
        Task<bool> DeleteAsync(int id);

        // Expense Rooms Operations
        Task<IEnumerable<ExpenseRoomResponseDto>> GetExpenseRoomsAsync(int expenseId);
        Task<ExpenseRoomResponseDto> AddExpenseRoomAsync(int expenseId, CreateExpenseRoomDto dto);
        Task<ExpenseRoomResponseDto?> UpdateExpenseRoomAsync(int expenseRoomId, UpdateExpenseRoomDto dto);
        Task<bool> DeleteExpenseRoomAsync(int expenseRoomId);

        // Approval Operations
        Task<ExpenseResponseDto?> ApproveExpenseAsync(int id, string status, int approvedBy, string? rejectionReason = null);
    }
}

