using System.ComponentModel.DataAnnotations;

namespace zaaerIntegration.DTOs.Zaaer
{
    public class ZaaerReservationUnitDayRateItem
    {
        [Required]
        public int UnitId { get; set; }
        [Required]
        public DateTime NightDate { get; set; }
        [Required]
        public decimal GrossRate { get; set; }
        public decimal? EwaAmount { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? NetAmount { get; set; }
    }

    public class ZaaerReservationRatesResponseDto : ZaaerReservationUnitDayRateItem
    {
        public int RateId { get; set; }
        public int ReservationId { get; set; }

        /// <summary>
        /// Zaaer System ID (معرف Zaaer)
        /// External ID from Zaaer integration system
        /// </summary>
        public int? ZaaerId { get; set; }
    }

    public class ZaaerReservationRatesUpsertDto
    {
        [Required]
        public List<ZaaerReservationUnitDayRateItem> Items { get; set; } = new();
        public decimal? EwaPercent { get; set; }
        public decimal? VatPercent { get; set; }
    }

    public class ZaaerApplySameAmountDto
    {
        [Required]
        public decimal Amount { get; set; }
        public int? UnitId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? EwaPercent { get; set; }
        public decimal? VatPercent { get; set; }
    }
}


