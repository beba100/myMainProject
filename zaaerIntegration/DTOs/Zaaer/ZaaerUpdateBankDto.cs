namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// Update Bank payload (only fields required by Zaaer UI)
    /// </summary>
    public class ZaaerUpdateBankDto
    {
        public string? BankNameEn { get; set; }
        public string? BankNameAr { get; set; }
        public bool? IsActive { get; set; }
        public string? AccountNumber { get; set; }
        public string? Iban { get; set; }
        public string? CurrencyCode { get; set; }
        public string? Description { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}


