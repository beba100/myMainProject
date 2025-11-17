using FinanceLedgerAPI.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
    public interface IReservationRatesService
    {
        Task<IEnumerable<ZaaerReservationRatesResponseDto>> GetByReservationAsync(int reservationId);
        Task UpsertRatesAsync(int reservationId, IEnumerable<ZaaerReservationUnitDayRateItem> items, decimal? ewaPercent, decimal? vatPercent);
        Task ApplySameAmountAsync(int reservationId, decimal amount, int? unitId, DateTime? dateFrom, DateTime? dateTo, decimal? ewaPercent, decimal? vatPercent);
        Task ReplaceRatesAsync(int reservationId, IEnumerable<ZaaerReservationUnitDayRateItem> items);
        // Task ReplaceRatesByUnitAsync(int reservationId, IDictionary<int, List<ZaaerReservationUnitDayRateItem>> itemsByUnit);
    }

    public class ReservationRatesService : IReservationRatesService
    {
        private readonly ApplicationDbContext _db;

        public ReservationRatesService(ApplicationDbContext db)
        { _db = db; }

        public async Task<IEnumerable<ZaaerReservationRatesResponseDto>> GetByReservationAsync(int reservationId)
        {
            var list = await _db.ReservationUnitDayRates
                .Where(r => r.ReservationId == reservationId)
                .OrderBy(r => r.UnitId).ThenBy(r => r.NightDate)
                .ToListAsync();

            return list.Select(r => new ZaaerReservationRatesResponseDto
            {
                RateId = r.RateId,
                ReservationId = r.ReservationId,
                UnitId = r.UnitId,
                NightDate = r.NightDate,
                GrossRate = r.GrossRate,
                EwaAmount = r.EwaAmount,
                VatAmount = r.VatAmount,
                NetAmount = r.NetAmount
            });
        }

        public async Task UpsertRatesAsync(int reservationId, IEnumerable<ZaaerReservationUnitDayRateItem> items, decimal? ewaPercent, decimal? vatPercent)
        {
            foreach (var item in items)
            {
                var (ewa, vat, net) = ComputeTaxes(item.GrossRate, ewaPercent, vatPercent);
                var existing = await _db.ReservationUnitDayRates
                    .FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.UnitId == item.UnitId && r.NightDate.Date == item.NightDate.Date);
                if (existing == null)
                {
                    _db.ReservationUnitDayRates.Add(new ReservationUnitDayRate
                    {
                        ReservationId = reservationId,
                        UnitId = item.UnitId,
                        NightDate = item.NightDate.Date,
                        GrossRate = item.GrossRate,
                        EwaAmount = item.EwaAmount ?? ewa,
                        VatAmount = item.VatAmount ?? vat,
                        NetAmount = item.NetAmount ?? net,
                        IsManual = true
                    });
                }
                else
                {
                    existing.GrossRate = item.GrossRate;
                    existing.EwaAmount = item.EwaAmount ?? ewa;
                    existing.VatAmount = item.VatAmount ?? vat;
                    existing.NetAmount = item.NetAmount ?? net;
                    existing.IsManual = true;
                    existing.UpdatedAt = KsaTime.Now;
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task ReplaceRatesAsync(int reservationId, IEnumerable<ZaaerReservationUnitDayRateItem> items)
        {
            var existing = await _db.ReservationUnitDayRates
                .Where(r => r.ReservationId == reservationId)
                .ToListAsync();

            if (existing.Count > 0)
            {
                _db.ReservationUnitDayRates.RemoveRange(existing);
            }

            foreach (var item in items)
            {
                _db.ReservationUnitDayRates.Add(new ReservationUnitDayRate
                {
                    ReservationId = reservationId,
                    UnitId = item.UnitId,
                    NightDate = item.NightDate.Date,
                    GrossRate = item.GrossRate,
                    EwaAmount = item.EwaAmount,
                    VatAmount = item.VatAmount,
                    NetAmount = item.NetAmount,
                    IsManual = true,
                    CreatedAt = KsaTime.Now
                });
            }

            await _db.SaveChangesAsync();
        }

        // public async Task ReplaceRatesByUnitAsync(int reservationId, IDictionary<int, List<ZaaerReservationUnitDayRateItem>> itemsByUnit)
        // {
        //     if (itemsByUnit == null || itemsByUnit.Count == 0)
        //     {
        //         return;
        //     }
        //
        //     var unitIds = itemsByUnit.Keys.Where(id => id > 0).Distinct().ToList();
        //     if (unitIds.Count == 0)
        //     {
        //         return;
        //     }
        //
        //     var existing = await _db.ReservationUnitDayRates
        //         .Where(r => r.ReservationId == reservationId && unitIds.Contains(r.UnitId))
        //         .ToListAsync();
        //
        //     if (existing.Count > 0)
        //     {
        //         _db.ReservationUnitDayRates.RemoveRange(existing);
        //     }
        //
        //     foreach (var kvp in itemsByUnit)
        //     {
        //         var unitId = kvp.Key;
        //         if (unitId <= 0) continue;
        //
        //         var list = kvp.Value;
        //         if (list == null || list.Count == 0) continue;
        //
        //         foreach (var item in list)
        //         {
        //             _db.ReservationUnitDayRates.Add(new ReservationUnitDayRate
        //             {
        //                 ReservationId = reservationId,
        //                 UnitId = unitId,
        //                 NightDate = item.NightDate.Date,
        //                 GrossRate = item.GrossRate,
        //                 EwaAmount = item.EwaAmount,
        //                 VatAmount = item.VatAmount,
        //                 NetAmount = item.NetAmount,
        //                 IsManual = true,
        //                 CreatedAt = KsaTime.Now
        //             });
        //         }
        //     }
        //
        //     await _db.SaveChangesAsync();
        // }

        public async Task ApplySameAmountAsync(int reservationId, decimal amount, int? unitId, DateTime? dateFrom, DateTime? dateTo, decimal? ewaPercent, decimal? vatPercent)
        {
            var query = _db.ReservationUnitDayRates.Where(r => r.ReservationId == reservationId);
            if (unitId.HasValue) query = query.Where(r => r.UnitId == unitId.Value);
            if (dateFrom.HasValue) query = query.Where(r => r.NightDate.Date >= dateFrom.Value.Date);
            if (dateTo.HasValue) query = query.Where(r => r.NightDate.Date <= dateTo.Value.Date);

            var list = await query.ToListAsync();
            var (ewa, vat, net) = ComputeTaxes(amount, ewaPercent, vatPercent);
            foreach (var r in list)
            {
                r.GrossRate = amount;
                r.EwaAmount = ewa;
                r.VatAmount = vat;
                r.NetAmount = net;
                r.IsManual = true;
                r.UpdatedAt = KsaTime.Now;
            }

            await _db.SaveChangesAsync();
        }

        private static (decimal? ewa, decimal? vat, decimal? net) ComputeTaxes(decimal gross, decimal? ewaPercent, decimal? vatPercent)
        {
            var hasEwa = ewaPercent.HasValue;
            var hasVat = vatPercent.HasValue;
            if (!hasEwa && !hasVat) return (null, null, null);

            var ewaRate = ewaPercent.GetValueOrDefault() / 100m;
            var vatRate = vatPercent.GetValueOrDefault() / 100m;
            var divisor = (1m + ewaRate) * (1m + vatRate);
            var baseBeforeTaxes = divisor == 0m ? gross : Math.Round(gross / divisor, 2);

            var ewa = hasEwa ? Math.Round(baseBeforeTaxes * ewaRate, 2) : 0m;
            var vat = hasVat ? Math.Round((baseBeforeTaxes + ewa) * vatRate, 2) : 0m;
            var net = Math.Round(baseBeforeTaxes, 2);

            return (ewa, vat, net);
        }
    }
}


