namespace zaaerIntegration.DTOs.Zaaer
{
    public class ZaaerExpenseResponseDto
    {
        public int ExpenseId { get; set; }
        public int HotelId { get; set; }
        public DateTime DateTime { get; set; }
        public string VoucherType { get; set; }
        public string PaidTo { get; set; }
        public string ReceivedBy { get; set; }
        public decimal Amount { get; set; }
        public int? PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? Purpose { get; set; }
        public string? Comment { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}


