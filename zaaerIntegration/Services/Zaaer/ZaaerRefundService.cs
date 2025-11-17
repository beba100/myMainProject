using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
    public interface IZaaerRefundService
    {
        Task<ZaaerRefundResponseDto> CreateRefundAsync(ZaaerCreateRefundDto createRefundDto);
        Task<ZaaerRefundResponseDto?> UpdateRefundAsync(int refundId, ZaaerUpdateRefundDto updateRefundDto);
        Task<ZaaerRefundResponseDto?> UpdateRefundByRefundNoAsync(string refundNo, ZaaerUpdateRefundDto updateRefundDto);
        Task<ZaaerRefundResponseDto?> UpdateRefundByZaaerIdAsync(int zaaerId, ZaaerUpdateRefundDto updateRefundDto);
        Task<ZaaerRefundResponseDto?> GetRefundByIdAsync(int refundId);
        Task<IEnumerable<ZaaerRefundResponseDto>> GetRefundsByHotelIdAsync(int hotelId);
        Task<bool> DeleteRefundAsync(int refundId);
    }

    public class ZaaerRefundService : IZaaerRefundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefundRepository _refundRepository;
        private readonly IMapper _mapper;

        public ZaaerRefundService(IUnitOfWork unitOfWork, IRefundRepository refundRepository, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _refundRepository = refundRepository;
            _mapper = mapper;
        }

        public async Task<ZaaerRefundResponseDto> CreateRefundAsync(ZaaerCreateRefundDto createRefundDto)
        {
            var refund = _mapper.Map<Refund>(createRefundDto);
            refund.CreatedAt = KsaTime.Now;

            var createdRefund = await _refundRepository.AddAsync(refund);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ZaaerRefundResponseDto>(createdRefund);
        }

        public async Task<ZaaerRefundResponseDto?> UpdateRefundAsync(int refundId, ZaaerUpdateRefundDto updateRefundDto)
        {
            var existingRefund = await _refundRepository.GetByIdAsync(refundId);
            if (existingRefund == null)
            {
                return null;
            }

            _mapper.Map(updateRefundDto, existingRefund);
            await _refundRepository.UpdateAsync(existingRefund);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ZaaerRefundResponseDto>(existingRefund);
        }

        public async Task<ZaaerRefundResponseDto?> UpdateRefundByRefundNoAsync(string refundNo, ZaaerUpdateRefundDto updateRefundDto)
        {
            var existingRefunds = await _refundRepository.FindAsync(r => r.RefundNo == refundNo);
            var refund = existingRefunds.FirstOrDefault();
            
            if (refund == null)
            {
                return null;
            }

            _mapper.Map(updateRefundDto, refund);
            await _refundRepository.UpdateAsync(refund);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ZaaerRefundResponseDto>(refund);
        }

        public async Task<ZaaerRefundResponseDto?> UpdateRefundByZaaerIdAsync(int zaaerId, ZaaerUpdateRefundDto updateRefundDto)
        {
            var existingRefunds = await _refundRepository.FindAsync(r => r.ZaaerId == zaaerId);
            var refund = existingRefunds.FirstOrDefault();

            if (refund == null)
            {
                return null;
            }

            _mapper.Map(updateRefundDto, refund);
            await _refundRepository.UpdateAsync(refund);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ZaaerRefundResponseDto>(refund);
        }

        public async Task<ZaaerRefundResponseDto?> GetRefundByIdAsync(int refundId)
        {
            var refund = await _refundRepository.GetByIdAsync(refundId);
            return _mapper.Map<ZaaerRefundResponseDto>(refund);
        }

        public async Task<IEnumerable<ZaaerRefundResponseDto>> GetRefundsByHotelIdAsync(int hotelId)
        {
            var refunds = await _refundRepository.FindAsync(r => r.HotelId == hotelId);
            return _mapper.Map<IEnumerable<ZaaerRefundResponseDto>>(refunds);
        }

        public async Task<bool> DeleteRefundAsync(int refundId)
        {
            var refund = await _refundRepository.GetByIdAsync(refundId);
            if (refund == null)
            {
                return false;
            }

            await _refundRepository.DeleteAsync(refund);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
