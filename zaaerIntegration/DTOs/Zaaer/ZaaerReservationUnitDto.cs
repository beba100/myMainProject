using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using zaaerIntegration.Converters;

namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// Minimal reservation unit DTO used by existing Zaaer create/update requests.
	/// The server computes all financial fields; the client sends only identifiers and dates.
	/// </summary>
	public class ZaaerReservationUnitDto
	{
		/// <summary>
		/// Zaaer System ID (معرف Zaaer)
		/// External ID from Zaaer integration system
		/// </summary>
		public int? ZaaerId { get; set; }

		/// <summary>
		/// For update flows: carries the existing unit identifier mapped in current logic.
		/// When creating a new unit inside an update payload, send 0.
		/// </summary>
		public int ReservationId { get; set; } = 0;

		[Required]
		public int ApartmentId { get; set; }

		[Required]
		[JsonConverter(typeof(FlexibleDateTimeJsonConverter))]
		public DateTime CheckInDate { get; set; }

		[Required]
		[JsonConverter(typeof(FlexibleDateTimeJsonConverter))]
		public DateTime CheckOutDate { get; set; }

		/// <summary>
		/// Actual or expected departure date if different from check-out.
		/// </summary>
		[JsonConverter(typeof(NullableDateTimeJsonConverter))]
		public DateTime? DepartureDate { get; set; }

		/// <summary>
		/// Number of nights as sent by Zaaer (no server-side calculation).
		/// </summary>
		public int? NumberOfNights { get; set; }

		/// <summary>
		/// Optional partner-sent rent amount (already includes partner-side calculations as they desire).
		/// The server will not perform tax calculations when provided.
		/// </summary>
		[JsonConverter(typeof(FlexibleDecimalJsonConverter))]
		public decimal? RentAmount { get; set; }

		/// <summary>
		/// VAT rate sent by Zaaer for this unit (if any).
		/// </summary>
		[JsonConverter(typeof(FlexibleDecimalJsonConverter))]
		public decimal? VatRate { get; set; }

		/// <summary>
		/// VAT amount sent by Zaaer for this unit (if any).
		/// </summary>
		[JsonConverter(typeof(FlexibleDecimalJsonConverter))]
		public decimal? VatAmount { get; set; }

		/// <summary>
		/// Lodging tax rate sent by Zaaer for this unit (if any).
		/// </summary>
		[JsonConverter(typeof(FlexibleDecimalJsonConverter))]
		public decimal? LodgingTaxRate { get; set; }

		/// <summary>
		/// Lodging tax amount sent by Zaaer for this unit (if any).
		/// </summary>
		[JsonConverter(typeof(FlexibleDecimalJsonConverter))]
		public decimal? LodgingTaxAmount { get; set; }

		/// <summary>
		/// Total amount (gross) sent by Zaaer for this unit (if any).
		/// </summary>
		[JsonConverter(typeof(FlexibleDecimalJsonConverter))]
		public decimal? TotalAmount { get; set; }

		/// <summary>
		/// Unit status sent by Zaaer.
		/// </summary>
		public string? Status { get; set; }

		/// <summary>
		/// Optional partner-sent day-rate breakdown for this unit. When provided, the server will
		/// persist these rows as-is without performing any tax calculations.
		/// </summary>
		public List<ZaaerProvidedDayRateDto>? DayRates { get; set; }
	}

	/// <summary>
	/// Incoming day-rate item from partner (does not require UnitId; it will be assigned after creation).
	/// </summary>
	public class ZaaerProvidedDayRateDto
	{
		[Required]
		public DateTime NightDate { get; set; }
		[Required]
		public decimal GrossRate { get; set; }
		public decimal? EwaAmount { get; set; }
		public decimal? VatAmount { get; set; }
		public decimal? NetAmount { get; set; }
	}
}


