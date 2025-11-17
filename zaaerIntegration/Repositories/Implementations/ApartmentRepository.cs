using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Apartment operations
    /// </summary>
    public class ApartmentRepository : GenericRepository<Apartment>, IApartmentRepository
    {
        public ApartmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Apartment> Apartments, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<Apartment, bool>>? filter = null)
        {
            var query = _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var apartments = await query
                .OrderBy(a => a.ApartmentCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (apartments, totalCount);
        }

        public async Task<IEnumerable<Apartment>> GetByHotelIdAsync(int hotelId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.HotelId == hotelId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByBuildingIdAsync(int buildingId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.BuildingId == buildingId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByFloorIdAsync(int floorId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.FloorId == floorId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByRoomTypeIdAsync(int roomTypeId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.RoomTypeId == roomTypeId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByStatusAsync(string status)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.Status == status)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<Apartment?> GetByApartmentCodeAsync(string apartmentCode)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .FirstOrDefaultAsync(a => a.ApartmentCode == apartmentCode);
        }

        public async Task<IEnumerable<Apartment>> GetByApartmentNameAsync(string apartmentName)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.ApartmentName.Contains(apartmentName))
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetAvailableAsync()
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.Status == "available")
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetOccupiedAsync()
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.Status == "occupied")
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetMaintenanceAsync()
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.Status == "maintenance")
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<Apartment?> GetWithDetailsAsync(int id)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .FirstOrDefaultAsync(a => a.ApartmentId == id);
        }

        public async Task<IEnumerable<Apartment>> GetWithDetailsByHotelIdAsync(int hotelId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.HotelId == hotelId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetWithDetailsByBuildingIdAsync(int buildingId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.BuildingId == buildingId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetWithDetailsByFloorIdAsync(int floorId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.FloorId == floorId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetWithDetailsByRoomTypeIdAsync(int roomTypeId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.RoomTypeId == roomTypeId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<object> GetStatisticsAsync()
        {
            var totalApartments = await _context.Apartments.CountAsync();
            var availableApartments = await _context.Apartments.CountAsync(a => a.Status == "available");
            var occupiedApartments = await _context.Apartments.CountAsync(a => a.Status == "occupied");
            var maintenanceApartments = await _context.Apartments.CountAsync(a => a.Status == "maintenance");

            var statusBreakdown = await _context.Apartments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var hotelBreakdown = await _context.Apartments
                .GroupBy(a => a.HotelId)
                .Select(g => new { 
                    HotelId = g.Key, 
                    Count = g.Count(),
                    HotelName = g.First().HotelSettings.HotelName
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var buildingBreakdown = await _context.Apartments
                .GroupBy(a => a.BuildingId)
                .Select(g => new { 
                    BuildingId = g.Key, 
                    Count = g.Count(),
                    BuildingName = g.First().Building.BuildingName
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var floorBreakdown = await _context.Apartments
                .GroupBy(a => a.FloorId)
                .Select(g => new { 
                    FloorId = g.Key, 
                    Count = g.Count(),
                    FloorName = g.First().Floor.FloorName
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var roomTypeBreakdown = await _context.Apartments
                .GroupBy(a => a.RoomTypeId)
                .Select(g => new { 
                    RoomTypeId = g.Key, 
                    Count = g.Count(),
                    RoomTypeName = g.First().RoomType.RoomTypeName
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var apartmentsWithReservations = await _context.Apartments
                .Where(a => a.ReservationUnits.Any())
                .CountAsync();

            var apartmentsWithoutReservations = totalApartments - apartmentsWithReservations;

            return new
            {
                TotalApartments = totalApartments,
                AvailableApartments = availableApartments,
                OccupiedApartments = occupiedApartments,
                MaintenanceApartments = maintenanceApartments,
                ApartmentsWithReservations = apartmentsWithReservations,
                ApartmentsWithoutReservations = apartmentsWithoutReservations,
                StatusBreakdown = statusBreakdown,
                HotelBreakdown = hotelBreakdown,
                BuildingBreakdown = buildingBreakdown,
                FloorBreakdown = floorBreakdown,
                RoomTypeBreakdown = roomTypeBreakdown
            };
        }

        public async Task<IEnumerable<Apartment>> SearchByNameAsync(string name)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.ApartmentName.Contains(name))
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> SearchByCodeAsync(string code)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.ApartmentCode.Contains(code))
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByHotelNameAsync(string hotelName)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.HotelSettings.HotelName.Contains(hotelName))
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByBuildingNameAsync(string buildingName)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.Building.BuildingName.Contains(buildingName))
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByFloorNameAsync(string floorName)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.Floor.FloorName.Contains(floorName))
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByRoomTypeNameAsync(string roomTypeName)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.RoomType.RoomTypeName.Contains(roomTypeName))
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<bool> ApartmentCodeExistsAsync(string apartmentCode, int? excludeId = null)
        {
            var query = _context.Apartments.Where(a => a.ApartmentCode == apartmentCode);
            
            if (excludeId.HasValue)
            {
                query = query.Where(a => a.ApartmentId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Apartment>> GetWithReservationsAsync()
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.ReservationUnits.Any())
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetWithoutReservationsAsync()
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => !a.ReservationUnits.Any())
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByReservationCountRangeAsync(int minCount, int maxCount)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.ReservationUnits.Count >= minCount && a.ReservationUnits.Count <= maxCount)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetTopByReservationCountAsync(int topCount = 10)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .OrderByDescending(a => a.ReservationUnits.Count)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.ReservationUnits.Sum(ru => ru.TotalAmount) >= minRevenue && 
                           a.ReservationUnits.Sum(ru => ru.TotalAmount) <= maxRevenue)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetTopByRevenueAsync(int topCount = 10)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .OrderByDescending(a => a.ReservationUnits.Sum(ru => ru.TotalAmount))
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetAvailableForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.Status == "available" && 
                           !a.ReservationUnits.Any(ru => 
                               ru.Status != "cancelled" && 
                               ru.CheckInDate < endDate && 
                               ru.CheckOutDate > startDate))
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetWithOverlappingReservationsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.ReservationUnits.Any(ru => 
                    ru.Status != "cancelled" && 
                    ru.CheckInDate < endDate && 
                    ru.CheckOutDate > startDate))
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByHotelAndBuildingAsync(int hotelId, int buildingId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.HotelId == hotelId && a.BuildingId == buildingId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByHotelAndFloorAsync(int hotelId, int floorId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.HotelId == hotelId && a.FloorId == floorId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByHotelAndRoomTypeAsync(int hotelId, int roomTypeId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.HotelId == hotelId && a.RoomTypeId == roomTypeId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByBuildingAndFloorAsync(int buildingId, int floorId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.BuildingId == buildingId && a.FloorId == floorId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByBuildingAndRoomTypeAsync(int buildingId, int roomTypeId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.BuildingId == buildingId && a.RoomTypeId == roomTypeId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByFloorAndRoomTypeAsync(int floorId, int roomTypeId)
        {
            return await _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .Where(a => a.FloorId == floorId && a.RoomTypeId == roomTypeId)
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByMultipleCriteriaAsync(int? hotelId = null, int? buildingId = null, int? floorId = null, int? roomTypeId = null, string? status = null)
        {
            var query = _context.Apartments
                .Include(a => a.HotelSettings)
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.RoomType)
                .Include(a => a.ReservationUnits)
                .AsQueryable();

            if (hotelId.HasValue)
                query = query.Where(a => a.HotelId == hotelId.Value);

            if (buildingId.HasValue)
                query = query.Where(a => a.BuildingId == buildingId.Value);

            if (floorId.HasValue)
                query = query.Where(a => a.FloorId == floorId.Value);

            if (roomTypeId.HasValue)
                query = query.Where(a => a.RoomTypeId == roomTypeId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            return await query
                .OrderBy(a => a.ApartmentCode)
                .ToListAsync();
        }

        public async Task<decimal> GetOccupancyRateAsync(int apartmentId, DateTime startDate, DateTime endDate)
        {
            var apartment = await _context.Apartments
                .Include(a => a.ReservationUnits)
                .FirstOrDefaultAsync(a => a.ApartmentId == apartmentId);

            if (apartment == null)
                return 0;

            var totalDays = (endDate - startDate).Days;
            var occupiedDays = apartment.ReservationUnits
                .Where(ru => ru.Status != "cancelled" &&
                           ru.CheckInDate < endDate &&
                           ru.CheckOutDate > startDate)
               .Sum(ru => (Math.Min(ru.CheckOutDate.Ticks, endDate.Ticks) - Math.Max(ru.CheckInDate.Ticks, startDate.Ticks)) > 0
    ? TimeSpan.FromTicks(Math.Min(ru.CheckOutDate.Ticks, endDate.Ticks) - Math.Max(ru.CheckInDate.Ticks, startDate.Ticks)).Days
    : 0);


			return totalDays > 0 ? (decimal)occupiedDays / totalDays * 100 : 0;
        }

        public async Task<decimal> GetRevenueAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits.Where(ru => ru.ApartmentId == apartmentId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            return await query.SumAsync(ru => ru.TotalAmount);
        }

        public async Task<int> GetReservationCountAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits.Where(ru => ru.ApartmentId == apartmentId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<decimal> GetAverageStayDurationAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits.Where(ru => ru.ApartmentId == apartmentId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            var reservations = await query.ToListAsync();
            if (!reservations.Any())
                return 0;

            return reservations.Average(ru => (decimal)(ru.CheckOutDate - ru.CheckInDate).Days);
        }

        public async Task<object> GetUtilizationStatisticsAsync(int apartmentId, DateTime startDate, DateTime endDate)
        {
            var apartment = await _context.Apartments
                .Include(a => a.ReservationUnits)
                .FirstOrDefaultAsync(a => a.ApartmentId == apartmentId);

            if (apartment == null)
                return new { Error = "Apartment not found" };

            var totalDays = (endDate - startDate).Days;
            var reservations = apartment.ReservationUnits
                .Where(ru => ru.Status != "cancelled" && 
                           ru.CheckInDate < endDate && 
                           ru.CheckOutDate > startDate)
                .ToList();

            var occupiedDays = reservations
                .Sum(ru => (Math.Min(ru.CheckOutDate.Ticks, endDate.Ticks) - Math.Max(ru.CheckInDate.Ticks, startDate.Ticks)) > 0 ? TimeSpan.FromTicks(Math.Min(ru.CheckOutDate.Ticks, endDate.Ticks) - Math.Max(ru.CheckInDate.Ticks, startDate.Ticks)).Days : 0);

            var occupancyRate = totalDays > 0 ? (decimal)occupiedDays / totalDays * 100 : 0;
            var totalRevenue = reservations.Sum(ru => ru.TotalAmount);
            var averageStayDuration = reservations.Any() ? reservations.Average(ru => (decimal)(ru.CheckOutDate - ru.CheckInDate).Days) : 0;

            return new
            {
                ApartmentId = apartmentId,
                ApartmentCode = apartment.ApartmentCode,
                ApartmentName = apartment.ApartmentName,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = totalDays,
                OccupiedDays = occupiedDays,
                OccupancyRate = occupancyRate,
                TotalRevenue = totalRevenue,
                AverageStayDuration = averageStayDuration,
                ReservationCount = reservations.Count
            };
        }
    }
}
