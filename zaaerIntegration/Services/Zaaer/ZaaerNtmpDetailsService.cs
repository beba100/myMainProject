using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
    public interface IZaaerNtmpDetailsService
    {
        Task<ZaaerNtmpDetailsResponseDto> CreateAsync(ZaaerCreateNtmpDetailsDto dto);
        Task<ZaaerNtmpDetailsResponseDto?> UpdateAsync(int detailsId, ZaaerUpdateNtmpDetailsDto dto);
        Task<IEnumerable<ZaaerNtmpDetailsResponseDto>> GetAllAsync();
    }

    public class ZaaerNtmpDetailsService : IZaaerNtmpDetailsService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public ZaaerNtmpDetailsService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db; _mapper = mapper;
        }

        public async Task<ZaaerNtmpDetailsResponseDto> CreateAsync(ZaaerCreateNtmpDetailsDto dto)
        {
            var entity = new NtmpDetails
            {
                HotelId = dto.HotelId,
                IsActive = dto.IsActive,
                GatewayApiKey = dto.GatewayApiKey,
                UserName = dto.UserName,
                PasswordHash = string.IsNullOrWhiteSpace(dto.Password) ? null : HashPassword(dto.Password)
            };
            _db.Set<NtmpDetails>().Add(entity);
            await _db.SaveChangesAsync();
            return MapResponse(entity);
        }

        public async Task<ZaaerNtmpDetailsResponseDto?> UpdateAsync(int detailsId, ZaaerUpdateNtmpDetailsDto dto)
        {
            var entity = await _db.Set<NtmpDetails>().FirstOrDefaultAsync(x => x.DetailsId == detailsId);
            if (entity == null) return null;
            if (dto.HotelId.HasValue) entity.HotelId = dto.HotelId.Value;
            if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
            if (dto.GatewayApiKey != null) entity.GatewayApiKey = dto.GatewayApiKey;
            if (dto.UserName != null) entity.UserName = dto.UserName;
            if (dto.Password != null) entity.PasswordHash = string.IsNullOrWhiteSpace(dto.Password) ? null : HashPassword(dto.Password);
            entity.UpdatedAt = KsaTime.Now;
            await _db.SaveChangesAsync();
            return MapResponse(entity);
        }

        public async Task<IEnumerable<ZaaerNtmpDetailsResponseDto>> GetAllAsync()
        {
            var list = await _db.Set<NtmpDetails>().OrderBy(x => x.DetailsId).ToListAsync();
            return list.Select(MapResponse);
        }

        private static string HashPassword(string password)
        {
            // Lightweight hash placeholder; in production use a strong hash (e.g., PBKDF2/BCrypt)
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        private static ZaaerNtmpDetailsResponseDto MapResponse(NtmpDetails e) => new ZaaerNtmpDetailsResponseDto
        {
            DetailsId = e.DetailsId,
            HotelId = e.HotelId,
            IsActive = e.IsActive,
            GatewayApiKey = e.GatewayApiKey,
            UserName = e.UserName,
            PasswordMask = string.IsNullOrEmpty(e.PasswordHash) ? null : "******",
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}


