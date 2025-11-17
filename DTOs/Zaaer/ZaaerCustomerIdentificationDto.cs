using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for customer identification via Zaaer integration
    /// </summary>
    public class ZaaerCustomerIdentificationDto
    {
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
