using System.Text.Json;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Models;
using zaaerIntegration.Services.Interfaces;
using zaaerIntegration.Repositories.Implementations;

namespace zaaerIntegration.Services.PartnerQueueing
{
	public interface IPartnerQueueService
	{
		Task EnqueueAsync(EnqueuePartnerRequestDto dto, CancellationToken cancellationToken = default);
		Task<(int pulled, int succeeded, int failed)> RunBatchAsync(int take = 50, CancellationToken cancellationToken = default, bool processAllTenants = true);
	}

	public sealed class PartnerQueueService : IPartnerQueueService
	{
		private const int MaxRetryAttempts = 5;

		private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
        private readonly MasterDbContext _masterDb;
        private readonly ILogger<PartnerQueueService> _logger;
        private readonly ITenantService _tenantService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IReadOnlyDictionary<string, IQueuedOperationHandler> _handlersByKey;
        private readonly IQueueSettingsProvider _queueSettingsProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public PartnerQueueService(
            MasterDbContext masterDb, 
            ILogger<PartnerQueueService> logger, 
            ITenantService tenantService, 
            IServiceProvider serviceProvider, 
            IEnumerable<IQueuedOperationHandler> handlers, 
            IQueueSettingsProvider queueSettingsProvider,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
		{
			_masterDb = masterDb;
			_logger = logger;
            _tenantService = tenantService;
            _serviceProvider = serviceProvider;
            _handlersByKey = handlers.ToDictionary(h => h.Key, h => h, StringComparer.OrdinalIgnoreCase);
            _queueSettingsProvider = queueSettingsProvider;
            _configuration = configuration;
		}

		public async Task EnqueueAsync(EnqueuePartnerRequestDto dto, CancellationToken cancellationToken = default)
		{
			// resolve tenant: prefer dto.HotelId, else read from current request via ITenantService
			Tenant? tenant = null;
			if (dto.HotelId.HasValue)
			{
				tenant = await _masterDb.Tenants
					.AsNoTracking()
					.FirstOrDefaultAsync(t => t.Id == dto.HotelId.Value, cancellationToken);
			}

			if (tenant == null)
			{
				try
				{
					tenant = _tenantService.GetTenant();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[Queue] Failed to resolve tenant from header. OperationKey={OperationKey}, HotelId={HotelId}, Error: {ErrorMessage}", 
						dto.OperationKey, dto.HotelId, ex.Message);
					throw new InvalidOperationException($"Failed to resolve tenant for enqueue operation. Error: {ex.Message}", ex);
				}
			}

			if (tenant == null)
			{
				_logger.LogWarning("[Queue] Attempted enqueue without resolving tenant. OperationKey={OperationKey}, HotelId={HotelId}", dto.OperationKey, dto.HotelId);
				throw new InvalidOperationException("Tenant not found for enqueue operation");
			}

			// Get connection string from tenant service (uses DatabaseName)
			string connectionString;
			try
			{
				connectionString = _tenantService.GetTenantConnectionString();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Queue] Failed to get connection string for tenant: {TenantCode}. Error: {ErrorMessage}", 
					tenant.Code, ex.Message);
				throw new InvalidOperationException($"Failed to get connection string for tenant: {tenant.Code}. Error: {ex.Message}", ex);
			}
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlServer(connectionString)
				.Options;

			await using var db = new ApplicationDbContext(options);
			var queueSettings = _queueSettingsProvider.ResolveForTenant(tenant);

			_logger.LogInformation("[Queue] Enqueue start: Hotel={HotelCode}, OperationKey={OperationKey}, RequestRef={RequestRef}, QueueEnabled={QueueEnabled}", tenant.Code, dto.OperationKey, dto.RequestRef, queueSettings.EnableQueueMode);

