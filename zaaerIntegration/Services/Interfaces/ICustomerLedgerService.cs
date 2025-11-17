using System.Threading;
using System.Threading.Tasks;
using FinanceLedgerAPI.Models;
using zaaerIntegration.Models;

namespace zaaerIntegration.Services.Interfaces
{
	/// <summary>
	/// Customer ledger service contract.
	/// خدمة دفتر الأستاذ لحسابات العملاء.
	/// </summary>
	public interface ICustomerLedgerService
	{
		/// <summary>
		/// Sync ledger entries for a payment receipt (create or update).
		/// مزامنة دفتر الأستاذ لسند القبض (إنشاء أو تحديث).
		/// </summary>
		Task SyncReceiptAsync(PaymentReceipt receipt, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create reversing effect when a receipt is cancelled.
		/// إنشاء حركة عكسية عند إلغاء السند.
		/// </summary>
		Task CancelReceiptAsync(PaymentReceipt receipt, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sync ledger entries for reservation charges.
		/// مزامنة دفتر الأستاذ لرسوم الحجز.
		/// </summary>
		Task SyncReservationAsync(Reservation reservation, CancellationToken cancellationToken = default);
	}
}

