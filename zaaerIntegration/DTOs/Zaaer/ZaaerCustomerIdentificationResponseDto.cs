namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// Response DTO for customer identification via Zaaer integration
    /// </summary>
    public class ZaaerCustomerIdentificationResponseDto
    {
        /// <summary>
        /// Identification ID
        /// </summary>
        public int IdentificationId { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        /// <summary>
        /// Customer ID
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// ID type ID
        /// </summary>
        public int IdTypeId { get; set; }

        /// <summary>
        /// ID number
        /// </summary>
        public string IdNumber { get; set; } = string.Empty;

        /// <summary>
        /// Created at date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated at date
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
