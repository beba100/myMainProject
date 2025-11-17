using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for ReservationUnit operations
    /// </summary>
    public class ReservationUnitRepository : GenericRepository<ReservationUnit>, IReservationUnitRepository
    {
        public ReservationUnitRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<ReservationUnit> ReservationUnits, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<ReservationUnit, bool>>? filter = null)
        {
            var query = _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var reservationUnits = await query
                .OrderByDescending(ru => ru.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reservationUnits, totalCount);
        }

        public async Task<IEnumerable<ReservationUnit>> GetByReservationIdAsync(int reservationId)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.ReservationId == reservationId)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByApartmentIdAsync(int apartmentId)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.ApartmentId == apartmentId)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByStatusAsync(string status)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Status == status)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByCheckInDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.CheckInDate >= startDate && ru.CheckInDate <= endDate)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByCheckOutDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.CheckOutDate >= startDate && ru.CheckOutDate <= endDate)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => (ru.CheckInDate <= endDate && ru.CheckOutDate >= startDate))
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByRentAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.RentAmount >= minAmount && ru.RentAmount <= maxAmount)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByTotalAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.TotalAmount >= minAmount && ru.TotalAmount <= maxAmount)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByNumberOfNightsRangeAsync(int minNights, int maxNights)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.NumberOfNights >= minNights && ru.NumberOfNights <= maxNights)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByVatRateRangeAsync(decimal minRate, decimal maxRate)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.VatRate >= minRate && ru.VatRate <= maxRate)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByLodgingTaxRateRangeAsync(decimal minRate, decimal maxRate)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.LodgingTaxRate >= minRate && ru.LodgingTaxRate <= maxRate)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<ReservationUnit?> GetWithDetailsAsync(int id)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .FirstOrDefaultAsync(ru => ru.UnitId == id);
        }

        public async Task<IEnumerable<ReservationUnit>> GetWithDetailsByReservationIdAsync(int reservationId)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.ReservationId == reservationId)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetWithDetailsByApartmentIdAsync(int apartmentId)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.ApartmentId == apartmentId)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<object> GetStatisticsAsync()
        {
            var totalReservationUnits = await _context.ReservationUnits.CountAsync();
            var totalRevenue = await _context.ReservationUnits.SumAsync(ru => ru.TotalAmount);
            var averageRentAmount = await _context.ReservationUnits.AverageAsync(ru => ru.RentAmount);
            var averageTotalAmount = await _context.ReservationUnits.AverageAsync(ru => ru.TotalAmount);
            var averageNumberOfNights = await _context.ReservationUnits.AverageAsync(ru => ru.NumberOfNights ?? 0);

            var statusBreakdown = await _context.ReservationUnits
                .GroupBy(ru => ru.Status)
                .Select(g => new { Status = g.Key, Count = g.Count(), Revenue = g.Sum(ru => ru.TotalAmount) })
                .ToListAsync();

            var monthlyRevenue = await _context.ReservationUnits
                .GroupBy(ru => new { Year = ru.CreatedAt.Year, Month = ru.CreatedAt.Month })
                .Select(g => new { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Count = g.Count(), 
                    Revenue = g.Sum(ru => ru.TotalAmount) 
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(12)
                .ToListAsync();

            var topApartments = await _context.ReservationUnits
                .GroupBy(ru => ru.ApartmentId)
                .Select(g => new { 
                    ApartmentId = g.Key, 
                    Count = g.Count(), 
                    Revenue = g.Sum(ru => ru.TotalAmount),
                    ApartmentName = g.First().Apartment.ApartmentName
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            var topHotels = await _context.ReservationUnits
                .GroupBy(ru => ru.Reservation.HotelId)
                .Select(g => new { 
                    HotelId = g.Key, 
                    Count = g.Count(), 
                    Revenue = g.Sum(ru => ru.TotalAmount),
                    HotelName = g.First().Reservation.HotelSettings.HotelName
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            var vatRateBreakdown = await _context.ReservationUnits
                .GroupBy(ru => ru.VatRate)
                .Select(g => new { VatRate = g.Key, Count = g.Count(), Revenue = g.Sum(ru => ru.TotalAmount) })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var lodgingTaxRateBreakdown = await _context.ReservationUnits
                .GroupBy(ru => ru.LodgingTaxRate)
                .Select(g => new { LodgingTaxRate = g.Key, Count = g.Count(), Revenue = g.Sum(ru => ru.TotalAmount) })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return new
            {
                TotalReservationUnits = totalReservationUnits,
                TotalRevenue = totalRevenue,
                AverageRentAmount = averageRentAmount,
                AverageTotalAmount = averageTotalAmount,
                AverageNumberOfNights = averageNumberOfNights,
                StatusBreakdown = statusBreakdown,
                MonthlyRevenue = monthlyRevenue,
                TopApartments = topApartments,
                TopHotels = topHotels,
                VatRateBreakdown = vatRateBreakdown,
                LodgingTaxRateBreakdown = lodgingTaxRateBreakdown
            };
        }

        public async Task<IEnumerable<ReservationUnit>> GetByHotelIdAsync(int hotelId)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Reservation.HotelId == hotelId)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByBuildingIdAsync(int buildingId)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Apartment.BuildingId == buildingId)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByFloorIdAsync(int floorId)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Apartment.FloorId == floorId)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByRoomTypeIdAsync(int roomTypeId)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Apartment.RoomTypeId == roomTypeId)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Reservation.CustomerId == customerId)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByCorporateCustomerIdAsync(int corporateCustomerId)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Reservation.CorporateId == corporateCustomerId)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByCreatedDateAsync(DateTime createdDate)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.CreatedAt.Date == createdDate.Date)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByCreatedDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.CreatedAt >= startDate && ru.CreatedAt <= endDate)
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetActiveAsync()
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Status == "reserved" || ru.Status == "confirmed")
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetCancelledAsync()
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Status == "cancelled")
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetCompletedAsync()
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Status == "completed")
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByApartmentNameAsync(string apartmentName)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Apartment.ApartmentName.Contains(apartmentName))
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByBuildingNameAsync(string buildingName)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Apartment.Building.BuildingName.Contains(buildingName))
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByRoomTypeNameAsync(string roomTypeName)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Apartment.RoomType.RoomTypeName.Contains(roomTypeName))
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByCustomerNameAsync(string customerName)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Reservation.CustomerId.ToString().Contains(customerName))
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByCorporateCustomerNameAsync(string corporateCustomerName)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Reservation.CorporateCustomer.CorporateName.Contains(corporateCustomerName))
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetByHotelNameAsync(string hotelName)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Include(ru => ru.Invoices)
                .Include(ru => ru.PaymentReceipts)
                .Include(ru => ru.Refunds)
                .Where(ru => ru.Reservation.HotelSettings.HotelName.Contains(hotelName))
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueByReservationIdAsync(int reservationId)
        {
            return await _context.ReservationUnits
                .Where(ru => ru.ReservationId == reservationId)
                .SumAsync(ru => ru.TotalAmount);
        }

        public async Task<decimal> GetTotalRevenueByApartmentIdAsync(int apartmentId)
        {
            return await _context.ReservationUnits
                .Where(ru => ru.ApartmentId == apartmentId)
                .SumAsync(ru => ru.TotalAmount);
        }

        public async Task<decimal> GetTotalRevenueByHotelIdAsync(int hotelId)
        {
            return await _context.ReservationUnits
                .Where(ru => ru.Reservation.HotelId == hotelId)
                .SumAsync(ru => ru.TotalAmount);
        }

        public async Task<decimal> GetTotalRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ReservationUnits
                .Where(ru => ru.CreatedAt >= startDate && ru.CreatedAt <= endDate)
                .SumAsync(ru => ru.TotalAmount);
        }

        public async Task<decimal> GetAverageRentAmountByApartmentIdAsync(int apartmentId)
        {
            return await _context.ReservationUnits
                .Where(ru => ru.ApartmentId == apartmentId)
                .AverageAsync(ru => ru.RentAmount);
        }

        public async Task<decimal> GetAverageTotalAmountByApartmentIdAsync(int apartmentId)
        {
            return await _context.ReservationUnits
                .Where(ru => ru.ApartmentId == apartmentId)
                .AverageAsync(ru => ru.TotalAmount);
        }

        public async Task<IEnumerable<object>> GetTopApartmentsByRevenueAsync(int topCount = 10)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Apartment)
                .GroupBy(ru => ru.ApartmentId)
                .Select(g => new { 
                    ApartmentId = g.Key, 
                    Revenue = g.Sum(ru => ru.TotalAmount),
                    Count = g.Count(),
                    ApartmentName = g.First().Apartment.ApartmentName
                })
                .OrderByDescending(x => x.Revenue)
                .Take(topCount)
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetTopHotelsByRevenueAsync(int topCount = 10)
        {
            return await _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .ThenInclude(r => r.HotelSettings)
                .GroupBy(ru => ru.Reservation.HotelId)
                .Select(g => new { 
                    HotelId = g.Key, 
                    Revenue = g.Sum(ru => ru.TotalAmount),
                    Count = g.Count(),
                    HotelName = g.First().Reservation.HotelSettings.HotelName
                })
                .OrderByDescending(x => x.Revenue)
                .Take(topCount)
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationUnit>> GetOverlappingDatesAsync(int apartmentId, DateTime checkInDate, DateTime checkOutDate, int? excludeUnitId = null)
        {
            var query = _context.ReservationUnits
                .Include(ru => ru.Reservation)
                .Include(ru => ru.Apartment)
                .Where(ru => ru.ApartmentId == apartmentId && 
                           ru.Status != "cancelled" &&
                           (ru.CheckInDate < checkOutDate && ru.CheckOutDate > checkInDate));

            if (excludeUnitId.HasValue)
            {
                query = query.Where(ru => ru.UnitId != excludeUnitId.Value);
            }

            return await query
                .OrderByDescending(ru => ru.CreatedAt)
                .ToListAsync();
        }
    }
}
