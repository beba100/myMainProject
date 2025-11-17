using zaaerIntegration.Data;
using zaaerIntegration.Models;

namespace zaaerIntegration.Services.PartnerQueueing
{
	public interface IQueuedOperationHandler
	{
		string Key { get; }
		Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct);
	}
}


