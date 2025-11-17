using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Tax Category - مجموعة الضرائب
	/// Provides a parent/group/category for taxes. Referenced by taxes.tax_id
	/// </summary>
	[Table("tax_categories")]
	public class TaxCategory
	{
		[Key]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[MaxLength(100)]
		[Column("name")] 
		public string Name { get; set; } = string.Empty;

		[MaxLength(100)]
		[Column("name_ar")] 
		public string? NameAr { get; set; }

		[MaxLength(500)]
		[Column("description")] 
		public string? Description { get; set; }

		[MaxLength(50)]
		[Column("status")] 
		public string? Status { get; set; } = "active";

		[Column("created_at")] 
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		[Column("updated_at")] 
		public DateTime? UpdatedAt { get; set; }
	}
}


