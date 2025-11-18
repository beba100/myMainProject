using System.Text.Json;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Models;
using zaaerIntegration.Repositories.Implementations;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Implementations;
using zaaerIntegration.Services.Interfaces;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.Expense;
using FinanceLedgerAPI.Models;
using ExpenseModel = FinanceLedgerAPI.Models.Expense;
using ExpenseRoomModel = FinanceLedgerAPI.Models.ExpenseRoom;

namespace zaaerIntegration.Services.PartnerQueueing.Handlers
{
	internal static class JsonConfig
	{
		public static readonly JsonSerializerOptions Options = new JsonSerializerOptions 
		{ 
			// Don't set PropertyNamingPolicy - rely on PropertyNameCaseInsensitive to handle both PascalCase and camelCase
			// This allows the deserializer to match properties regardless of case (PascalCase from Zaaer or camelCase from controller)
			PropertyNameCaseInsensitive = true,
			// Ensure converters specified in DTO attributes are respected
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true
		};
	}

	// Customer
	public sealed class ZaaerCustomerCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Customer.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerCustomerService(uow, new CustomerRepository(db), new CustomerIdentificationRepository(db), mapper);
			var dto = JsonSerializer.Deserialize<ZaaerCreateCustomerDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateCustomerAsync(dto);
		}
	}

	public sealed class ZaaerCustomerUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Customer.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Customer.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerCustomerService(uow, new CustomerRepository(db), new CustomerIdentificationRepository(db), mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateCustomerDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			
			// Match controller logic: try by ZaaerId first, then fallback to internal ID
			ZaaerCustomerResponseDto result;
			try
			{
				result = await service.UpdateCustomerByZaaerIdAsync(item.TargetId.Value, dto);
			}
			catch (ArgumentException)
			{
				result = await service.UpdateCustomerAsync(item.TargetId.Value, dto);
			}
		}
	}

	// Invoice
	public sealed class ZaaerInvoiceCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Invoice.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerInvoiceService(uow, new InvoiceRepository(db), db, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerCreateInvoiceDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateInvoiceAsync(dto);
		}
	}

	public sealed class ZaaerInvoiceUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Invoice.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Invoice.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerInvoiceService(uow, uow.InvoiceRepository, db, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateInvoiceDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var invoiceResponse = await service.UpdateInvoiceAsync(item.TargetId.Value, dto);
			if (invoiceResponse == null)
			{
				throw new InvalidOperationException($"Invoice with ID {item.TargetId.Value} not found");
			}
		}
	}

	// PaymentReceipt
	public sealed class ZaaerPaymentReceiptCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.PaymentReceipt.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<CustomerLedgerService>>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var ledger = new CustomerLedgerService(uow, logger);
			var service = new ZaaerPaymentReceiptService(uow, new PaymentReceiptRepository(db), ledger, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerCreatePaymentReceiptDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreatePaymentReceiptAsync(dto);
		}
	}

	public sealed class ZaaerPaymentReceiptUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.PaymentReceipt.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for PaymentReceipt.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<CustomerLedgerService>>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var ledger = new CustomerLedgerService(uow, logger);
			var service = new ZaaerPaymentReceiptService(uow, new PaymentReceiptRepository(db), ledger, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdatePaymentReceiptDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			
			// Match controller logic: try by ZaaerId first (since Zaaer sends zaaerId in URL), then fall back to ReceiptId
			var paymentReceiptByZaaerId = await service.UpdatePaymentReceiptByZaaerIdAsync(item.TargetId.Value, dto);
			if (paymentReceiptByZaaerId != null)
			{
				return;
			}
			
			// If not found by ZaaerId, try by ReceiptId (backward compatibility)
			var paymentReceiptResponse = await service.UpdatePaymentReceiptAsync(item.TargetId.Value, dto);
			if (paymentReceiptResponse == null)
			{
				throw new InvalidOperationException($"Payment receipt with ID or ZaaerId {item.TargetId.Value} not found");
			}
		}
	}

	public sealed class ZaaerPaymentReceiptUpdateByNumberHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.PaymentReceipt.UpdateByNumber";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<CustomerLedgerService>>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var ledger = new CustomerLedgerService(uow, logger);
			var service = new ZaaerPaymentReceiptService(uow, uow.PaymentReceiptRepository, ledger, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdatePaymentReceiptDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var receiptNo = item.PayloadType; // carrying string id
			if (string.IsNullOrWhiteSpace(receiptNo)) throw new InvalidOperationException("Missing receipt number for UpdateByNumber");
			await service.UpdatePaymentReceiptByReceiptNoAsync(receiptNo!, dto);
		}
	}

	public sealed class ZaaerPaymentReceiptUpdateByZaaerIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.PaymentReceipt.UpdateByZaaerId";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id (zaaerId) for PaymentReceipt.UpdateByZaaerId");
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<CustomerLedgerService>>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var ledger = new CustomerLedgerService(uow, logger);
			var service = new ZaaerPaymentReceiptService(uow, new PaymentReceiptRepository(db), ledger, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdatePaymentReceiptDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var result = await service.UpdatePaymentReceiptByZaaerIdAsync(item.TargetId.Value, dto);
			if (result == null)
			{
				throw new InvalidOperationException($"Payment receipt with ZaaerId {item.TargetId.Value} not found");
			}
		}
	}

	// Refund
	public sealed class ZaaerRefundCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Refund.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerRefundService(uow, new RefundRepository(db), mapper);
			var dto = JsonSerializer.Deserialize<ZaaerCreateRefundDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateRefundAsync(dto);
		}
	}

	public sealed class ZaaerRefundUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Refund.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Refund.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerRefundService(uow, new RefundRepository(db), mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateRefundDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var refundResponse = await service.UpdateRefundAsync(item.TargetId.Value, dto);
			if (refundResponse == null)
			{
				throw new InvalidOperationException($"Refund with ID {item.TargetId.Value} not found");
			}
		}
	}

	public sealed class ZaaerRefundUpdateByNumberHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Refund.UpdateByNumber";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerRefundService(uow, new RefundRepository(db), mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateRefundDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var refundNo = item.PayloadType;
			if (string.IsNullOrWhiteSpace(refundNo)) throw new InvalidOperationException("Missing refund number for UpdateByNumber");
			var refundResponse = await service.UpdateRefundByRefundNoAsync(refundNo!, dto);
			if (refundResponse == null)
			{
				throw new InvalidOperationException($"Refund with number {refundNo} not found");
			}
		}
	}

	public sealed class ZaaerRefundUpdateByZaaerIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Refund.UpdateByZaaerId";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id (zaaerId) for Refund.UpdateByZaaerId");
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerRefundService(uow, new RefundRepository(db), mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateRefundDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var result = await service.UpdateRefundByZaaerIdAsync(item.TargetId.Value, dto);
			if (result == null)
			{
				throw new InvalidOperationException($"Refund with ZaaerId {item.TargetId.Value} not found");
			}
		}
	}

	// Credit Note
	public sealed class ZaaerCreditNoteCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.CreditNote.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var service = new ZaaerCreditNoteService(db, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerCreateCreditNoteDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateCreditNoteAsync(dto);
		}
	}

	// Apartment
	public sealed class ZaaerApartmentCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Apartment.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var logger = sp.GetRequiredService<ILogger<ZaaerApartmentService>>();
			var service = new ZaaerApartmentService(uow, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerCreateApartmentDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateApartmentAsync(dto);
		}
	}

	public sealed class ZaaerApartmentCreateBulkHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Apartment.CreateBulk";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var logger = sp.GetRequiredService<ILogger<ZaaerApartmentService>>();
			var service = new ZaaerApartmentService(uow, mapper, logger);
			var dto = JsonSerializer.Deserialize<List<ZaaerCreateApartmentDto>>(item.PayloadJson ?? "[]", JsonConfig.Options)!;
			await service.CreateApartmentsAsync(dto);
		}
	}

	public sealed class ZaaerApartmentUpdateByZaaerIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Apartment.UpdateByZaaerId";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Apartment.UpdateByZaaerId");
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var logger = sp.GetRequiredService<ILogger<ZaaerApartmentService>>();
			var service = new ZaaerApartmentService(uow, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateApartmentDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.UpdateApartmentByZaaerIdAsync(item.TargetId.Value, dto);
		}
	}

	public sealed class ZaaerApartmentUpdateByCodeHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Apartment.UpdateByCode";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var logger = sp.GetRequiredService<ILogger<ZaaerApartmentService>>();
			var service = new ZaaerApartmentService(uow, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateApartmentDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var code = item.PayloadType;
			if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("Missing apartment code for UpdateByCode");
			await service.UpdateApartmentByCodeAsync(code!, dto);
		}
	}
}

