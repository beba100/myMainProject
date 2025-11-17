using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    public class ZaaerCreateNtmpDetailsDto
    {
        [Required]
        public int HotelId { get; set; }
        public bool IsActive { get; set; } = true;
        [MaxLength(300)] public string? GatewayApiKey { get; set; }
        [MaxLength(150)] public string? UserName { get; set; }
        [MaxLength(300)] public string? Password { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }

    public class ZaaerUpdateNtmpDetailsDto
    {
        [Required]
        public int DetailsId { get; set; }
        public int? HotelId { get; set; }
        public bool? IsActive { get; set; }
        [MaxLength(300)] public string? GatewayApiKey { get; set; }
        [MaxLength(150)] public string? UserName { get; set; }
        [MaxLength(300)] public string? Password { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }

    public class ZaaerNtmpDetailsResponseDto
    {
        public int DetailsId { get; set; }
        public int HotelId { get; set; }
        public bool IsActive { get; set; }
        public string? GatewayApiKey { get; set; }
        public string? UserName { get; set; }
        public string? PasswordMask { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}


