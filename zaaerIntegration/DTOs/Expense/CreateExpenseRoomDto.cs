using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Expense
{
    /// <summary>
    /// DTO لإنشاء expense_room جديد
    /// </summary>
    public class CreateExpenseRoomDto
    {
        [Required]
        public int ApartmentId { get; set; }

        /// <summary>
        /// Purpose - الغرض من ربط النفقة بالغرفة
        /// يتم ملؤه عندما يختار المستخدم غرفة من dropdown
        /// </summary>
        [MaxLength(500)]
        public string? Purpose { get; set; }
    }
}

