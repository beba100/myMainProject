using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new apartment
    /// </summary>
    public class CreateApartmentDto
    {
        [Required]
        public int HotelId { get; set; }

        public int? BuildingId { get; set; }

        public int? FloorId { get; set; }

        public int? RoomTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string ApartmentCode { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ApartmentName { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "available";
    }
}
