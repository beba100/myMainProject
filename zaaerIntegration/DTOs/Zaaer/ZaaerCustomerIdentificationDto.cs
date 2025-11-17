using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for customer identification via Zaaer integration
    /// </summary>
    public class ZaaerCustomerIdentificationDto
    {
        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        /// <summary>
        /// Customer ID (for referencing which customer this identification belongs to)
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// ID type ID
        /// </summary>
        [Required]
        public int IdTypeId { get; set; }

        /// <summary>
        /// ID number
        /// </summary>
        [Required]
        [StringLength(50)]
        public string IdNumber { get; set; } = string.Empty;
    }
}
