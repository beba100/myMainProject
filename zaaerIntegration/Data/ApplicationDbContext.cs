using Microsoft.EntityFrameworkCore;
using FinanceLedgerAPI.Models;
using zaaerIntegration.Models;

namespace zaaerIntegration.Data
{
    /// <summary>
    /// Application Database Context
    /// سياق قاعدة البيانات للتطبيق
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<PaymentReceipt> PaymentReceipts { get; set; }
        public DbSet<CorporateCustomer> CorporateCustomers { get; set; }
        public DbSet<GuestType> GuestTypes { get; set; }
        public DbSet<GuestCategory> GuestCategories { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<IdType> IdTypes { get; set; }
        public DbSet<VisitPurpose> VisitPurposes { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseRoom> ExpenseRooms { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<ExpenseImage> ExpenseImages { get; set; }
        public DbSet<ReservationUnit> ReservationUnits { get; set; }
        public DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
        public DbSet<CustomerIdentification> CustomerIdentifications { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Penalty> Penalties { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<CreditNote> CreditNotes { get; set; }
        public DbSet<HotelSettings> HotelSettings { get; set; }
        // User, Role, Permission, RolePermission removed - now in Master DB only
        public DbSet<RoomTypeRate> RoomTypeRates { get; set; }
        public DbSet<SeasonalRate> SeasonalRates { get; set; }
        public DbSet<SeasonalRateItem> SeasonalRateItems { get; set; }
        public DbSet<ZatcaDetails> ZatcaDetails { get; set; }
        public DbSet<NtmpDetails> NtmpDetails { get; set; }
        public DbSet<ShomoosDetails> ShomoosDetails { get; set; }
        public DbSet<PartnerQueue> PartnerRequestQueue { get; set; }
        public DbSet<PartnerRequestLog> PartnerRequestLog { get; set; }
        public DbSet<IntegrationResponse> IntegrationResponses { get; set; }
        public DbSet<ReservationUnitDayRate> ReservationUnitDayRates { get; set; }
        public DbSet<ReservationUnitSwitch> ReservationUnitSwitches { get; set; }
        public DbSet<ReservationUnitSwitch> ReservationUnitSwaps { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<RateType> RateTypes { get; set; }
        public DbSet<RateTypeUnitItem> RateTypeUnitItems { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<TaxCategory> TaxCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore all problematic navigation properties globally
            IgnoreProblematicNavigationProperties(modelBuilder);

            // Configure relationships and constraints
            ConfigureCustomerRelationships(modelBuilder);
            ConfigureReservationRelationships(modelBuilder);
            ConfigureReservationUnitRelationships(modelBuilder);
            ConfigureInvoiceRelationships(modelBuilder);
            ConfigurePaymentRelationships(modelBuilder);
            ConfigureRefundRelationships(modelBuilder);
            ConfigureCreditNoteRelationships(modelBuilder);
            ConfigureApartmentRelationships(modelBuilder);
            ConfigureBuildingRelationships(modelBuilder);
            ConfigureFloorRelationships(modelBuilder);
            ConfigureRoomTypeRelationships(modelBuilder);
            ConfigureCorporateCustomerRelationships(modelBuilder);
            // User, Role, Permission, RolePermission relationships removed - now in Master DB only
            ConfigureRoomTypeRateRelationships(modelBuilder);
            ConfigureSeasonalRateRelationships(modelBuilder);
            ConfigureZatcaDetails(modelBuilder);
            ConfigureNtmpDetails(modelBuilder);
            ConfigureShomoosDetails(modelBuilder);
            ConfigureExpenseRelationships(modelBuilder);
            ConfigureIntegrationResponse(modelBuilder);
            ConfigureReservationUnitDayRates(modelBuilder);
            ConfigureReservationUnitSwitches(modelBuilder);
            ConfigureReservationUnitSwaps(modelBuilder);
            ConfigureActivityLogs(modelBuilder);
            ConfigureRateTypes(modelBuilder);
            ConfigureTaxes(modelBuilder);
            
            // Configure HotelSettings entity to ensure nullable properties are properly configured
            ConfigureHotelSettings(modelBuilder);

            // partner queue entities
            modelBuilder.Entity<PartnerQueue>(entity =>
            {
                entity.HasIndex(e => e.RequestRef).HasDatabaseName("IX_partner_request_queue_request_ref");
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.OperationKey).HasMaxLength(150);
                entity.Property(e => e.PayloadType).HasMaxLength(200);
            });

            modelBuilder.Entity<PartnerRequestLog>(entity =>
            {
                entity.HasIndex(e => e.RequestRef).HasDatabaseName("IX_partner_request_log_request_ref");
                entity.Property(e => e.Status).HasMaxLength(50);
            });
        }

        private void ConfigureTaxes(ModelBuilder modelBuilder)
        {
            // Tax -> TaxCategory (optional) FK on tax_id
            modelBuilder.Entity<Tax>()
                .HasOne(t => t.TaxCategory)
                .WithMany()
                .HasForeignKey(t => t.TaxId)
                .HasConstraintName("FK_Taxes_TaxCategories")
                .OnDelete(DeleteBehavior.SetNull);

            // Useful indexes
            modelBuilder.Entity<Tax>()
                .HasIndex(t => t.TaxId)
                .HasDatabaseName("IX_Taxes_TaxId");
        }

        private void ConfigureHotelSettings(ModelBuilder modelBuilder)
        {
            // Explicitly configure HotelSettings nullable properties
            // This ensures EF Core knows these columns should allow NULL values
            modelBuilder.Entity<HotelSettings>(entity =>
            {
                // All string properties that are nullable in the model
                entity.Property(e => e.HotelCode).IsRequired(false);
                entity.Property(e => e.HotelName).IsRequired(false);
                entity.Property(e => e.DefaultCurrency).IsRequired(false);
                entity.Property(e => e.CompanyName).IsRequired(false);
                entity.Property(e => e.LogoUrl).IsRequired(false); // CRITICAL: This must be nullable
                entity.Property(e => e.Phone).IsRequired(false);
                entity.Property(e => e.Email).IsRequired(false);
                entity.Property(e => e.TaxNumber).IsRequired(false);
                entity.Property(e => e.CrNumber).IsRequired(false);
                entity.Property(e => e.CountryCode).IsRequired(false);
                entity.Property(e => e.City).IsRequired(false);
                entity.Property(e => e.ContactPerson).IsRequired(false);
                entity.Property(e => e.Address).IsRequired(false);
                entity.Property(e => e.Latitude).IsRequired(false);
                entity.Property(e => e.Longitude).IsRequired(false);
                entity.Property(e => e.PropertyType).IsRequired(false);
                entity.Property(e => e.CreatedAt).IsRequired(false);
                entity.Property(e => e.ZaaerId).IsRequired(false);
            });
        }

        private void IgnoreProblematicNavigationProperties(ModelBuilder modelBuilder)
        {
            // Ignore all navigation properties that cause shadow property issues
            modelBuilder.Entity<Invoice>()
                .Ignore(i => i.Reservation)
                .Ignore(i => i.ReservationUnit)
                .Ignore(i => i.PaymentReceipts)
                .Ignore(i => i.Refunds)
                .Ignore(i => i.CreditNotes)
                .Ignore(i => i.CustomerTransactions);

            modelBuilder.Entity<Reservation>()
                .Ignore(r => r.VisitPurpose)
                .Ignore(r => r.CorporateCustomer)
                .Ignore(r => r.Invoices)
                .Ignore(r => r.PaymentReceipts)
                .Ignore(r => r.Refunds);

            modelBuilder.Entity<ReservationUnit>()
                .Ignore(ru => ru.Apartment)
                .Ignore(ru => ru.Invoices)
                .Ignore(ru => ru.PaymentReceipts)
                .Ignore(ru => ru.Refunds);

            modelBuilder.Entity<PaymentReceipt>()
                .Ignore(pr => pr.Reservation)
                .Ignore(pr => pr.ReservationUnit)
                .Ignore(pr => pr.PaymentMethodNavigation)
                .Ignore(pr => pr.BankNavigation)
                .Ignore(pr => pr.CustomerTransactions);

            modelBuilder.Entity<Refund>()
                .Ignore(r => r.Reservation)
                .Ignore(r => r.ReservationUnit)
                .Ignore(r => r.PaymentMethodNavigation)
                .Ignore(r => r.BankNavigation)
                .Ignore(r => r.CustomerTransactions);

            modelBuilder.Entity<CreditNote>()
                .Ignore(cn => cn.Reservation);

            modelBuilder.Entity<Apartment>()
                .Ignore(a => a.HotelSettings)
                .Ignore(a => a.Building)
                .Ignore(a => a.Floor)
                .Ignore(a => a.RoomType)
                .Ignore(a => a.ReservationUnits);

            modelBuilder.Entity<Building>()
                .Ignore(b => b.HotelSettings)
                .Ignore(b => b.Floors)
                .Ignore(b => b.Apartments);

            modelBuilder.Entity<Floor>()
                .Ignore(f => f.HotelSettings)
                .Ignore(f => f.Apartments);

            modelBuilder.Entity<RoomType>()
                .Ignore(rt => rt.HotelSettings)
                .Ignore(rt => rt.Apartments);

            modelBuilder.Entity<RoomTypeRate>()
                .Ignore(rtr => rtr.RoomType)
                .Ignore(rtr => rtr.HotelSettings);

            // NOTE: RateTypeUnitItem navigation property is NOT ignored here
            // Instead, relationship configuration is removed in ConfigureRateTypeRelationships
            // This allows rate_type_id values that don't exist in rate_types table
            // Manual joins or manual loading can be used in Service if needed

            modelBuilder.Entity<CorporateCustomer>()
                .Ignore(cc => cc.HotelSettings)
                .Ignore(cc => cc.Reservations);

            modelBuilder.Entity<Customer>()
                .Ignore(c => c.GuestType)
                .Ignore(c => c.GuestCategory)
                .Ignore(c => c.Nationality)
                .Ignore(c => c.HotelSettings)
                .Ignore(c => c.Reservations)
                .Ignore(c => c.Invoices)
                .Ignore(c => c.PaymentReceipts)
                .Ignore(c => c.Refunds)
                .Ignore(c => c.CustomerAccounts)
                .Ignore(c => c.CustomerTransactions)
                .Ignore(c => c.Identifications);

        }

        private void ConfigureCustomerRelationships(ModelBuilder modelBuilder)
        {
            // Customer relationships
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.GuestType)
                .WithMany(gt => gt.Customers)
                .HasForeignKey(c => c.GtypeId)
                .HasConstraintName("FK_Customers_GuestTypes")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Nationality)
                .WithMany(n => n.Customers)
                .HasForeignKey(c => c.NId)
                .HasConstraintName("FK_Customers_Nationalities")
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Customer>()
                .HasOne(c => c.GuestCategory)
                .WithMany(gc => gc.Customers)
                .HasForeignKey(c => c.GuestCategoryId)
                .HasConstraintName("FK_Customers_GuestCategories")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.HotelSettings)
                .WithMany(hs => hs.Customers)
                .HasForeignKey(c => c.HotelId)
                .HasConstraintName("FK_Customers_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

        }

        private void ConfigureReservationRelationships(ModelBuilder modelBuilder)
        {
            // Reservation relationships
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.HotelSettings)
                .WithMany(hs => hs.Reservations)
                .HasForeignKey(r => r.HotelId)
                .HasConstraintName("FK_Reservations_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne<Customer>()
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.CustomerId)
                .HasConstraintName("FK_Reservations_Customers")
                .OnDelete(DeleteBehavior.Restrict);

            // Note: VisitPurpose and CorporateCustomer relationships are not configured
            // because their navigation properties are ignored to prevent shadow property issues
        }

        private void ConfigureReservationUnitRelationships(ModelBuilder modelBuilder)
        {
            // ReservationUnit relationships
            modelBuilder.Entity<ReservationUnit>()
                .HasOne(ru => ru.Reservation)
                .WithMany(r => r.ReservationUnits)
                .HasForeignKey(ru => ru.ReservationId)
                .HasConstraintName("FK_ReservationUnits_Reservations")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReservationUnit>()
                .HasOne<Apartment>()
                .WithMany(a => a.ReservationUnits)
                .HasForeignKey(ru => ru.ApartmentId)
                .HasConstraintName("FK_ReservationUnits_Apartments")
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureInvoiceRelationships(ModelBuilder modelBuilder)
        {
            // Invoice relationships
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.HotelSettings)
                .WithMany(hs => hs.Invoices)
                .HasForeignKey(i => i.HotelId)
                .HasConstraintName("FK_Invoices_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Reservation)
                .WithMany()
                .HasForeignKey(i => i.ReservationId)
                .HasConstraintName("FK_Invoices_Reservations")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.ReservationUnit)
                .WithMany()
                .HasForeignKey(i => i.UnitId)
                .HasConstraintName("FK_Invoices_ReservationUnits")
                .OnDelete(DeleteBehavior.Restrict);

        }

        private void ConfigurePaymentRelationships(ModelBuilder modelBuilder)
        {
            // Payment Receipt relationships
            modelBuilder.Entity<PaymentReceipt>()
                .HasOne(pr => pr.HotelSettings)
                .WithMany(hs => hs.PaymentReceipts)
                .HasForeignKey(pr => pr.HotelId)
                .HasConstraintName("FK_PaymentReceipts_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentReceipt>()
                .HasOne(pr => pr.Reservation)
                .WithMany()
                .HasForeignKey(pr => pr.ReservationId)
                .HasConstraintName("FK_PaymentReceipts_Reservations")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentReceipt>()
                .HasOne(pr => pr.Invoice)
                .WithMany(i => i.PaymentReceipts)
                .HasForeignKey(pr => pr.InvoiceId)
                .HasConstraintName("FK_PaymentReceipts_Invoices")
                .OnDelete(DeleteBehavior.Restrict);

            // Note: PaymentMethodNavigation and BankNavigation relationships are not configured
            // because their navigation properties are ignored to prevent shadow property issues
        }

        private void ConfigureRefundRelationships(ModelBuilder modelBuilder)
        {
            // Refund relationships
            modelBuilder.Entity<Refund>()
                .HasOne(r => r.HotelSettings)
                .WithMany(hs => hs.Refunds)
                .HasForeignKey(r => r.HotelId)
                .HasConstraintName("FK_Refunds_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Refund>()
                .HasOne(r => r.Reservation)
                .WithMany()
                .HasForeignKey(r => r.ReservationId)
                .HasConstraintName("FK_Refunds_Reservations")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Refund>()
                .HasOne(r => r.Invoice)
                .WithMany(i => i.Refunds)
                .HasForeignKey(r => r.InvoiceId)
                .HasConstraintName("FK_Refunds_Invoices")
                .OnDelete(DeleteBehavior.Restrict);

            // Note: PaymentMethodNavigation and BankNavigation relationships are not configured
            // because their navigation properties are ignored to prevent shadow property issues
        }

        private void ConfigureApartmentRelationships(ModelBuilder modelBuilder)
        {
            // Apartment relationships
            modelBuilder.Entity<Apartment>()
                .HasOne(a => a.HotelSettings)
                .WithMany(hs => hs.Apartments)
                .HasForeignKey(a => a.HotelId)
                .HasConstraintName("FK_Apartments_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            // Note: Building relationship is removed to allow building_id = 0 without FK issues
            // modelBuilder.Entity<Apartment>()
            //     .HasOne(a => a.Building)
            //     .WithMany(b => b.Apartments)
            //     .HasForeignKey(a => a.BuildingId)
            //     .HasConstraintName("FK_Apartments_Buildings")
            //     .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Apartment>()
                .HasOne(a => a.Floor)
                .WithMany(f => f.Apartments)
                .HasForeignKey(a => a.FloorId)
                .HasConstraintName("FK_Apartments_Floors")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Apartment>()
                .HasOne(a => a.RoomType)
                .WithMany(rt => rt.Apartments)
                .HasForeignKey(a => a.RoomTypeId)
                .HasConstraintName("FK_Apartments_RoomTypes")
                .OnDelete(DeleteBehavior.Restrict);

        }

        private void ConfigureExpenseRelationships(ModelBuilder modelBuilder)
        {
            // Expense -> HotelSettings
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.HotelSettings)
                .WithMany()
                .HasForeignKey(e => e.HotelId)
                .HasConstraintName("FK_Expenses_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            // Expense -> ExpenseCategory
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.ExpenseCategory)
                .WithMany()
                .HasForeignKey(e => e.ExpenseCategoryId)
                .HasConstraintName("FK_Expenses_ExpenseCategories")
                .OnDelete(DeleteBehavior.SetNull);

            // Expense -> ExpenseRooms (One-to-Many)
            modelBuilder.Entity<Expense>()
                .HasMany(e => e.ExpenseRooms)
                .WithOne(er => er.Expense)
                .HasForeignKey(er => er.ExpenseId)
                .HasConstraintName("FK_ExpenseRooms_Expenses")
                .OnDelete(DeleteBehavior.Cascade);

            // Expense -> ExpenseImages (One-to-Many)
            modelBuilder.Entity<Expense>()
                .HasMany(e => e.ExpenseImages)
                .WithOne(ei => ei.Expense)
                .HasForeignKey(ei => ei.ExpenseId)
                .HasConstraintName("FK_ExpenseImages_Expenses")
                .OnDelete(DeleteBehavior.Cascade);

            // ExpenseRoom -> Apartment (optional - nullable for room categories)
            // Foreign Key: expense_rooms.zaaer_id -> apartments.zaaer_id
            modelBuilder.Entity<ExpenseRoom>()
                .HasOne(er => er.Apartment)
                .WithMany()
                .HasForeignKey(er => er.ZaaerId)
                .HasPrincipalKey(a => a.ZaaerId) // ✅ Use zaaer_id as principal key instead of apartment_id
                .HasConstraintName("FK_ExpenseRooms_Apartments_ZaaerId")
                .OnDelete(DeleteBehavior.NoAction) // ✅ SQL Server uses NO ACTION instead of RESTRICT
                .IsRequired(false); // ✅ Allow null for room categories

            // ExpenseCategory -> HotelSettings
            modelBuilder.Entity<ExpenseCategory>()
                .HasOne(ec => ec.HotelSettings)
                .WithMany()
                .HasForeignKey(ec => ec.HotelId)
                .HasConstraintName("FK_ExpenseCategories_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureBuildingRelationships(ModelBuilder modelBuilder)
        {
            // Building relationships
            modelBuilder.Entity<Building>()
                .HasOne(b => b.HotelSettings)
                .WithMany(hs => hs.Buildings)
                .HasForeignKey(b => b.HotelId)
                .HasConstraintName("FK_Buildings_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

        }

        private void ConfigureFloorRelationships(ModelBuilder modelBuilder)
        {
            // Floor relationships
            modelBuilder.Entity<Floor>()
                .HasOne(f => f.HotelSettings)
                .WithMany(hs => hs.Floors)
                .HasForeignKey(f => f.HotelId)
                .HasConstraintName("FK_Floors_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship between Floor and Building
            modelBuilder.Entity<Floor>()
                .HasOne(f => f.Building)
                .WithMany(b => b.Floors)
                .HasForeignKey(f => f.BuildingId)
                .HasConstraintName("FK_Floors_Buildings")
                .OnDelete(DeleteBehavior.Restrict);

        }

        private void ConfigureRoomTypeRelationships(ModelBuilder modelBuilder)
        {
            // RoomType relationships
            modelBuilder.Entity<RoomType>()
                .HasOne(rt => rt.HotelSettings)
                .WithMany(hs => hs.RoomTypes)
                .HasForeignKey(rt => rt.HotelId)
                .HasConstraintName("FK_RoomTypes_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

        }

        private void ConfigureCorporateCustomerRelationships(ModelBuilder modelBuilder)
        {
            // CorporateCustomer relationships
            modelBuilder.Entity<CorporateCustomer>()
                .HasOne(cc => cc.HotelSettings)
                .WithMany(hs => hs.CorporateCustomers)
                .HasForeignKey(cc => cc.HotelId)
                .HasConstraintName("FK_CorporateCustomers_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

        }

        private void ConfigureCreditNoteRelationships(ModelBuilder modelBuilder)
        {
            // CreditNote relationships
            modelBuilder.Entity<CreditNote>()
                .HasOne(cn => cn.HotelSettings)
                .WithMany(hs => hs.CreditNotes)
                .HasForeignKey(cn => cn.HotelId)
                .HasConstraintName("FK_CreditNotes_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CreditNote>()
                .HasOne(cn => cn.Invoice)
                .WithMany(i => i.CreditNotes)
                .HasForeignKey(cn => cn.InvoiceId)
                .HasConstraintName("FK_CreditNotes_Invoices")
                .OnDelete(DeleteBehavior.Restrict);

            // Note: Reservation relationship is not configured to prevent shadow property issues
            // The ReservationId foreign key will still work for data integrity
        }

        // User, Role, Permission, RolePermission relationships removed
        // These entities are now in Master DB only, not in Tenant DB

        private void ConfigureRoomTypeRateRelationships(ModelBuilder modelBuilder)
        {
            // RoomTypeRate relationships
            modelBuilder.Entity<RoomTypeRate>()
                .HasOne(rtr => rtr.HotelSettings)
                .WithMany()
                .HasForeignKey(rtr => rtr.HotelId)
                .HasConstraintName("FK_RoomTypeRates_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoomTypeRate>()
                .HasOne(rtr => rtr.RoomType)
                .WithMany()
                .HasForeignKey(rtr => rtr.RoomTypeId)
                .HasConstraintName("FK_RoomTypeRates_RoomTypes")
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: one rate per room type per hotel
            modelBuilder.Entity<RoomTypeRate>()
                .HasIndex(rtr => new { rtr.RoomTypeId, rtr.HotelId })
                .IsUnique()
                .HasDatabaseName("IX_RoomTypeRates_RoomTypeId_HotelId");
        }

        private void ConfigureSeasonalRateRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SeasonalRate>()
                .HasOne(sr => sr.HotelSettings)
                .WithMany()
                .HasForeignKey(sr => sr.HotelId)
                .HasConstraintName("FK_SeasonalRates_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SeasonalRateItem>()
                .HasOne(i => i.Season)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.SeasonId)
                .HasConstraintName("FK_SeasonalRateItems_SeasonalRates")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SeasonalRateItem>()
                .HasOne(i => i.RoomType)
                .WithMany()
                .HasForeignKey(i => i.RoomTypeId)
                .HasConstraintName("FK_SeasonalRateItems_RoomTypes")
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure single item per room type per season
            modelBuilder.Entity<SeasonalRateItem>()
                .HasIndex(i => new { i.SeasonId, i.RoomTypeId })
                .IsUnique()
                .HasDatabaseName("IX_SeasonalRateItems_Season_RoomType");
        }

        private void ConfigureZatcaDetails(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ZatcaDetails>()
                .HasOne(z => z.HotelSettings)
                .WithMany()
                .HasForeignKey(z => z.HotelId)
                .HasConstraintName("FK_ZatcaDetails_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            // optional: one record per hotel (unique)
            modelBuilder.Entity<ZatcaDetails>()
                .HasIndex(z => z.HotelId)
                .IsUnique()
                .HasDatabaseName("IX_ZatcaDetails_HotelId");
        }

        private void ConfigureNtmpDetails(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NtmpDetails>()
                .HasOne(n => n.HotelSettings)
                .WithMany()
                .HasForeignKey(n => n.HotelId)
                .HasConstraintName("FK_NtmpDetails_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            // Optional: one NTMP config per hotel
            modelBuilder.Entity<NtmpDetails>()
                .HasIndex(n => n.HotelId)
                .IsUnique()
                .HasDatabaseName("IX_NtmpDetails_HotelId");
        }

        private void ConfigureShomoosDetails(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShomoosDetails>()
                .HasOne(n => n.HotelSettings)
                .WithMany()
                .HasForeignKey(n => n.HotelId)
                .HasConstraintName("FK_ShomoosDetails_HotelSettings")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShomoosDetails>()
                .HasIndex(n => n.HotelId)
                .IsUnique()
                .HasDatabaseName("IX_ShomoosDetails_HotelId");
        }

        private void ConfigureIntegrationResponse(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IntegrationResponse>(entity =>
            {
                entity.HasOne<HotelSettings>()
                    .WithMany()
                    .HasForeignKey(e => e.HotelId)
                    .HasConstraintName("FK_IntegrationResponses_HotelSettings")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.HotelId).HasDatabaseName("IX_IntegrationResponses_HotelId");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_IntegrationResponses_CreatedAt");
                entity.HasIndex(e => e.Service).HasDatabaseName("IX_IntegrationResponses_Service");
                entity.Property(e => e.Service).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
            });
        }

        private void ConfigureReservationUnitDayRates(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReservationUnitDayRate>(entity =>
            {
                entity.HasOne(r => r.Reservation)
                    .WithMany()
                    .HasForeignKey(r => r.ReservationId)
                    .HasConstraintName("FK_RUDR_Reservation")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.ReservationUnit)
                    .WithMany()
                    .HasForeignKey(r => r.UnitId)
                    .HasConstraintName("FK_RUDR_ReservationUnit")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UnitId, e.NightDate })
                    .IsUnique()
                    .HasDatabaseName("UQ_RUDR_Unit_Date");
            });
        }

        private void ConfigureReservationUnitSwitches(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReservationUnitSwitch>(entity =>
            {
                entity.HasOne(s => s.Reservation)
                    .WithMany()
                    .HasForeignKey(s => s.ReservationId)
                    .HasConstraintName("FK_RUSwitch_Reservation")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.ReservationUnit)
                    .WithMany()
                    .HasForeignKey(s => s.UnitId)
                    .HasConstraintName("FK_RUSwitch_ReservationUnit")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(s => s.ApplyMode).HasMaxLength(30);
                entity.HasIndex(s => s.ReservationId).HasDatabaseName("IX_RUSwitch_Reservation");
                entity.HasIndex(s => s.UnitId).HasDatabaseName("IX_RUSwitch_Unit");
            });
        }

        private void ConfigureReservationUnitSwaps(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReservationUnitSwitch>(entity =>
            {
                entity.HasOne(s => s.Reservation)
                    .WithMany()
                    .HasForeignKey(s => s.ReservationId)
                    .HasConstraintName("FK_RUS_Reservation")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.ReservationUnit)
                    .WithMany()
                    .HasForeignKey(s => s.UnitId)
                    .HasConstraintName("FK_RUS_ReservationUnit")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(s => s.ApplyMode).HasMaxLength(30);
                entity.Property(s => s.Comment).HasMaxLength(500);
                entity.HasIndex(s => s.ReservationId).HasDatabaseName("IX_RUS_ReservationId");
            });
        }

        private void ConfigureActivityLogs(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasIndex(x => x.HotelId).HasDatabaseName("IX_ActivityLogs_HotelId");
                entity.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_ActivityLogs_CreatedAt");
                entity.Property(x => x.EventKey).HasMaxLength(100);
                entity.Property(x => x.RefType).HasMaxLength(50);
                entity.Property(x => x.RefNo).HasMaxLength(100);
                entity.Property(x => x.CreatedBy).HasMaxLength(200);
            });
        }

        private void ConfigureRateTypes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RateType>(entity =>
            {
                entity.HasOne(r => r.HotelSettings)
                    .WithMany()
                    .HasForeignKey(r => r.HotelId)
                    .HasConstraintName("FK_RateTypes_HotelSettings")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => r.HotelId).HasDatabaseName("IX_RateTypes_HotelId");
                entity.HasIndex(r => new { r.HotelId, r.ShortCode })
                    .IsUnique()
                    .HasDatabaseName("UQ_RateTypes_HotelId_ShortCode");
                entity.Property(r => r.ShortCode).HasMaxLength(50);
                entity.Property(r => r.Title).HasMaxLength(255);
            });

            modelBuilder.Entity<RateTypeUnitItem>(entity =>
            {
                // Remove Foreign Key constraint - data comes from Zaaer system without enforced referential integrity
                // NOTE: Relationship configuration is commented out (similar to Building relationship)
                // This allows rate_type_id values that don't exist in rate_types table
                // Navigation property in Entity model is ignored to prevent FK constraint creation
                // Manual joins can be used in Service if needed
                
                // Keep index for performance but without FK constraint
                entity.HasIndex(i => i.RateTypeId).HasDatabaseName("IX_RateTypeUnitItems_RateTypeId");
                entity.HasIndex(i => new { i.RateTypeId, i.UnitTypeName })
                    .IsUnique()
                    .HasDatabaseName("UQ_RateTypeUnitItems_RateTypeId_UnitTypeName");
                entity.Property(i => i.UnitTypeName).HasMaxLength(100);
                entity.Property(i => i.Rate).HasColumnType("decimal(18,2)");
            });
        }
    }
}
