using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using zaaerIntegration.Services.PartnerQueueing;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Middleware
{
	public sealed class PartnerQueueMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IQueueSettingsProvider _queueSettings;

		public PartnerQueueMiddleware(RequestDelegate next, IQueueSettingsProvider queueSettings)
		{
			_next = next;
			_queueSettings = queueSettings;
		}

		public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider, ILogger<PartnerQueueMiddleware> logger)
		{
			var queueSettings = _queueSettings.GetSettings();
			if (!queueSettings.EnableQueueMode || !queueSettings.UseMiddleware)
			{
				await _next(context);
				return;
			}

		var method = context.Request.Method;
		var path = context.Request.Path.Value ?? "Unknown";
		
		logger.LogInformation("PartnerQueueMiddleware: Intercepting {Method} request to {Path}", method, path);
		
		if (!(HttpMethods.IsPost(method) || HttpMethods.IsPut(method)))
		{
			await _next(context);
			return;
		}

		context.Request.EnableBuffering();
		string bodyJson;
		using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
		{
			bodyJson = await reader.ReadToEndAsync();
			context.Request.Body.Position = 0;
		}

		int? hotelId = null;
		using (var scopeForTenant = serviceProvider.CreateScope())
		{
			var tenantService = scopeForTenant.ServiceProvider.GetRequiredService<ITenantService>();
			var tenant = tenantService.GetTenant();
			hotelId = tenant?.Id;
		}

		var dto = new EnqueuePartnerRequestDto
		{
			Partner = queueSettings.DefaultPartner,
			Operation = path,
			PayloadJson = bodyJson,
			HotelId = hotelId
		};

		logger.LogInformation("PartnerQueueMiddleware: Queuing {Method} request to {Operation} for HotelId {HotelId}", 
			method, dto.Operation, hotelId);

		using var scope = serviceProvider.CreateScope();
		var queueService = scope.ServiceProvider.GetRequiredService<IPartnerQueueService>();
		await queueService.EnqueueAsync(dto);
		
		logger.LogInformation("PartnerQueueMiddleware: Successfully queued request {RequestRef} for {Operation}", 
			dto.RequestRef, dto.Operation);

		context.Response.StatusCode = StatusCodes.Status202Accepted;
		await context.Response.WriteAsJsonAsync(new
		{
			queued = true,
			requestRef = dto.RequestRef,
			operation = dto.Operation,
			hotelId = dto.HotelId
		});
		}
	}
}


