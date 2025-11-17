using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
    public interface IZaaerShomoosDetailsService
    {
        Task<ZaaerShomoosDetailsResponseDto> CreateAsync(ZaaerCreateShomoosDetailsDto dto);
        Task<ZaaerShomoosDetailsResponseDto?> UpdateAsync(int detailsId, ZaaerUpdateShomoosDetailsDto dto);
        Task<IEnumerable<ZaaerShomoosDetailsResponseDto>> GetAllAsync();
    }

    public class ZaaerShomoosDetailsService : IZaaerShomoosDetailsService
    {
        private readonly ApplicationDbContext _db;

        public ZaaerShomoosDetailsService(ApplicationDbContext db)
        { _db = db; }

        public async Task<ZaaerShomoosDetailsResponseDto> CreateAsync(ZaaerCreateShomoosDetailsDto dto)
        {
            var entity = new ShomoosDetails
            {
                HotelId = dto.HotelId,
                IsActive = dto.IsActive,
                UserId = dto.UserId,
                BranchCode = dto.BranchCode,
                BranchSecret = dto.BranchSecret,
                LanguageCode = dto.LanguageCode
            };
            _db.Set<ShomoosDetails>().Add(entity);
            await _db.SaveChangesAsync();
            return MapResponse(entity);
        }

        public async Task<ZaaerShomoosDetailsResponseDto?> UpdateAsync(int detailsId, ZaaerUpdateShomoosDetailsDto dto)
        {
            var entity = await _db.Set<ShomoosDetails>().FirstOrDefaultAsync(x => x.DetailsId == detailsId);
            if (entity == null) return null;
            if (dto.HotelId.HasValue) entity.HotelId = dto.HotelId.Value;
            if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
            if (dto.UserId != null) entity.UserId = dto.UserId;
            if (dto.BranchCode != null) entity.BranchCode = dto.BranchCode;
            if (dto.BranchSecret != null) entity.BranchSecret = dto.BranchSecret;
            if (dto.LanguageCode != null) entity.LanguageCode = dto.LanguageCode;
            entity.UpdatedAt = KsaTime.Now;
            await _db.SaveChangesAsync();
            return MapResponse(entity);
        }

        public async Task<IEnumerable<ZaaerShomoosDetailsResponseDto>> GetAllAsync()
        {
            var list = await _db.Set<ShomoosDetails>().OrderBy(x => x.DetailsId).ToListAsync();
            return list.Select(MapResponse);
        }

        private static ZaaerShomoosDetailsResponseDto MapResponse(ShomoosDetails e) => new ZaaerShomoosDetailsResponseDto
        {
            DetailsId = e.DetailsId,
            HotelId = e.HotelId,
            IsActive = e.IsActive,
            UserId = e.UserId,
            BranchCode = e.BranchCode,
            BranchSecretMask = string.IsNullOrEmpty(e.BranchSecret) ? null : "******",
            LanguageCode = e.LanguageCode,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}


