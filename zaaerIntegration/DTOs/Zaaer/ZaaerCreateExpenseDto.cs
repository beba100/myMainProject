using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    public class ZaaerCreateExpenseDto
    {
        [Required]
        public int HotelId { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        [Required]
        public string VoucherType { get; set; }
        [Required]
        public string PaidTo { get; set; }
        [Required]
        public string ReceivedBy { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public int? PaymentMethodId { get; set; }
        public string? Purpose { get; set; }
        public string? Comment { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}


