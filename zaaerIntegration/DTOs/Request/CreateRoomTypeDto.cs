using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new room type
    /// </summary>
    public class CreateRoomTypeDto
    {
        [Required]
        public int HotelId { get; set; }

        [Required]
        [StringLength(200)]
        public string RoomTypeName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? RoomTypeDesc { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Base rate must be a positive number")]
        public decimal? BaseRate { get; set; }
    }
}
