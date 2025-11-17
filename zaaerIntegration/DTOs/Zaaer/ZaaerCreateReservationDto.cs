using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using zaaerIntegration.Converters;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for creating a reservation via Zaaer integration
    /// </summary>
    public class ZaaerCreateReservationDto
    {
        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        /// <summary>
        /// Reservation number
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ReservationNo { get; set; } = string.Empty;

        /// <summary>
        /// Hotel ID
        /// </summary>
        [Required]
        public int HotelId { get; set; }

        /// <summary>
        /// Customer ID
        /// </summary>
        [Required]
        public int CustomerId { get; set; }

        /// <summary>
        /// Reservation date
        /// </summary>
        [Required]
        [JsonConverter(typeof(FlexibleDateTimeJsonConverter))]
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Rental type: daily, monthly, yearly, InHour
        /// </summary>
        [StringLength(20)]
        public string? RentalType { get; set; }

        /// <summary>
        /// Number of months for monthly rental type (عدد الشهور للحجز الشهري)
        /// </summary>
        public int? NumberOfMonths { get; set; }

        /// <summary>
        /// Total penalties to be added to the reservation total
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? TotalPenalties { get; set; }

        /// <summary>
        /// Total discounts to be subtracted from the reservation total
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? TotalDiscounts { get; set; }

        /// <summary>
        /// Total nights (calculated field)
        /// </summary>
        public int? TotalNights { get; set; }

        /// <summary>
        /// Total amount https://aleairy.tryasp.net/api/zaaer/Apartment/466
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Amount paid
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? AmountPaid { get; set; }

        /// <summary>
        /// Balance amount
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? BalanceAmount { get; set; }

        /// <summary>
        /// Reservation status
        /// </summary>
        [StringLength(50)]
        public string Status { get; set; } = "Unconfirmed";

        /// <summary>
        /// Created by user ID
        /// </summary>
        [JsonConverter(typeof(NullableIntJsonConverter))]
        public int? CreatedBy { get; set; }

        /// <summary>
        /// External reference number (usually same as ZaaerId)
        /// </summary>
        [JsonConverter(typeof(NullableIntJsonConverter))]
        public int? ExternalRefNo { get; set; }

        /// <summary>
        /// Subtotal
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? Subtotal { get; set; }

        /// <summary>
        /// VAT rate
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? VatRate { get; set; }

        /// <summary>
        /// VAT amount
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? VatAmount { get; set; }

        /// <summary>
        /// Lodging tax rate
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? LodgingTaxRate { get; set; }

        /// <summary>
        /// Lodging tax amount
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? LodgingTaxAmount { get; set; }

        /// <summary>
        /// Total tax amount
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? TotalTaxAmount { get; set; }

        /// <summary>
        /// Total extra charges (إجمالي الإضافات)
        /// </summary>
        [JsonConverter(typeof(FlexibleDecimalJsonConverter))]
        public decimal? TotalExtra { get; set; }

        /// <summary>
        /// Corporate ID
        /// </summary>
        public int? CorporateId { get; set; }

        /// <summary>
        /// Reservation type: individual, corporate
        /// </summary>
        [StringLength(20)]
        public string ReservationType { get; set; } = "individual";

        /// <summary>
        /// Visit purpose ID
        /// </summary>
        public int? VisitPurposeId { get; set; }

        /// <summary>
        /// Automatic extension enabled (تمديد تلقائي)
        /// When true, the reservation can be extended automatically by the partner system (Zaaer)
        /// JSON property name: isAutoExtend (camelCase)
        /// Accepts: true/false (boolean) or 1/0 (integer)
        /// </summary>
        [JsonPropertyName("isAutoExtend")]
        [JsonConverter(typeof(FlexibleNullableBooleanJsonConverter))]
        public bool? IsAutoExtend { get; set; }

        /// <summary>
        /// Price Type ID (معرف نوع السعر)
        /// The rate type ID sent by Zaaer
        /// JSON property name: priceTypeId (camelCase)
        /// </summary>
        [JsonPropertyName("priceTypeId")]
        public int? PriceTypeId { get; set; }

        /// <summary>
        /// Planned check-in date for the reservation
        /// </summary>
        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? CheckInDate { get; set; }

        /// <summary>
        /// Planned check-out date for the reservation
        /// </summary>
        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? CheckOutDate { get; set; }

        /// <summary>
        /// Departure date (actual or expected)
        /// </summary>
        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? DepartureDate { get; set; }

        /// <summary>
        /// List of reservation units
        /// </summary>
        public List<ZaaerReservationUnitDto> ReservationUnits { get; set; } = new List<ZaaerReservationUnitDto>();
    }
}
