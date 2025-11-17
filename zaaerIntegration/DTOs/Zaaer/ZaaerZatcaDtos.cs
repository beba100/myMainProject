using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
	public class ZaaerCreateZatcaDetailsDto
	{
		[Required]
		public int HotelId { get; set; }
		[Required, MaxLength(200)] public string CompanyName { get; set; } = string.Empty;
		[MaxLength(100)] public string? TaxNumber { get; set; }
		[MaxLength(100)] public string? GroupTaxId { get; set; }
		[MaxLength(100)] public string? CorporateRegistrationNumber { get; set; }
		[MaxLength(50)] public string? Environment { get; set; }
		[MaxLength(50)] public string? Otp { get; set; }
		[MaxLength(500)] public string? Address { get; set; }
		[MaxLength(200)] public string? StreetName { get; set; }
		[MaxLength(50)] public string? BuildingNumber { get; set; }
		[MaxLength(100)] public string? PlotIdentification { get; set; }
		[MaxLength(100)] public string? CitySubdivisionName { get; set; }
		[MaxLength(100)] public string? City { get; set; }
		[MaxLength(20)] public string? PostalZone { get; set; }
		[MaxLength(100)] public string? CountrySubEntity { get; set; }
		[MaxLength(200)] public string? CompanyRegistrationName { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }
	}

	public class ZaaerUpdateZatcaDetailsDto
	{
		[Required]
		public int DetailsId { get; set; }
		public int? HotelId { get; set; }
		public string? CompanyName { get; set; }
		public string? TaxNumber { get; set; }
		public string? GroupTaxId { get; set; }
		public string? CorporateRegistrationNumber { get; set; }
		public string? Environment { get; set; }
		public string? Otp { get; set; }
		public string? Address { get; set; }
		public string? StreetName { get; set; }
		public string? BuildingNumber { get; set; }
		public string? PlotIdentification { get; set; }
		public string? CitySubdivisionName { get; set; }
		public string? City { get; set; }
		public string? PostalZone { get; set; }
		public string? CountrySubEntity { get; set; }
		public string? CompanyRegistrationName { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }
	}

	public class ZaaerZatcaDetailsResponseDto
	{
		public int DetailsId { get; set; }
		public int HotelId { get; set; }
		public string CompanyName { get; set; } = string.Empty;
		public string? TaxNumber { get; set; }
		public string? GroupTaxId { get; set; }
		public string? CorporateRegistrationNumber { get; set; }
		public string? Environment { get; set; }
		public string? Otp { get; set; }
		public string? Address { get; set; }
		public string? StreetName { get; set; }
		public string? BuildingNumber { get; set; }
		public string? PlotIdentification { get; set; }
		public string? CitySubdivisionName { get; set; }
		public string? City { get; set; }
		public string? PostalZone { get; set; }
		public string? CountrySubEntity { get; set; }
		public string? CompanyRegistrationName { get; set; }

		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }
	}
}


