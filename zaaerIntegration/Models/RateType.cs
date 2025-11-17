using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Rate type header. Contains general info (short_code, title, status); per-unit-type rates are in RateTypeUnitItem.
	/// </summary>
	[Table("rate_types")]
	public class RateType
	{
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("short_code")]
		[MaxLength(50)]
		[Required]
		public string ShortCode { get; set; } = string.Empty;

		[Column("title")]
		[MaxLength(255)]
		[Required]
		public string Title { get; set; } = string.Empty;

		[Column("status")]
		[Required]
		public bool Status { get; set; } = true;

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }

		// Navigation property kept for code usage (manual loading in Service)
		// FK constraint is removed to allow data from Zaaer system without enforced referential integrity
		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;
		public ICollection<RateTypeUnitItem> UnitItems { get; set; } = new List<RateTypeUnitItem>();
	}
}

