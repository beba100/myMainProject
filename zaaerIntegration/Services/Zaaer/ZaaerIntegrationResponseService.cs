using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
    public interface IZaaerIntegrationResponseService
    {
        Task<ZaaerIntegrationResponseDto> CreateAsync(ZaaerCreateIntegrationResponseDto dto);
        Task<IEnumerable<ZaaerIntegrationResponseDto>> SearchAsync(ZaaerIntegrationResponseQuery query);
        Task<ZaaerIntegrationResponseDto?> UpdateAsync(int responseId, ZaaerCreateIntegrationResponseDto dto);
    }

    public class ZaaerIntegrationResponseService : IZaaerIntegrationResponseService
    {
        private readonly ApplicationDbContext _db;

        public ZaaerIntegrationResponseService(ApplicationDbContext db)
        { _db = db; }

        public async Task<ZaaerIntegrationResponseDto> CreateAsync(ZaaerCreateIntegrationResponseDto dto)
        {
            var entity = new IntegrationResponse
            {
                HotelId = dto.HotelId,
                ResNo = dto.ResNo,
                Service = dto.Service,
                EventType = dto.EventType,
                UnitNumber = dto.UnitNumber,
                Guest = dto.Guest,
                ErrorMessage = dto.ErrorMessage,
                Status = dto.Status,
                CreatedAt = KsaTime.Now
            };

            _db.Set<IntegrationResponse>().Add(entity);
            await _db.SaveChangesAsync();

            return ToDto(entity);
        }

        public async Task<IEnumerable<ZaaerIntegrationResponseDto>> SearchAsync(ZaaerIntegrationResponseQuery query)
        {
            var q = _db.Set<IntegrationResponse>().AsQueryable();

            if (query.HotelId.HasValue)
                q = q.Where(x => x.HotelId == query.HotelId.Value);
            if (query.DateFrom.HasValue)
                q = q.Where(x => x.CreatedAt >= query.DateFrom.Value);
            if (query.DateTo.HasValue)
                q = q.Where(x => x.CreatedAt <= query.DateTo.Value);
            if (!string.IsNullOrWhiteSpace(query.ResNo))
                q = q.Where(x => x.ResNo != null && x.ResNo.Contains(query.ResNo));
            if (!string.IsNullOrWhiteSpace(query.Service))
                q = q.Where(x => x.Service == query.Service);
            if (!string.IsNullOrWhiteSpace(query.EventType))
                q = q.Where(x => x.EventType != null && x.EventType.Contains(query.EventType));
            if (!string.IsNullOrWhiteSpace(query.Status))
                q = q.Where(x => x.Status == query.Status);

            q = q.OrderByDescending(x => x.CreatedAt);

            if (query.Skip.HasValue) q = q.Skip(query.Skip.Value);
            if (query.Take.HasValue) q = q.Take(query.Take.Value);

            var list = await q.ToListAsync();
            return list.Select(ToDto);
        }

        public async Task<ZaaerIntegrationResponseDto?> UpdateAsync(int responseId, ZaaerCreateIntegrationResponseDto dto)
        {
            var entity = await _db.Set<IntegrationResponse>().FirstOrDefaultAsync(x => x.ResponseId == responseId);
            if (entity == null) return null;
            entity.HotelId = dto.HotelId;
            entity.ResNo = dto.ResNo;
            entity.Service = dto.Service;
            entity.EventType = dto.EventType;
            entity.UnitNumber = dto.UnitNumber;
            entity.Guest = dto.Guest;
            entity.ErrorMessage = dto.ErrorMessage;
            entity.Status = dto.Status;
            await _db.SaveChangesAsync();
            return ToDto(entity);
        }

        private static ZaaerIntegrationResponseDto ToDto(IntegrationResponse e) => new ZaaerIntegrationResponseDto
        {
            HotelId = e.HotelId,
            ResponseId = e.ResponseId,
            ResNo = e.ResNo,
            Service = e.Service,
            EventType = e.EventType,
            UnitNumber = e.UnitNumber,
            Guest = e.Guest,
            ErrorMessage = e.ErrorMessage,
            Status = e.Status,
            CreatedAt = e.CreatedAt
        };
    }
}


