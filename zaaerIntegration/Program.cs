using Microsoft.EntityFrameworkCore;
using Serilog;
using zaaerIntegration.Configuration;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Implementations;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Implementations;
using zaaerIntegration.Services.Interfaces;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.Expense;
using zaaerIntegration.Middleware;
using zaaerIntegration.Services.PartnerQueueing;
using zaaerIntegration.Services.PartnerQueueing.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// ? ===== Multi-Tenant Configuration =====
// Configure Master Database Context (قاعدة البيانات المركزية)
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterDb")));

// HttpContextAccessor - للوصول إلى HTTP Request
builder.Services.AddHttpContextAccessor();

// Tenant Services - للحصول على معلومات الفندق الحالي
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IQueueSettingsProvider, QueueSettingsProvider>();

// Tenant DB Context Resolver - لإنشاء DbContext ديناميكي للـ Tenant
builder.Services.AddScoped<TenantDbContextResolver>();

// Configure Dynamic ApplicationDbContext for each Tenant
// ? لا يوجد هنا! سيُنشأ DbContext لكل request ديناميكياً
builder.Services.AddScoped<ApplicationDbContext>(sp =>
{
    var resolver = sp.GetRequiredService<TenantDbContextResolver>();
    return resolver.GetCurrentDbContext();
});

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register repositories
builder.Services.AddScoped<IGenericRepository<FinanceLedgerAPI.Models.Customer>, GenericRepository<FinanceLedgerAPI.Models.Customer>>();
builder.Services.AddScoped<IGenericRepository<FinanceLedgerAPI.Models.Floor>, GenericRepository<FinanceLedgerAPI.Models.Floor>>();
builder.Services.AddScoped<IGenericRepository<FinanceLedgerAPI.Models.Apartment>, GenericRepository<FinanceLedgerAPI.Models.Apartment>>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IPaymentReceiptRepository, PaymentReceiptRepository>();
builder.Services.AddScoped<ICorporateCustomerRepository, CorporateCustomerRepository>();
builder.Services.AddScoped<IReservationUnitRepository, ReservationUnitRepository>();
builder.Services.AddScoped<IApartmentRepository, ApartmentRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IRefundRepository, RefundRepository>();
builder.Services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
builder.Services.AddScoped<ICustomerIdentificationRepository, CustomerIdentificationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentReceiptService, PaymentReceiptService>();
builder.Services.AddScoped<ICustomerLedgerService, CustomerLedgerService>();
builder.Services.AddScoped<ICorporateCustomerService, CorporateCustomerService>();
builder.Services.AddScoped<IReservationUnitService, ReservationUnitService>();
builder.Services.AddScoped<IApartmentService, ApartmentService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IRefundService, RefundService>();
builder.Services.AddScoped<ICreditNoteService, CreditNoteService>();

// Register Zaaer services
builder.Services.AddScoped<IZaaerCustomerService, ZaaerCustomerService>();
builder.Services.AddScoped<IZaaerReservationService, ZaaerReservationService>();
builder.Services.AddScoped<IZaaerPaymentReceiptService, ZaaerPaymentReceiptService>();
builder.Services.AddScoped<IZaaerInvoiceService, ZaaerInvoiceService>();
builder.Services.AddScoped<IZaaerRefundService, ZaaerRefundService>();
builder.Services.AddScoped<IZaaerCreditNoteService, ZaaerCreditNoteService>();
builder.Services.AddScoped<IZaaerRoomTypeService, ZaaerRoomTypeService>();
builder.Services.AddScoped<IZaaerFloorService, ZaaerFloorService>();
builder.Services.AddScoped<IZaaerApartmentService, ZaaerApartmentService>();
builder.Services.AddScoped<IZaaerMaintenanceService, ZaaerMaintenanceService>();
builder.Services.AddScoped<IZaaerTaxService, ZaaerTaxService>();
builder.Services.AddScoped<IZaaerHotelSettingsService, ZaaerHotelSettingsService>();
builder.Services.AddScoped<IZaaerBuildingService, ZaaerBuildingService>();
builder.Services.AddScoped<IZaaerUserService, ZaaerUserService>();
builder.Services.AddScoped<IZaaerRoleService, ZaaerRoleService>();
builder.Services.AddScoped<IZaaerSeasonalRateService, ZaaerSeasonalRateService>();
builder.Services.AddScoped<IZaaerRateTypeService, ZaaerRateTypeService>();
builder.Services.AddScoped<IReservationRatesService, ReservationRatesService>();
builder.Services.AddScoped<IReservationUnitSwitchService, ReservationUnitSwitchService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IZaaerZatcaDetailsService, ZaaerZatcaDetailsService>();
builder.Services.AddScoped<IZaaerNtmpDetailsService, ZaaerNtmpDetailsService>();
builder.Services.AddScoped<IZaaerShomoosDetailsService, ZaaerShomoosDetailsService>();
builder.Services.AddScoped<IZaaerIntegrationResponseService, ZaaerIntegrationResponseService>();
builder.Services.AddScoped<IZaaerRoomTypeRateService, ZaaerRoomTypeRateService>();
builder.Services.AddScoped<IZaaerBankService, ZaaerBankService>();
builder.Services.AddScoped<IZaaerExpenseService, ZaaerExpenseService>();

