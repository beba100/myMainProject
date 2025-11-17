using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for updating an apartment via Zaaer integration
    /// </summary>
    public class ZaaerUpdateApartmentDto
    {
        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        /// <summary>
        /// Hotel ID
        /// </summary>
        public int? HotelId { get; set; }

        /// <summary>
        /// Building ID (can be 0 or null)
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
        [StringLength(50)]
        public string? ApartmentCode { get; set; }

        /// <summary>
        /// Apartment Name
        /// </summary>
        [StringLength(200)]
        public string? ApartmentName { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [StringLength(50)]
        public string? Status { get; set; }

        /// <summary>
        /// Housekeeping Status (حالة النظافة)
        /// Current housekeeping status of the apartment (e.g., "clean", "dirty", "inspected")
        /// </summary>
        [StringLength(50)]
        public string? HousekeepingStatus { get; set; }
    }
}
