using AutoMapper;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service for CreditNote business logic
    /// </summary>
    public class CreditNoteService : ICreditNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreditNoteService> _logger;

        public CreditNoteService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreditNoteService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(IEnumerable<CreditNoteResponseDto> creditNotes, int totalCount)> GetAllCreditNotesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var (creditNotes, totalCount) = await _unitOfWork.CreditNotes.GetPagedAsync(pageNumber, pageSize);
                var creditNoteDtos = _mapper.Map<IEnumerable<CreditNoteResponseDto>>(creditNotes);
                return (creditNoteDtos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credit notes");
                throw;
            }
        }

        public async Task<CreditNoteResponseDto?> GetCreditNoteByIdAsync(int id)
        {
            try
            {
                var creditNote = await _unitOfWork.CreditNotes.GetByIdAsync(id);
                return creditNote != null ? _mapper.Map<CreditNoteResponseDto>(creditNote) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credit note with ID {CreditNoteId}", id);
                throw;
            }
        }

        public async Task<CreditNoteResponseDto> CreateCreditNoteAsync(CreateCreditNoteDto createCreditNoteDto)
        {
            try
            {
                var creditNote = _mapper.Map<FinanceLedgerAPI.Models.CreditNote>(createCreditNoteDto);
                await _unitOfWork.CreditNotes.AddAsync(creditNote);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<CreditNoteResponseDto>(creditNote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating credit note");
                throw;
            }
        }

        public async Task<CreditNoteResponseDto?> UpdateCreditNoteAsync(int id, UpdateCreditNoteDto updateCreditNoteDto)
        {
            try
            {
                var existingCreditNote = await _unitOfWork.CreditNotes.GetByIdAsync(id);
                if (existingCreditNote == null)
                    return null;

                _mapper.Map(updateCreditNoteDto, existingCreditNote);
                await _unitOfWork.CreditNotes.UpdateAsync(existingCreditNote);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<CreditNoteResponseDto>(existingCreditNote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating credit note with ID {CreditNoteId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCreditNoteAsync(int id)
        {
            try
            {
                var creditNote = await _unitOfWork.CreditNotes.GetByIdAsync(id);
                if (creditNote == null)
                    return false;

                await _unitOfWork.CreditNotes.DeleteAsync(creditNote);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting credit note with ID {CreditNoteId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<CreditNoteResponseDto>> GetCreditNotesByHotelIdAsync(int hotelId)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.HotelId == hotelId);
                return _mapper.Map<IEnumerable<CreditNoteResponseDto>>(creditNotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credit notes for hotel {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<IEnumerable<CreditNoteResponseDto>> GetCreditNotesByReservationIdAsync(int reservationId)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.ReservationId == reservationId);
                return _mapper.Map<IEnumerable<CreditNoteResponseDto>>(creditNotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credit notes for reservation {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<IEnumerable<CreditNoteResponseDto>> GetCreditNotesByCustomerIdAsync(int customerId)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.CustomerId == customerId);
                return _mapper.Map<IEnumerable<CreditNoteResponseDto>>(creditNotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credit notes for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<CreditNoteResponseDto>> GetCreditNotesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.CreditNoteDate >= startDate && cn.CreditNoteDate <= endDate);
                return _mapper.Map<IEnumerable<CreditNoteResponseDto>>(creditNotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credit notes by date range");
                throw;
            }
        }

        public async Task<IEnumerable<CreditNoteResponseDto>> GetCreditNotesByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.CreditAmount >= minAmount && cn.CreditAmount <= maxAmount);
                return _mapper.Map<IEnumerable<CreditNoteResponseDto>>(creditNotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credit notes by amount range");
                throw;
            }
        }

        public async Task<CreditNoteStatisticsDto> GetCreditNoteStatisticsAsync()
        {
            try
            {
                var allCreditNotes = await _unitOfWork.CreditNotes.GetAllAsync();
                var creditNotes = allCreditNotes;

                var statistics = new CreditNoteStatisticsDto
                {
                    TotalCreditNotes = creditNotes.Count(),
                    TotalCreditNoteAmount = creditNotes.Sum(cn => cn.CreditAmount),
                    AverageCreditNoteAmount = creditNotes.Any() ? creditNotes.Average(cn => cn.CreditAmount) : 0,
                    MaxCreditNoteAmount = creditNotes.Any() ? creditNotes.Max(cn => cn.CreditAmount) : 0,
                    MinCreditNoteAmount = creditNotes.Any() ? creditNotes.Min(cn => cn.CreditAmount) : 0,
                    CreditNotesByHotel = creditNotes.GroupBy(cn => cn.HotelId).ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    CreditNotesByMonth = creditNotes.GroupBy(cn => cn.CreditNoteDate.ToString("yyyy-MM")).ToDictionary(g => g.Key, g => g.Count()),
                    TotalCreditNoteAmountByHotel = creditNotes.GroupBy(cn => cn.HotelId).ToDictionary(g => g.Key.ToString(), g => g.Sum(cn => cn.CreditAmount))
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credit note statistics");
                throw;
            }
        }

        public async Task<IEnumerable<CreditNoteResponseDto>> SearchCreditNotesByCreditNoteNumberAsync(string creditNoteNumber)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.CreditNoteNo.Contains(creditNoteNumber));
                return _mapper.Map<IEnumerable<CreditNoteResponseDto>>(creditNotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching credit notes by credit note number {CreditNoteNumber}", creditNoteNumber);
                throw;
            }
        }

        public async Task<IEnumerable<CreditNoteResponseDto>> SearchCreditNotesByCustomerNameAsync(string customerName)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.CustomerId != null);
                return _mapper.Map<IEnumerable<CreditNoteResponseDto>>(creditNotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching credit notes by customer name {CustomerName}", customerName);
                throw;
            }
        }

        public async Task<decimal> GetTotalCreditNoteAmountByHotelIdAsync(int hotelId)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.HotelId == hotelId);
                return creditNotes.Sum(cn => cn.CreditAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total credit note amount for hotel {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<decimal> GetTotalCreditNoteAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.CreditNoteDate >= startDate && cn.CreditNoteDate <= endDate);
                return creditNotes.Sum(cn => cn.CreditAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total credit note amount by date range");
                throw;
            }
        }

        public async Task<decimal> GetTotalCreditNoteAmountByCustomerIdAsync(int customerId)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.CustomerId == customerId);
                return creditNotes.Sum(cn => cn.CreditAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total credit note amount for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<decimal> GetTotalCreditNoteAmountByReservationIdAsync(int reservationId)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.ReservationId == reservationId);
                return creditNotes.Sum(cn => cn.CreditAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total credit note amount for reservation {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<decimal> GetAverageCreditNoteAmountByHotelIdAsync(int hotelId)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.HotelId == hotelId);
                return creditNotes.Any() ? creditNotes.Average(cn => cn.CreditAmount) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving average credit note amount for hotel {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<decimal> GetAverageCreditNoteAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var creditNotes = await _unitOfWork.CreditNotes.FindAsync(cn => cn.CreditNoteDate >= startDate && cn.CreditNoteDate <= endDate);
                return creditNotes.Any() ? creditNotes.Average(cn => cn.CreditAmount) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving average credit note amount by date range");
                throw;
            }
        }
    }
}
