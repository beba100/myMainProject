using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Expense
{
    /// <summary>
    /// DTO لتحديث expense_room
    /// </summary>
    public class UpdateExpenseRoomDto
    {
        public int? ApartmentId { get; set; } // ✅ For backward compatibility

        /// <summary>
        /// Zaaer System ID (استخدام ZaaerId للبحث عن Apartment)
        /// يتم استخدامه إذا كان ApartmentId غير موجود
        /// </summary>
        public int? ZaaerId { get; set; }

        /// <summary>
        /// Purpose - الغرض من ربط النفقة بالغرفة
        /// </summary>
        [MaxLength(500)]
        public string? Purpose { get; set; }

        /// <summary>
        /// Amount - المبلغ المرتبط بهذه الغرفة
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? Amount { get; set; }
    }
}

