using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Tax Entity - جدول الضرائب
	/// Represents tax configurations for hotels
	/// </summary>
	[Table("taxes")]
	public class Tax
	{
		[Key]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		[Column("zaaer_id")]
		public int? ZaaerId { get; set; }

		/// <summary>
		/// Hotel ID
		/// </summary>
		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		/// <summary>
		/// Tax Name (اسم الضريبة)
		/// </summary>
		[Column("tax_name")]
		[Required]
		[MaxLength(100)]
		public string TaxName { get; set; } = string.Empty;

		/// <summary>
		/// Tax Type (e.g., "vat", "lodging_tax", "service_tax")
		/// </summary>
		[Column("tax_type")]
		[Required]
		[MaxLength(50)]
		public string TaxType { get; set; } = string.Empty;

		/// <summary>
		/// Tax ID (internal ID, same as Id but for external reference)
		/// </summary>
		[Column("tax_id")]
		public int? TaxId { get; set; }

		// Navigation to tax category/group referenced by tax_id
		[ForeignKey("TaxId")]
		public TaxCategory? TaxCategory { get; set; }

		/// <summary>
		/// Tax Rate (نسبة الضريبة)
		/// </summary>
		[Column("tax_rate", TypeName = "decimal(5,2)")]
		[Required]
		public decimal TaxRate { get; set; }

		/// <summary>
		/// Calculation Method (e.g., "percentage", "fixed")
		/// </summary>
		[Column("method")]
		[MaxLength(50)]
		public string? Method { get; set; }

		/// <summary>
		/// Tax Status - Enabled (1 = enabled, 0 = disabled)
		/// </summary>
		[Column("enabled")]
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// Tax Status (e.g., "active", "inactive") - legacy field
		/// </summary>
		[Column("status")]
		[MaxLength(50)]
		public string? Status { get; set; }

		/// <summary>
		/// Tax Code (e.g., "VAT", "LODGING_TAX")
		/// </summary>
		[Column("tax_code")]
		[MaxLength(50)]
		public string? TaxCode { get; set; }

		/// <summary>
		/// Apply On (where to apply the tax)
		/// </summary>
		[Column("apply_on")]
		[MaxLength(100)]
		public string? ApplyOn { get; set; }

		/// <summary>
		/// Description or notes
		/// </summary>
		[Column("description")]
		[MaxLength(500)]
		public string? Description { get; set; }

		/// <summary>
		/// Created date
		/// </summary>
		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		/// <summary>
		/// Updated date
		/// </summary>
		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;
	}
}

