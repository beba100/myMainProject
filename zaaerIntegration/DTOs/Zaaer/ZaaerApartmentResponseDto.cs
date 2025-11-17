using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for apartment response via Zaaer integration
    /// </summary>
    public class ZaaerApartmentResponseDto
    {
        /// <summary>
        /// Apartment ID
        /// </summary>
        public int ApartmentId { get; set; }

        /// <summary>
        /// Hotel ID
        /// </summary>
        public int HotelId { get; set; }

        /// <summary>
        /// Building ID
        /// </summary>
        public int? BuildingId { get; set; }

        /// <summary>
        /// Floor ID
        /// </summary>
        public int? FloorId { get; set; }

        /// <summary>
        /// Room Type ID
        /// </summary>
        public int? RoomTypeId { get; set; }

        /// <summary>
        /// Apartment Code
        /// </summary>
        public string ApartmentCode { get; set; } = string.Empty;

        /// <summary>
        /// Apartment Name
        /// </summary>
        public string? ApartmentName { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; } = "available";

        /// <summary>
        /// Housekeeping Status (حالة النظافة)
        /// Current housekeeping status of the apartment (e.g., "clean", "dirty", "inspected")
        /// </summary>
        public string? HousekeepingStatus { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
