namespace FinanceLedgerAPI.Enums
{
	/// <summary>
	/// Reservation Unit Status Enum
	/// حالات وحدات الحجز
	/// </summary>
	public enum ReservationUnitStatus
	{
		/// <summary>
		/// Reserved - محجوز
		/// </summary>
		Reserved = 0,

		/// <summary>
		/// Checked-In - تم تسجيل الدخول
		/// </summary>
		CheckedIn = 1,

		/// <summary>
		/// Checked-Out - تم تسجيل الخروج
		/// </summary>
		CheckedOut = 2,

		/// <summary>
		/// Cancelled - ملغي
		/// </summary>
		Cancelled = 3,

		/// <summary>
		/// No-Show - لم يحضر
		/// </summary>
		NoShow = 4,

		/// <summary>
		/// Maintenance - صيانة
		/// </summary>
		Maintenance = 5,

		/// <summary>
		/// Available - متاح
		/// </summary>
		Available = 6
	}

	/// <summary>
	/// Reservation Unit Status Helper
	/// مساعد حالات وحدات الحجز
	/// </summary>
	public static class ReservationUnitStatusHelper
	{
		/// <summary>
		/// Get status display name
		/// الحصول على اسم الحالة للعرض
		/// </summary>
		public static string GetDisplayName(ReservationUnitStatus status)
		{
			return status switch
			{
				ReservationUnitStatus.Reserved => "Reserved",
				ReservationUnitStatus.CheckedIn => "Checked-In",
				ReservationUnitStatus.CheckedOut => "Checked-Out",
				ReservationUnitStatus.Cancelled => "Cancelled",
				ReservationUnitStatus.NoShow => "No-Show",
				ReservationUnitStatus.Maintenance => "Maintenance",
				ReservationUnitStatus.Available => "Available",
				_ => "Unknown"
			};
		}

		/// <summary>
		/// Get status display name in Arabic
		/// الحصول على اسم الحالة بالعربية
		/// </summary>
		public static string GetDisplayNameAr(ReservationUnitStatus status)
		{
			return status switch
			{
				ReservationUnitStatus.Reserved => "محجوز",
				ReservationUnitStatus.CheckedIn => "تم تسجيل الدخول",
				ReservationUnitStatus.CheckedOut => "تم تسجيل الخروج",
				ReservationUnitStatus.Cancelled => "ملغي",
				ReservationUnitStatus.NoShow => "لم يحضر",
				ReservationUnitStatus.Maintenance => "صيانة",
				ReservationUnitStatus.Available => "متاح",
				_ => "غير معروف"
			};
		}

		/// <summary>
		/// Get status color for UI
		/// الحصول على لون الحالة للواجهة
		/// </summary>
		public static string GetStatusColor(ReservationUnitStatus status)
		{
			return status switch
			{
				ReservationUnitStatus.Reserved => "blue",        // Blue for Reserved
				ReservationUnitStatus.CheckedIn => "green",      // Green for Checked-In
				ReservationUnitStatus.CheckedOut => "red",       // Red for Checked-Out
				ReservationUnitStatus.Cancelled => "grey",       // Grey for Cancelled
				ReservationUnitStatus.NoShow => "orange",        // Orange for No-Show
				ReservationUnitStatus.Maintenance => "yellow",  // Yellow for Maintenance
				ReservationUnitStatus.Available => "lightblue", // Light Blue for Available
				_ => "default"
			};
		}

		/// <summary>
		/// Check if status allows check-in
		/// التحقق من إمكانية تسجيل الدخول
		/// </summary>
		public static bool CanCheckIn(ReservationUnitStatus status)
		{
			return status == ReservationUnitStatus.Reserved;
		}

		/// <summary>
		/// Check if status allows check-out
		/// التحقق من إمكانية تسجيل الخروج
		/// </summary>
		public static bool CanCheckOut(ReservationUnitStatus status)
		{
			return status == ReservationUnitStatus.CheckedIn;
		}

		/// <summary>
		/// Check if status allows cancellation
		/// التحقق من إمكانية الإلغاء
		/// </summary>
		public static bool CanCancel(ReservationUnitStatus status)
		{
			return status == ReservationUnitStatus.Reserved || status == ReservationUnitStatus.CheckedIn;
		}

		/// <summary>
		/// Check if status allows maintenance
		/// التحقق من إمكانية وضع الصيانة
		/// </summary>
		public static bool CanSetMaintenance(ReservationUnitStatus status)
		{
			return status == ReservationUnitStatus.Available || status == ReservationUnitStatus.Reserved;
		}

		/// <summary>
		/// Check if status allows reservation
		/// التحقق من إمكانية الحجز
		/// </summary>
		public static bool CanReserve(ReservationUnitStatus status)
		{
			return status == ReservationUnitStatus.Available;
		}

		/// <summary>
		/// Check if status is active (occupied or in use)
		/// التحقق من أن الحالة نشطة (مشغولة أو قيد الاستخدام)
		/// </summary>
		public static bool IsActive(ReservationUnitStatus status)
		{
			return status == ReservationUnitStatus.Reserved || status == ReservationUnitStatus.CheckedIn;
		}

		/// <summary>
		/// Check if status is available for new reservations
		/// التحقق من أن الحالة متاحة للحجوزات الجديدة
		/// </summary>
		public static bool IsAvailableForReservation(ReservationUnitStatus status)
		{
			return status == ReservationUnitStatus.Available;
		}

		/// <summary>
		/// Get next possible statuses from current status
		/// الحصول على الحالات الممكنة التالية من الحالة الحالية
		/// </summary>
		public static List<ReservationUnitStatus> GetNextPossibleStatuses(ReservationUnitStatus currentStatus)
		{
			return currentStatus switch
			{
				ReservationUnitStatus.Reserved => new List<ReservationUnitStatus> 
				{ 
					ReservationUnitStatus.CheckedIn, 
					ReservationUnitStatus.Cancelled, 
					ReservationUnitStatus.NoShow,
					ReservationUnitStatus.Maintenance
				},
				ReservationUnitStatus.CheckedIn => new List<ReservationUnitStatus> 
				{ 
					ReservationUnitStatus.CheckedOut, 
					ReservationUnitStatus.Cancelled 
				},
				ReservationUnitStatus.CheckedOut => new List<ReservationUnitStatus> 
				{ 
					ReservationUnitStatus.Available 
				},
				ReservationUnitStatus.Cancelled => new List<ReservationUnitStatus> 
				{ 
					ReservationUnitStatus.Available 
				},
				ReservationUnitStatus.NoShow => new List<ReservationUnitStatus> 
				{ 
					ReservationUnitStatus.Available 
				},
				ReservationUnitStatus.Maintenance => new List<ReservationUnitStatus> 
				{ 
					ReservationUnitStatus.Available 
				},
				ReservationUnitStatus.Available => new List<ReservationUnitStatus> 
				{ 
					ReservationUnitStatus.Reserved, 
					ReservationUnitStatus.Maintenance 
				},
				_ => new List<ReservationUnitStatus>()
			};
		}
	}
}