namespace zaaerIntegration.Services.PartnerQueueing.Handlers
{
    public sealed class ZaaerCustomerUpdateByNumberHandler : IQueuedOperationHandler
    {
        public string Key => "Zaaer.Customer.UpdateByNumber";
        public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
        {
            var mapper = sp.GetRequiredService<IMapper>();
            using var uow = new UnitOfWork(db, ownsContext: false);
            var customerRepo = new CustomerRepository(db);
            var identificationRepo = new CustomerIdentificationRepository(db);
            var service = new ZaaerCustomerService(uow, customerRepo, identificationRepo, mapper);
            var dto = JsonSerializer.Deserialize<ZaaerUpdateCustomerDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
            var path = item.Operation ?? string.Empty;
            var idx = path.LastIndexOf('/');
            var customerNo = idx >= 0 && idx + 1 < path.Length ? path[(idx + 1)..] : item.PayloadType;
            if (string.IsNullOrWhiteSpace(customerNo)) throw new InvalidOperationException("Missing customer number for UpdateByNumber");
            await service.UpdateCustomerByNumberAsync(customerNo!, dto);
        }
    }
}

// RoomTypeRate handlers moved here for centralization
namespace zaaerIntegration.Services.PartnerQueueing.Handlers
{
	public sealed class ZaaerRoomTypeRateCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.RoomTypeRate.Create";

		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<ZaaerRoomTypeRateService>>();
			var service = new ZaaerRoomTypeRateService(db, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerCreateRoomTypeRateDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateRoomTypeRateAsync(dto);
		}
	}

	public sealed class ZaaerRoomTypeRateUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.RoomTypeRate.UpdateById";

		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for RoomTypeRate.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<ZaaerRoomTypeRateService>>();
			var service = new ZaaerRoomTypeRateService(db, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateRoomTypeRateDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.UpdateRoomTypeRateAsync(item.TargetId.Value, dto);
		}
	}

	public sealed class ZaaerRoomTypeRateUpdateByZaaerIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.RoomTypeRate.UpdateByZaaerId";

		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id (zaaerId) for RoomTypeRate.UpdateByZaaerId");
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<ZaaerRoomTypeRateService>>();
			var service = new ZaaerRoomTypeRateService(db, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateRoomTypeRateDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var result = await service.UpdateRoomTypeRateByZaaerIdAsync(item.TargetId.Value, dto);
			if (result == null)
			{
				throw new InvalidOperationException($"RoomTypeRate with ZaaerId {item.TargetId.Value} not found");
			}
		}
	}
}

