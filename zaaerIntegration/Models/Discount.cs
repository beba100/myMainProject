using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceLedgerAPI.Models
{
	/// <summary>
	/// Discount Entity - الخصم
	/// Represents discounts applied to reservations (promotional, loyalty, staff, negotiated)
	/// </summary>
	[Table("discounts")]
	public class Discount
	{
		[Key]
		[Column("discount_id")]
		public int DiscountId { get; set; }

		[Required]
		[Column("hotel_id")]
		public int HotelId { get; set; }

		[Required]
		[Column("reservation_id")]
		public int ReservationId { get; set; }

		[Column("unit_id")]
		public int? UnitId { get; set; }

		// Discount Details
		[Required]
		[MaxLength(50)]
		[Column("discount_type")]
		public string DiscountType { get; set; } = string.Empty;

		[MaxLength(50)]
		[Column("discount_code")]
		public string? DiscountCode { get; set; }

		[Required]
		[MaxLength(100)]
		[Column("discount_name")]
		public string DiscountName { get; set; } = string.Empty;

		[MaxLength(100)]
		[Column("discount_name_ar")]
		public string? DiscountNameAr { get; set; }

		[MaxLength(500)]
		[Column("description")]
		public string? Description { get; set; }

		// Application Target
		[Required]
		[MaxLength(20)]
		[Column("apply_on")]
		public string ApplyOn { get; set; } = string.Empty;

		// Calculation Method
		[Required]
		[MaxLength(20)]
		[Column("calculation_method")]
		public string CalculationMethod { get; set; } = string.Empty;

		[Required]
		[Column("calculation_value", TypeName = "decimal(10, 2)")]
		public decimal CalculationValue { get; set; }

		// Applied Amount
		[Required]
		[Column("discount_amount", TypeName = "decimal(18, 2)")]
		public decimal DiscountAmount { get; set; }

		// Tax Handling
		[Required]
		[Column("is_before_tax")]
		public bool IsBeforeTax { get; set; } = true;

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
	/// Discount Types Enum
	/// </summary>
	public static class DiscountTypes
	{
		public const string Promotional = "Promotional";
		public const string Loyalty = "Loyalty";
		public const string Staff = "Staff";
		public const string Negotiated = "Negotiated";
		public const string GroupBooking = "GroupBooking";
		public const string LongStay = "LongStay";
		public const string Other = "Other";
	}

	/// <summary>
	/// Discount Application Target Enum
	/// </summary>
	public static class DiscountApplyOn
	{
		public const string Rent = "Rent";
		public const string Extra = "Extra";
		public const string Total = "Total";
	}

	/// <summary>
	/// Discount Calculation Methods Enum
	/// </summary>
	public static class DiscountCalculationMethods
	{
		public const string Amount = "Amount";
		public const string Percentage = "Percentage";
	}
}

