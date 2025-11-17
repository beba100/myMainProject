using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// جدول أنواع الغرف - Room types table
	/// </summary>
	[Table("room_types")]
	public class RoomType
	{
		[Key]
		[Column("roomtype_id")]
		public int RoomTypeId { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("roomtype_name")]
		[Required]
		[MaxLength(200)]
		public string RoomTypeName { get; set; }

		[Column("roomtype_desc")]
		[MaxLength(500)]
		public string RoomTypeDesc { get; set; }

		[Column("base_rate", TypeName = "decimal(12,2)")]
		public decimal? BaseRate { get; set; }

		[Column("season_rate", TypeName = "decimal(12,2)")]
		public decimal? SeasonRate { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }

		// Navigation properties
		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;
		public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
	}
}

