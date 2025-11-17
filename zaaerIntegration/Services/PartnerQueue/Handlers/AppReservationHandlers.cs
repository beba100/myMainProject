using System.Text.Json;
using AutoMapper;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.Models;
using zaaerIntegration.Repositories.Implementations;
using zaaerIntegration.Services.Implementations;

namespace zaaerIntegration.Services.PartnerQueueing.Handlers
{
	public sealed class AppReservationCreateHandler : IQueuedOperationHandler
	{
		public string Key => "App.Reservation.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var reservationRepo = new ReservationRepository(db);
			var unitRepo = new ReservationUnitRepository(db);
			var service = new ReservationService(reservationRepo, unitRepo, mapper);
			var dto = JsonSerializer.Deserialize<CreateReservationDto>(item.PayloadJson ?? "{}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
			await service.CreateReservationAsync(dto);
		}
	}

	public sealed class AppReservationUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "App.Reservation.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for App.Reservation.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			var reservationRepo = new ReservationRepository(db);
			var unitRepo = new ReservationUnitRepository(db);
			var service = new ReservationService(reservationRepo, unitRepo, mapper);
			var dto = JsonSerializer.Deserialize<UpdateReservationDto>(item.PayloadJson ?? "{}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
			await service.UpdateReservationAsync(item.TargetId.Value, dto);
		}
	}
}