// Reservation handlers moved here for centralization
namespace zaaerIntegration.Services.PartnerQueueing.Handlers
{
	public sealed class ZaaerReservationCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Reservation.Create";

		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<ZaaerReservationService>>();
			using var uow = new UnitOfWork(db, ownsContext: false);
            var ratesService = new ReservationRatesService(db);
	            var ledgerLogger = sp.GetRequiredService<ILogger<CustomerLedgerService>>();
	            var customerLedgerService = new CustomerLedgerService(uow, ledgerLogger);
	            var service = new ZaaerReservationService(uow, uow.ReservationRepository, uow.ReservationUnitRepository, uow.InvoiceRepository, ratesService, customerLedgerService, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerCreateReservationDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateReservationAsync(dto);
		}
	}

	public sealed class ZaaerReservationUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Reservation.UpdateById";

		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<ZaaerReservationService>>();
			using var uow = new UnitOfWork(db, ownsContext: false);
            var ratesService = new ReservationRatesService(db);
	            var ledgerLogger = sp.GetRequiredService<ILogger<CustomerLedgerService>>();
	            var customerLedgerService = new CustomerLedgerService(uow, ledgerLogger);
	            var service = new ZaaerReservationService(uow, uow.ReservationRepository, uow.ReservationUnitRepository, uow.InvoiceRepository, ratesService, customerLedgerService, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateReservationDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for UpdateById");
			
			// Match controller logic: try by ZaaerId first (since Zaaer sends zaaerId in URL), then fall back to ReservationId
			var reservationByZaaerId = await service.UpdateReservationByZaaerIdAsync(item.TargetId.Value, dto);
			if (reservationByZaaerId != null)
			{
				return;
			}
			
			// If not found by ZaaerId, try by ReservationId (backward compatibility)
			var reservationResponse = await service.UpdateReservationAsync(item.TargetId.Value, dto);
			if (reservationResponse == null)
			{
				throw new InvalidOperationException($"Reservation with ID or ZaaerId {item.TargetId.Value} not found");
			}
		}
	}

	public sealed class ZaaerReservationUpdateByNumberHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Reservation.UpdateByNumber";

		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<ZaaerReservationService>>();
			using var uow = new UnitOfWork(db, ownsContext: false);
            var ratesService = new ReservationRatesService(db);
	            var ledgerLogger = sp.GetRequiredService<ILogger<CustomerLedgerService>>();
	            var customerLedgerService = new CustomerLedgerService(uow, ledgerLogger);
	            var service = new ZaaerReservationService(uow, uow.ReservationRepository, uow.ReservationUnitRepository, uow.InvoiceRepository, ratesService, customerLedgerService, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateReservationDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var reservationNo = item.PayloadType; // carrying number
			if (string.IsNullOrWhiteSpace(reservationNo)) throw new InvalidOperationException("Missing reservation number for UpdateByNumber");
			var reservationResponse = await service.UpdateReservationByNumberAsync(reservationNo!, dto);
			if (reservationResponse == null)
			{
				throw new InvalidOperationException($"Reservation with number '{reservationNo}' not found");
			}
		}
	}

	public sealed class ZaaerReservationUpdateByZaaerIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Reservation.UpdateByZaaerId";

		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var logger = sp.GetRequiredService<ILogger<ZaaerReservationService>>();
			using var uow = new UnitOfWork(db, ownsContext: false);
            var ratesService = new ReservationRatesService(db);
	            var ledgerLogger = sp.GetRequiredService<ILogger<CustomerLedgerService>>();
	            var customerLedgerService = new CustomerLedgerService(uow, ledgerLogger);
	            var service = new ZaaerReservationService(uow, uow.ReservationRepository, uow.ReservationUnitRepository, uow.InvoiceRepository, ratesService, customerLedgerService, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateReservationDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id (zaaerId) for UpdateByZaaerId");
			var result = await service.UpdateReservationByZaaerIdAsync(item.TargetId.Value, dto);
			if (result == null)
			{
				throw new InvalidOperationException($"Reservation with ZaaerId {item.TargetId.Value} not found");
			}
		}
	}
}

