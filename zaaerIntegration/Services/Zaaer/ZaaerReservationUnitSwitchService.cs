using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
    public interface IReservationUnitSwitchService
    {
        Task<ZaaerReservationUnitSwitchResponseDto> CreateAsync(ZaaerCreateReservationUnitSwitchDto dto);
        Task<IEnumerable<ZaaerReservationUnitSwitchResponseDto>> GetByReservationAsync(int reservationId);
    }

    public class ReservationUnitSwitchService : IReservationUnitSwitchService
    {
        private readonly ApplicationDbContext _db;
        public ReservationUnitSwitchService(ApplicationDbContext db) { _db = db; }

        public async Task<ZaaerReservationUnitSwitchResponseDto> CreateAsync(ZaaerCreateReservationUnitSwitchDto dto)
        {
            // Validate reservation and unit
            var unit = await _db.ReservationUnits.FirstOrDefaultAsync(u => u.UnitId == dto.UnitId && u.ReservationId == dto.ReservationId);
            if (unit == null)
                throw new InvalidOperationException($"Reservation unit not found (reservationId={dto.ReservationId}, unitId={dto.UnitId}).");

            // Append swap record
            var entity = new ReservationUnitSwitch
            {
                ReservationId = dto.ReservationId,
                UnitId = dto.UnitId,
                FromApartmentId = dto.FromApartmentId,
                ToApartmentId = dto.ToApartmentId,
                ApplyMode = dto.ApplyMode,
                EffectiveDate = dto.EffectiveDate,
                Comment = dto.Comment,
                CreatedAt = KsaTime.Now
            };
            _db.ReservationUnitSwaps.Add(entity);

            // Update the reservation unit's apartment
            unit.ApartmentId = dto.ToApartmentId;

            // Price policy: we keep existing per-night overrides; client may then apply new prices
            // If needed later, we can extend here to recompute ReservationUnitDayRates based on ApplyMode

            await _db.SaveChangesAsync();

            return new ZaaerReservationUnitSwitchResponseDto
            {
                SwitchId = entity.SwitchId,
                ReservationId = entity.ReservationId,
                UnitId = entity.UnitId,
                FromApartmentId = entity.FromApartmentId,
                ToApartmentId = entity.ToApartmentId,
                ApplyMode = entity.ApplyMode,
                EffectiveDate = entity.EffectiveDate,
                Comment = entity.Comment,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<IEnumerable<ZaaerReservationUnitSwitchResponseDto>> GetByReservationAsync(int reservationId)
        {
            var list = await _db.ReservationUnitSwaps
                .Where(s => s.ReservationId == reservationId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return list.Select(s => new ZaaerReservationUnitSwitchResponseDto
            {
                SwitchId = s.SwitchId,
                ReservationId = s.ReservationId,
                UnitId = s.UnitId,
                FromApartmentId = s.FromApartmentId,
                ToApartmentId = s.ToApartmentId,
                ApplyMode = s.ApplyMode,
                EffectiveDate = s.EffectiveDate,
                Comment = s.Comment,
                CreatedAt = s.CreatedAt
            });
        }
    }
}
