using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service implementation for ReservationUnit operations
    /// </summary>
    public class ReservationUnitService : IReservationUnitService
    {
        private readonly IReservationUnitRepository _reservationUnitRepository;
        private readonly IMapper _mapper;

        public ReservationUnitService(IReservationUnitRepository reservationUnitRepository, IMapper mapper)
        {
            _reservationUnitRepository = reservationUnitRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<ReservationUnitResponseDto> ReservationUnits, int TotalCount)> GetAllReservationUnitsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null)
        {
            try
            {
                System.Linq.Expressions.Expression<Func<ReservationUnit, bool>>? filter = null;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filter = ru => ru.Apartment.ApartmentName.Contains(searchTerm) ||
                                 ru.Apartment.ApartmentCode.Contains(searchTerm) ||
                                 ru.Status.Contains(searchTerm) ||
                                 ru.Reservation.ReservationNo.Contains(searchTerm);
                }

                var (reservationUnits, totalCount) = await _reservationUnitRepository.GetPagedAsync(
                    pageNumber, 
                    pageSize, 
                    filter);

                var reservationUnitDtos = _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
                return (reservationUnitDtos, totalCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units: {ex.Message}", ex);
            }
        }

        public async Task<ReservationUnitResponseDto?> GetReservationUnitByIdAsync(int id)
        {
            try
            {
                var reservationUnit = await _reservationUnitRepository.GetWithDetailsAsync(id);
                return reservationUnit != null ? _mapper.Map<ReservationUnitResponseDto>(reservationUnit) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation unit with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<ReservationUnitResponseDto> CreateReservationUnitAsync(CreateReservationUnitDto createReservationUnitDto)
        {
            try
            {
                // Check apartment availability
                var isAvailable = await _reservationUnitRepository.GetOverlappingDatesAsync(
                    createReservationUnitDto.ApartmentId, 
                    createReservationUnitDto.CheckInDate, 
                    createReservationUnitDto.CheckOutDate);

                if (isAvailable.Any())
                {
                    throw new InvalidOperationException($"Apartment is not available for the specified dates.");
                }

                // Calculate number of nights if not provided
                if (!createReservationUnitDto.NumberOfNights.HasValue)
                {
                    createReservationUnitDto.NumberOfNights = (int)(createReservationUnitDto.CheckOutDate - createReservationUnitDto.CheckInDate).TotalDays;
                }

                if (!createReservationUnitDto.DepartureDate.HasValue)
                {
                    createReservationUnitDto.DepartureDate = createReservationUnitDto.CheckOutDate;
                }

                // Calculate VAT amount if not provided
                if (!createReservationUnitDto.VatAmount.HasValue)
                {
                    createReservationUnitDto.VatAmount = createReservationUnitDto.RentAmount * (createReservationUnitDto.VatRate / 100);
                }

                // Calculate lodging tax amount if not provided
                if (!createReservationUnitDto.LodgingTaxAmount.HasValue)
                {
                    createReservationUnitDto.LodgingTaxAmount = createReservationUnitDto.RentAmount * (createReservationUnitDto.LodgingTaxRate / 100);
                }

                // Calculate total amount if not provided
                if (createReservationUnitDto.TotalAmount == 0)
                {
                    createReservationUnitDto.TotalAmount = createReservationUnitDto.RentAmount + 
                                                        (createReservationUnitDto.VatAmount ?? 0) + 
                                                        (createReservationUnitDto.LodgingTaxAmount ?? 0);
                }

                var reservationUnit = _mapper.Map<ReservationUnit>(createReservationUnitDto);
                reservationUnit.CreatedAt = KsaTime.Now;

                var createdReservationUnit = await _reservationUnitRepository.AddAsync(reservationUnit);
                return _mapper.Map<ReservationUnitResponseDto>(createdReservationUnit);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating reservation unit: {ex.Message}", ex);
            }
        }

        public async Task<ReservationUnitResponseDto?> UpdateReservationUnitAsync(int id, UpdateReservationUnitDto updateReservationUnitDto)
        {
            try
            {
                var existingReservationUnit = await _reservationUnitRepository.GetByIdAsync(id);
                if (existingReservationUnit == null)
                {
                    return null;
                }

                // Check apartment availability (excluding current unit)
                var isAvailable = await _reservationUnitRepository.GetOverlappingDatesAsync(
                    updateReservationUnitDto.ApartmentId, 
                    updateReservationUnitDto.CheckInDate, 
                    updateReservationUnitDto.CheckOutDate, 
                    id);

                if (isAvailable.Any())
                {
                    throw new InvalidOperationException($"Apartment is not available for the specified dates.");
                }

                // Calculate number of nights if not provided
                if (!updateReservationUnitDto.NumberOfNights.HasValue)
                {
                    updateReservationUnitDto.NumberOfNights = (int)(updateReservationUnitDto.CheckOutDate - updateReservationUnitDto.CheckInDate).TotalDays;
                }

                if (!updateReservationUnitDto.DepartureDate.HasValue)
                {
                    updateReservationUnitDto.DepartureDate = updateReservationUnitDto.CheckOutDate;
                }

                // Calculate VAT amount if not provided
                if (!updateReservationUnitDto.VatAmount.HasValue)
                {
                    updateReservationUnitDto.VatAmount = updateReservationUnitDto.RentAmount * (updateReservationUnitDto.VatRate / 100);
                }

                // Calculate lodging tax amount if not provided
                if (!updateReservationUnitDto.LodgingTaxAmount.HasValue)
                {
                    updateReservationUnitDto.LodgingTaxAmount = updateReservationUnitDto.RentAmount * (updateReservationUnitDto.LodgingTaxRate / 100);
                }

                // Calculate total amount if not provided
                if (updateReservationUnitDto.TotalAmount == 0)
                {
                    updateReservationUnitDto.TotalAmount = updateReservationUnitDto.RentAmount + 
                                                        (updateReservationUnitDto.VatAmount ?? 0) + 
                                                        (updateReservationUnitDto.LodgingTaxAmount ?? 0);
                }

                _mapper.Map(updateReservationUnitDto, existingReservationUnit);

                await _reservationUnitRepository.UpdateAsync(existingReservationUnit);

                return _mapper.Map<ReservationUnitResponseDto>(existingReservationUnit);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating reservation unit with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteReservationUnitAsync(int id)
        {
            try
            {
                var reservationUnit = await _reservationUnitRepository.GetByIdAsync(id);
                if (reservationUnit == null)
                {
                    return false;
                }

                await _reservationUnitRepository.DeleteAsync(reservationUnit);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting reservation unit with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByReservationIdAsync(int reservationId)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetWithDetailsByReservationIdAsync(reservationId);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units for reservation {reservationId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByApartmentIdAsync(int apartmentId)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetWithDetailsByApartmentIdAsync(apartmentId);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units for apartment {apartmentId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByStatusAsync(string status)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByStatusAsync(status);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by status {status}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCheckInDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByCheckInDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by check-in date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCheckOutDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByCheckOutDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by check-out date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByRentAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByRentAmountRangeAsync(minAmount, maxAmount);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by rent amount range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByTotalAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByTotalAmountRangeAsync(minAmount, maxAmount);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by total amount range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByNumberOfNightsRangeAsync(int minNights, int maxNights)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByNumberOfNightsRangeAsync(minNights, maxNights);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by number of nights range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByVatRateRangeAsync(decimal minRate, decimal maxRate)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByVatRateRangeAsync(minRate, maxRate);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by VAT rate range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByLodgingTaxRateRangeAsync(decimal minRate, decimal maxRate)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByLodgingTaxRateRangeAsync(minRate, maxRate);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by lodging tax rate range: {ex.Message}", ex);
            }
        }

        public async Task<object> GetReservationUnitStatisticsAsync()
        {
            try
            {
                return await _reservationUnitRepository.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation unit statistics: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByHotelIdAsync(int hotelId)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByHotelIdAsync(hotelId);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByBuildingIdAsync(int buildingId)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByBuildingIdAsync(buildingId);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units for building {buildingId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByFloorIdAsync(int floorId)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByFloorIdAsync(floorId);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units for floor {floorId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByRoomTypeIdAsync(int roomTypeId)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByRoomTypeIdAsync(roomTypeId);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units for room type {roomTypeId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCustomerIdAsync(int customerId)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByCustomerIdAsync(customerId);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units for customer {customerId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCorporateCustomerIdAsync(int corporateCustomerId)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByCorporateCustomerIdAsync(corporateCustomerId);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units for corporate customer {corporateCustomerId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCreatedDateAsync(DateTime createdDate)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByCreatedDateAsync(createdDate);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by created date: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetReservationUnitsByCreatedDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByCreatedDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation units by created date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetActiveReservationUnitsAsync()
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetActiveAsync();
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving active reservation units: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetCancelledReservationUnitsAsync()
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetCancelledAsync();
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving cancelled reservation units: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetCompletedReservationUnitsAsync()
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetCompletedAsync();
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving completed reservation units: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByApartmentNameAsync(string apartmentName)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByApartmentNameAsync(apartmentName);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching reservation units by apartment name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByBuildingNameAsync(string buildingName)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByBuildingNameAsync(buildingName);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching reservation units by building name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByRoomTypeNameAsync(string roomTypeName)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByRoomTypeNameAsync(roomTypeName);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching reservation units by room type name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByCustomerNameAsync(string customerName)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByCustomerNameAsync(customerName);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching reservation units by customer name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByCorporateCustomerNameAsync(string corporateCustomerName)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByCorporateCustomerNameAsync(corporateCustomerName);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching reservation units by corporate customer name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> SearchReservationUnitsByHotelNameAsync(string hotelName)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetByHotelNameAsync(hotelName);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching reservation units by hotel name: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetTotalRevenueByReservationIdAsync(int reservationId)
        {
            try
            {
                return await _reservationUnitRepository.GetTotalRevenueByReservationIdAsync(reservationId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving total revenue for reservation {reservationId}: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetTotalRevenueByApartmentIdAsync(int apartmentId)
        {
            try
            {
                return await _reservationUnitRepository.GetTotalRevenueByApartmentIdAsync(apartmentId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving total revenue for apartment {apartmentId}: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetTotalRevenueByHotelIdAsync(int hotelId)
        {
            try
            {
                return await _reservationUnitRepository.GetTotalRevenueByHotelIdAsync(hotelId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving total revenue for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetTotalRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _reservationUnitRepository.GetTotalRevenueByDateRangeAsync(startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving total revenue by date range: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetAverageRentAmountByApartmentIdAsync(int apartmentId)
        {
            try
            {
                return await _reservationUnitRepository.GetAverageRentAmountByApartmentIdAsync(apartmentId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving average rent amount for apartment {apartmentId}: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetAverageTotalAmountByApartmentIdAsync(int apartmentId)
        {
            try
            {
                return await _reservationUnitRepository.GetAverageTotalAmountByApartmentIdAsync(apartmentId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving average total amount for apartment {apartmentId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<object>> GetTopApartmentsByRevenueAsync(int topCount = 10)
        {
            try
            {
                return await _reservationUnitRepository.GetTopApartmentsByRevenueAsync(topCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top apartments by revenue: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<object>> GetTopHotelsByRevenueAsync(int topCount = 10)
        {
            try
            {
                return await _reservationUnitRepository.GetTopHotelsByRevenueAsync(topCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top hotels by revenue: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ReservationUnitResponseDto>> GetOverlappingDatesAsync(int apartmentId, DateTime checkInDate, DateTime checkOutDate, int? excludeUnitId = null)
        {
            try
            {
                var reservationUnits = await _reservationUnitRepository.GetOverlappingDatesAsync(apartmentId, checkInDate, checkOutDate, excludeUnitId);
                return _mapper.Map<IEnumerable<ReservationUnitResponseDto>>(reservationUnits);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving overlapping dates: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateReservationUnitStatusAsync(int id, string status)
        {
            try
            {
                var reservationUnit = await _reservationUnitRepository.GetByIdAsync(id);
                if (reservationUnit == null)
                {
                    return false;
                }

                reservationUnit.Status = status;
                await _reservationUnitRepository.UpdateAsync(reservationUnit);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating reservation unit status: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateReservationUnitAmountsAsync(int id, decimal rentAmount, decimal? vatAmount, decimal? lodgingTaxAmount, decimal totalAmount)
        {
            try
            {
                var reservationUnit = await _reservationUnitRepository.GetByIdAsync(id);
                if (reservationUnit == null)
                {
                    return false;
                }

                reservationUnit.RentAmount = rentAmount;
                reservationUnit.VatAmount = vatAmount;
                reservationUnit.LodgingTaxAmount = lodgingTaxAmount;
                reservationUnit.TotalAmount = totalAmount;
                await _reservationUnitRepository.UpdateAsync(reservationUnit);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating reservation unit amounts: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateReservationUnitDatesAsync(int id, DateTime checkInDate, DateTime checkOutDate, int? numberOfNights)
        {
            try
            {
                var reservationUnit = await _reservationUnitRepository.GetByIdAsync(id);
                if (reservationUnit == null)
                {
                    return false;
                }

                reservationUnit.CheckInDate = checkInDate;
                reservationUnit.CheckOutDate = checkOutDate;
                reservationUnit.NumberOfNights = numberOfNights ?? (int)(checkOutDate - checkInDate).TotalDays;
                await _reservationUnitRepository.UpdateAsync(reservationUnit);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating reservation unit dates: {ex.Message}", ex);
            }
        }

        public async Task<bool> CheckApartmentAvailabilityAsync(int apartmentId, DateTime checkInDate, DateTime checkOutDate, int? excludeUnitId = null)
        {
            try
            {
                var overlappingUnits = await _reservationUnitRepository.GetOverlappingDatesAsync(apartmentId, checkInDate, checkOutDate, excludeUnitId);
                return !overlappingUnits.Any();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking apartment availability: {ex.Message}", ex);
            }
        }
    }
}