			await db.PartnerRequestQueue.AddAsync(new PartnerQueue
			{
				RequestRef = dto.RequestRef,
				Partner = string.IsNullOrWhiteSpace(dto.Partner) ? queueSettings.DefaultPartner : dto.Partner!,
				Operation = dto.Operation,
				PayloadJson = dto.PayloadJson,
                OperationKey = dto.OperationKey,
                TargetId = dto.TargetId,
                PayloadType = dto.PayloadType,
				Status = "Queued",
				HotelId = dto.HotelId,
				CreatedAt = KsaTime.Now
			}, cancellationToken);
			await db.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("[Queue] Enqueue persisted: RequestRef={RequestRef}, Hotel={HotelCode}", dto.RequestRef, tenant.Code);
		}

		public async Task<(int pulled, int succeeded, int failed)> RunBatchAsync(int take = 50, CancellationToken cancellationToken = default, bool processAllTenants = true)
		{
			int pulled = 0, succeeded = 0, failed = 0;

			List<Tenant> tenants;
			if (processAllTenants)
			{
				tenants = await _masterDb.Tenants
					.AsNoTracking()
					.Where(t => t.EnableQueueMode == true)
					.ToListAsync(cancellationToken);
			}
			else
			{
				var current = _tenantService.GetTenant();
				if (current == null)
				{
					// Fallback to all tenants if header missing
				tenants = await _masterDb.Tenants
					.AsNoTracking()
					.Where(t => t.EnableQueueMode == true)
					.ToListAsync(cancellationToken);
				}
				else
				{
					tenants = new List<Tenant> { current };
				}
			}
			foreach (var tenant in tenants)
			{
				var queueSettings = _queueSettingsProvider.ResolveForTenant(tenant);
				if (!queueSettings.EnableQueueMode)
				{
					_logger.LogInformation("[Queue] Skipping tenant {Tenant} because queue mode is disabled by settings", tenant.Code);
					continue;
				}
				_logger.LogInformation("[Queue] Processing batch for tenant {Tenant}, Take={Take}", tenant.Code, take);

				// Build connection string manually for batch processing
				// Since we're processing multiple tenants, we can't rely on the current tenant context
				var connectionString = BuildConnectionStringForTenant(tenant);
				var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseSqlServer(connectionString)
					.Options;
				await using var db = new ApplicationDbContext(options);

				// Ensure queue tables exist for this tenant (create if missing)
				// Note: This is still needed because tenant databases might be new or missing tables
				// However, we only check/create once per tenant per batch run for efficiency
				try
				{
					await EnsureQueueTablesAsync(db, cancellationToken);
					
					// Ensure all required tables exist for tenant operations
					await EnsureRequiredTablesAsync(db, cancellationToken);
				}
				catch (Exception ensureEx)
				{
					// Log but don't fail - tables might already exist or there might be permission issues
					_logger.LogWarning(ensureEx, "Failed to ensure tables exist for tenant {TenantCode}, continuing anyway", tenant.Code);
				}

				List<PartnerQueue> items;
				try
				{
					// Only fetch items that are NOT already completed (exclude Succeeded and Failed)
					// Include Queued, RetryScheduled, and Processing (in case of stuck items)
					items = await db.PartnerRequestQueue
						.Where(q => q.Status != "Succeeded" && (q.Status != "Failed" || q.Attempts < MaxRetryAttempts))
						.OrderBy(q => q.CreatedAt)
						.Take(take)
						.ToListAsync(cancellationToken);
				}
				catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208)
				{
					_logger.LogError(ex, "Queue tables are missing in tenant DB: {Message}", ex.Message);
					continue;
				}

				if (items.Count == 0)
				{
					_logger.LogInformation("[Queue] No pending items for tenant {Tenant}", tenant.Code);
					continue;
				}
				pulled += items.Count;
				_logger.LogInformation("[Queue] Pulled {Count} queued items for tenant {Tenant}", items.Count, tenant.Code);

                foreach (var item in items)
				{
                    try
					{
                        _logger.LogInformation("[Queue] Starting handler for QueueId={QueueId}, RequestRef={RequestRef}, OperationKey={OperationKey}, Status={Status}, Attempts={Attempts}, HotelId={HotelId}, PayloadType={PayloadType}",
                            item.QueueId, item.RequestRef, item.OperationKey, item.Status, item.Attempts, item.HotelId, item.PayloadType);
                        // route by operation string; extend as needed
                        item.Status = "Processing";
                        item.Attempts += 1;
                        item.UpdatedAt = KsaTime.Now;
                        await db.SaveChangesAsync(cancellationToken);

                        using (var diScope = _serviceProvider.CreateScope())
                        {
                            if (!string.IsNullOrWhiteSpace(item.OperationKey) && _handlersByKey.TryGetValue(item.OperationKey!, out var handler))
                            {
                                await handler.HandleAsync(item, db, diScope.ServiceProvider, cancellationToken);
                            }
                            else
                            {
                                throw new InvalidOperationException($"Unknown or missing operation_key for request_ref={item.RequestRef}");
                            }
                        }

                        // Handler succeeded - fetch fresh item from DB to ensure correct tracking
                        _logger.LogInformation("Handler succeeded for queue item {QueueId}, RequestRef={RequestRef}, OperationKey={OperationKey}", 
                            item.QueueId, item.RequestRef, item.OperationKey);
                        
                        var currentQueueItem = await db.PartnerRequestQueue
                            .FirstOrDefaultAsync(q => q.QueueId == item.QueueId, cancellationToken);
                        
                        if (currentQueueItem != null)
                        {
                            _logger.LogInformation("Found queue item {QueueId} in DB, updating status to Succeeded", item.QueueId);
                            currentQueueItem.Status = "Succeeded";
                            currentQueueItem.UpdatedAt = KsaTime.Now;
                            currentQueueItem.LastError = null;
                        }
                        else
                        {
                            // Fallback: update original item if fresh fetch fails
                            _logger.LogWarning("Queue item {QueueId} not found after handler execution, using fallback Update method", item.QueueId);
                            item.Status = "Succeeded";
                            item.UpdatedAt = KsaTime.Now;
                            item.LastError = null;
                            db.PartnerRequestQueue.Update(item);
                        }
                        
                        await db.PartnerRequestLog.AddAsync(new PartnerRequestLog
                        {
                            RequestRef = item.RequestRef,
                            Partner = item.Partner,
                            Operation = item.Operation,
                            Status = "Succeeded",
                            Message = "Success",
                            CreatedAt = KsaTime.Now,
                            HotelId = item.HotelId
                        }, cancellationToken);
                        
                        try
                        {
                            await db.SaveChangesAsync(cancellationToken);
                            _logger.LogInformation("Successfully saved queue item {QueueId} status update and log entry", item.QueueId);
                            succeeded++;
                        }
                        catch (Exception saveEx)
                        {
                            _logger.LogError(saveEx, "CRITICAL: Failed to save queue item {QueueId} status update. RequestRef={RequestRef}, OperationKey={OperationKey}, Error={Error}", 
                                item.QueueId, item.RequestRef, item.OperationKey, saveEx.Message);
                            throw; // Re-throw to be caught by outer catch block
                        }
					}
					catch (Exception ex)
					{
						_logger.LogError(ex,
							"[Queue] Handler failed for QueueId={QueueId}, RequestRef={RequestRef}, OperationKey={OperationKey}, HotelId={HotelId}, PayloadType={PayloadType}, PayloadPreview={PayloadPreview}",
							item.QueueId, item.RequestRef, item.OperationKey, item.HotelId, item.PayloadType, Truncate(item.PayloadJson));
						// On failure, fetch fresh item from DB to ensure correct tracking
						var errorMessage = ex.Message.Length > 2000 ? ex.Message.Substring(0, 2000) : ex.Message;
						
						_logger.LogError(ex, "Handler failed for queue item {QueueId}, RequestRef={RequestRef}, OperationKey={OperationKey}, Error={Error}", 
							item.QueueId, item.RequestRef, item.OperationKey, ex.Message);
						// detach any tracked entities (failed handler may have left entities in Added state causing duplicate saves)
						db.ChangeTracker.Clear();
						
						var currentQueueItem = await db.PartnerRequestQueue
							.FirstOrDefaultAsync(q => q.QueueId == item.QueueId, cancellationToken);
						
						if (currentQueueItem != null)
						{
							_logger.LogInformation("Found queue item {QueueId} in DB, updating status to Failed", item.QueueId);
							currentQueueItem.Status = "Failed";
							currentQueueItem.LastError = errorMessage;
							currentQueueItem.UpdatedAt = KsaTime.Now;
						}
						else
						{
							_logger.LogWarning("Queue item {QueueId} not found after handler failure, using fallback Update method", item.QueueId);
							var fallback = new PartnerQueue
							{
								QueueId = item.QueueId,
								Status = "Failed",
								LastError = errorMessage,
								UpdatedAt = KsaTime.Now
							};
							db.PartnerRequestQueue.Attach(fallback);
							db.Entry(fallback).Property(q => q.Status).IsModified = true;
							db.Entry(fallback).Property(q => q.LastError).IsModified = true;
							db.Entry(fallback).Property(q => q.UpdatedAt).IsModified = true;
						}
						
						await db.PartnerRequestLog.AddAsync(new PartnerRequestLog
						{
							RequestRef = item.RequestRef,
							Partner = item.Partner,
							Operation = item.Operation,
							Status = "Failed",
							Message = errorMessage,
							CreatedAt = KsaTime.Now,
							HotelId = item.HotelId
						}, cancellationToken);
						
						try
						{
							await db.SaveChangesAsync(cancellationToken);
							_logger.LogInformation("Successfully saved queue item {QueueId} failure status and log entry", item.QueueId);
							failed++;
						}
						catch (Exception saveEx)
						{
							_logger.LogError(saveEx, "CRITICAL: Failed to save queue item {QueueId} failure status. RequestRef={RequestRef}, OperationKey={OperationKey}, OriginalError={OriginalError}, SaveError={SaveError}", 
								item.QueueId, item.RequestRef, item.OperationKey, ex.Message, saveEx.Message);
							failed++; // Still count as failed even if save failed
						}
					}
				}
			}

			return (pulled, succeeded, failed);
		}

		private static async Task EnsureQueueTablesAsync(ApplicationDbContext db, CancellationToken cancellationToken)
		{
			const string createSql = @"
IF OBJECT_ID('dbo.partner_request_queue', 'U') IS NULL
BEGIN
  CREATE TABLE dbo.partner_request_queue (
    queue_id INT IDENTITY(1,1) PRIMARY KEY,
    request_ref NVARCHAR(64) NOT NULL,
    partner NVARCHAR(50) NOT NULL,
    operation NVARCHAR(200) NOT NULL,
    payload_json NVARCHAR(MAX) NULL,
    status NVARCHAR(50) NOT NULL DEFAULT 'Queued',
    attempts INT NOT NULL DEFAULT 0,
    last_error NVARCHAR(MAX) NULL,
    next_attempt_at DATETIME NULL,
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    updated_at DATETIME NULL,
    hotel_id INT NULL
  );
  CREATE INDEX IX_partner_request_queue_request_ref ON dbo.partner_request_queue(request_ref);
END;

IF OBJECT_ID('dbo.partner_request_log', 'U') IS NULL
BEGIN
  CREATE TABLE dbo.partner_request_log (
    log_id INT IDENTITY(1,1) PRIMARY KEY,
    request_ref NVARCHAR(64) NULL,
    partner NVARCHAR(50) NULL,
    operation NVARCHAR(200) NULL,
    status NVARCHAR(50) NULL,
    message NVARCHAR(MAX) NULL,
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    hotel_id INT NULL
  );
  CREATE INDEX IX_partner_request_log_request_ref ON dbo.partner_request_log(request_ref);
END;";

			const string alterSql = @"
IF COL_LENGTH('dbo.partner_request_queue','operation_key') IS NULL
  ALTER TABLE dbo.partner_request_queue ADD operation_key NVARCHAR(150) NULL;
IF COL_LENGTH('dbo.partner_request_queue','target_id') IS NULL
  ALTER TABLE dbo.partner_request_queue ADD target_id INT NULL;
IF COL_LENGTH('dbo.partner_request_queue','payload_type') IS NULL
  ALTER TABLE dbo.partner_request_queue ADD payload_type NVARCHAR(200) NULL;";

			await db.Database.ExecuteSqlRawAsync(createSql, cancellationToken);
			await db.Database.ExecuteSqlRawAsync(alterSql, cancellationToken);
		}

		private static async Task EnsureRequiredTablesAsync(ApplicationDbContext db, CancellationToken cancellationToken)
		{
			// Create rate_types and rate_type_unit_items tables
			// Note: We create tables without Foreign Key constraints if referenced tables don't exist
			const string rateTypesSql = @"
-- Create rate_types table if it doesn't exist (without FK if hotel_settings doesn't exist)
IF OBJECT_ID('dbo.rate_types', 'U') IS NULL AND OBJECT_ID('dbo.new_rate_types', 'U') IS NULL
BEGIN
  DECLARE @hasHotelSettings BIT = 0;
  IF OBJECT_ID('dbo.hotel_settings', 'U') IS NOT NULL SET @hasHotelSettings = 1;

  IF @hasHotelSettings = 1
  BEGIN
    -- Create with Foreign Key constraint
    CREATE TABLE [dbo].[rate_types] (
      [id] INT IDENTITY(1,1) NOT NULL,
      [hotel_id] INT NOT NULL,
      [short_code] NVARCHAR(50) NOT NULL,
      [title] NVARCHAR(255) NOT NULL,
      [status] BIT NOT NULL DEFAULT 1,
      [created_at] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
      [updated_at] DATETIME2(7) NULL,
      CONSTRAINT [PK_rate_types] PRIMARY KEY ([id]),
      CONSTRAINT [FK_rate_types_hotel_settings_hotel_id] FOREIGN KEY ([hotel_id]) REFERENCES [dbo].[hotel_settings] ([id]),
      CONSTRAINT [UQ_rate_types_hotel_id_short_code] UNIQUE ([hotel_id], [short_code])
    );
  END
  ELSE
  BEGIN
    -- Create without Foreign Key constraint (hotel_settings doesn't exist)
    CREATE TABLE [dbo].[rate_types] (
      [id] INT IDENTITY(1,1) NOT NULL,
      [hotel_id] INT NOT NULL,
      [short_code] NVARCHAR(50) NOT NULL,
      [title] NVARCHAR(255) NOT NULL,
      [status] BIT NOT NULL DEFAULT 1,
      [created_at] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
      [updated_at] DATETIME2(7) NULL,
      CONSTRAINT [PK_rate_types] PRIMARY KEY ([id]),
      CONSTRAINT [UQ_rate_types_hotel_id_short_code] UNIQUE ([hotel_id], [short_code])
    );
  END;
  CREATE INDEX [IX_rate_types_hotel_id] ON [dbo].[rate_types]([hotel_id]);
END;

-- Create rate_type_unit_items table if it doesn't exist
IF OBJECT_ID('dbo.rate_type_unit_items', 'U') IS NULL
BEGIN
  -- Determine which rate_types table to reference
  DECLARE @rateTypesTable NVARCHAR(128);
  IF OBJECT_ID('dbo.rate_types', 'U') IS NOT NULL 
    SET @rateTypesTable = N'rate_types';
  ELSE IF OBJECT_ID('dbo.new_rate_types', 'U') IS NOT NULL 
    SET @rateTypesTable = N'new_rate_types';
  ELSE 
    RETURN; -- Cannot create rate_type_unit_items without rate_types

  -- Create rate_type_unit_items table
  DECLARE @sql NVARCHAR(MAX) = N'
  CREATE TABLE [dbo].[rate_type_unit_items] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [rate_type_id] INT NOT NULL,
    [unit_type_name] NVARCHAR(100) NOT NULL,
    [rate] DECIMAL(18, 2) NOT NULL,
    [is_enabled] BIT NOT NULL DEFAULT 0,
    [created_at] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [updated_at] DATETIME2(7) NULL,
    CONSTRAINT [PK_rate_type_unit_items] PRIMARY KEY ([id]),
    CONSTRAINT [FK_rate_type_unit_items_rate_types] FOREIGN KEY ([rate_type_id]) REFERENCES [dbo].[' + @rateTypesTable + '] ([id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_rate_type_unit_items_rate_type_id_unit_type_name] UNIQUE ([rate_type_id], [unit_type_name])
  );
  CREATE INDEX [IX_rate_type_unit_items_rate_type_id] ON [dbo].[rate_type_unit_items]([rate_type_id]);';
  
  EXEC sp_executesql @sql;
END;";

			try
			{
				await db.Database.ExecuteSqlRawAsync(rateTypesSql, cancellationToken);
			}
			catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2714 || sqlEx.Number == 1750 || sqlEx.Number == 208 || sqlEx.Number == 1767)
			{
				// Table already exists, constraint already exists, foreign key reference failed, or FK table doesn't exist - ignore
				// Error 2714: Object already exists
				// Error 1750: Could not create constraint
				// Error 208: Invalid object name (foreign key reference)
				// Error 1767: Foreign key references invalid table
			}
		}

		/// <summary>
		/// بناء Connection String لـ Tenant محدد (للـ batch processing)
		/// </summary>
		/// <param name="tenant">الـ Tenant المطلوب</param>
		/// <returns>Connection String</returns>
		private string BuildConnectionStringForTenant(Tenant tenant)
		{
			if (tenant == null)
			{
				_logger.LogError("BuildConnectionStringForTenant: Tenant is null");
				throw new ArgumentNullException(nameof(tenant));
			}

			if (string.IsNullOrWhiteSpace(tenant.DatabaseName))
			{
				_logger.LogError("DatabaseName is null or empty for tenant: {TenantCode} (Id: {TenantId})", 
					tenant.Code, tenant.Id);
				throw new InvalidOperationException(
					$"DatabaseName is not set for tenant: {tenant.Code}. " +
					"Please add DatabaseName to the tenant record in Master Database.");
			}

			// قراءة إعدادات قاعدة البيانات من appsettings.json
			var server = _configuration["TenantDatabase:Server"]?.Trim();
			var userId = _configuration["TenantDatabase:UserId"]?.Trim();
			var password = _configuration["TenantDatabase:Password"]?.Trim();

			// التحقق من وجود جميع الإعدادات المطلوبة
			if (string.IsNullOrWhiteSpace(server))
			{
				_logger.LogError("TenantDatabase:Server is missing in appsettings.json");
				throw new InvalidOperationException(
					"TenantDatabase:Server is not configured in appsettings.json. " +
					"Please add TenantDatabase:Server setting.");
			}

			if (string.IsNullOrWhiteSpace(userId))
			{
				_logger.LogError("TenantDatabase:UserId is missing in appsettings.json");
				throw new InvalidOperationException(
					"TenantDatabase:UserId is not configured in appsettings.json. " +
					"Please add TenantDatabase:UserId setting.");
			}

			if (string.IsNullOrWhiteSpace(password))
			{
				_logger.LogError("TenantDatabase:Password is missing in appsettings.json");
				throw new InvalidOperationException(
					"TenantDatabase:Password is not configured in appsettings.json. " +
					"Please add TenantDatabase:Password setting.");
			}

			// بناء Connection String ديناميكياً
			var connectionString = $"Server={server}; Database={tenant.DatabaseName}; User Id={userId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

			_logger.LogDebug("Built connection string for tenant: {TenantCode}, Database: {DatabaseName}, Server: {Server}", 
				tenant.Code, tenant.DatabaseName, server);

			return connectionString;
		}

		private static string? Truncate(string? value, int maxLength = 500)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
		}
	}

	public sealed class EnqueuePartnerRequestDto
	{
		public string RequestRef { get; set; } = Guid.NewGuid().ToString("N");
		public string Partner { get; set; } = "Zaaer";
		public string Operation { get; set; } = string.Empty;
		public string? PayloadJson { get; set; }
		public int? HotelId { get; set; }
		public string? OperationKey { get; set; }
		public int? TargetId { get; set; }
		public string? PayloadType { get; set; }
	}
}


