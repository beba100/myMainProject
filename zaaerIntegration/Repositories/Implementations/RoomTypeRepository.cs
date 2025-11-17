using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for RoomType operations
    /// </summary>
    public class RoomTypeRepository : GenericRepository<RoomType>, IRoomTypeRepository
    {
        public RoomTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<RoomType> RoomTypes, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<RoomType, bool>>? filter = null)
        {
            var query = _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var roomTypes = await query
                .OrderBy(rt => rt.RoomTypeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (roomTypes, totalCount);
        }

        public async Task<IEnumerable<RoomType>> GetByHotelIdAsync(int hotelId)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.HotelId == hotelId)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<RoomType?> GetByNameAsync(int hotelId, string roomTypeName)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .FirstOrDefaultAsync(rt => rt.HotelId == hotelId && rt.RoomTypeName == roomTypeName);
        }

        public async Task<IEnumerable<RoomType>> GetByNameAsync(string roomTypeName)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.RoomTypeName.Contains(roomTypeName))
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<RoomType?> GetWithDetailsAsync(int id)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);
        }

        public async Task<IEnumerable<RoomType>> GetWithDetailsByHotelIdAsync(int hotelId)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.HotelId == hotelId)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<object> GetStatisticsAsync()
        {
            var totalRoomTypes = await _context.RoomTypes.CountAsync();
            var roomTypesWithApartments = await _context.RoomTypes.CountAsync(rt => rt.Apartments.Any());
            var roomTypesWithoutApartments = totalRoomTypes - roomTypesWithApartments;

            var hotelBreakdown = await _context.RoomTypes
                .GroupBy(rt => rt.HotelId)
                .Select(g => new { 
                    HotelId = g.Key, 
                    Count = g.Count(),
                    HotelName = g.First().HotelSettings.HotelName
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var apartmentBreakdown = await _context.RoomTypes
                .GroupBy(rt => rt.RoomTypeId)
                .Select(g => new { 
                    RoomTypeId = g.Key, 
                    ApartmentCount = g.First().Apartments.Count,
                    RoomTypeName = g.First().RoomTypeName,
                    HotelName = g.First().HotelSettings.HotelName
                })
                .OrderByDescending(x => x.ApartmentCount)
                .Take(10)
                .ToListAsync();

            var revenueBreakdown = await _context.RoomTypes
                .GroupBy(rt => rt.RoomTypeId)
                .Select(g => new { 
                    RoomTypeId = g.Key, 
                    Revenue = g.First().Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)),
                    ApartmentCount = g.First().Apartments.Count,
                    RoomTypeName = g.First().RoomTypeName,
                    HotelName = g.First().HotelSettings.HotelName
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            var baseRateBreakdown = await _context.RoomTypes
                .GroupBy(rt => rt.RoomTypeId)
                .Select(g => new { 
                    RoomTypeId = g.Key, 
                    BaseRate = g.First().BaseRate,
                    RoomTypeName = g.First().RoomTypeName,
                    HotelName = g.First().HotelSettings.HotelName
                })
                .OrderByDescending(x => x.BaseRate)
                .Take(10)
                .ToListAsync();

            return new
            {
                TotalRoomTypes = totalRoomTypes,
                RoomTypesWithApartments = roomTypesWithApartments,
                RoomTypesWithoutApartments = roomTypesWithoutApartments,
                HotelBreakdown = hotelBreakdown,
                ApartmentBreakdown = apartmentBreakdown,
                RevenueBreakdown = revenueBreakdown,
                BaseRateBreakdown = baseRateBreakdown
            };
        }

        public async Task<IEnumerable<RoomType>> SearchByNameAsync(string name)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.RoomTypeName.Contains(name))
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> SearchByDescriptionAsync(string description)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.RoomTypeDesc.Contains(description))
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByHotelNameAsync(string hotelName)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.HotelSettings.HotelName.Contains(hotelName))
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<bool> RoomTypeNameExistsAsync(int hotelId, string roomTypeName, int? excludeId = null)
        {
            var query = _context.RoomTypes.Where(rt => rt.HotelId == hotelId && rt.RoomTypeName == roomTypeName);
            
            if (excludeId.HasValue)
            {
                query = query.Where(rt => rt.RoomTypeId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<RoomType>> GetWithApartmentsAsync()
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.Apartments.Any())
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetWithoutApartmentsAsync()
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => !rt.Apartments.Any())
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByApartmentCountRangeAsync(int minCount, int maxCount)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.Apartments.Count >= minCount && rt.Apartments.Count <= maxCount)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetTopByApartmentCountAsync(int topCount = 10)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .OrderByDescending(rt => rt.Apartments.Count)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByBaseRateRangeAsync(decimal minRate, decimal maxRate)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.BaseRate >= minRate && rt.BaseRate <= maxRate)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetTopByBaseRateAsync(int topCount = 10)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .OrderByDescending(rt => rt.BaseRate)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) >= minRevenue && 
                           rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) <= maxRevenue)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetTopByRevenueAsync(int topCount = 10)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .OrderByDescending(rt => rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)))
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByReservationCountRangeAsync(int minCount, int maxCount)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.Apartments.Sum(a => a.ReservationUnits.Count) >= minCount && 
                           rt.Apartments.Sum(a => a.ReservationUnits.Count) <= maxCount)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetTopByReservationCountAsync(int topCount = 10)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .OrderByDescending(rt => rt.Apartments.Sum(a => a.ReservationUnits.Count))
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<decimal> GetOccupancyRateAsync(int roomTypeId, DateTime startDate, DateTime endDate)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);

            if (roomType == null)
                return 0;

            var totalDays = (endDate - startDate).Days;
            var totalApartmentDays = roomType.Apartments.Count * totalDays;
            
            if (totalApartmentDays == 0)
                return 0;

            var occupiedDays = roomType.Apartments
                .SelectMany(a => a.ReservationUnits)
                .Where(ru => ru.Status != "cancelled" && 
                           ru.CheckInDate < endDate && 
                           ru.CheckOutDate > startDate)
                .Sum(ru => (ru.CheckOutDate < endDate ? ru.CheckOutDate : endDate).Subtract(ru.CheckInDate > startDate ? ru.CheckInDate : startDate).Days);

            return (decimal)occupiedDays / totalApartmentDays * 100;
        }

        public async Task<decimal> GetRevenueAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.RoomTypeId == roomTypeId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            return await query.SumAsync(ru => ru.TotalAmount);
        }

        public async Task<int> GetReservationCountAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.RoomTypeId == roomTypeId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<decimal> GetAverageStayDurationAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.RoomTypeId == roomTypeId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            var reservations = await query.ToListAsync();
            if (!reservations.Any())
                return 0;

            return reservations.Average(ru => (decimal)(ru.CheckOutDate - ru.CheckInDate).Days);
        }

        public async Task<object> GetUtilizationStatisticsAsync(int roomTypeId, DateTime startDate, DateTime endDate)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);

            if (roomType == null)
                return new { Error = "Room type not found" };

            var totalDays = (endDate - startDate).Days;
            var totalApartmentDays = roomType.Apartments.Count * totalDays;
            
            var reservations = roomType.Apartments
                .SelectMany(a => a.ReservationUnits)
                .Where(ru => ru.Status != "cancelled" && 
                           ru.CheckInDate < endDate && 
                           ru.CheckOutDate > startDate)
                .ToList();

            var occupiedDays = reservations
                .Sum(ru => (ru.CheckOutDate < endDate ? ru.CheckOutDate : endDate).Subtract(ru.CheckInDate > startDate ? ru.CheckInDate : startDate).Days);

            var occupancyRate = totalApartmentDays > 0 ? (decimal)occupiedDays / totalApartmentDays * 100 : 0;
            var totalRevenue = reservations.Sum(ru => ru.TotalAmount);
            var averageStayDuration = reservations.Any() ? reservations.Average(ru => (decimal)(ru.CheckOutDate - ru.CheckInDate).Days) : 0;

            return new
            {
                RoomTypeId = roomTypeId,
                RoomTypeName = roomType.RoomTypeName,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = totalDays,
                TotalApartmentDays = totalApartmentDays,
                OccupiedDays = occupiedDays,
                OccupancyRate = occupancyRate,
                TotalRevenue = totalRevenue,
                AverageStayDuration = averageStayDuration,
                ReservationCount = reservations.Count,
                ApartmentCount = roomType.Apartments.Count
            };
        }

        public async Task<IEnumerable<RoomType>> GetByMultipleCriteriaAsync(int? hotelId = null, string? roomTypeName = null, decimal? minBaseRate = null, decimal? maxBaseRate = null)
        {
            var query = _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .AsQueryable();

            if (hotelId.HasValue)
                query = query.Where(rt => rt.HotelId == hotelId.Value);

            if (!string.IsNullOrEmpty(roomTypeName))
                query = query.Where(rt => rt.RoomTypeName.Contains(roomTypeName));

            if (minBaseRate.HasValue)
                query = query.Where(rt => rt.BaseRate >= minBaseRate.Value);

            if (maxBaseRate.HasValue)
                query = query.Where(rt => rt.BaseRate <= maxBaseRate.Value);

            return await query
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<object> GetApartmentStatisticsAsync(int roomTypeId)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);

            if (roomType == null)
                return new { Error = "Room type not found" };

            var totalApartments = roomType.Apartments.Count;
            var apartmentsWithReservations = roomType.Apartments.Count(a => a.ReservationUnits.Any());
            var apartmentsWithoutReservations = totalApartments - apartmentsWithReservations;

            return new
            {
                RoomTypeId = roomTypeId,
                RoomTypeName = roomType.RoomTypeName,
                TotalApartments = totalApartments,
                ApartmentsWithReservations = apartmentsWithReservations,
                ApartmentsWithoutReservations = apartmentsWithoutReservations
            };
        }

        public async Task<object> GetReservationStatisticsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.RoomTypeId == roomTypeId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            var reservations = await query.ToListAsync();
            var totalReservations = reservations.Count;
            var confirmedReservations = reservations.Count(r => r.Status == "confirmed");
            var cancelledReservations = reservations.Count(r => r.Status == "cancelled");
            var completedReservations = reservations.Count(r => r.Status == "completed");

            return new
            {
                RoomTypeId = roomTypeId,
                StartDate = startDate,
                EndDate = endDate,
                TotalReservations = totalReservations,
                ConfirmedReservations = confirmedReservations,
                CancelledReservations = cancelledReservations,
                CompletedReservations = completedReservations
            };
        }

        public async Task<object> GetRevenueStatisticsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.RoomTypeId == roomTypeId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            var reservations = await query.ToListAsync();
            var totalRevenue = reservations.Sum(r => r.TotalAmount);
            var averageRevenue = reservations.Any() ? reservations.Average(r => r.TotalAmount) : 0;
            var maxRevenue = reservations.Any() ? reservations.Max(r => r.TotalAmount) : 0;
            var minRevenue = reservations.Any() ? reservations.Min(r => r.TotalAmount) : 0;

            return new
            {
                RoomTypeId = roomTypeId,
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                AverageRevenue = averageRevenue,
                MaxRevenue = maxRevenue,
                MinRevenue = minRevenue
            };
        }

        public async Task<object> GetOccupancyStatisticsAsync(int roomTypeId, DateTime startDate, DateTime endDate)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);

            if (roomType == null)
                return new { Error = "Room type not found" };

            var totalDays = (endDate - startDate).Days;
            var totalApartmentDays = roomType.Apartments.Count * totalDays;
            
            var reservations = roomType.Apartments
                .SelectMany(a => a.ReservationUnits)
                .Where(ru => ru.Status != "cancelled" && 
                           ru.CheckInDate < endDate && 
                           ru.CheckOutDate > startDate)
                .ToList();

            var occupiedDays = reservations
                .Sum(ru => (ru.CheckOutDate < endDate ? ru.CheckOutDate : endDate).Subtract(ru.CheckInDate > startDate ? ru.CheckInDate : startDate).Days);

            var occupancyRate = totalApartmentDays > 0 ? (decimal)occupiedDays / totalApartmentDays * 100 : 0;

            return new
            {
                RoomTypeId = roomTypeId,
                RoomTypeName = roomType.RoomTypeName,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = totalDays,
                TotalApartmentDays = totalApartmentDays,
                OccupiedDays = occupiedDays,
                OccupancyRate = occupancyRate,
                ApartmentCount = roomType.Apartments.Count
            };
        }

        public async Task<object> GetPerformanceMetricsAsync(int roomTypeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);

            if (roomType == null)
                return new { Error = "Room type not found" };

            var query = roomType.Apartments
                .SelectMany(a => a.ReservationUnits)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            var reservations = query.ToList();
            var totalRevenue = reservations.Sum(r => r.TotalAmount);
            var averageStayDuration = reservations.Any() ? reservations.Average(r => (decimal)(r.CheckOutDate - r.CheckInDate).Days) : 0;
            var totalReservations = reservations.Count;

            return new
            {
                RoomTypeId = roomTypeId,
                RoomTypeName = roomType.RoomTypeName,
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                AverageStayDuration = averageStayDuration,
                TotalReservations = totalReservations,
                ApartmentCount = roomType.Apartments.Count,
                BaseRate = roomType.BaseRate
            };
        }

        public async Task<IEnumerable<RoomType>> GetByHotelAndBaseRateRangeAsync(int hotelId, decimal minRate, decimal maxRate)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.HotelId == hotelId && rt.BaseRate >= minRate && rt.BaseRate <= maxRate)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByHotelAndApartmentCountRangeAsync(int hotelId, int minCount, int maxCount)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.HotelId == hotelId && rt.Apartments.Count >= minCount && rt.Apartments.Count <= maxCount)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByHotelAndRevenueRangeAsync(int hotelId, decimal minRevenue, decimal maxRevenue)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.HotelId == hotelId && 
                           rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) >= minRevenue && 
                           rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) <= maxRevenue)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByHotelAndReservationCountRangeAsync(int hotelId, int minCount, int maxCount)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.HotelId == hotelId && 
                           rt.Apartments.Sum(a => a.ReservationUnits.Count) >= minCount && 
                           rt.Apartments.Sum(a => a.ReservationUnits.Count) <= maxCount)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<object> GetHotelRoomTypeStatisticsAsync(int hotelId)
        {
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .Where(rt => rt.HotelId == hotelId)
                .ToListAsync();

            var totalRoomTypes = roomTypes.Count;
            var roomTypesWithApartments = roomTypes.Count(rt => rt.Apartments.Any());
            var roomTypesWithoutApartments = totalRoomTypes - roomTypesWithApartments;
            var totalApartments = roomTypes.Sum(rt => rt.Apartments.Count);
            var totalReservations = roomTypes.Sum(rt => rt.Apartments.Sum(a => a.ReservationUnits.Count));
            var totalRevenue = roomTypes.Sum(rt => rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)));

            return new
            {
                HotelId = hotelId,
                TotalRoomTypes = totalRoomTypes,
                RoomTypesWithApartments = roomTypesWithApartments,
                RoomTypesWithoutApartments = roomTypesWithoutApartments,
                TotalApartments = totalApartments,
                TotalReservations = totalReservations,
                TotalRevenue = totalRevenue
            };
        }

        public async Task<object> GetHotelRoomTypeOccupancyStatisticsAsync(int hotelId, DateTime startDate, DateTime endDate)
        {
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .Where(rt => rt.HotelId == hotelId)
                .ToListAsync();

            var totalDays = (endDate - startDate).Days;
            var totalApartmentDays = roomTypes.Sum(rt => rt.Apartments.Count) * totalDays;
            
            var reservations = roomTypes
                .SelectMany(rt => rt.Apartments)
                .SelectMany(a => a.ReservationUnits)
                .Where(ru => ru.Status != "cancelled" && 
                           ru.CheckInDate < endDate && 
                           ru.CheckOutDate > startDate)
                .ToList();

            var occupiedDays = reservations
                .Sum(ru => (ru.CheckOutDate < endDate ? ru.CheckOutDate : endDate).Subtract(ru.CheckInDate > startDate ? ru.CheckInDate : startDate).Days);

            var occupancyRate = totalApartmentDays > 0 ? (decimal)occupiedDays / totalApartmentDays * 100 : 0;

            return new
            {
                HotelId = hotelId,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = totalDays,
                TotalApartmentDays = totalApartmentDays,
                OccupiedDays = occupiedDays,
                OccupancyRate = occupancyRate,
                RoomTypeCount = roomTypes.Count,
                ApartmentCount = roomTypes.Sum(rt => rt.Apartments.Count)
            };
        }

        public async Task<object> GetHotelRoomTypeRevenueStatisticsAsync(int hotelId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.RoomType.HotelId == hotelId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            var reservations = await query.ToListAsync();
            var totalRevenue = reservations.Sum(r => r.TotalAmount);
            var averageRevenue = reservations.Any() ? reservations.Average(r => r.TotalAmount) : 0;
            var maxRevenue = reservations.Any() ? reservations.Max(r => r.TotalAmount) : 0;
            var minRevenue = reservations.Any() ? reservations.Min(r => r.TotalAmount) : 0;

            return new
            {
                HotelId = hotelId,
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                AverageRevenue = averageRevenue,
                MaxRevenue = maxRevenue,
                MinRevenue = minRevenue,
                ReservationCount = reservations.Count
            };
        }

        public async Task<object> GetHotelRoomTypePerformanceMetricsAsync(int hotelId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.RoomType.HotelId == hotelId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            var reservations = await query.ToListAsync();
            var totalRevenue = reservations.Sum(r => r.TotalAmount);
            var averageStayDuration = reservations.Any() ? reservations.Average(r => (decimal)(r.CheckOutDate - r.CheckInDate).Days) : 0;
            var totalReservations = reservations.Count;

            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .Where(rt => rt.HotelId == hotelId)
                .ToListAsync();

            return new
            {
                HotelId = hotelId,
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                AverageStayDuration = averageStayDuration,
                TotalReservations = totalReservations,
                RoomTypeCount = roomTypes.Count,
                ApartmentCount = roomTypes.Sum(rt => rt.Apartments.Count)
            };
        }

        public async Task<IEnumerable<RoomType>> GetByAverageRevenueRangeAsync(decimal minRevenue, decimal maxRevenue)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) / Math.Max(rt.Apartments.Count, 1) >= minRevenue && 
                           rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) / Math.Max(rt.Apartments.Count, 1) <= maxRevenue)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetTopByAverageRevenueAsync(int topCount = 10)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .OrderByDescending(rt => rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) / Math.Max(rt.Apartments.Count, 1))
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByOccupancyRateRangeAsync(decimal minRate, decimal maxRate, DateTime startDate, DateTime endDate)
        {
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .ToListAsync();

            var totalDays = (endDate - startDate).Days;
            
            return roomTypes.Where(rt =>
            {
                var totalApartmentDays = rt.Apartments.Count * totalDays;
                if (totalApartmentDays == 0) return false;

                var occupiedDays = rt.Apartments
                    .SelectMany(a => a.ReservationUnits)
                    .Where(ru => ru.Status != "cancelled" && 
                               ru.CheckInDate < endDate && 
                               ru.CheckOutDate > startDate)
                    .Sum(ru => (ru.CheckOutDate < endDate ? ru.CheckOutDate : endDate).Subtract(ru.CheckInDate > startDate ? ru.CheckInDate : startDate).Days);

                var occupancyRate = (decimal)occupiedDays / totalApartmentDays * 100;
                return occupancyRate >= minRate && occupancyRate <= maxRate;
            })
            .OrderBy(rt => rt.RoomTypeName)
            .ToList();
        }

        public async Task<IEnumerable<RoomType>> GetTopByOccupancyRateAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .ToListAsync();

            var totalDays = (endDate - startDate).Days;
            
            return roomTypes
                .Select(rt =>
                {
                    var totalApartmentDays = rt.Apartments.Count * totalDays;
                    var occupiedDays = rt.Apartments
                        .SelectMany(a => a.ReservationUnits)
                        .Where(ru => ru.Status != "cancelled" && 
                                   ru.CheckInDate < endDate && 
                                   ru.CheckOutDate > startDate)
                        .Sum(ru => (ru.CheckOutDate < endDate ? ru.CheckOutDate : endDate).Subtract(ru.CheckInDate > startDate ? ru.CheckInDate : startDate).Days);

                    var occupancyRate = totalApartmentDays > 0 ? (decimal)occupiedDays / totalApartmentDays * 100 : 0;
                    return new { RoomType = rt, OccupancyRate = occupancyRate };
                })
                .OrderByDescending(x => x.OccupancyRate)
                .Take(topCount)
                .Select(x => x.RoomType)
                .ToList();
        }

        public async Task<IEnumerable<RoomType>> GetByAverageStayDurationRangeAsync(decimal minDuration, decimal maxDuration, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            var reservations = await query.ToListAsync();
            var roomTypeGroups = reservations
                .GroupBy(ru => ru.Apartment.RoomTypeId)
                .Where(g => g.Any())
                .Select(g => new
                {
                    RoomTypeId = g.Key,
                    AverageDuration = g.Average(ru => (decimal)(ru.CheckOutDate - ru.CheckInDate).Days)
                })
                .Where(x => x.AverageDuration >= minDuration && x.AverageDuration <= maxDuration)
                .ToList();

            var roomTypeIds = roomTypeGroups.Select(x => x.RoomTypeId).ToList();
            
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => roomTypeIds.Contains(rt.RoomTypeId))
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetTopByAverageStayDurationAsync(int topCount = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            var reservations = await query.ToListAsync();
            var roomTypeGroups = reservations
                .GroupBy(ru => ru.Apartment.RoomTypeId)
                .Where(g => g.Any())
                .Select(g => new
                {
                    RoomTypeId = g.Key,
                    AverageDuration = g.Average(ru => (decimal)(ru.CheckOutDate - ru.CheckInDate).Days)
                })
                .OrderByDescending(x => x.AverageDuration)
                .Take(topCount)
                .ToList();

            var roomTypeIds = roomTypeGroups.Select(x => x.RoomTypeId).ToList();
            
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => roomTypeIds.Contains(rt.RoomTypeId))
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByProfitabilityAsync(decimal minProfitability)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.BaseRate > 0 && 
                           rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) / rt.BaseRate >= minProfitability)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetTopByProfitabilityAsync(int topCount = 10)
        {
            return await _context.RoomTypes
                .Include(rt => rt.HotelSettings)
                .Include(rt => rt.Apartments)
                .Where(rt => rt.BaseRate > 0)
                .OrderByDescending(rt => rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) / rt.BaseRate)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomType>> GetByUtilizationEfficiencyAsync(decimal minEfficiency, DateTime startDate, DateTime endDate)
        {
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .ToListAsync();

            var totalDays = (endDate - startDate).Days;
            
            return roomTypes.Where(rt =>
            {
                var totalApartmentDays = rt.Apartments.Count * totalDays;
                if (totalApartmentDays == 0) return false;

                var occupiedDays = rt.Apartments
                    .SelectMany(a => a.ReservationUnits)
                    .Where(ru => ru.Status != "cancelled" && 
                               ru.CheckInDate < endDate && 
                               ru.CheckOutDate > startDate)
                    .Sum(ru => (ru.CheckOutDate < endDate ? ru.CheckOutDate : endDate).Subtract(ru.CheckInDate > startDate ? ru.CheckInDate : startDate).Days);

                var occupancyRate = (decimal)occupiedDays / totalApartmentDays * 100;
                var revenue = rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount));
                var efficiency = occupancyRate * revenue / Math.Max(rt.BaseRate ?? 1, 1);
                
                return efficiency >= minEfficiency;
            })
            .OrderBy(rt => rt.RoomTypeName)
            .ToList();
        }

        public async Task<IEnumerable<RoomType>> GetTopByUtilizationEfficiencyAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .ToListAsync();

            var totalDays = (endDate - startDate).Days;
            
            return roomTypes
                .Select(rt =>
                {
                    var totalApartmentDays = rt.Apartments.Count * totalDays;
                    var occupiedDays = rt.Apartments
                        .SelectMany(a => a.ReservationUnits)
                        .Where(ru => ru.Status != "cancelled" && 
                                   ru.CheckInDate < endDate && 
                                   ru.CheckOutDate > startDate)
                        .Sum(ru => (ru.CheckOutDate < endDate ? ru.CheckOutDate : endDate).Subtract(ru.CheckInDate > startDate ? ru.CheckInDate : startDate).Days);

                    var occupancyRate = totalApartmentDays > 0 ? (decimal)occupiedDays / totalApartmentDays * 100 : 0;
                    var revenue = rt.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount));
                    var efficiency = occupancyRate * revenue / Math.Max(rt.BaseRate ?? 1, 1);
                    
                    return new { RoomType = rt, Efficiency = efficiency };
                })
                .OrderByDescending(x => x.Efficiency)
                .Take(topCount)
                .Select(x => x.RoomType)
                .ToList();
        }
    }
}
