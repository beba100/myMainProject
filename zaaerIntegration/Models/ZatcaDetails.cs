using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// ZATCA (e-invoicing) details per hotel
	/// </summary>
	[Table("zatca_details")]
	public class ZatcaDetails
	{
		[Key]
		[Column("details_id")]
		public int DetailsId { get; set; }

		[Column("hotel_id")]
		[Required]
		public int HotelId { get; set; }

		[Column("company_name")]
		[MaxLength(200)]
		[Required]
		public string CompanyName { get; set; } = string.Empty;

		[Column("tax_number")]
		[MaxLength(100)]
		public string? TaxNumber { get; set; }

		[Column("group_tax_id")]
		[MaxLength(100)]
		public string? GroupTaxId { get; set; }

		[Column("corporate_registration_number")]
		[MaxLength(100)]
		public string? CorporateRegistrationNumber { get; set; }

		[Column("environment")]
		[MaxLength(50)]
		public string? Environment { get; set; }

		[Column("otp")]
		[MaxLength(50)]
		public string? Otp { get; set; }

		[Column("address")]
		[MaxLength(500)]
		public string? Address { get; set; }

		[Column("street_name")]
		[MaxLength(200)]
		public string? StreetName { get; set; }

		[Column("building_number")]
		[MaxLength(50)]
		public string? BuildingNumber { get; set; }

		[Column("plot_identification")]
		[MaxLength(100)]
		public string? PlotIdentification { get; set; }

		[Column("city_subdivision_name")]
		[MaxLength(100)]
		public string? CitySubdivisionName { get; set; }

		[Column("city")]
		[MaxLength(100)]
		public string? City { get; set; }

		[Column("postal_zone")]
		[MaxLength(20)]
		public string? PostalZone { get; set; }

		[Column("country_sub_entity")]
		[MaxLength(100)]
		public string? CountrySubEntity { get; set; }

		[Column("company_registration_name")]
		[MaxLength(200)]
		public string? CompanyRegistrationName { get; set; }

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

		[ForeignKey("HotelId")]
		public HotelSettings HotelSettings { get; set; } = null!;
	}
}


