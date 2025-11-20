using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Expense
{
    /// <summary>
    /// DTO لإنشاء expense_room جديد
    /// </summary>
    public class CreateExpenseRoomDto
    {
        /// <summary>
        /// Apartment ID (استخدام ApartmentId مباشرة)
        /// </summary>
        public int? ApartmentId { get; set; }

        /// <summary>
        /// Zaaer System ID (استخدام ZaaerId للبحث عن Apartment)
        /// يتم استخدامه إذا كان ApartmentId غير موجود
        /// </summary>
        public int? ZaaerId { get; set; }

        /// <summary>
        /// Category Code (مثل CAT_BUILDING, CAT_RECEPTION, CAT_CORRIDORS)
        /// يتم استخدامه للفئات الخاصة بدلاً من ZaaerId
        /// </summary>
        public string? CategoryCode { get; set; }

        /// <summary>
        /// Purpose - الغرض من ربط النفقة بالغرفة
        /// يتم ملؤه عندما يختار المستخدم غرفة من dropdown
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