// Register Expense Service (new CRUD operations with X-Hotel-Code header)
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// Partner Queue services
builder.Services.AddScoped<IPartnerQueueService, PartnerQueueService>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerReservationCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerReservationUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerReservationUpdateByNumberHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerCustomerUpdateByNumberHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerActivityLogCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerReservationUnitSwitchCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerReservationRatesApplyAllHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerReservationRatesUpsertHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRoomTypeRateCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRoomTypeRateUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRoomTypeRateUpdateByZaaerIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerCustomerCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerCustomerUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerInvoiceCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerInvoiceUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerPaymentReceiptCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerPaymentReceiptUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerPaymentReceiptUpdateByNumberHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRefundCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRefundUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRefundUpdateByNumberHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerCreditNoteCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerApartmentCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerApartmentCreateBulkHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerApartmentUpdateByZaaerIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerApartmentUpdateByCodeHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, AppReservationCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, AppReservationUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerFloorCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerFloorCreateBulkHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerFloorUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerHotelSettingsCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerHotelSettingsUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerUserCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerUserUpdateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRoleCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRoleUpdateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerBankCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerBankUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerExpenseCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerExpenseUpdateByIdHandler>();

// Register Expense handlers (new ExpenseController with X-Hotel-Code header support)
builder.Services.AddScoped<IQueuedOperationHandler, ExpenseCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ExpenseUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ExpenseDeleteHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ExpenseRoomAddHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ExpenseRoomUpdateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ExpenseRoomDeleteHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRoomTypeCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRoomTypeUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerBuildingCreateWithFloorsHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerBuildingUpdateWithFloorsHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerBuildingUpdateWithFloorsSafeHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerSeasonalRateCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerSeasonalRateUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRateTypeCreateHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRateTypeUpdateByIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRateTypeUpdateByZaaerIdHandler>();
builder.Services.AddScoped<IQueuedOperationHandler, ZaaerRateTypeDeleteByZaaerIdHandler>();
builder.Services.AddHostedService<PartnerQueueBackgroundWorker>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Zaaer Integration API - Multi-Tenant", Version = "v1" });
    
    // ? إضافة X-Hotel-Code Header في Swagger
    c.AddSecurityDefinition("X-Hotel-Code", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Hotel Code Header (e.g., Dammam1, Dammam2)",
        Name = "X-Hotel-Code",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "X-Hotel-Code"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "X-Hotel-Code"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Include XML comments if the file exists
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "zaaerIntegration.xml");
    if (File.Exists(xmlFile))
    {
        c.IncludeXmlComments(xmlFile);
    }

    // Examples for Reservation Tools payloads
    c.SchemaFilter<zaaerIntegration.Filters.ReservationToolSchemaExample>();
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zaaer Integration API v1");
    });
}

app.UseHttpsRedirection();

// Enable static files for frontend
app.UseStaticFiles();

// Enable default files (index.html)
app.UseDefaultFiles();

app.UseCors("AllowAll");

// ? تشغيل Multi-Tenant Middleware
// يجب أن يكون بعد CORS وقبل Authorization
app.UseTenantMiddleware();

app.UseAuthorization();

// Optional global queue middleware (proxy). Use separate flag so controllers can still enqueue while global proxy is off
if (builder.Configuration.GetValue<bool>("PartnerQueue:UseMiddleware"))
{
    app.UseMiddleware<PartnerQueueMiddleware>();
}

app.MapControllers();

// ? Ensure Master Database connection and verify tenants exist
// النظام يعتمد فقط على Master DB (db29328) - لا يضيف بيانات
using (var scope = app.Services.CreateScope())
{
    try
    {
        var masterContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        
        // التأكد من وجود الجداول فقط (لا يضيف بيانات)
        await masterContext.Database.EnsureCreatedAsync();
        
        // التحقق من وجود بيانات Tenants في Master DB فقط
        // البيانات موجودة بالفعل في Master DB - لا نضيف بيانات هنا
        // 🔴 DEBUG BREAKPOINT: ضع Breakpoint هنا لاختبار Master DB Connection
        var tenantsCount = await masterContext.Tenants.CountAsync();
        
        // 🔴 DEBUG BREAKPOINT: ضع Breakpoint هنا لرؤية عدد Tenants
        if (tenantsCount == 0)
        {
            Log.Warning("⚠️ No tenants found in Master DB (db29328).");
            Log.Warning("⚠️ Please add tenants manually to Master Database table 'Tenants'.");
            Log.Warning("⚠️ Required columns: Code, Name, DatabaseName");
            Log.Warning("⚠️ Optional columns: BaseUrl, ConnectionString (system uses DatabaseName instead)");
        }
        else
        {
            Log.Information("✅ Master Database connection successful. Found {Count} tenant(s) in Master DB", tenantsCount);
            
            // Log available tenants for debugging
            // 🔴 DEBUG BREAKPOINT: ضع Breakpoint هنا لرؤية Tenants في Master DB
            var tenantCodes = await masterContext.Tenants
                .Select(t => t.Code)
                .ToListAsync();
            Log.Information("✅ Available tenant codes: {Codes}", string.Join(", ", tenantCodes));
            
            // 🔴 DEBUG: يمكنك إضافة هذا الكود لرؤية Tenants في Debug
            // var allTenants = await masterContext.Tenants
            //     .Select(t => new { t.Id, t.Code, t.Name, t.DatabaseName })
            //     .ToListAsync();
            // Log.Information("✅ Tenants: {Tenants}", System.Text.Json.JsonSerializer.Serialize(allTenants));
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Failed to connect to Master Database: {Message}", ex.Message);
        Log.Error("❌ Make sure Master DB (db29328) is accessible and connection string is correct in appsettings.json");
        // لا نوقف التطبيق - فقط نسجل الخطأ
    }
}

app.Run();
