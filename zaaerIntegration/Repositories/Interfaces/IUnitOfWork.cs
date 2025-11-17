using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work Interface
    /// واجهة وحدة العمل
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Generic repositories
        IGenericRepository<Customer> Customers { get; }
        
        // Specific repositories
        ICustomerRepository CustomerRepository { get; }
        IReservationRepository ReservationRepository { get; }
        IInvoiceRepository InvoiceRepository { get; }
        IPaymentReceiptRepository PaymentReceiptRepository { get; }
        ICorporateCustomerRepository CorporateCustomerRepository { get; }
        IReservationUnitRepository ReservationUnitRepository { get; }
        IApartmentRepository ApartmentRepository { get; }
        IBuildingRepository BuildingRepository { get; }
        IRoomTypeRepository RoomTypeRepository { get; }
        IGenericRepository<Reservation> Reservations { get; }
        IGenericRepository<Apartment> Apartments { get; }
        IGenericRepository<Building> Buildings { get; }
        IGenericRepository<Floor> Floors { get; }
        IGenericRepository<RoomType> RoomTypes { get; }
        IGenericRepository<Invoice> Invoices { get; }
        IGenericRepository<PaymentReceipt> PaymentReceipts { get; }
        IGenericRepository<CorporateCustomer> CorporateCustomers { get; }
        IGenericRepository<GuestType> GuestTypes { get; }
        IGenericRepository<GuestCategory> GuestCategories { get; }
        IGenericRepository<Nationality> Nationalities { get; }
        IGenericRepository<IdType> IdTypes { get; }
        IGenericRepository<VisitPurpose> VisitPurposes { get; }
        IGenericRepository<PaymentMethod> PaymentMethods { get; }
        IGenericRepository<Bank> Banks { get; }
        IGenericRepository<ReservationUnit> ReservationUnits { get; }
        IGenericRepository<CustomerAccount> CustomerAccounts { get; }
        IGenericRepository<CustomerTransaction> CustomerTransactions { get; }
        IGenericRepository<CustomerIdentification> CustomerIdentifications { get; }
        IGenericRepository<Discount> Discounts { get; }
        IGenericRepository<Penalty> Penalties { get; }
        IGenericRepository<Refund> Refunds { get; }
        IGenericRepository<CreditNote> CreditNotes { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<Role> Roles { get; }
        IGenericRepository<Permission> Permissions { get; }
        IGenericRepository<RolePermission> RolePermissions { get; }
        IGenericRepository<HotelSettings> HotelSettings { get; }
        
        // New tables (Rate Types, Seasonal Rates, etc.)
        IGenericRepository<SeasonalRate> SeasonalRates { get; }
        IGenericRepository<SeasonalRateItem> SeasonalRateItems { get; }
        IGenericRepository<RateType> RateTypes { get; }
        IGenericRepository<RateTypeUnitItem> RateTypeUnitItems { get; }
        IGenericRepository<RoomTypeRate> RoomTypeRates { get; }
        
        // Integration & Configuration tables
        IGenericRepository<ZatcaDetails> ZatcaDetails { get; }
        IGenericRepository<NtmpDetails> NtmpDetails { get; }
        IGenericRepository<ShomoosDetails> ShomoosDetails { get; }
        IGenericRepository<IntegrationResponse> IntegrationResponses { get; }
        IGenericRepository<ActivityLog> ActivityLogs { get; }
        IGenericRepository<ReservationUnitSwitch> ReservationUnitSwitches { get; }
        IGenericRepository<ReservationUnitDayRate> ReservationUnitDayRates { get; }
        IGenericRepository<Maintenance> Maintenances { get; }
        IGenericRepository<Tax> Taxes { get; }

        // Expense tables
        IGenericRepository<Expense> Expenses { get; }
        IGenericRepository<ExpenseRoom> ExpenseRooms { get; }
        IGenericRepository<ExpenseCategory> ExpenseCategories { get; }

        /// <summary>
        /// Save changes to database
        /// حفظ التغييرات في قاعدة البيانات
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begin transaction
        /// بدء المعاملة
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commit transaction
        /// تأكيد المعاملة
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rollback transaction
        /// إلغاء المعاملة
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