// Floor and HotelSettings handlers
namespace zaaerIntegration.Services.PartnerQueueing.Handlers
{
	public sealed class ZaaerFloorCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Floor.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var floorRepo = new GenericRepository<FinanceLedgerAPI.Models.Floor>(db);
			var service = new ZaaerFloorService(uow, floorRepo, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerCreateFloorDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateFloorAsync(dto);
		}
	}

	public sealed class ZaaerFloorCreateBulkHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Floor.CreateBulk";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var floorRepo = new GenericRepository<FinanceLedgerAPI.Models.Floor>(db);
			var service = new ZaaerFloorService(uow, floorRepo, mapper);
			var dto = JsonSerializer.Deserialize<List<ZaaerCreateFloorDto>>(item.PayloadJson ?? "[]", JsonConfig.Options)!;
			await service.CreateFloorsAsync(dto);
		}
	}

	public sealed class ZaaerFloorUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Floor.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Zaaer.Floor.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var floorRepo = new GenericRepository<FinanceLedgerAPI.Models.Floor>(db);
			var service = new ZaaerFloorService(uow, floorRepo, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateFloorDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.UpdateFloorAsync(item.TargetId.Value, dto);
		}
	}

	public sealed class ZaaerHotelSettingsCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.HotelSettings.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var logger = sp.GetRequiredService<ILogger<ZaaerHotelSettingsService>>();
			var service = new ZaaerHotelSettingsService(uow, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerCreateHotelSettingsDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateHotelSettingsAsync(dto);
		}
	}

	public sealed class ZaaerHotelSettingsUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.HotelSettings.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Zaaer.HotelSettings.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var logger = sp.GetRequiredService<ILogger<ZaaerHotelSettingsService>>();
			var service = new ZaaerHotelSettingsService(uow, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateHotelSettingsDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var hotelSettings = await service.UpdateHotelSettingsAsync(item.TargetId.Value, dto);
			if (hotelSettings == null)
			{
				throw new InvalidOperationException($"Hotel settings with ID {item.TargetId.Value} not found");
			}
		}
	}

	public sealed class ZaaerHotelSettingsUpdateByZaaerIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.HotelSettings.UpdateByZaaerId";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var logger = sp.GetRequiredService<ILogger<ZaaerHotelSettingsService>>();
			var service = new ZaaerHotelSettingsService(uow, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateHotelSettingsDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			
			if (!dto.ZaaerId.HasValue)
			{
				throw new InvalidOperationException("ZaaerId is required in the request body for UpdateByZaaerId");
			}
			
			var hotelSettings = await service.UpdateHotelSettingsByZaaerIdAsync(dto.ZaaerId.Value, dto);
			if (hotelSettings == null)
			{
				throw new InvalidOperationException($"Hotel settings with Zaaer ID {dto.ZaaerId.Value} not found");
			}
		}
	}
}

// User, Role, Bank, Expense, RoomType handlers
namespace zaaerIntegration.Services.PartnerQueueing.Handlers
{
	public sealed class ZaaerUserCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.User.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<ZaaerCreateUserDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			
			// Handle password hashing (use default password if not provided)
			var password = !string.IsNullOrWhiteSpace(dto.Password) ? dto.Password : "Za@er123";
			var passwordHash = HashPassword(password);
			
			var entity = mapper.Map<User>(dto);
			entity.PasswordHash = passwordHash; // Set the hashed password
			entity.CreatedAt = KsaTime.Now;
			entity.IsActive = true;
			
