using AutoMapper;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service for Refund business logic
    /// </summary>
    public class RefundService : IRefundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RefundService> _logger;

        public RefundService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RefundService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(IEnumerable<RefundResponseDto> refunds, int totalCount)> GetAllRefundsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var (refunds, totalCount) = await _unitOfWork.Refunds.GetPagedAsync(pageNumber, pageSize);
                var refundDtos = _mapper.Map<IEnumerable<RefundResponseDto>>(refunds);
                return (refundDtos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds");
                throw;
            }
        }

        public async Task<RefundResponseDto?> GetRefundByIdAsync(int id)
        {
            try
            {
                var refund = await _unitOfWork.Refunds.GetByIdAsync(id);
                return refund != null ? _mapper.Map<RefundResponseDto>(refund) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refund with ID {RefundId}", id);
                throw;
            }
        }

        public async Task<RefundResponseDto> CreateRefundAsync(CreateRefundDto createRefundDto)
        {
            try
            {
                var refund = _mapper.Map<FinanceLedgerAPI.Models.Refund>(createRefundDto);
                await _unitOfWork.Refunds.AddAsync(refund);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<RefundResponseDto>(refund);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund");
                throw;
            }
        }

        public async Task<RefundResponseDto?> UpdateRefundAsync(int id, UpdateRefundDto updateRefundDto)
        {
            try
            {
                var existingRefund = await _unitOfWork.Refunds.GetByIdAsync(id);
                if (existingRefund == null)
                    return null;

                _mapper.Map(updateRefundDto, existingRefund);
                await _unitOfWork.Refunds.UpdateAsync(existingRefund);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<RefundResponseDto>(existingRefund);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund with ID {RefundId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteRefundAsync(int id)
        {
            try
            {
                var refund = await _unitOfWork.Refunds.GetByIdAsync(id);
                if (refund == null)
                    return false;

                await _unitOfWork.Refunds.DeleteAsync(refund);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting refund with ID {RefundId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RefundResponseDto>> GetRefundsByHotelIdAsync(int hotelId)
        {
            try
            {
                var refunds = await _unitOfWork.Refunds.FindAsync(r => r.HotelId == hotelId);
                return _mapper.Map<IEnumerable<RefundResponseDto>>(refunds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds for hotel {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<IEnumerable<RefundResponseDto>> GetRefundsByReservationIdAsync(int reservationId)
        {
            try
            {
                var refunds = await _unitOfWork.Refunds.FindAsync(r => r.ReservationId == reservationId);
                return _mapper.Map<IEnumerable<RefundResponseDto>>(refunds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds for reservation {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<IEnumerable<RefundResponseDto>> GetRefundsByCustomerIdAsync(int customerId)
        {
            try
            {
                var refunds = await _unitOfWork.Refunds.FindAsync(r => r.CustomerId == customerId);
                return _mapper.Map<IEnumerable<RefundResponseDto>>(refunds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<RefundResponseDto>> GetRefundsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var refunds = await _unitOfWork.Refunds.FindAsync(r => r.RefundDate >= startDate && r.RefundDate <= endDate);
                return _mapper.Map<IEnumerable<RefundResponseDto>>(refunds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds by date range");
                throw;
            }
        }

        public async Task<IEnumerable<RefundResponseDto>> GetRefundsByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            try
            {
                var refunds = await _unitOfWork.Refunds.FindAsync(r => r.RefundAmount >= minAmount && r.RefundAmount <= maxAmount);
                return _mapper.Map<IEnumerable<RefundResponseDto>>(refunds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds by amount range");
                throw;
            }
        }

        public async Task<RefundStatisticsDto> GetRefundStatisticsAsync()
        {
            try
            {
                var allRefunds = await _unitOfWork.Refunds.GetAllAsync();
                var refunds = allRefunds;

                var statistics = new RefundStatisticsDto
                {
                    TotalRefunds = refunds.Count(),
                    TotalRefundAmount = refunds.Sum(r => r.RefundAmount),
                    AverageRefundAmount = refunds.Any() ? refunds.Average(r => r.RefundAmount) : 0,
                    MaxRefundAmount = refunds.Any() ? refunds.Max(r => r.RefundAmount) : 0,
                    MinRefundAmount = refunds.Any() ? refunds.Min(r => r.RefundAmount) : 0,
                    RefundsByHotel = refunds.GroupBy(r => r.HotelId).ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    RefundsByMonth = refunds.GroupBy(r => r.RefundDate.ToString("yyyy-MM")).ToDictionary(g => g.Key, g => g.Count()),
                    TotalRefundAmountByHotel = refunds.GroupBy(r => r.HotelId).ToDictionary(g => g.Key.ToString(), g => g.Sum(r => r.RefundAmount))
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refund statistics");
                throw;
            }
        }

        public async Task<IEnumerable<RefundResponseDto>> SearchRefundsByRefundNumberAsync(string refundNumber)
        {
            try
            {
                var refunds = await _unitOfWork.Refunds.FindAsync(r => r.RefundNo.Contains(refundNumber));
                return _mapper.Map<IEnumerable<RefundResponseDto>>(refunds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching refunds by refund number {RefundNumber}", refundNumber);
                throw;
            }
        }

        public async Task<IEnumerable<RefundResponseDto>> SearchRefundsByCustomerNameAsync(string customerName)
        {
            try
            {
                var refunds = await _unitOfWork.Refunds.FindAsync(r => r.CustomerId != null);
                return _mapper.Map<IEnumerable<RefundResponseDto>>(refunds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching refunds by customer name {CustomerName}", customerName);
                throw;
            }
        }

        public async Task<decimal> GetTotalRefundAmountByHotelIdAsync(int hotelId)
        {
            try
            {
                var refunds = await _unitOfWork.Refunds.FindAsync(r => r.HotelId == hotelId);
                return refunds.Sum(r => r.RefundAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total refund amount for hotel {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<decimal> GetTotalRefundAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var refunds = await _unitOfWork.Refunds.FindAsync(r => r.RefundDate >= startDate && r.RefundDate <= endDate);
                return refunds.Sum(r => r.RefundAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total refund amount by date range");
                throw;
            }
        }
    }
}
