using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using zaaerIntegration.Data;
using Microsoft.EntityFrameworkCore;

namespace zaaerIntegration.Services.PartnerQueueing
{
	/// <summary>
	/// Background worker that periodically processes queued partner requests.
	/// </summary>
	public sealed class PartnerQueueBackgroundWorker : BackgroundService
	{
		private readonly ILogger<PartnerQueueBackgroundWorker> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IConfiguration _configuration;

		public PartnerQueueBackgroundWorker(
			ILogger<PartnerQueueBackgroundWorker> logger,
			IServiceProvider serviceProvider,
			IConfiguration configuration)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_configuration = configuration;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			// Read configuration directly (IConfiguration is singleton-safe)
			var section = _configuration.GetSection("PartnerQueue");
			var enabled = section.GetValue<bool>("EnableBackgroundWorker");
			
			if (!enabled)
			{
				// Create a scope to access scoped MasterDbContext
				using var scope = _serviceProvider.CreateScope();
				var masterDb = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
				enabled = await masterDb.Tenants.AsNoTracking().AnyAsync(t => t.EnableQueueWorker == true, stoppingToken);
				if (!enabled)
				{
					_logger.LogInformation("PartnerQueue background worker is disabled by configuration and no tenant override is enabled.");
					return;
				}
			}

			var intervalSeconds = Math.Max(5, section.GetValue<int>("WorkerIntervalSeconds", 180));
			var batchSize = Math.Max(1, section.GetValue<int>("WorkerBatchSize", 50));

			_logger.LogInformation("PartnerQueue background worker started. Interval={Interval}s, BatchSize={Batch}", intervalSeconds, batchSize);

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _serviceProvider.CreateScope();
					var service = scope.ServiceProvider.GetRequiredService<IPartnerQueueService>();
					var (pulled, succeeded, failed) = await service.RunBatchAsync(batchSize, stoppingToken);
					if (pulled > 0)
					{
						_logger.LogInformation("PartnerQueue batch: pulled={Pulled}, succeeded={Succeeded}, failed={Failed}", pulled, succeeded, failed);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "PartnerQueue background worker error: {Message}", ex.Message);
				}

				await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
			}
		}
	}
}


