using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Penalty Entity - الجزاء
	/// Represents penalties applied to reservations (late checkout, early checkin, damage fees, etc.)
	/// </summary>
	[Table("penalties")]
	public class Penalty
	{
		[Key]
		[Column("penalty_id")]
		public int PenaltyId { get; set; }

		[Required]
		[Column("hotel_id")]
		public int HotelId { get; set; }

		[Required]
		[Column("reservation_id")]
		public int ReservationId { get; set; }

		[Column("unit_id")]
		public int? UnitId { get; set; }

		// Penalty Details
		[Required]
		[MaxLength(50)]
		[Column("penalty_type")]
		public string PenaltyType { get; set; } = string.Empty;

		[Required]
		[MaxLength(100)]
		[Column("penalty_name")]
		public string PenaltyName { get; set; } = string.Empty;

		[MaxLength(100)]
		[Column("penalty_name_ar")]
		public string? PenaltyNameAr { get; set; }

		[MaxLength(500)]
		[Column("description")]
		public string? Description { get; set; }

		// Calculation Method
		[MaxLength(50)]
		[Column("calculation_method")]
		public string? CalculationMethod { get; set; }

		[Column("calculation_value", TypeName = "decimal(10, 2)")]
		public decimal? CalculationValue { get; set; }

		// Amounts
		[Required]
		[Column("base_amount", TypeName = "decimal(18, 2)")]
		public decimal BaseAmount { get; set; }

		[Column("vat_rate", TypeName = "decimal(5, 2)")]
		public decimal? VatRate { get; set; }

		[Column("vat_amount", TypeName = "decimal(18, 2)")]
		public decimal? VatAmount { get; set; }

		[Required]
		[Column("total_amount", TypeName = "decimal(18, 2)")]
		public decimal TotalAmount { get; set; }

		// Tracking
		[Required]
		[Column("applied_date")]
		public DateTime AppliedDate { get; set; } = DateTime.Now;

		[MaxLength(100)]
		[Column("applied_by")]
		public string? AppliedBy { get; set; }

		[Required]
		[Column("is_active")]
		public bool IsActive { get; set; } = true;

		[Required]
		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[Column("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		[MaxLength(1000)]
		[Column("notes")]
		public string? Notes { get; set; }

		// Navigation Properties
		public virtual Reservation? Reservation { get; set; }
		public virtual ReservationUnit? ReservationUnit { get; set; }
	}

	/// <summary>
	/// Penalty Types Enum
	/// </summary>
	public static class PenaltyTypes
	{
		public const string EarlyCheckIn = "EarlyCheckIn";
		public const string LateCheckOut = "LateCheckOut";
		public const string DamageFee = "DamageFee";
		public const string CancellationFee = "CancellationFee";
		public const string NoShow = "NoShow";
		public const string Other = "Other";
	}

	/// <summary>
	/// Penalty Calculation Methods Enum
	/// </summary>
	public static class PenaltyCalculationMethods
	{
		public const string PercentageOfLastNight = "PercentageOfLastNight";
		public const string PercentageOfTotal = "PercentageOfTotal";
		public const string FixedAmount = "FixedAmount";
	}
}

