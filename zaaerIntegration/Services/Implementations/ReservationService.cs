using AutoMapper;
using FinanceLedgerAPI.Models;
using FinanceLedgerAPI.Enums;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service implementation for Reservation operations
    /// </summary>
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IReservationUnitRepository _reservationUnitRepository;
        private readonly IMapper _mapper;

        public ReservationService(IReservationRepository reservationRepository, IReservationUnitRepository reservationUnitRepository, IMapper mapper)
        {
            _reservationRepository = reservationRepository;
            _reservationUnitRepository = reservationUnitRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<ReservationResponseDto> Reservations, int TotalCount)> GetAllReservationsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null)
        {
            try
            {
                System.Linq.Expressions.Expression<Func<FinanceLedgerAPI.Models.Reservation, bool>>? filter = null;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filter = r => r.ReservationNo.Contains(searchTerm);
                }

                var (reservations, totalCount) = await _reservationRepository.GetPagedAsync(
                    pageNumber, 
                    pageSize, 
                    filter);

                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);
                return (reservationDtos, totalCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservations: {ex.Message}", ex);
            }
        }

        public async Task<ReservationResponseDto?> GetReservationByIdAsync(int id)
        {
            try
            {
                var reservation = await _reservationRepository.GetWithDetailsAsync(id);
                return reservation != null ? _mapper.Map<ReservationResponseDto>(reservation) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<ReservationResponseDto?> GetReservationByNoAsync(string reservationNo)
        {
            try
            {
                var reservation = await _reservationRepository.GetWithDetailsByReservationNoAsync(reservationNo);
                return reservation != null ? _mapper.Map<ReservationResponseDto>(reservation) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation with number {reservationNo}: {ex.Message}", ex);
            }
        }

        public async Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto createReservationDto)
        {
            try
            {
                // Check if reservation number already exists
                if (await _reservationRepository.ReservationNoExistsAsync(createReservationDto.ReservationNo))
                {
                    throw new InvalidOperationException($"Reservation with number '{createReservationDto.ReservationNo}' already exists.");
                }

                var reservation = _mapper.Map<Reservation>(createReservationDto);
                reservation.ReservationDate = createReservationDto.ReservationDate ?? DateTime.Now;
                reservation.CreatedAt = KsaTime.Now;

                // Calculate balance amount if not provided
                if (reservation.TotalAmount.HasValue && reservation.AmountPaid.HasValue)
                {
                    reservation.BalanceAmount = reservation.TotalAmount.Value - reservation.AmountPaid.Value;
                }

                var createdReservation = await _reservationRepository.AddAsync(reservation);

                // Create reservation units for each apartment
                if (createReservationDto.ApartmentIds != null && createReservationDto.ApartmentIds.Any())
                {
                    foreach (var apartmentId in createReservationDto.ApartmentIds)
                    {
                        var reservationUnit = new ReservationUnit
                        {
                            ReservationId = createdReservation.ReservationId,
                            ApartmentId = apartmentId,
                            CheckInDate = DateTime.Now,
                            CheckOutDate = DateTime.Now.AddDays(1),
                            DepartureDate = DateTime.Now.AddDays(1),
                            NumberOfNights = 1,
                            StatusEnum = ReservationUnitStatus.Reserved,
                            CreatedAt = KsaTime.Now
                        };

                        await _reservationUnitRepository.AddAsync(reservationUnit);
                    }
                }

                return _mapper.Map<ReservationResponseDto>(createdReservation);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating reservation: {ex.Message}", ex);
            }
        }

        public async Task<ReservationResponseDto?> UpdateReservationAsync(int id, UpdateReservationDto updateReservationDto)
        {
            try
            {
                var existingReservation = await _reservationRepository.GetByIdAsync(id);
                if (existingReservation == null)
                {
                    return null;
                }

                // Check if reservation number already exists (excluding current reservation)
                if (await _reservationRepository.ReservationNoExistsAsync(updateReservationDto.ReservationNo, id))
                {
                    throw new InvalidOperationException($"Reservation with number '{updateReservationDto.ReservationNo}' already exists.");
                }

                _mapper.Map(updateReservationDto, existingReservation);

                // Recalculate balance amount
                if (existingReservation.TotalAmount.HasValue && existingReservation.AmountPaid.HasValue)
                {
                    existingReservation.BalanceAmount = existingReservation.TotalAmount.Value - existingReservation.AmountPaid.Value;
                }

                await _reservationRepository.UpdateAsync(existingReservation);

                return _mapper.Map<ReservationResponseDto>(existingReservation);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating reservation with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteReservationAsync(int id)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null)
                {
                    return false;
                }

                await _reservationRepository.DeleteAsync(reservation);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting reservation with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetReservationsByCustomerIdAsync(int customerId)
        {
            try
            {
                var reservations = await _reservationRepository.GetByCustomerIdAsync(customerId);
                return _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservations for customer {customerId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetReservationsByHotelIdAsync(int hotelId)
        {
            try
            {
                var reservations = await _reservationRepository.GetByHotelIdAsync(hotelId);
                return _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservations for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetReservationsByStatusAsync(string status)
        {
            try
            {
                var reservations = await _reservationRepository.GetByStatusAsync(status);
                return _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservations with status {status}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var reservations = await _reservationRepository.GetByDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservations by date range: {ex.Message}", ex);
            }
        }


        public async Task<IEnumerable<ReservationResponseDto>> SearchReservationsByCustomerNameAsync(string customerName)
        {
            try
            {
                var reservations = await _reservationRepository.GetByCustomerNameAsync(customerName);
                return _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching reservations by customer name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationResponseDto>> SearchReservationsByHotelNameAsync(string hotelName)
        {
            try
            {
                var reservations = await _reservationRepository.GetByHotelNameAsync(hotelName);
                return _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching reservations by hotel name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationResponseDto>> SearchReservationsByNoAsync(string reservationNo)
        {
            try
            {
                var reservations = await _reservationRepository.GetByReservationNoSearchAsync(reservationNo);
                return _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching reservations by number: {ex.Message}", ex);
            }
        }

        public async Task<object> GetReservationStatisticsAsync()
        {
            try
            {
                return await _reservationRepository.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation statistics: {ex.Message}", ex);
            }
        }

        public async Task<bool> ReservationNoExistsAsync(string reservationNo, int? excludeId = null)
        {
            try
            {
                return await _reservationRepository.ReservationNoExistsAsync(reservationNo, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking reservation number existence: {ex.Message}", ex);
            }
        }


        public async Task<bool> UpdateReservationStatusAsync(int id, string status)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null)
                {
                    return false;
                }

                reservation.Status = status;
                await _reservationRepository.UpdateAsync(reservation);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating reservation status: {ex.Message}", ex);
            }
        }

        public async Task<bool> CancelReservationAsync(int id, string cancellationReason)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null)
                {
                    return false;
                }

                reservation.Status = "Cancelled";
                await _reservationRepository.UpdateAsync(reservation);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error cancelling reservation: {ex.Message}", ex);
            }
        }

        public async Task<bool> CompleteReservationAsync(int id)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null)
                {
                    return false;
                }

                reservation.Status = "Completed";
                await _reservationRepository.UpdateAsync(reservation);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error completing reservation: {ex.Message}", ex);
            }
        }
    }
}
