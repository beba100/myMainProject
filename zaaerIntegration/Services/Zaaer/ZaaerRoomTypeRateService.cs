using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
    /// <summary>
    /// Service for Zaaer Room Type Rate integration
    /// </summary>
    public interface IZaaerRoomTypeRateService
    {
        Task<ZaaerRoomTypeRateResponseDto> CreateRoomTypeRateAsync(ZaaerCreateRoomTypeRateDto createDto);
        Task<ZaaerRoomTypeRateResponseDto?> UpdateRoomTypeRateAsync(int rateId, ZaaerUpdateRoomTypeRateDto updateDto);
        Task<ZaaerRoomTypeRateResponseDto?> UpdateRoomTypeRateByZaaerIdAsync(int zaaerId, ZaaerUpdateRoomTypeRateDto updateDto);
        Task<IEnumerable<ZaaerRoomTypeRateResponseDto>> GetRoomTypeRatesByHotelIdAsync(int hotelId);
        Task<IEnumerable<ZaaerRoomTypeRateResponseDto>> GetRoomTypeRatesByRoomTypeIdAsync(int roomTypeId);
        Task<ZaaerRoomTypeRateResponseDto?> GetRoomTypeRateByIdAsync(int rateId);
        Task<bool> DeleteRoomTypeRateAsync(int rateId);
    }

    public class ZaaerRoomTypeRateService : IZaaerRoomTypeRateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ZaaerRoomTypeRateService> _logger;

        public ZaaerRoomTypeRateService(ApplicationDbContext context, IMapper mapper, ILogger<ZaaerRoomTypeRateService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ZaaerRoomTypeRateResponseDto> CreateRoomTypeRateAsync(ZaaerCreateRoomTypeRateDto createDto)
        {
            // Check if rate already exists for this room type
            var existingRate = await _context.RoomTypeRates
                .FirstOrDefaultAsync(r => r.RoomTypeId == createDto.RoomTypeId && r.HotelId == createDto.HotelId);

            if (existingRate != null)
            {
                throw new InvalidOperationException($"Rate already exists for RoomTypeId {createDto.RoomTypeId} in Hotel {createDto.HotelId}");
            }

            var roomTypeRate = _mapper.Map<RoomTypeRate>(createDto);
            roomTypeRate.CreatedAt = KsaTime.Now;

            _context.RoomTypeRates.Add(roomTypeRate);
            await _context.SaveChangesAsync();

            return await MapToResponseDto(roomTypeRate);
        }

        public async Task<ZaaerRoomTypeRateResponseDto?> UpdateRoomTypeRateAsync(int rateId, ZaaerUpdateRoomTypeRateDto updateDto)
        {
            var existingRate = await _context.RoomTypeRates
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RateId == rateId);

            if (existingRate == null)
            {
                return null;
            }

            _mapper.Map(updateDto, existingRate);
            existingRate.UpdatedAt = KsaTime.Now;

            await _context.SaveChangesAsync();

            return await MapToResponseDto(existingRate);
        }

        public async Task<ZaaerRoomTypeRateResponseDto?> UpdateRoomTypeRateByZaaerIdAsync(int zaaerId, ZaaerUpdateRoomTypeRateDto updateDto)
        {
            // Find room type rate by ZaaerId
            var query = _context.RoomTypeRates
                .Include(r => r.RoomType)
                .Where(r => r.ZaaerId == zaaerId);

            // Optionally filter by HotelId if provided
            if (updateDto.HotelId.HasValue)
            {
                query = query.Where(r => r.HotelId == updateDto.HotelId.Value);
            }

            // Optionally filter by RoomTypeId if provided
            if (updateDto.RoomTypeId.HasValue)
            {
                query = query.Where(r => r.RoomTypeId == updateDto.RoomTypeId.Value);
            }

            var existingRate = await query.FirstOrDefaultAsync();

            if (existingRate == null)
            {
                _logger.LogWarning("RoomTypeRate with ZaaerId {ZaaerId} not found", zaaerId);
                return null;
            }

            _mapper.Map(updateDto, existingRate);
            existingRate.UpdatedAt = KsaTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("RoomTypeRate updated successfully: ZaaerId={ZaaerId}, RateId={RateId}", zaaerId, existingRate.RateId);

            return await MapToResponseDto(existingRate);
        }

        public async Task<IEnumerable<ZaaerRoomTypeRateResponseDto>> GetRoomTypeRatesByHotelIdAsync(int hotelId)
        {
            var rates = await _context.RoomTypeRates
                .Include(r => r.RoomType)
                .Where(r => r.HotelId == hotelId)
                .OrderBy(r => r.RoomType.RoomTypeName)
                .ToListAsync();

            var result = new List<ZaaerRoomTypeRateResponseDto>();
            foreach (var rate in rates)
            {
                result.Add(await MapToResponseDto(rate));
            }

            return result;
        }

        public async Task<IEnumerable<ZaaerRoomTypeRateResponseDto>> GetRoomTypeRatesByRoomTypeIdAsync(int roomTypeId)
        {
            var rates = await _context.RoomTypeRates
                .Include(r => r.RoomType)
                .Where(r => r.RoomTypeId == roomTypeId)
                .ToListAsync();

            var result = new List<ZaaerRoomTypeRateResponseDto>();
            foreach (var rate in rates)
            {
                result.Add(await MapToResponseDto(rate));
            }

            return result;
        }

        public async Task<ZaaerRoomTypeRateResponseDto?> GetRoomTypeRateByIdAsync(int rateId)
        {
            var rate = await _context.RoomTypeRates
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RateId == rateId);

            if (rate == null)
            {
                return null;
            }

            return await MapToResponseDto(rate);
        }

        public async Task<bool> DeleteRoomTypeRateAsync(int rateId)
        {
            var rate = await _context.RoomTypeRates.FindAsync(rateId);
            if (rate == null)
            {
                return false;
            }

            _context.RoomTypeRates.Remove(rate);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<ZaaerRoomTypeRateResponseDto> MapToResponseDto(RoomTypeRate rate)
        {
            var dto = _mapper.Map<ZaaerRoomTypeRateResponseDto>(rate);
            
            if (rate.RoomType != null)
            {
                dto.RoomTypeName = rate.RoomType.RoomTypeName;
            }

            return dto;
        }
    }
}