			await db.Set<User>().AddAsync(entity, ct);
			await db.SaveChangesAsync(ct);
		}
		
		private static string HashPassword(string password)
		{
			using var sha256 = System.Security.Cryptography.SHA256.Create();
			var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
			return Convert.ToBase64String(hashedBytes);
		}
	}

	public sealed class ZaaerUserUpdateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.User.Update";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<ZaaerUpdateUserDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			
			// Updated: find user by ZaaerId (queue payload no longer contains UserId)
			var existing = await db.Set<User>()
				.FirstOrDefaultAsync(u => u.ZaaerId.HasValue && dto.ZaaerId.HasValue && u.ZaaerId.Value == dto.ZaaerId.Value, ct);
			if (existing == null) return;
			
			// Handle password update if provided
			if (!string.IsNullOrWhiteSpace(dto.Password))
			{
				existing.PasswordHash = HashPassword(dto.Password);
			}
			
			mapper.Map(dto, existing);
			existing.UpdatedAt = KsaTime.Now;
			await db.SaveChangesAsync(ct);
		}
		
		private static string HashPassword(string password)
		{
			using var sha256 = System.Security.Cryptography.SHA256.Create();
			var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
			return Convert.ToBase64String(hashedBytes);
		}
	}

	// ZaaerRoleCreateHandler and ZaaerRoleUpdateHandler disabled
	// Role model is now in Master DB only, not in Tenant DB (ApplicationDbContext)
	// Use MasterUserService and Master DB for role management instead
	
	// public sealed class ZaaerRoleCreateHandler : IQueuedOperationHandler
	// {
	// 	public string Key => "Zaaer.Role.Create";
	// 	public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
	// 	{
	// 		var mapper = sp.GetRequiredService<IMapper>();
	// 		var dto = JsonSerializer.Deserialize<ZaaerCreateRoleDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
	// 		var entity = mapper.Map<Role>(dto);
	// 		await db.Set<Role>().AddAsync(entity, ct);
	// 		await db.SaveChangesAsync(ct);
	// 	}
	// }

	// public sealed class ZaaerRoleUpdateHandler : IQueuedOperationHandler
	// {
	// 	public string Key => "Zaaer.Role.Update";
	// 	public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
	// 	{
	// 		var mapper = sp.GetRequiredService<IMapper>();
	// 		var dto = JsonSerializer.Deserialize<ZaaerUpdateRoleDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
	// 		var existing = await db.Set<Role>().FirstOrDefaultAsync(r => r.RoleId == dto.RoleId, ct);
	// 		if (existing == null) return;
	// 		mapper.Map(dto, existing);
	// 		await db.SaveChangesAsync(ct);
	// 	}
	// }

	public sealed class ZaaerBankCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Bank.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<ZaaerCreateBankDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var entity = mapper.Map<Bank>(dto);
			await db.Set<Bank>().AddAsync(entity, ct);
			await db.SaveChangesAsync(ct);
		}
	}

	public sealed class ZaaerBankUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Bank.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Zaaer.Bank.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<ZaaerUpdateBankDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var existing = await db.Set<Bank>().FindAsync(new object?[] { item.TargetId.Value }, ct);
			if (existing == null) return;
			mapper.Map(dto, existing);
			await db.SaveChangesAsync(ct);
		}
	}

	public sealed class ZaaerExpenseCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Expense.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<ZaaerCreateExpenseDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var entity = mapper.Map<ExpenseModel>(dto);
			await db.Set<ExpenseModel>().AddAsync(entity, ct);
			await db.SaveChangesAsync(ct);
		}
	}

	public sealed class ZaaerExpenseUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Expense.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Zaaer.Expense.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<ZaaerUpdateExpenseDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var existing = await db.Set<ExpenseModel>().FindAsync(new object?[] { item.TargetId.Value }, ct);
			if (existing == null) return;
			mapper.Map(dto, existing);
			await db.SaveChangesAsync(ct);
		}
	}

	// Expense handlers (new ExpenseController with X-Hotel-Code header support)
	public sealed class ExpenseCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Expense.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			// In queue processing, ApplicationDbContext is already configured for the correct tenant DB
			// Get HotelId from HotelSettings in the tenant DB
			var hotelSettings = await db.HotelSettings
				.AsNoTracking()
				.FirstOrDefaultAsync(ct);
			
			if (hotelSettings == null)
			{
				throw new InvalidOperationException("HotelSettings not found in tenant database");
			}

			var dto = JsonSerializer.Deserialize<zaaerIntegration.DTOs.Expense.CreateExpenseDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			
			// Create expense directly using db context (similar to ZaaerExpenseCreateHandler)
			var expense = new ExpenseModel
			{
				HotelId = hotelSettings.HotelId,
				DateTime = dto.DateTime,
				Comment = dto.Comment,
				ExpenseCategoryId = dto.ExpenseCategoryId,
				TaxRate = dto.TaxRate,
				TaxAmount = dto.TaxAmount,
				TotalAmount = dto.TotalAmount,
				CreatedAt = DateTime.Now
			};

			await db.Set<ExpenseModel>().AddAsync(expense, ct);
			await db.SaveChangesAsync(ct);

			// Add expense_rooms if provided
			if (dto.ExpenseRooms != null && dto.ExpenseRooms.Any())
			{
				foreach (var roomDto in dto.ExpenseRooms)
				{
					// ✅ البحث عن Apartment باستخدام ApartmentId أو ZaaerId
					Apartment? apartment = null;
					
					if (roomDto.ApartmentId.HasValue)
					{
						// البحث باستخدام ApartmentId
						apartment = await db.Apartments
							.AsNoTracking()
							.FirstOrDefaultAsync(a => a.ApartmentId == roomDto.ApartmentId.Value && a.HotelId == hotelSettings.HotelId, ct);
					}
					else if (roomDto.ZaaerId.HasValue)
					{
						// ✅ البحث باستخدام ZaaerId (من الـ frontend)
						apartment = await db.Apartments
							.AsNoTracking()
							.FirstOrDefaultAsync(a => a.ZaaerId == roomDto.ZaaerId.Value && a.HotelId == hotelSettings.HotelId, ct);
					}

					if (apartment == null)
					{
						// Log warning but continue (skip invalid apartment)
						continue;
					}

					var expenseRoom = new ExpenseRoomModel
					{
						ExpenseId = expense.ExpenseId,
						ApartmentId = apartment.ApartmentId, // ✅ استخدام ApartmentId من الـ apartment الموجود
						Purpose = roomDto.Purpose,
						Amount = roomDto.Amount, // ✅ إضافة Amount
						CreatedAt = DateTime.Now
					};

					await db.Set<ExpenseRoomModel>().AddAsync(expenseRoom, ct);
				}

				await db.SaveChangesAsync(ct);
			}
		}
	}

	public sealed class ExpenseUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Expense.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Expense.UpdateById");
			
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<zaaerIntegration.DTOs.Expense.UpdateExpenseDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var existing = await db.Set<ExpenseModel>().FindAsync(new object?[] { item.TargetId.Value }, ct);
			
			if (existing == null)
			{
				throw new InvalidOperationException($"Expense with ID {item.TargetId.Value} not found");
			}

			// Update fields
			if (dto.DateTime.HasValue) existing.DateTime = dto.DateTime.Value;
			if (dto.Comment != null) existing.Comment = dto.Comment;
			if (dto.ExpenseCategoryId.HasValue) existing.ExpenseCategoryId = dto.ExpenseCategoryId;
			if (dto.TaxRate.HasValue) existing.TaxRate = dto.TaxRate;
			if (dto.TaxAmount.HasValue) existing.TaxAmount = dto.TaxAmount;
			if (dto.TotalAmount.HasValue) existing.TotalAmount = dto.TotalAmount.Value;

			existing.UpdatedAt = DateTime.Now;

			await db.SaveChangesAsync(ct);
		}
	}

	public sealed class ExpenseDeleteHandler : IQueuedOperationHandler
	{
		public string Key => "Expense.Delete";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Expense.Delete");
			
			var expense = await db.Set<ExpenseModel>()
				.Include(e => e.ExpenseRooms)
				.FirstOrDefaultAsync(e => e.ExpenseId == item.TargetId.Value, ct);

			if (expense == null)
			{
				throw new InvalidOperationException($"Expense with ID {item.TargetId.Value} not found for deletion");
			}

			// Delete expense_rooms first (cascade delete)
			if (expense.ExpenseRooms != null && expense.ExpenseRooms.Any())
			{
				db.Set<ExpenseRoomModel>().RemoveRange(expense.ExpenseRooms);
			}

			db.Set<ExpenseModel>().Remove(expense);
			await db.SaveChangesAsync(ct);
		}
	}

	public sealed class ExpenseRoomAddHandler : IQueuedOperationHandler
	{
		public string Key => "Expense.Room.Add";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			// Parse expenseId from operation path (e.g., /api/expenses/123/rooms) or use TargetId
			var expenseId = item.TargetId ?? throw new InvalidOperationException("Missing target_id (expenseId) for Expense.Room.Add");
			
			// Verify expense exists
			var expense = await db.Set<ExpenseModel>()
				.AsNoTracking()
				.FirstOrDefaultAsync(e => e.ExpenseId == expenseId, ct);

			if (expense == null)
			{
				throw new InvalidOperationException($"Expense with id {expenseId} not found");
			}

			// Get HotelId from HotelSettings
			var hotelSettings = await db.HotelSettings
				.AsNoTracking()
				.FirstOrDefaultAsync(ct);

			if (hotelSettings == null)
			{
				throw new InvalidOperationException("HotelSettings not found in tenant database");
			}

			var dto = JsonSerializer.Deserialize<zaaerIntegration.DTOs.Expense.CreateExpenseRoomDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			
			// ✅ البحث عن Apartment باستخدام ApartmentId أو ZaaerId
			Apartment? apartment = null;
			
			if (dto.ApartmentId.HasValue)
			{
				// البحث باستخدام ApartmentId
				apartment = await db.Apartments
					.AsNoTracking()
					.FirstOrDefaultAsync(a => a.ApartmentId == dto.ApartmentId.Value && a.HotelId == hotelSettings.HotelId, ct);
			}
			else if (dto.ZaaerId.HasValue)
			{
				// ✅ البحث باستخدام ZaaerId (من الـ frontend)
				apartment = await db.Apartments
					.AsNoTracking()
					.FirstOrDefaultAsync(a => a.ZaaerId == dto.ZaaerId.Value && a.HotelId == hotelSettings.HotelId, ct);
			}

			if (apartment == null)
			{
				throw new InvalidOperationException($"Apartment not found: ApartmentId={dto.ApartmentId}, ZaaerId={dto.ZaaerId}, HotelId={hotelSettings.HotelId}");
			}

			var expenseRoom = new ExpenseRoomModel
			{
				ExpenseId = expenseId,
				ApartmentId = apartment.ApartmentId, // ✅ استخدام ApartmentId من الـ apartment الموجود
				Purpose = dto.Purpose,
				Amount = dto.Amount, // ✅ إضافة Amount
				CreatedAt = DateTime.Now
			};

			await db.Set<ExpenseRoomModel>().AddAsync(expenseRoom, ct);
			await db.SaveChangesAsync(ct);
		}
	}

	public sealed class ExpenseRoomUpdateHandler : IQueuedOperationHandler
	{
		public string Key => "Expense.Room.Update";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			// Get roomId from TargetId
			if (!item.TargetId.HasValue)
			{
				throw new InvalidOperationException("Missing target_id (roomId) for Expense.Room.Update");
			}

			var roomId = item.TargetId.Value;
			var expenseRoom = await db.Set<ExpenseRoomModel>()
				.Include(er => er.Expense)
				.FirstOrDefaultAsync(er => er.ExpenseRoomId == roomId, ct);

			if (expenseRoom == null)
			{
				throw new InvalidOperationException($"ExpenseRoom with ID {roomId} not found");
			}

			// Get HotelId from HotelSettings
			var hotelSettings = await db.HotelSettings
				.AsNoTracking()
				.FirstOrDefaultAsync(ct);

			if (hotelSettings == null)
			{
				throw new InvalidOperationException("HotelSettings not found in tenant database");
			}

			var dto = JsonSerializer.Deserialize<zaaerIntegration.DTOs.Expense.UpdateExpenseRoomDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;

			// Update ApartmentId if provided
			if (dto.ApartmentId.HasValue)
			{
				// Verify apartment exists in the same hotel
				var apartment = await db.Apartments
					.AsNoTracking()
					.FirstOrDefaultAsync(a => a.ApartmentId == dto.ApartmentId.Value && a.HotelId == hotelSettings.HotelId, ct);

				if (apartment == null)
				{
					throw new InvalidOperationException($"Apartment with id {dto.ApartmentId.Value} not found");
				}

				expenseRoom.ApartmentId = dto.ApartmentId.Value;
			}

			// Update Purpose if provided
			if (dto.Purpose != null)
			{
				expenseRoom.Purpose = dto.Purpose;
			}

			await db.SaveChangesAsync(ct);
		}
	}

	public sealed class ExpenseRoomDeleteHandler : IQueuedOperationHandler
	{
		public string Key => "Expense.Room.Delete";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			// Get roomId from TargetId
			if (!item.TargetId.HasValue)
			{
				throw new InvalidOperationException("Missing target_id (roomId) for Expense.Room.Delete");
			}

			var roomId = item.TargetId.Value;
			var expenseRoom = await db.Set<ExpenseRoomModel>()
				.Include(er => er.Expense)
				.FirstOrDefaultAsync(er => er.ExpenseRoomId == roomId, ct);

			if (expenseRoom == null)
			{
				throw new InvalidOperationException($"ExpenseRoom with ID {roomId} not found for deletion");
			}

			db.Set<ExpenseRoomModel>().Remove(expenseRoom);
			await db.SaveChangesAsync(ct);
		}
	}

	public sealed class ZaaerRoomTypeCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.RoomType.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<ZaaerCreateRoomTypeDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var entity = mapper.Map<RoomType>(dto);
			db.Set<RoomType>().Add(entity);
			await db.SaveChangesAsync(ct);
		}
	}

	public sealed class ZaaerRoomTypeUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.RoomType.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Zaaer.RoomType.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<ZaaerUpdateRoomTypeDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var existing = await db.Set<RoomType>().FindAsync(new object?[] { item.TargetId.Value }, ct);
			if (existing == null) return;
			mapper.Map(dto, existing);
			await db.SaveChangesAsync(ct);
		}
	}

	public sealed class ZaaerMaintenanceCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Maintenance.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var logger = sp.GetRequiredService<ILogger<ZaaerMaintenanceService>>();
			var service = new ZaaerMaintenanceService(uow, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerCreateMaintenanceDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateMaintenanceAsync(dto);
		}
	}

	public sealed class ZaaerMaintenanceUpdateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Maintenance.Update";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var logger = sp.GetRequiredService<ILogger<ZaaerMaintenanceService>>();
			var service = new ZaaerMaintenanceService(uow, mapper, logger);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateMaintenanceDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			
			if (!dto.ZaaerId.HasValue)
			{
				throw new InvalidOperationException("ZaaerId is required in the request body for Maintenance.Update");
			}
			
			var maintenance = await service.UpdateMaintenanceAsync(dto.ZaaerId.Value, dto);
			if (maintenance == null)
			{
				throw new InvalidOperationException($"Maintenance record with ZaaerId {dto.ZaaerId.Value} not found");
			}
		}
	}

	public sealed class ZaaerSeasonalRateCreateHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.SeasonalRate.Create";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<ZaaerCreateSeasonalRateDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var service = new ZaaerSeasonalRateService(db, mapper);
			await service.CreateAsync(dto);
		}
	}

	public sealed class ZaaerSeasonalRateUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.SeasonalRate.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for Zaaer.SeasonalRate.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			var dto = JsonSerializer.Deserialize<ZaaerUpdateSeasonalRateDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var service = new ZaaerSeasonalRateService(db, mapper);
			await service.UpdateAsync(item.TargetId.Value, dto);
		}
	}

	public sealed class ZaaerBuildingCreateWithFloorsHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Building.CreateWithFloors";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerBuildingService(uow);
			var dto = JsonSerializer.Deserialize<ZaaerCreateBuildingDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.CreateBuildingWithFloorsAsync(dto);
		}
	}

	public sealed class ZaaerBuildingUpdateWithFloorsHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Building.UpdateWithFloors";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerBuildingService(uow);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateBuildingDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.UpdateBuildingWithFloorsAsync(dto);
		}
	}

	public sealed class ZaaerBuildingUpdateWithFloorsSafeHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.Building.UpdateWithFloorsSafe";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerBuildingService(uow);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateBuildingDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.UpdateBuildingWithFloorsSafeAsync(dto);
		}
	}
}

