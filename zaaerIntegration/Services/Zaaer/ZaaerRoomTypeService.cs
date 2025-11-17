using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
    /// <summary>
    /// Service for Zaaer Room Type integration
    /// </summary>
    public interface IZaaerRoomTypeService
    {
        Task<ZaaerRoomTypeResponseDto> CreateRoomTypeAsync(ZaaerCreateRoomTypeDto createRoomTypeDto);
        Task<ZaaerRoomTypeResponseDto?> UpdateRoomTypeAsync(int roomTypeId, ZaaerUpdateRoomTypeDto updateRoomTypeDto);
        Task<ZaaerRoomTypeResponseDto?> UpdateRoomTypeByZaaerIdAsync(int zaaerId, ZaaerUpdateRoomTypeDto updateRoomTypeDto);
        Task<IEnumerable<ZaaerRoomTypeResponseDto>> GetRoomTypesByHotelIdAsync(int hotelId);
        Task<ZaaerRoomTypeResponseDto?> GetRoomTypeByIdAsync(int roomTypeId);
        Task<bool> DeleteRoomTypeAsync(int roomTypeId);
    }

    public class ZaaerRoomTypeService : IZaaerRoomTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ZaaerRoomTypeService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ZaaerRoomTypeResponseDto> CreateRoomTypeAsync(ZaaerCreateRoomTypeDto createRoomTypeDto)
        {
            var roomType = _mapper.Map<RoomType>(createRoomTypeDto);

            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();

            return _mapper.Map<ZaaerRoomTypeResponseDto>(roomType);
        }

        public async Task<ZaaerRoomTypeResponseDto?> UpdateRoomTypeAsync(int roomTypeId, ZaaerUpdateRoomTypeDto updateRoomTypeDto)
        {
            var existingRoomType = await _context.RoomTypes.FindAsync(roomTypeId);
            if (existingRoomType == null)
            {
                return null;
            }

            _mapper.Map(updateRoomTypeDto, existingRoomType);

            await _context.SaveChangesAsync();

            return _mapper.Map<ZaaerRoomTypeResponseDto>(existingRoomType);
        }

        public async Task<ZaaerRoomTypeResponseDto?> UpdateRoomTypeByZaaerIdAsync(int zaaerId, ZaaerUpdateRoomTypeDto updateRoomTypeDto)
        {
            var query = _context.RoomTypes.AsQueryable();
            query = query.Where(rt => rt.ZaaerId == zaaerId);
            if (updateRoomTypeDto.HotelId.HasValue)
            {
                var hotelId = updateRoomTypeDto.HotelId.Value;
                query = query.Where(rt => rt.HotelId == hotelId);
            }

            var existing = await query.FirstOrDefaultAsync();
            if (existing == null)
            {
                return null;
            }

            _mapper.Map(updateRoomTypeDto, existing);
            await _context.SaveChangesAsync();
            return _mapper.Map<ZaaerRoomTypeResponseDto>(existing);
        }

        public async Task<IEnumerable<ZaaerRoomTypeResponseDto>> GetRoomTypesByHotelIdAsync(int hotelId)
        {
            var roomTypes = await _context.RoomTypes
                .Where(rt => rt.HotelId == hotelId)
                .OrderBy(rt => rt.RoomTypeName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ZaaerRoomTypeResponseDto>>(roomTypes);
        }

        public async Task<ZaaerRoomTypeResponseDto?> GetRoomTypeByIdAsync(int roomTypeId)
        {
            var roomType = await _context.RoomTypes.FindAsync(roomTypeId);
            if (roomType == null)
            {
                return null;
            }

            return _mapper.Map<ZaaerRoomTypeResponseDto>(roomType);
        }

        public async Task<bool> DeleteRoomTypeAsync(int roomTypeId)
        {
            var roomType = await _context.RoomTypes.FindAsync(roomTypeId);
            if (roomType == null)
            {
                return false;
            }

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
