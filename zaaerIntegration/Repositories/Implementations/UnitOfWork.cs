using Microsoft.EntityFrameworkCore.Storage;
using FinanceLedgerAPI.Models;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Repositories.Implementations;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Unit of Work Implementation
    /// تنفيذ وحدة العمل
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly bool _ownsContext;
        private IDbContextTransaction? _transaction;

        // Repository properties
        public IGenericRepository<Customer> Customers { get; private set; }
        
        // Specific repositories
        public ICustomerRepository CustomerRepository { get; private set; }
        public IReservationRepository ReservationRepository { get; private set; }
        public IInvoiceRepository InvoiceRepository { get; private set; }
        public IPaymentReceiptRepository PaymentReceiptRepository { get; private set; }
        public ICorporateCustomerRepository CorporateCustomerRepository { get; private set; }
        public IReservationUnitRepository ReservationUnitRepository { get; private set; }
        public IApartmentRepository ApartmentRepository { get; private set; }
        public IBuildingRepository BuildingRepository { get; private set; }
        public IRoomTypeRepository RoomTypeRepository { get; private set; }
        public IGenericRepository<Reservation> Reservations { get; private set; }
        public IGenericRepository<Apartment> Apartments { get; private set; }
        public IGenericRepository<Building> Buildings { get; private set; }
        public IGenericRepository<Floor> Floors { get; private set; }
        public IGenericRepository<RoomType> RoomTypes { get; private set; }
        public IGenericRepository<Invoice> Invoices { get; private set; }
        public IGenericRepository<PaymentReceipt> PaymentReceipts { get; private set; }
        public IGenericRepository<CorporateCustomer> CorporateCustomers { get; private set; }
        public IGenericRepository<GuestType> GuestTypes { get; private set; }
        public IGenericRepository<GuestCategory> GuestCategories { get; private set; }
        public IGenericRepository<Nationality> Nationalities { get; private set; }
        public IGenericRepository<IdType> IdTypes { get; private set; }
        public IGenericRepository<VisitPurpose> VisitPurposes { get; private set; }
        public IGenericRepository<PaymentMethod> PaymentMethods { get; private set; }
        public IGenericRepository<Bank> Banks { get; private set; }
        public IGenericRepository<ReservationUnit> ReservationUnits { get; private set; }
        public IGenericRepository<CustomerAccount> CustomerAccounts { get; private set; }
        public IGenericRepository<CustomerTransaction> CustomerTransactions { get; private set; }
        public IGenericRepository<CustomerIdentification> CustomerIdentifications { get; private set; }
        public IGenericRepository<Discount> Discounts { get; private set; }
        public IGenericRepository<Penalty> Penalties { get; private set; }
        public IGenericRepository<Refund> Refunds { get; private set; }
        public IGenericRepository<CreditNote> CreditNotes { get; private set; }
        public IGenericRepository<User> Users { get; private set; }
        public IGenericRepository<Role> Roles { get; private set; }
        public IGenericRepository<Permission> Permissions { get; private set; }
        public IGenericRepository<RolePermission> RolePermissions { get; private set; }
        public IGenericRepository<HotelSettings> HotelSettings { get; private set; }
        
        // New tables (Rate Types, Seasonal Rates, etc.)
        public IGenericRepository<SeasonalRate> SeasonalRates { get; private set; }
        public IGenericRepository<SeasonalRateItem> SeasonalRateItems { get; private set; }
        public IGenericRepository<RateType> RateTypes { get; private set; }
        public IGenericRepository<RateTypeUnitItem> RateTypeUnitItems { get; private set; }
        public IGenericRepository<RoomTypeRate> RoomTypeRates { get; private set; }
        
        // Integration & Configuration tables
        public IGenericRepository<ZatcaDetails> ZatcaDetails { get; private set; }
        public IGenericRepository<NtmpDetails> NtmpDetails { get; private set; }
        public IGenericRepository<ShomoosDetails> ShomoosDetails { get; private set; }
        public IGenericRepository<IntegrationResponse> IntegrationResponses { get; private set; }
        public IGenericRepository<ActivityLog> ActivityLogs { get; private set; }
        public IGenericRepository<ReservationUnitSwitch> ReservationUnitSwitches { get; private set; }
        public IGenericRepository<ReservationUnitDayRate> ReservationUnitDayRates { get; private set; }
        public IGenericRepository<Maintenance> Maintenances { get; private set; }
        public IGenericRepository<Tax> Taxes { get; private set; }

        // Expense tables
        public IGenericRepository<Expense> Expenses { get; private set; }
        public IGenericRepository<ExpenseRoom> ExpenseRooms { get; private set; }
        public IGenericRepository<ExpenseCategory> ExpenseCategories { get; private set; }

        public UnitOfWork(ApplicationDbContext context, bool ownsContext = true)
        {
            _context = context;
            _ownsContext = ownsContext;
            
            // Initialize repositories
            Customers = new GenericRepository<Customer>(_context);
            
            // Initialize specific repositories
            CustomerRepository = new CustomerRepository(_context);
            ReservationRepository = new ReservationRepository(_context);
            InvoiceRepository = new InvoiceRepository(_context);
            PaymentReceiptRepository = new PaymentReceiptRepository(_context);
            CorporateCustomerRepository = new CorporateCustomerRepository(_context);
            ReservationUnitRepository = new ReservationUnitRepository(_context);
            ApartmentRepository = new ApartmentRepository(_context);
            BuildingRepository = new BuildingRepository(_context);
            RoomTypeRepository = new RoomTypeRepository(_context);
            Reservations = new GenericRepository<Reservation>(_context);
            Apartments = new GenericRepository<Apartment>(_context);
            Buildings = new GenericRepository<Building>(_context);
            Floors = new GenericRepository<Floor>(_context);
            RoomTypes = new GenericRepository<RoomType>(_context);
            Invoices = new GenericRepository<Invoice>(_context);
            PaymentReceipts = new GenericRepository<PaymentReceipt>(_context);
            CorporateCustomers = new GenericRepository<CorporateCustomer>(_context);
            GuestTypes = new GenericRepository<GuestType>(_context);
            GuestCategories = new GenericRepository<GuestCategory>(_context);
            Nationalities = new GenericRepository<Nationality>(_context);
            IdTypes = new GenericRepository<IdType>(_context);
            VisitPurposes = new GenericRepository<VisitPurpose>(_context);
            PaymentMethods = new GenericRepository<PaymentMethod>(_context);
            Banks = new GenericRepository<Bank>(_context);
            ReservationUnits = new GenericRepository<ReservationUnit>(_context);
            CustomerAccounts = new GenericRepository<CustomerAccount>(_context);
            CustomerTransactions = new GenericRepository<CustomerTransaction>(_context);
            CustomerIdentifications = new GenericRepository<CustomerIdentification>(_context);
            Discounts = new GenericRepository<Discount>(_context);
            Penalties = new GenericRepository<Penalty>(_context);
            Refunds = new GenericRepository<Refund>(_context);
            CreditNotes = new GenericRepository<CreditNote>(_context);
            Users = new GenericRepository<User>(_context);
            Roles = new GenericRepository<Role>(_context);
            Permissions = new GenericRepository<Permission>(_context);
            RolePermissions = new GenericRepository<RolePermission>(_context);
            HotelSettings = new GenericRepository<HotelSettings>(_context);
            
            // New tables (Rate Types, Seasonal Rates, etc.)
            SeasonalRates = new GenericRepository<SeasonalRate>(_context);
            SeasonalRateItems = new GenericRepository<SeasonalRateItem>(_context);
            RateTypes = new GenericRepository<RateType>(_context);
            RateTypeUnitItems = new GenericRepository<RateTypeUnitItem>(_context);
            RoomTypeRates = new GenericRepository<RoomTypeRate>(_context);
            
            // Integration & Configuration tables
            ZatcaDetails = new GenericRepository<ZatcaDetails>(_context);
            NtmpDetails = new GenericRepository<NtmpDetails>(_context);
            ShomoosDetails = new GenericRepository<ShomoosDetails>(_context);
            IntegrationResponses = new GenericRepository<IntegrationResponse>(_context);
            ActivityLogs = new GenericRepository<ActivityLog>(_context);
            ReservationUnitSwitches = new GenericRepository<ReservationUnitSwitch>(_context);
            ReservationUnitDayRates = new GenericRepository<ReservationUnitDayRate>(_context);
            Maintenances = new GenericRepository<Maintenance>(_context);
            Taxes = new GenericRepository<Tax>(_context);
            
            // Expense tables
            Expenses = new GenericRepository<Expense>(_context);
            ExpenseRooms = new GenericRepository<ExpenseRoom>(_context);
            ExpenseCategories = new GenericRepository<ExpenseCategory>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            if (_ownsContext)
            {
                _context.Dispose();
            }
        }
    }
}
