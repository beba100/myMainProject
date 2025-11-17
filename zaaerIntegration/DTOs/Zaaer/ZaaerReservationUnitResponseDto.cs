using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for reservation unit response via Zaaer integration
    /// </summary>
    public class ZaaerReservationUnitResponseDto
    {
        /// <summary>
        /// Unit ID
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// Reservation ID
        /// </summary>
        public int ReservationId { get; set; }

        /// <summary>
        /// Apartment ID
        /// </summary>
        public int ApartmentId { get; set; }

        /// <summary>
        /// Check-in date
        /// </summary>
        public DateTime CheckInDate { get; set; }

        /// <summary>
        /// Check-out date
        /// </summary>
        public DateTime CheckOutDate { get; set; }

        /// <summary>
        /// Departure date
        /// </summary>
        [JsonPropertyName("departure_date")]
        public DateTime? DepartureDate { get; set; }

        /// <summary>
        /// Number of nights
        /// </summary>
        public int? NumberOfNights { get; set; }

        /// <summary>
        /// Rent amount
        /// </summary>
        public decimal? RentAmount { get; set; }

        /// <summary>
        /// VAT rate
        /// </summary>
        public decimal? VatRate { get; set; }

        /// <summary>
        /// VAT amount
        /// </summary>
        public decimal? VatAmount { get; set; }

        /// <summary>
        /// Lodging tax rate
        /// </summary>
        public decimal? LodgingTaxRate { get; set; }

        /// <summary>
        /// Lodging tax amount
        /// </summary>
        public decimal? LodgingTaxAmount { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; } = "Reserved";

        /// <summary>
        /// Created at
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        /// <summary>
        /// List of day rates for this unit (يوم بيوم)
        /// </summary>
        public List<ZaaerDayRateResponseDto> DayRates { get; set; } = new List<ZaaerDayRateResponseDto>();
    }

    /// <summary>
    /// Day rate response DTO (لإرجاع أسعار الأيام في الاستجابة)
    /// </summary>
    public class ZaaerDayRateResponseDto
    {
        public int RateId { get; set; }
        public int UnitId { get; set; }
        public DateTime NightDate { get; set; }
        public decimal GrossRate { get; set; }
        public decimal? EwaAmount { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? NetAmount { get; set; }
        public bool IsManual { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }
}
