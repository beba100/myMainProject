using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using zaaerIntegration.Converters;

namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// DTO for updating a tax record via Zaaer integration
	/// </summary>
	public class ZaaerUpdateTaxDto
	{
		/// <summary>
		/// Zaaer System ID (معرف Zaaer) - used to find the tax to update
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		/// <summary>
		/// Tax ID (internal ID)
		/// </summary>
		public int? TaxId { get; set; }

		/// <summary>
		/// Hotel ID
		/// </summary>
		public int? HotelId { get; set; }

		/// <summary>
		/// Tax Name (اسم الضريبة)
		/// </summary>
		[StringLength(100)]
		public string? TaxName { get; set; }

		/// <summary>
		/// Tax Rate (نسبة الضريبة) - can be sent as string or number
		/// </summary>
		[JsonConverter(typeof(FlexibleDecimalJsonConverter))]
		public decimal? TaxRate { get; set; }

		/// <summary>
		/// Calculation Method (e.g., "percentage", "fixed")
		/// </summary>
		[StringLength(50)]
		public string? Method { get; set; }

		/// <summary>
		/// Enabled (1 = enabled, 0 = disabled) - can be sent as int or bool
		/// </summary>
		[JsonConverter(typeof(FlexibleNullableBooleanJsonConverter))]
		public bool? Enabled { get; set; }

		/// <summary>
		/// Tax Type (e.g., "VAT", "lodging_tax", "service_tax")
		/// </summary>
		[StringLength(50)]
		public string? TaxType { get; set; }

		/// <summary>
		/// Tax Code (e.g., "VAT", "LODGING_TAX")
		/// </summary>
		[StringLength(50)]
		public string? TaxCode { get; set; }

		/// <summary>
		/// Apply On (where to apply the tax)
		/// </summary>
		[StringLength(100)]
		[JsonPropertyName("applyon")]
		public string? ApplyOn { get; set; }
	}
}

