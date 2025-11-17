using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    public class ZaaerCreateShomoosDetailsDto
    {
        [Required]
        public int HotelId { get; set; }
        public bool IsActive { get; set; } = true;
        [MaxLength(150)] public string? UserId { get; set; }
        [MaxLength(100)] public string? BranchCode { get; set; }
        [MaxLength(300)] public string? BranchSecret { get; set; }
        [MaxLength(20)] public string? LanguageCode { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }

    public class ZaaerUpdateShomoosDetailsDto
    {
        [Required]
        public int DetailsId { get; set; }
        public int? HotelId { get; set; }
        public bool? IsActive { get; set; }
        [MaxLength(150)] public string? UserId { get; set; }
        [MaxLength(100)] public string? BranchCode { get; set; }
        [MaxLength(300)] public string? BranchSecret { get; set; }
        [MaxLength(20)] public string? LanguageCode { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }

    public class ZaaerShomoosDetailsResponseDto
    {
        public int DetailsId { get; set; }
        public int HotelId { get; set; }
        public bool IsActive { get; set; }
        public string? UserId { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchSecretMask { get; set; }
        public string? LanguageCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}


