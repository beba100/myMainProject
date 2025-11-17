using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Rate type unit item. Stores specific rates for different unit types within a rate type.
	/// </summary>
	[Table("rate_type_unit_items")]
	public class RateTypeUnitItem
	{
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("rate_type_id")]
		[Required]
		public int RateTypeId { get; set; }

		[Column("unit_type_name")]
		[MaxLength(100)]
		[Required]
		public string UnitTypeName { get; set; } = string.Empty;

		[Column("rate", TypeName = "decimal(18,2)")]
		[Required]
		public decimal Rate { get; set; }

		[Column("is_enabled")]
		[Required]
		public bool IsEnabled { get; set; } = false;

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		// Navigation property kept for code usage (manual loading in Service)
		// FK constraint is removed to allow data from Zaaer system without enforced referential integrity
		// Note: Foreign Key constraint is removed in ApplicationDbContext to allow rate_type_id values that don't exist in rate_types table
		// [ForeignKey("RateTypeId")] - Removed to prevent FK constraint creation
		public RateType? RateType { get; set; }
	}
}
