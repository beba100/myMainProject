using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Models;
using zaaerIntegration.Services.Interfaces;
using zaaerIntegration.Services.PartnerQueueing;

namespace zaaerIntegration.Controllers
{
	[ApiController]
	[Route("api/partner-requests")]
	public class PartnerRequestsController : ControllerBase
	{
		private readonly TenantDbContextResolver _dbResolver;
		private readonly ILogger<PartnerRequestsController> _logger;
		private readonly IEnumerable<IQueuedOperationHandler> _handlers;

		public PartnerRequestsController(TenantDbContextResolver dbResolver, ILogger<PartnerRequestsController> logger, IEnumerable<IQueuedOperationHandler> handlers)
		{
			_dbResolver = dbResolver;
			_logger = logger;
			_handlers = handlers;
		}

		[HttpGet]
		public async Task<IActionResult> List([FromQuery] string? status = null, [FromQuery] string? search = null, [FromQuery] int skip = 0, [FromQuery] int take = 50)
		{
			await using var db = _dbResolver.GetCurrentDbContext();
			var query = db.PartnerRequestQueue.AsQueryable();
			if (!string.IsNullOrWhiteSpace(status)) query = query.Where(q => q.Status == status);
			if (!string.IsNullOrWhiteSpace(search))
			{
				query = query.Where(q => q.RequestRef.Contains(search) || q.Operation.Contains(search) || (q.OperationKey != null && q.OperationKey.Contains(search)));
			}
			var total = await query.CountAsync();
			var items = await query.OrderByDescending(q => q.CreatedAt).Skip(skip).Take(take).ToListAsync();
			return Ok(new { total, items });
		}

		[HttpGet("{id:int}")]
		public async Task<IActionResult> Details([FromRoute] int id)
		{
			await using var db = _dbResolver.GetCurrentDbContext();
			var item = await db.PartnerRequestQueue.FirstOrDefaultAsync(q => q.QueueId == id);
			if (item == null) return NotFound();
			var logs = await db.PartnerRequestLog.Where(l => l.RequestRef == item.RequestRef).OrderByDescending(l => l.CreatedAt).ToListAsync();
			return Ok(new { item, logs });
		}

		[HttpPost("{id:int}/process")]
		public async Task<IActionResult> ProcessNow([FromRoute] int id, CancellationToken ct)
		{
			await using var db = _dbResolver.GetCurrentDbContext();
			var item = await db.PartnerRequestQueue.FirstOrDefaultAsync(q => q.QueueId == id, ct);
			if (item == null) return NotFound();

			_logger.LogInformation("Manual queue processing requested. QueueId={QueueId}, RequestRef={RequestRef}, OperationKey={OperationKey}, Status={Status}, Attempts={Attempts}, HotelId={HotelId}",
				item.QueueId, item.RequestRef, item.OperationKey, item.Status, item.Attempts, item.HotelId);

			var handlersByKey = _handlers.ToDictionary(h => h.Key, h => h, StringComparer.OrdinalIgnoreCase);
			try
			{
				item.Status = "Processing"; item.UpdatedAt = KsaTime.Now; await db.SaveChangesAsync(ct);
				using var scope = HttpContext.RequestServices.CreateScope();
				if (!string.IsNullOrWhiteSpace(item.OperationKey) && handlersByKey.TryGetValue(item.OperationKey!, out var handler))
				{
					await handler.HandleAsync(item, db, scope.ServiceProvider, ct);
				}
				else
				{
					throw new InvalidOperationException($"Unknown or missing operation_key for request_ref={item.RequestRef}");
				}
				_logger.LogInformation("Manual queue processing succeeded. QueueId={QueueId}, RequestRef={RequestRef}, OperationKey={OperationKey}", item.QueueId, item.RequestRef, item.OperationKey);
				item.Status = "Succeeded"; item.Attempts += 1; item.UpdatedAt = KsaTime.Now;
				await db.PartnerRequestLog.AddAsync(new PartnerRequestLog { RequestRef = item.RequestRef, Partner = item.Partner, Operation = item.Operation, Status = item.Status, Message = "Processed manually", CreatedAt = KsaTime.Now, HotelId = item.HotelId }, ct);
				await db.SaveChangesAsync(ct);
				return Ok(new { processed = true });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"Manual queue processing failed. QueueId={QueueId}, RequestRef={RequestRef}, OperationKey={OperationKey}, Status={Status}, HotelId={HotelId}, PayloadPreview={PayloadPreview}",
					item.QueueId, item.RequestRef, item.OperationKey, item.Status, item.HotelId, Truncate(item.PayloadJson));
				item.Status = "Failed"; item.LastError = ex.Message; item.Attempts += 1; item.UpdatedAt = KsaTime.Now;
				await db.PartnerRequestLog.AddAsync(new PartnerRequestLog { RequestRef = item.RequestRef, Partner = item.Partner, Operation = item.Operation, Status = item.Status, Message = ex.Message, CreatedAt = KsaTime.Now, HotelId = item.HotelId }, ct);
				await db.SaveChangesAsync(ct);
				return StatusCode(500, new { processed = false, error = ex.Message });
			}
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete([FromRoute] int id)
		{
			await using var db = _dbResolver.GetCurrentDbContext();
			var item = await db.PartnerRequestQueue.FirstOrDefaultAsync(q => q.QueueId == id);
			if (item == null) return NotFound();
			db.PartnerRequestQueue.Remove(item);
			await db.SaveChangesAsync();
			return NoContent();
		}

		[HttpPost("run-batch")]
		public async Task<IActionResult> RunBatch([FromServices] IPartnerQueueService queueService, [FromQuery] int take = 50, [FromQuery] bool allTenants = false, CancellationToken cancellationToken = default)
		{
			var (pulled, succeeded, failed) = await queueService.RunBatchAsync(take, cancellationToken, processAllTenants: allTenants);
			return Ok(new { pulled, succeeded, failed });
		}
		private static string? Truncate(string? value, int maxLength = 500)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
		}
	}
}
