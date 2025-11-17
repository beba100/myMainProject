namespace zaaerIntegration.DTOs.Zaaer
{
    public class ZaaerBankResponseDto
    {
        public int BankId { get; set; }
        public string BankNameEn { get; set; }
        public string BankNameAr { get; set; }
        public bool IsActive { get; set; }
        public string? AccountNumber { get; set; }
        public string? Iban { get; set; }
        public string? CurrencyCode { get; set; }
        public string? Description { get; set; }
        public string? SwiftCode { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}