namespace zaaerIntegration.Services.PartnerQueueing.Handlers
{
    public sealed class ZaaerActivityLogCreateHandler : IQueuedOperationHandler
    {
        public string Key => "Zaaer.ActivityLog.Create";
        public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
        {
            var service = new ActivityLogService(db);
            var dto = JsonSerializer.Deserialize<ZaaerCreateActivityLogDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
            await service.CreateAsync(dto);
        }
    }

    public sealed class ZaaerReservationUnitSwitchCreateHandler : IQueuedOperationHandler
    {
        public string Key => "Zaaer.ReservationUnitSwitch.Create";
        public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
        {
            var service = new ReservationUnitSwitchService(db);
            var dto = JsonSerializer.Deserialize<ZaaerCreateReservationUnitSwitchDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
            await service.CreateAsync(dto);
        }
    }

    public sealed class ZaaerReservationRatesApplyAllHandler : IQueuedOperationHandler
    {
        public string Key => "Zaaer.ReservationRates.ApplyAll";
        public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
        {
            var path = item.Operation ?? string.Empty;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var reservationIdStr = segments.Reverse().Skip(1).FirstOrDefault();
            if (!int.TryParse(reservationIdStr, out var reservationId)) throw new InvalidOperationException("Missing reservationId for ApplyAll");
            var dto = JsonSerializer.Deserialize<ZaaerApplySameAmountDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
            var service = new ReservationRatesService(db);
            await service.ApplySameAmountAsync(reservationId, dto.Amount, dto.UnitId, dto.DateFrom, dto.DateTo, dto.EwaPercent, dto.VatPercent);
        }
    }

