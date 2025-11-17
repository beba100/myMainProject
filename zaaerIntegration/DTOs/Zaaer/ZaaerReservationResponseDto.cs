using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    /// <summary>
    /// DTO for reservation response via Zaaer integration
    /// </summary>
    public class ZaaerReservationResponseDto
    {
        /// <summary>
        /// Reservation ID
        /// </summary>
        public int ReservationId { get; set; }

        /// <summary>
        /// Reservation number
        /// </summary>
        public string? ReservationNo { get; set; }

        /// <summary>
        /// Hotel ID
        /// </summary>
        public int HotelId { get; set; }

        /// <summary>
        /// Customer ID
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Reservation date
        /// </summary>
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Rental type: Daily, Monthly, Yearly, InHour
        /// </summary>
        public string? RentalType { get; set; }

        /// <summary>
        /// Number of months for monthly rental type (عدد الشهور للحجز الشهري)
        /// </summary>
        public int? NumberOfMonths { get; set; }

        /// <summary>
        /// Total nights
        /// </summary>
        public int? TotalNights { get; set; }

        /// <summary>
        /// Total penalties added to reservation (يزيد الإجمالي)
        /// </summary>
        public decimal? TotalPenalties { get; set; }

        /// <summary>
        /// Total discounts subtracted from reservation (يقلل الإجمالي)
        /// </summary>
        public decimal? TotalDiscounts { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Amount paid
        /// </summary>
        public decimal? AmountPaid { get; set; }

        /// <summary>
        /// Balance amount
        /// </summary>
        public decimal? BalanceAmount { get; set; }

        /// <summary>
        /// Reservation status
        /// </summary>
        public string Status { get; set; } = "Unconfirmed";

        /// <summary>
        /// Created by user ID
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// Created at
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Subtotal
        /// </summary>
        public decimal? Subtotal { get; set; }

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
        /// Total tax amount
        /// </summary>
        public decimal? TotalTaxAmount { get; set; }

        /// <summary>
        /// Total extra charges (إجمالي الإضافات)
        /// </summary>
        public decimal? TotalExtra { get; set; }

        /// <summary>
        /// Corporate ID
        /// </summary>
        public int? CorporateId { get; set; }

        /// <summary>
        /// Reservation type
        /// </summary>
        public string ReservationType { get; set; } = "Individual";

        /// <summary>
        /// Visit purpose ID
        /// </summary>
        public int? VisitPurposeId { get; set; }

        /// <summary>
        /// Automatic extension enabled (تمديد تلقائي)
        /// When true, the reservation can be extended automatically by the partner system (Zaaer)
        /// </summary>
        public bool? IsAutoExtend { get; set; }

        /// <summary>
        /// Price Type ID (معرف نوع السعر)
        /// The rate type ID sent by Zaaer
        /// </summary>
        public int? PriceTypeId { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }

        /// <summary>
        /// Planned check-in date for the reservation
        /// </summary>
        public DateTime? CheckInDate { get; set; }

        /// <summary>
        /// Planned check-out date for the reservation
        /// </summary>
        public DateTime? CheckOutDate { get; set; }

        /// <summary>
        /// Departure date (actual or expected)
        /// </summary>
        public DateTime? DepartureDate { get; set; }

        /// <summary>
        /// List of reservation units
        /// </summary>
        public List<ZaaerReservationUnitResponseDto> ReservationUnits { get; set; } = new List<ZaaerReservationUnitResponseDto>();
    }
}
