using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
    public interface IActivityLogService
    {
        Task<ZaaerActivityLogResponseDto> CreateAsync(ZaaerCreateActivityLogDto dto);
        Task<IEnumerable<ZaaerActivityLogResponseDto>> SearchAsync(ZaaerActivityLogQuery query);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly ApplicationDbContext _db;
        public ActivityLogService(ApplicationDbContext db) { _db = db; }

        public async Task<ZaaerActivityLogResponseDto> CreateAsync(ZaaerCreateActivityLogDto dto)
        {
            var entity = new ActivityLog
            {
                HotelId = dto.HotelId,
                EventKey = dto.EventKey,
                Message = dto.Message,
                ReservationId = dto.ReservationId,
                UnitId = dto.UnitId,
                RefType = dto.RefType,
                RefId = dto.RefId,
                RefNo = dto.RefNo,
                AmountFrom = dto.AmountFrom,
                AmountTo = dto.AmountTo,
                CreatedBy = dto.CreatedBy,
                CreatedAt = KsaTime.Now
            };
            _db.ActivityLogs.Add(entity);
            await _db.SaveChangesAsync();
            return ToDto(entity);
        }

        public async Task<IEnumerable<ZaaerActivityLogResponseDto>> SearchAsync(ZaaerActivityLogQuery query)
        {
            var q = _db.ActivityLogs.AsQueryable();
            if (query.HotelId.HasValue) q = q.Where(x => x.HotelId == query.HotelId);
            if (query.DateFrom.HasValue) q = q.Where(x => x.CreatedAt >= query.DateFrom.Value);
            if (query.DateTo.HasValue) q = q.Where(x => x.CreatedAt <= query.DateTo.Value);
            if (!string.IsNullOrWhiteSpace(query.EventKey)) q = q.Where(x => x.EventKey == query.EventKey);
            if (query.ReservationId.HasValue) q = q.Where(x => x.ReservationId == query.ReservationId);
            if (query.UnitId.HasValue) q = q.Where(x => x.UnitId == query.UnitId);
            q = q.OrderByDescending(x => x.CreatedAt);
            if (query.Skip.HasValue) q = q.Skip(query.Skip.Value);
            if (query.Take.HasValue) q = q.Take(query.Take.Value);
            var list = await q.ToListAsync();
            return list.Select(ToDto);
        }

        private static ZaaerActivityLogResponseDto ToDto(ActivityLog a) => new ZaaerActivityLogResponseDto
        {
            LogId = a.LogId,
            HotelId = a.HotelId,
            EventKey = a.EventKey,
            Message = a.Message,
            ReservationId = a.ReservationId,
            UnitId = a.UnitId,
            RefType = a.RefType,
            RefId = a.RefId,
            RefNo = a.RefNo,
            AmountFrom = a.AmountFrom,
            AmountTo = a.AmountTo,
            CreatedBy = a.CreatedBy,
            CreatedAt = a.CreatedAt
        };
    }
}


