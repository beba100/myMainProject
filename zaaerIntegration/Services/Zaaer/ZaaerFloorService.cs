using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
    /// <summary>
    /// Interface for Zaaer Floor Service
    /// </summary>
    public interface IZaaerFloorService
    {
        Task<ZaaerFloorResponseDto> CreateFloorAsync(ZaaerCreateFloorDto createFloorDto);
        Task<IEnumerable<ZaaerFloorResponseDto>> CreateFloorsAsync(List<ZaaerCreateFloorDto> createFloorDtos);
        Task<ZaaerFloorResponseDto?> UpdateFloorAsync(int floorId, ZaaerUpdateFloorDto updateFloorDto);
        Task<ZaaerFloorResponseDto?> GetFloorByIdAsync(int floorId);
        Task<IEnumerable<ZaaerFloorResponseDto>> GetFloorsByHotelIdAsync(int hotelId);
        Task<bool> DeleteFloorAsync(int floorId);
    }

    /// <summary>
    /// Service for Zaaer Floor integration
    /// </summary>
    public class ZaaerFloorService : IZaaerFloorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Floor> _floorRepository;
        private readonly IMapper _mapper;

        public ZaaerFloorService(
            IUnitOfWork unitOfWork,
            IGenericRepository<Floor> floorRepository,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _floorRepository = floorRepository;
            _mapper = mapper;
        }

        public async Task<ZaaerFloorResponseDto> CreateFloorAsync(ZaaerCreateFloorDto createFloorDto)
        {
            var floor = _mapper.Map<Floor>(createFloorDto);
            var createdFloor = await _floorRepository.AddAsync(floor);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ZaaerFloorResponseDto>(createdFloor);
        }

        public async Task<IEnumerable<ZaaerFloorResponseDto>> CreateFloorsAsync(List<ZaaerCreateFloorDto> createFloorDtos)
        {
            var floors = _mapper.Map<List<Floor>>(createFloorDtos);
            var createdFloors = await _floorRepository.AddRangeAsync(floors);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<IEnumerable<ZaaerFloorResponseDto>>(createdFloors);
        }

        public async Task<ZaaerFloorResponseDto?> UpdateFloorAsync(int floorId, ZaaerUpdateFloorDto updateFloorDto)
        {
            var existingFloor = await _floorRepository.GetByIdAsync(floorId);
            if (existingFloor == null)
            {
                return null;
            }

            _mapper.Map(updateFloorDto, existingFloor);
            await _floorRepository.UpdateAsync(existingFloor);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ZaaerFloorResponseDto>(existingFloor);
        }

        public async Task<ZaaerFloorResponseDto?> GetFloorByIdAsync(int floorId)
        {
            var floor = await _floorRepository.GetByIdAsync(floorId);
            if (floor == null)
            {
                return null;
            }

            return _mapper.Map<ZaaerFloorResponseDto>(floor);
        }

        public async Task<IEnumerable<ZaaerFloorResponseDto>> GetFloorsByHotelIdAsync(int hotelId)
        {
            var floors = await _floorRepository.FindAsync(f => f.HotelId == hotelId);
            return _mapper.Map<IEnumerable<ZaaerFloorResponseDto>>(floors);
        }

        public async Task<bool> DeleteFloorAsync(int floorId)
        {
            var floor = await _floorRepository.GetByIdAsync(floorId);
            if (floor == null)
            {
                return false;
            }

            await _floorRepository.DeleteAsync(floor);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
