using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول الطوابق - Floors table
	/// </summary>
	[Table("floors")]
	public class Floor
	{
		[Key]
		[Column("floor_id")]
		public int FloorId { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		/// <summary>
		/// Building foreign key (required for linking floors to buildings)
		/// </summary>
		[Column("building_id")]
		[Required]
		public int BuildingId { get; set; }

		[Column("floor_number")]
		[Required]
		public int FloorNumber { get; set; }

		[Column("floor_name")]
		[MaxLength(100)]
		public string FloorName { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }

		// Navigation properties
		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;

		[ForeignKey("BuildingId")]
		public Building? Building { get; set; }

		public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
	}
}

