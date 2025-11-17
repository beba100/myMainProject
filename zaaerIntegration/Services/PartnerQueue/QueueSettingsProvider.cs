using FinanceLedgerAPI.Models;
using Microsoft.Extensions.Configuration;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.PartnerQueueing
{
    public record QueueSettings(
        bool EnableQueueMode,
        bool EnableBackgroundWorker,
        int WorkerIntervalSeconds,
        int WorkerBatchSize,
        bool UseMiddleware,
        string DefaultPartner);

    public interface IQueueSettingsProvider
    {
        QueueSettings Defaults { get; }
        QueueSettings GetSettings();
        QueueSettings ResolveForTenant(Tenant? tenant);
    }

    public sealed class QueueSettingsProvider : IQueueSettingsProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ITenantService _tenantService;
        private readonly QueueSettings _defaults;

        public QueueSettingsProvider(IConfiguration configuration, ITenantService tenantService)
        {
            _configuration = configuration;
            _tenantService = tenantService;
            _defaults = BuildDefaults(configuration);
        }

        public QueueSettings Defaults => _defaults;

        public QueueSettings GetSettings()
        {
            var tenant = _tenantService.GetTenant();
            return ResolveForTenant(tenant);
        }

        public QueueSettings ResolveForTenant(Tenant? tenant)
        {
            if (tenant == null)
            {
                return _defaults;
            }

            return new QueueSettings(
                tenant.EnableQueueMode ?? _defaults.EnableQueueMode,
                tenant.EnableQueueWorker ?? _defaults.EnableBackgroundWorker,
                tenant.QueueWorkerIntervalSeconds ?? _defaults.WorkerIntervalSeconds,
                tenant.QueueWorkerBatchSize ?? _defaults.WorkerBatchSize,
                tenant.UseQueueMiddleware ?? _defaults.UseMiddleware,
                string.IsNullOrWhiteSpace(tenant.DefaultPartner) ? _defaults.DefaultPartner : tenant.DefaultPartner!);
        }

        private static QueueSettings BuildDefaults(IConfiguration configuration)
        {
            var section = configuration.GetSection("PartnerQueue");
            var enableMode = section.GetValue<bool>("EnableQueueMode");
            var enableWorker = section.GetValue<bool>("EnableBackgroundWorker");
            var interval = Math.Max(5, section.GetValue<int>("WorkerIntervalSeconds", 180));
            var batch = Math.Max(1, section.GetValue<int>("WorkerBatchSize", 50));
            var useMiddleware = section.GetValue<bool>("UseMiddleware");
            var partner = section.GetValue<string>("DefaultPartner") ?? "Zaaer";
            return new QueueSettings(enableMode, enableWorker, interval, batch, useMiddleware, partner);
        }
    }
}