    public sealed class ZaaerReservationRatesUpsertHandler : IQueuedOperationHandler
    {
        public string Key => "Zaaer.ReservationRates.Upsert";
        public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
        {
            var path = item.Operation ?? string.Empty;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var reservationIdStr = segments.LastOrDefault();
            if (!int.TryParse(reservationIdStr, out var reservationId)) throw new InvalidOperationException("Missing reservationId for Upsert");
            var dto = JsonSerializer.Deserialize<ZaaerReservationRatesUpsertDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
            var service = new ReservationRatesService(db);
            await service.UpsertRatesAsync(reservationId, dto.Items, dto.EwaPercent, dto.VatPercent);
        }
    }

    // RateType
    public sealed class ZaaerRateTypeCreateHandler : IQueuedOperationHandler
    {
        public string Key => "Zaaer.RateType.Create";
        public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
        {
            var mapper = sp.GetRequiredService<IMapper>();
            using var uow = new UnitOfWork(db, ownsContext: false);
            var service = new ZaaerRateTypeService(uow, db, mapper);
            var dto = JsonSerializer.Deserialize<ZaaerCreateRateTypeDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
            await service.CreateAsync(dto);
        }
    }

	public sealed class ZaaerRateTypeUpdateByIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.RateType.UpdateById";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id for RateType.UpdateById");
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerRateTypeService(uow, db, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateRateTypeDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			await service.UpdateAsync(item.TargetId.Value, dto);
		}
	}

	public sealed class ZaaerRateTypeUpdateByZaaerIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.RateType.UpdateByZaaerId";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id (zaaerId) for RateType.UpdateByZaaerId");
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerRateTypeService(uow, db, mapper);
			var dto = JsonSerializer.Deserialize<ZaaerUpdateRateTypeDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
			var result = await service.UpdateByZaaerIdAsync(item.TargetId.Value, dto);
			if (result == null)
			{
				throw new InvalidOperationException($"RateType with ZaaerId {item.TargetId.Value} not found");
			}
		}
	}

	public sealed class ZaaerRateTypeDeleteByZaaerIdHandler : IQueuedOperationHandler
	{
		public string Key => "Zaaer.RateType.DeleteByZaaerId";
		public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
		{
			if (!item.TargetId.HasValue) throw new InvalidOperationException("Missing target_id (zaaerId) for RateType.DeleteByZaaerId");
			var mapper = sp.GetRequiredService<IMapper>();
			using var uow = new UnitOfWork(db, ownsContext: false);
			var service = new ZaaerRateTypeService(uow, db, mapper);
			var result = await service.DeleteByZaaerIdAsync(item.TargetId.Value);
			if (!result)
			{
				throw new InvalidOperationException($"RateType with ZaaerId {item.TargetId.Value} not found");
			}
		}
	}

    // Tax
    public sealed class ZaaerTaxCreateHandler : IQueuedOperationHandler
    {
        public string Key => "Zaaer.Tax.Create";
        public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
        {
            var mapper = sp.GetRequiredService<IMapper>();
            var logger = sp.GetRequiredService<ILogger<ZaaerTaxService>>();
            using var uow = new UnitOfWork(db, ownsContext: false);
            var service = new ZaaerTaxService(uow, mapper, logger);
            var dto = JsonSerializer.Deserialize<ZaaerCreateTaxDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
            await service.CreateTaxAsync(dto);
        }
    }

    public sealed class ZaaerTaxUpdateHandler : IQueuedOperationHandler
    {
        public string Key => "Zaaer.Tax.Update";
        public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
        {
            var path = item.Operation ?? string.Empty;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var zaaerIdStr = segments.LastOrDefault();
            if (!int.TryParse(zaaerIdStr, out var zaaerId)) throw new InvalidOperationException("Missing zaaerId for Tax.Update");

            var mapper = sp.GetRequiredService<IMapper>();
            var logger = sp.GetRequiredService<ILogger<ZaaerTaxService>>();
            using var uow = new UnitOfWork(db, ownsContext: false);
            var service = new ZaaerTaxService(uow, mapper, logger);
            var dto = JsonSerializer.Deserialize<ZaaerUpdateTaxDto>(item.PayloadJson ?? "{}", JsonConfig.Options)!;
            var result = await service.UpdateTaxAsync(zaaerId, dto);
            if (result == null)
            {
                throw new InvalidOperationException($"Tax with ZaaerId {zaaerId} not found");
            }
        }
    }

    public sealed class ZaaerTaxDeleteHandler : IQueuedOperationHandler
    {
        public string Key => "Zaaer.Tax.Delete";
        public async Task HandleAsync(PartnerQueue item, ApplicationDbContext db, IServiceProvider sp, CancellationToken ct)
        {
            var path = item.Operation ?? string.Empty;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var zaaerIdStr = segments.LastOrDefault();
            if (!int.TryParse(zaaerIdStr, out var zaaerId)) throw new InvalidOperationException("Missing zaaerId for Tax.Delete");

            var mapper = sp.GetRequiredService<IMapper>();
            var logger = sp.GetRequiredService<ILogger<ZaaerTaxService>>();
            using var uow = new UnitOfWork(db, ownsContext: false);
            var service = new ZaaerTaxService(uow, mapper, logger);
            var deleted = await service.DeleteTaxAsync(zaaerId);
            if (!deleted)
            {
                throw new InvalidOperationException($"Tax with ZaaerId {zaaerId} not found for deletion");
            }
        }
    }
}


