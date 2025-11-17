using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// Create Bank payload (only fields required by Zaaer UI)
    /// </summary>
    public class ZaaerCreateBankDto
    {
        [Required]
        public string BankNameEn { get; set; }

        [Required]
        public string BankNameAr { get; set; }

        public bool IsActive { get; set; } = true;

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


