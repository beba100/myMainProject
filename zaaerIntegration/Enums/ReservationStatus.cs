namespace FinanceLedgerAPI.Enums
{
	/// <summary>
	/// Reservation Status Enum
	/// حالات الحجز
	/// </summary>
	public enum ReservationStatus
	{
		/// <summary>
		/// Unconfirmed - غير مؤكد (Pending)
		/// </summary>
		Unconfirmed = 0,

		/// <summary>
		/// Confirmed - مؤكد
		/// </summary>
		Confirmed = 1,

		/// <summary>
		/// Checked-In - تم تسجيل الدخول
		/// </summary>
		CheckedIn = 2,

		/// <summary>
		/// Checked-Out - تم تسجيل الخروج
		/// </summary>
		CheckedOut = 3,

		/// <summary>
		/// Cancelled - ملغي
		/// </summary>
		Cancelled = 4
	}

	/// <summary>
	/// Reservation Status Helper
	/// مساعد حالات الحجز
	/// </summary>
	public static class ReservationStatusHelper
	{
		/// <summary>
		/// Get status display name
		/// الحصول على اسم الحالة للعرض
		/// </summary>
		public static string GetDisplayName(ReservationStatus status)
		{
			return status switch
			{
				ReservationStatus.Unconfirmed => "Unconfirmed",
				ReservationStatus.Confirmed => "Confirmed",
				ReservationStatus.CheckedIn => "Checked-In",
				ReservationStatus.CheckedOut => "Checked-Out",
				ReservationStatus.Cancelled => "Cancelled",
				_ => "Unknown"
			};
		}

		/// <summary>
		/// Get status display name in Arabic
		/// الحصول على اسم الحالة بالعربية
		/// </summary>
		public static string GetDisplayNameAr(ReservationStatus status)
		{
			return status switch
			{
				ReservationStatus.Unconfirmed => "غير مؤكد",
				ReservationStatus.Confirmed => "مؤكد",
				ReservationStatus.CheckedIn => "تم تسجيل الدخول",
				ReservationStatus.CheckedOut => "تم تسجيل الخروج",
				ReservationStatus.Cancelled => "ملغي",
				_ => "غير معروف"
			};
		}

		/// <summary>
		/// Get status color for UI
		/// الحصول على لون الحالة للواجهة
		/// </summary>
		public static string GetStatusColor(ReservationStatus status)
		{
			return status switch
			{
				ReservationStatus.Unconfirmed => "orange",    // Orange for Unconfirmed
				ReservationStatus.Confirmed => "blue",        // Blue for Confirmed
				ReservationStatus.CheckedIn => "green",       // Green for Checked-In
				ReservationStatus.CheckedOut => "red",        // Red for Checked-Out
				ReservationStatus.Cancelled => "grey",        // Grey for Cancelled
				_ => "default"
			};
		}

		/// <summary>
		/// Check if status allows check-in
		/// التحقق من إمكانية تسجيل الدخول
		/// </summary>
		public static bool CanCheckIn(ReservationStatus status)
		{
			return status == ReservationStatus.Confirmed;
		}

		/// <summary>
		/// Check if status allows check-out
		/// التحقق من إمكانية تسجيل الخروج
		/// </summary>
		public static bool CanCheckOut(ReservationStatus status)
		{
			return status == ReservationStatus.CheckedIn;
		}

		/// <summary>
		/// Check if status allows cancellation
		/// التحقق من إمكانية الإلغاء
		/// </summary>
		public static bool CanCancel(ReservationStatus status)
		{
			return status == ReservationStatus.Unconfirmed || status == ReservationStatus.Confirmed;
		}
	}
}
