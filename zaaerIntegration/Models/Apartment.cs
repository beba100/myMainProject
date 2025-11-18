using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// ���� �����/����� - Apartments/Rooms table
	/// </summary>
	[Table("apartments")]
	public class Apartment
	{
		[Key]
		[Column("apartment_id")]
		public int ApartmentId { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("building_id")]
		public int? BuildingId { get; set; }

		[Column("floor_id")]
		public int? FloorId { get; set; }

		[Column("roomtype_id")]
		public int? RoomTypeId { get; set; }

		[Column("apartment_code")]
		[Required]
		[MaxLength(50)]
		public string ApartmentCode { get; set; }

		[Column("apartment_name")]
		[MaxLength(200)]
		public string ApartmentName { get; set; }

	[Column("status")]
	[MaxLength(50)]
	public string Status { get; set; } = "available";

	/// <summary>
	/// Housekeeping Status (���� �������)
	/// Current housekeeping status of the apartment (e.g., "clean", "dirty", "inspected")
	/// </summary>
	[Column("housekeeping_status")]
	[MaxLength(50)]
	public string? HousekeepingStatus { get; set; }

	/// <summary>
	/// Zaaer System ID (���� Zaaer)
	/// External ID from Zaaer integration system
	/// </summary>
	[Column("zaaer_id")]
	public int? ZaaerId { get; set; }

        // Navigation properties
		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;

        [ForeignKey("BuildingId")]
		public Building? Building { get; set; }

		[ForeignKey("FloorId")]
		public Floor? Floor { get; set; }

		[ForeignKey("RoomTypeId")]
		public RoomType? RoomType { get; set; }

		public ICollection<ReservationUnit> ReservationUnits { get; set; } = new List<ReservationUnit>();
	}
}

