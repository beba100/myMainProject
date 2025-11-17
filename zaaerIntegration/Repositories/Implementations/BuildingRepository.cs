using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Building operations
    /// </summary>
    public class BuildingRepository : GenericRepository<Building>, IBuildingRepository
    {
        public BuildingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Building> Buildings, int TotalCount)> GetPagedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            System.Linq.Expressions.Expression<Func<Building, bool>>? filter = null)
        {
            var query = _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var buildings = await query
                .OrderBy(b => b.BuildingName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (buildings, totalCount);
        }

        public async Task<IEnumerable<Building>> GetByHotelIdAsync(int hotelId)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.HotelId == hotelId)
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<Building?> GetByBuildingNumberAsync(string buildingNumber)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .FirstOrDefaultAsync(b => b.BuildingNumber == buildingNumber);
        }

        public async Task<IEnumerable<Building>> GetByBuildingNameAsync(string buildingName)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.BuildingName.Contains(buildingName))
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<Building?> GetWithDetailsAsync(int id)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .FirstOrDefaultAsync(b => b.BuildingId == id);
        }

        public async Task<IEnumerable<Building>> GetWithDetailsByHotelIdAsync(int hotelId)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.HotelId == hotelId)
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<object> GetStatisticsAsync()
        {
            var totalBuildings = await _context.Buildings.CountAsync();
            var buildingsWithFloors = await _context.Buildings.CountAsync(b => b.Floors.Any());
            var buildingsWithApartments = await _context.Buildings.CountAsync(b => b.Apartments.Any());
            var buildingsWithoutFloors = totalBuildings - buildingsWithFloors;
            var buildingsWithoutApartments = totalBuildings - buildingsWithApartments;

            var hotelBreakdown = await _context.Buildings
                .GroupBy(b => b.HotelId)
                .Select(g => new { 
                    HotelId = g.Key, 
                    Count = g.Count(),
                    HotelName = g.First().HotelSettings.HotelName
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var floorBreakdown = await _context.Buildings
                .GroupBy(b => b.BuildingId)
                .Select(g => new { 
                    BuildingId = g.Key, 
                    FloorCount = g.First().Floors.Count,
                    ApartmentCount = g.First().Apartments.Count,
                    BuildingName = g.First().BuildingName
                })
                .OrderByDescending(x => x.FloorCount)
                .Take(10)
                .ToListAsync();

            var apartmentBreakdown = await _context.Buildings
                .GroupBy(b => b.BuildingId)
                .Select(g => new { 
                    BuildingId = g.Key, 
                    ApartmentCount = g.First().Apartments.Count,
                    FloorCount = g.First().Floors.Count,
                    BuildingName = g.First().BuildingName
                })
                .OrderByDescending(x => x.ApartmentCount)
                .Take(10)
                .ToListAsync();

            var revenueBreakdown = await _context.Buildings
                .GroupBy(b => b.BuildingId)
                .Select(g => new { 
                    BuildingId = g.Key, 
                    Revenue = g.First().Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)),
                    ApartmentCount = g.First().Apartments.Count,
                    BuildingName = g.First().BuildingName
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            return new
            {
                TotalBuildings = totalBuildings,
                BuildingsWithFloors = buildingsWithFloors,
                BuildingsWithApartments = buildingsWithApartments,
                BuildingsWithoutFloors = buildingsWithoutFloors,
                BuildingsWithoutApartments = buildingsWithoutApartments,
                HotelBreakdown = hotelBreakdown,
                FloorBreakdown = floorBreakdown,
                ApartmentBreakdown = apartmentBreakdown,
                RevenueBreakdown = revenueBreakdown
            };
        }

        public async Task<IEnumerable<Building>> SearchByNameAsync(string name)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.BuildingName.Contains(name))
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> SearchByNumberAsync(string number)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.BuildingNumber.Contains(number))
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetByHotelNameAsync(string hotelName)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.HotelSettings.HotelName.Contains(hotelName))
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<bool> BuildingNumberExistsAsync(string buildingNumber, int? excludeId = null)
        {
            var query = _context.Buildings.Where(b => b.BuildingNumber == buildingNumber);
            
            if (excludeId.HasValue)
            {
                query = query.Where(b => b.BuildingId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Building>> GetWithFloorsAsync()
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.Floors.Any())
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetWithoutFloorsAsync()
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => !b.Floors.Any())
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetWithApartmentsAsync()
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.Apartments.Any())
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetWithoutApartmentsAsync()
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => !b.Apartments.Any())
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetByFloorCountRangeAsync(int minCount, int maxCount)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.Floors.Count >= minCount && b.Floors.Count <= maxCount)
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetByApartmentCountRangeAsync(int minCount, int maxCount)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.Apartments.Count >= minCount && b.Apartments.Count <= maxCount)
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetTopByFloorCountAsync(int topCount = 10)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .OrderByDescending(b => b.Floors.Count)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetTopByApartmentCountAsync(int topCount = 10)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .OrderByDescending(b => b.Apartments.Count)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) >= minRevenue && 
                           b.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)) <= maxRevenue)
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetTopByRevenueAsync(int topCount = 10)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .OrderByDescending(b => b.Apartments.Sum(a => a.ReservationUnits.Sum(ru => ru.TotalAmount)))
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetByReservationCountRangeAsync(int minCount, int maxCount)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.Apartments.Sum(a => a.ReservationUnits.Count) >= minCount && 
                           b.Apartments.Sum(a => a.ReservationUnits.Count) <= maxCount)
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetTopByReservationCountAsync(int topCount = 10)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .OrderByDescending(b => b.Apartments.Sum(a => a.ReservationUnits.Count))
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<decimal> GetOccupancyRateAsync(int buildingId, DateTime startDate, DateTime endDate)
        {
            var building = await _context.Buildings
                .Include(b => b.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId);

            if (building == null)
                return 0;

            var totalDays = (endDate - startDate).Days;
            var totalApartmentDays = building.Apartments.Count * totalDays;
            
            if (totalApartmentDays == 0)
                return 0;

            var occupiedDays = building.Apartments
                .SelectMany(a => a.ReservationUnits)
                .Where(ru => ru.Status != "cancelled" && 
                           ru.CheckInDate < endDate && 
                           ru.CheckOutDate > startDate)
                .Sum(ru => (ru.CheckOutDate < endDate ? ru.CheckOutDate : endDate).Subtract(ru.CheckInDate > startDate ? ru.CheckInDate : startDate).Days);

            return (decimal)occupiedDays / totalApartmentDays * 100;
        }

        public async Task<decimal> GetRevenueAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.BuildingId == buildingId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            return await query.SumAsync(ru => ru.TotalAmount);
        }

        public async Task<int> GetReservationCountAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.BuildingId == buildingId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<decimal> GetAverageStayDurationAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.BuildingId == buildingId);

            if (startDate.HasValue)
                query = query.Where(ru => ru.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ru => ru.CheckOutDate <= endDate.Value);

            var reservations = await query.ToListAsync();
            if (!reservations.Any())
                return 0;

            return reservations.Average(ru => (decimal)(ru.CheckOutDate - ru.CheckInDate).Days);
        }

        public async Task<object> GetUtilizationStatisticsAsync(int buildingId, DateTime startDate, DateTime endDate)
        {
            var building = await _context.Buildings
                .Include(b => b.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId);

            if (building == null)
                return new { Error = "Building not found" };

            var totalDays = (endDate - startDate).Days;
            var totalApartmentDays = building.Apartments.Count * totalDays;
            
            var reservations = building.Apartments
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
                BuildingId = buildingId,
                BuildingName = building.BuildingName,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = totalDays,
                TotalApartmentDays = totalApartmentDays,
                OccupiedDays = occupiedDays,
                OccupancyRate = occupancyRate,
                TotalRevenue = totalRevenue,
                AverageStayDuration = averageStayDuration,
                ReservationCount = reservations.Count,
                ApartmentCount = building.Apartments.Count
            };
        }

        public async Task<IEnumerable<Building>> GetByAddressAsync(string address)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.Address.Contains(address))
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> SearchByAddressAsync(string address)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.Address.Contains(address))
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<Building?> GetByHotelAndBuildingNumberAsync(int hotelId, string buildingNumber)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .FirstOrDefaultAsync(b => b.HotelId == hotelId && b.BuildingNumber == buildingNumber);
        }

        public async Task<IEnumerable<Building>> GetByHotelAndBuildingNameAsync(int hotelId, string buildingName)
        {
            return await _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .Where(b => b.HotelId == hotelId && b.BuildingName.Contains(buildingName))
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Building>> GetByMultipleCriteriaAsync(int? hotelId = null, string? buildingNumber = null, string? buildingName = null, string? address = null)
        {
            var query = _context.Buildings
                .Include(b => b.HotelSettings)
                .Include(b => b.Floors)
                .Include(b => b.Apartments)
                .AsQueryable();

            if (hotelId.HasValue)
                query = query.Where(b => b.HotelId == hotelId.Value);

            if (!string.IsNullOrEmpty(buildingNumber))
                query = query.Where(b => b.BuildingNumber.Contains(buildingNumber));

            if (!string.IsNullOrEmpty(buildingName))
                query = query.Where(b => b.BuildingName.Contains(buildingName));

            if (!string.IsNullOrEmpty(address))
                query = query.Where(b => b.Address.Contains(address));

            return await query
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        public async Task<object> GetFloorStatisticsAsync(int buildingId)
        {
            var building = await _context.Buildings
                .Include(b => b.Floors)
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId);

            if (building == null)
                return new { Error = "Building not found" };

            var totalFloors = building.Floors.Count;
            var floorsWithApartments = building.Floors.Count(f => f.Apartments.Any());
            var floorsWithoutApartments = totalFloors - floorsWithApartments;

            return new
            {
                BuildingId = buildingId,
                BuildingName = building.BuildingName,
                TotalFloors = totalFloors,
                FloorsWithApartments = floorsWithApartments,
                FloorsWithoutApartments = floorsWithoutApartments
            };
        }

        public async Task<object> GetApartmentStatisticsAsync(int buildingId)
        {
            var building = await _context.Buildings
                .Include(b => b.Apartments)
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId);

            if (building == null)
                return new { Error = "Building not found" };

            var totalApartments = building.Apartments.Count;
            var apartmentsWithReservations = building.Apartments.Count(a => a.ReservationUnits.Any());
            var apartmentsWithoutReservations = totalApartments - apartmentsWithReservations;

            return new
            {
                BuildingId = buildingId,
                BuildingName = building.BuildingName,
                TotalApartments = totalApartments,
                ApartmentsWithReservations = apartmentsWithReservations,
                ApartmentsWithoutReservations = apartmentsWithoutReservations
            };
        }

        public async Task<object> GetReservationStatisticsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.BuildingId == buildingId);

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
                BuildingId = buildingId,
                StartDate = startDate,
                EndDate = endDate,
                TotalReservations = totalReservations,
                ConfirmedReservations = confirmedReservations,
                CancelledReservations = cancelledReservations,
                CompletedReservations = completedReservations
            };
        }

        public async Task<object> GetRevenueStatisticsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ReservationUnits
                .Where(ru => ru.Apartment.BuildingId == buildingId);

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
                BuildingId = buildingId,
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                AverageRevenue = averageRevenue,
                MaxRevenue = maxRevenue,
                MinRevenue = minRevenue
            };
        }

        public async Task<object> GetOccupancyStatisticsAsync(int buildingId, DateTime startDate, DateTime endDate)
        {
            var building = await _context.Buildings
                .Include(b => b.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId);

            if (building == null)
                return new { Error = "Building not found" };

            var totalDays = (endDate - startDate).Days;
            var totalApartmentDays = building.Apartments.Count * totalDays;
            
            var reservations = building.Apartments
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
                BuildingId = buildingId,
                BuildingName = building.BuildingName,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = totalDays,
                TotalApartmentDays = totalApartmentDays,
                OccupiedDays = occupiedDays,
                OccupancyRate = occupancyRate,
                ApartmentCount = building.Apartments.Count
            };
        }

        public async Task<object> GetPerformanceMetricsAsync(int buildingId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var building = await _context.Buildings
                .Include(b => b.Apartments)
                .ThenInclude(a => a.ReservationUnits)
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId);

            if (building == null)
                return new { Error = "Building not found" };

            var query = building.Apartments
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
                BuildingId = buildingId,
                BuildingName = building.BuildingName,
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                AverageStayDuration = averageStayDuration,
                TotalReservations = totalReservations,
                ApartmentCount = building.Apartments.Count,
                FloorCount = building.Floors.Count
            };
        }
    }
}
