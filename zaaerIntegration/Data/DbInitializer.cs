using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace zaaerIntegration.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if we already have data
            if (await context.Customers.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Create sample data
            var sampleHotel = new HotelSettings
            {
                HotelCode = "HOTEL001",
                HotelName = "Sample Hotel",
                Address = "123 Main Street, City",
                CreatedAt = KsaTime.Now
            };

            var sampleGuestType = new GuestType
            {
                GtypeName = "Individual",
                GtypeNameAr = "فردي",
                GtypeActive = true,
                CreatedAt = KsaTime.Now
            };

            var sampleNationality = new Nationality
            {
                NName = "Saudi",
                NNameAr = "سعودي",
                IsActive = true,
                CreatedAt = KsaTime.Now
            };

            var sampleIdType = new IdType
            {
                ItName = "National ID",
                ItNameAr = "هوية وطنية",
                ItActive = true,
                CreatedAt = KsaTime.Now
            };

            var sampleGuestCategory = new GuestCategory
            {
                GcName = "Regular",
                GcNameAr = "عادي",
                GcActive = true,
                CreatedAt = KsaTime.Now
            };

            // Add to context
            context.HotelSettings.Add(sampleHotel);
            context.GuestTypes.Add(sampleGuestType);
            context.Nationalities.Add(sampleNationality);
            context.IdTypes.Add(sampleIdType);
            context.GuestCategories.Add(sampleGuestCategory);

            await context.SaveChangesAsync();

            // Create sample customers
            var sampleCustomers = new List<Customer>
            {
                new Customer
                {
                    CustomerName = "Ahmed Al-Rashid",
                    CustomerNo = "CUST001",
                    HotelId = sampleHotel.HotelId,
                    GtypeId = sampleGuestType.GtypeId,
                    NId = sampleNationality.NId,
                    GuestCategoryId = sampleGuestCategory.GcId,
                    MobileNo = "+966501234567",
                    Email = "ahmed@example.com",
                    Gender = "Male",
                    EnteredAt = KsaTime.Now,
                    CreatedAt = KsaTime.Now,
                    IsActive = true
                },
                new Customer
                {
                    CustomerName = "Fatima Al-Zahra",
                    CustomerNo = "CUST002",
                    HotelId = sampleHotel.HotelId,
                    GtypeId = sampleGuestType.GtypeId,
                    NId = sampleNationality.NId,
                    GuestCategoryId = sampleGuestCategory.GcId,
                    MobileNo = "+966507654321",
                    Email = "fatima@example.com",
                    Gender = "Female",
                    EnteredAt = KsaTime.Now,
                    CreatedAt = KsaTime.Now,
                    IsActive = true
                }
            };

            context.Customers.AddRange(sampleCustomers);
            await context.SaveChangesAsync();
        }
    }
}
