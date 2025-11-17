using System;
using AutoMapper;
using System.Linq;
using FinanceLedgerAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
    /// <summary>
    /// Interface for Zaaer Reservation Service
    /// </summary>
    public interface IZaaerReservationService
    {
        Task<ZaaerReservationResponseDto> CreateReservationAsync(ZaaerCreateReservationDto createReservationDto);
        Task<ZaaerReservationResponseDto?> UpdateReservationAsync(int reservationId, ZaaerUpdateReservationDto updateReservationDto);
        Task<ZaaerReservationResponseDto?> UpdateReservationByNumberAsync(string reservationNo, ZaaerUpdateReservationDto updateReservationDto);
        Task<ZaaerReservationResponseDto?> UpdateReservationByZaaerIdAsync(int zaaerId, ZaaerUpdateReservationDto updateReservationDto);
        Task<ZaaerReservationResponseDto?> GetReservationByIdAsync(int reservationId);
        Task<ZaaerReservationResponseDto?> GetReservationByNumberAsync(string reservationNo);
        Task<IEnumerable<ZaaerReservationResponseDto>> GetReservationsByHotelIdAsync(int hotelId);
        Task<bool> DeleteReservationAsync(int reservationId);
    }

    /// <summary>
    /// Service for Zaaer Reservation integration
    /// </summary>
    public class ZaaerReservationService : IZaaerReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReservationRepository _reservationRepository;
        private readonly IReservationUnitRepository _reservationUnitRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ZaaerReservationService> _logger;
        private readonly IReservationRatesService _reservationRatesService;
        private readonly ICustomerLedgerService _customerLedgerService;

        public ZaaerReservationService(
            IUnitOfWork unitOfWork,
            IReservationRepository reservationRepository,
            IReservationUnitRepository reservationUnitRepository,
            IInvoiceRepository invoiceRepository,
            IReservationRatesService reservationRatesService,
            ICustomerLedgerService customerLedgerService,
            IMapper mapper,
            ILogger<ZaaerReservationService> logger)
        {
            _unitOfWork = unitOfWork;
            _reservationRepository = reservationRepository;
            _reservationUnitRepository = reservationUnitRepository;
            _invoiceRepository = invoiceRepository;
            _reservationRatesService = reservationRatesService;
            _customerLedgerService = customerLedgerService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ZaaerReservationResponseDto> CreateReservationAsync(ZaaerCreateReservationDto createReservationDto)
        {
            if (!string.IsNullOrWhiteSpace(createReservationDto.ReservationNo))
            {
                var existingSameNo = await _reservationRepository.FindAsync(r =>
                    r.HotelId == createReservationDto.HotelId && r.ReservationNo == createReservationDto.ReservationNo);
                if (existingSameNo.Any())
                {
                    var byNumber = await UpdateReservationByNumberAsync(createReservationDto.ReservationNo, MapCreateToUpdate(createReservationDto));
                    if (byNumber != null)
                    {
                        _logger.LogInformation("CreateReservationAsync idempotent update: ReservationNo={ReservationNo}", createReservationDto.ReservationNo);
                        return byNumber;
                    }
                }
            }

            var reservation = _mapper.Map<Reservation>(createReservationDto);
            if (createReservationDto.ZaaerId.HasValue)
            {
                reservation.ZaaerId = createReservationDto.ZaaerId.Value;
            }
            if (createReservationDto.ExternalRefNo.HasValue)
            {
                reservation.ExternalRefNo = createReservationDto.ExternalRefNo.Value;
            }
            if (!string.IsNullOrWhiteSpace(createReservationDto.RentalType))
            {
                reservation.RentalType = createReservationDto.RentalType!;
            }
            if (createReservationDto.NumberOfMonths.HasValue)
            {
                reservation.NumberOfMonths = createReservationDto.NumberOfMonths.Value;
            }
            reservation.CreatedAt = KsaTime.Now;

            Reservation? createdReservation = null;
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                createdReservation = await _reservationRepository.AddAsync(reservation);
                await _unitOfWork.SaveChangesAsync();

            if (createReservationDto.ReservationUnits != null && createReservationDto.ReservationUnits.Any())
            {
                foreach (var unitDto in createReservationDto.ReservationUnits)
                {
                    var reservationIdValue = createdReservation.ZaaerId.HasValue 
                        ? createdReservation.ZaaerId.Value 
                        : createdReservation.ReservationId;
                        var effectiveCheckOut = unitDto.DepartureDate ?? unitDto.CheckOutDate;
                        var numberOfNights = unitDto.NumberOfNights ?? (int)Math.Max(1, Math.Ceiling((effectiveCheckOut - unitDto.CheckInDate).TotalDays));

                    var reservationUnit = new ReservationUnit
                    {
                        ReservationId = reservationIdValue,
                        ApartmentId = unitDto.ApartmentId,
                        CheckInDate = unitDto.CheckInDate,
                        CheckOutDate = unitDto.CheckOutDate,
                            DepartureDate = unitDto.DepartureDate ?? unitDto.CheckOutDate,
                            NumberOfNights = numberOfNights,
                        RentAmount = unitDto.RentAmount ?? 0m,
                        VatAmount = unitDto.VatAmount,
                        LodgingTaxAmount = unitDto.LodgingTaxAmount,
                        TotalAmount = unitDto.TotalAmount ?? unitDto.RentAmount ?? 0m,
                        Status = string.IsNullOrWhiteSpace(unitDto.Status) ? "confirmed" : unitDto.Status!,
                        CreatedAt = KsaTime.Now,
                        ZaaerId = unitDto.ZaaerId
                    };

                    if (unitDto.VatRate.HasValue)
                    {
                        reservationUnit.VatRate = unitDto.VatRate.Value;
                    }
                    else if (createReservationDto.VatRate.HasValue)
                    {
                        reservationUnit.VatRate = createReservationDto.VatRate.Value;
                    }

                    await _reservationUnitRepository.AddAsync(reservationUnit);
                    }
                }

                createdReservation.TotalNights = createReservationDto.TotalNights;
                createdReservation.Subtotal = createReservationDto.Subtotal;
                createdReservation.VatRate = createReservationDto.VatRate;
                createdReservation.VatAmount = createReservationDto.VatAmount;
                createdReservation.LodgingTaxRate = createReservationDto.LodgingTaxRate;
                createdReservation.LodgingTaxAmount = createReservationDto.LodgingTaxAmount;
                createdReservation.TotalTaxAmount = createReservationDto.TotalTaxAmount;
                createdReservation.TotalExtra = createReservationDto.TotalExtra;
                createdReservation.TotalPenalties = createReservationDto.TotalPenalties;
                createdReservation.TotalDiscounts = createReservationDto.TotalDiscounts;
                createdReservation.TotalAmount = createReservationDto.TotalAmount;
                createdReservation.AmountPaid = createReservationDto.AmountPaid;
                createdReservation.BalanceAmount = createReservationDto.BalanceAmount;
                createdReservation.CheckInDate = createReservationDto.CheckInDate;
                createdReservation.CheckOutDate = createReservationDto.CheckOutDate;
                createdReservation.DepartureDate = createReservationDto.DepartureDate;
                if (createReservationDto.IsAutoExtend.HasValue) createdReservation.IsAutoExtend = createReservationDto.IsAutoExtend.Value;
                if (createReservationDto.PriceTypeId.HasValue) createdReservation.PriceTypeId = createReservationDto.PriceTypeId.Value;

                await _reservationRepository.UpdateAsync(createdReservation);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                var updateOnDuplicate = await TryHandleDuplicateOnCreateAsync(createReservationDto, ex);
                if (updateOnDuplicate != null)
                {
                    return updateOnDuplicate;
                }

                _logger.LogError(ex, "CreateReservationAsync failed for ReservationNo={ReservationNo}, HotelId={HotelId}", createReservationDto.ReservationNo, createReservationDto.HotelId);
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception during CreateReservationAsync");
            }
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            if (createdReservation == null)
            {
                throw new InvalidOperationException("Reservation creation failed before day-rate generation.");
            }

            try
            {
                await GenerateDayRatesAsync(createdReservation.ReservationId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GenerateDayRatesAsync failed for ReservationId={ReservationId}. Continuing without day rates.", createdReservation.ReservationId);
            }

            try
            {
                await _customerLedgerService.SyncReservationAsync(createdReservation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SyncReservationAsync failed for ReservationId={ReservationId}. Continuing without ledger update.", createdReservation.ReservationId);
            }
            
                try
                {
                await UpdateApartmentStatusFromReservationUnitsAsync(createdReservation.ReservationId);
                }
                catch (Exception ex)
                {
                _logger.LogWarning(ex, "UpdateApartmentStatusFromReservationUnitsAsync failed for ReservationId={ReservationId}. Continuing without apartment status update.", createdReservation.ReservationId);
            }
            
            var response = _mapper.Map<ZaaerReservationResponseDto>(createdReservation);
            return response;
        }

        public async Task<ZaaerReservationResponseDto?> UpdateReservationAsync(int reservationId, ZaaerUpdateReservationDto updateReservationDto)
        {
            var existingReservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (existingReservation == null)
            {
                return null;
            }

            _mapper.Map(updateReservationDto, existingReservation);
            if (!string.IsNullOrWhiteSpace(updateReservationDto.RentalType))
            {
                existingReservation.RentalType = updateReservationDto.RentalType!;
            }
            // Update reservation type only if provided (don't set to null)
            if (!string.IsNullOrWhiteSpace(updateReservationDto.ReservationType))
            {
                existingReservation.ReservationType = updateReservationDto.ReservationType!;
            }
            // Ensure ReservationType is never null (database constraint)
            if (string.IsNullOrWhiteSpace(existingReservation.ReservationType))
            {
                existingReservation.ReservationType = "Individual";
            }
            // Update status only if provided (don't set to null)
            if (!string.IsNullOrWhiteSpace(updateReservationDto.Status))
            {
                existingReservation.Status = updateReservationDto.Status!;
            }
            // Ensure Status is never null (database constraint)
            if (string.IsNullOrWhiteSpace(existingReservation.Status))
            {
                existingReservation.Status = "Unconfirmed";
            }
            // Update number of months if provided
            if (updateReservationDto.NumberOfMonths.HasValue)
            {
                existingReservation.NumberOfMonths = updateReservationDto.NumberOfMonths.Value;
            }
            if (updateReservationDto.TotalPenalties.HasValue)
            {
                existingReservation.TotalPenalties = updateReservationDto.TotalPenalties.Value;
            }
            if (updateReservationDto.TotalDiscounts.HasValue)
            {
                existingReservation.TotalDiscounts = updateReservationDto.TotalDiscounts.Value;
            }

            await _reservationRepository.UpdateAsync(existingReservation);
            await _unitOfWork.SaveChangesAsync();

            // Handle reservation units update with proper change tracking
            if (updateReservationDto.ReservationUnits != null && updateReservationDto.ReservationUnits.Any())
            {
                await UpdateReservationUnitsAsync(reservationId, updateReservationDto.ReservationUnits);
            }
            else
            {
                _logger.LogDebug("UpdateReservationAsync: ReservationUnits payload empty for ReservationId={ReservationId}", reservationId);
            }

            // Do not recalculate totals; trust values sent by Zaaer
            if (updateReservationDto.Subtotal.HasValue) existingReservation.Subtotal = updateReservationDto.Subtotal.Value;
            if (updateReservationDto.VatRate.HasValue) existingReservation.VatRate = updateReservationDto.VatRate.Value;
            if (updateReservationDto.VatAmount.HasValue) existingReservation.VatAmount = updateReservationDto.VatAmount.Value;
            if (updateReservationDto.LodgingTaxRate.HasValue) existingReservation.LodgingTaxRate = updateReservationDto.LodgingTaxRate.Value;
            if (updateReservationDto.LodgingTaxAmount.HasValue) existingReservation.LodgingTaxAmount = updateReservationDto.LodgingTaxAmount.Value;
            if (updateReservationDto.TotalTaxAmount.HasValue) existingReservation.TotalTaxAmount = updateReservationDto.TotalTaxAmount.Value;
            if (updateReservationDto.TotalExtra.HasValue) existingReservation.TotalExtra = updateReservationDto.TotalExtra.Value;
            if (updateReservationDto.TotalAmount.HasValue) existingReservation.TotalAmount = updateReservationDto.TotalAmount.Value;
            if (updateReservationDto.AmountPaid.HasValue) existingReservation.AmountPaid = updateReservationDto.AmountPaid.Value;
            if (updateReservationDto.BalanceAmount.HasValue) existingReservation.BalanceAmount = updateReservationDto.BalanceAmount.Value;
            if (updateReservationDto.TotalNights.HasValue) existingReservation.TotalNights = updateReservationDto.TotalNights.Value;
            if (updateReservationDto.CheckInDate.HasValue) existingReservation.CheckInDate = updateReservationDto.CheckInDate.Value;
            if (updateReservationDto.CheckOutDate.HasValue) existingReservation.CheckOutDate = updateReservationDto.CheckOutDate.Value;
            if (updateReservationDto.DepartureDate.HasValue) existingReservation.DepartureDate = updateReservationDto.DepartureDate.Value;
            if (updateReservationDto.IsAutoExtend.HasValue) existingReservation.IsAutoExtend = updateReservationDto.IsAutoExtend.Value;
            if (updateReservationDto.PriceTypeId.HasValue) existingReservation.PriceTypeId = updateReservationDto.PriceTypeId.Value;

            await _reservationRepository.UpdateAsync(existingReservation);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await GenerateDayRatesAsync(existingReservation.ReservationId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GenerateDayRatesAsync failed for ReservationId={ReservationId} during update. Continuing without day rates.", existingReservation.ReservationId);
            }

            try
            {
                await _customerLedgerService.SyncReservationAsync(existingReservation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SyncReservationAsync failed for ReservationId={ReservationId}. Continuing without ledger update.", existingReservation.ReservationId);
            }

            // Update apartment statuses based on reservation units (outside transaction)
            // This runs after save to ensure data consistency
            try
            {
                await UpdateApartmentStatusFromReservationUnitsAsync(reservationId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "UpdateApartmentStatusFromReservationUnitsAsync failed for ReservationId={ReservationId}. Continuing without apartment status update.", reservationId);
            }

            var updateResponse = _mapper.Map<ZaaerReservationResponseDto>(existingReservation);
            // await LoadDayRatesIntoResponseAsync(updateResponse);
            return updateResponse;
        }

        public async Task<ZaaerReservationResponseDto?> UpdateReservationByNumberAsync(string reservationNo, ZaaerUpdateReservationDto updateReservationDto)
        {
            var existingReservation = await _reservationRepository.FindAsync(r => r.ReservationNo == reservationNo);
            var reservation = existingReservation.FirstOrDefault();
            
            if (reservation == null)
            {
                return null;
            }

            _mapper.Map(updateReservationDto, reservation);
            
            // Explicitly set ZaaerId and ExternalRefNo to ensure they're saved
            if (updateReservationDto.ZaaerId.HasValue)
            {
                reservation.ZaaerId = updateReservationDto.ZaaerId.Value;
            }
            if (updateReservationDto.ExternalRefNo.HasValue)
            {
                reservation.ExternalRefNo = updateReservationDto.ExternalRefNo.Value;
            }
            
            if (!string.IsNullOrWhiteSpace(updateReservationDto.RentalType))
            {
                reservation.RentalType = updateReservationDto.RentalType!;
            }
            // Update reservation type only if provided (don't set to null)
            if (!string.IsNullOrWhiteSpace(updateReservationDto.ReservationType))
            {
                reservation.ReservationType = updateReservationDto.ReservationType!;
            }
            // Ensure ReservationType is never null (database constraint)
            if (string.IsNullOrWhiteSpace(reservation.ReservationType))
            {
                reservation.ReservationType = "Individual";
            }
            // Update status only if provided (don't set to null)
            if (!string.IsNullOrWhiteSpace(updateReservationDto.Status))
            {
                reservation.Status = updateReservationDto.Status!;
            }
            // Ensure Status is never null (database constraint)
            if (string.IsNullOrWhiteSpace(reservation.Status))
            {
                reservation.Status = "Unconfirmed";
            }
            // Update number of months if provided
            if (updateReservationDto.NumberOfMonths.HasValue)
            {
                reservation.NumberOfMonths = updateReservationDto.NumberOfMonths.Value;
            }
            if (updateReservationDto.TotalPenalties.HasValue)
            {
                reservation.TotalPenalties = updateReservationDto.TotalPenalties.Value;
            }
            if (updateReservationDto.TotalDiscounts.HasValue)
            {
                reservation.TotalDiscounts = updateReservationDto.TotalDiscounts.Value;
            }
            
            await _reservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            // Handle reservation units update with proper change tracking
            if (updateReservationDto.ReservationUnits != null && updateReservationDto.ReservationUnits.Any())
            {
                await UpdateReservationUnitsAsync(reservation.ReservationId, updateReservationDto.ReservationUnits);
            }
            else
            {
                _logger.LogDebug("UpdateReservationByNumberAsync: ReservationUnits payload empty for ReservationId={ReservationId}", reservation.ReservationId);
            }

            // Trust Zaaer-provided values; do not recalculate from units
            if (updateReservationDto.Subtotal.HasValue) reservation.Subtotal = updateReservationDto.Subtotal.Value;
            if (updateReservationDto.VatRate.HasValue) reservation.VatRate = updateReservationDto.VatRate.Value;
            if (updateReservationDto.VatAmount.HasValue) reservation.VatAmount = updateReservationDto.VatAmount.Value;
            if (updateReservationDto.LodgingTaxRate.HasValue) reservation.LodgingTaxRate = updateReservationDto.LodgingTaxRate.Value;
            if (updateReservationDto.LodgingTaxAmount.HasValue) reservation.LodgingTaxAmount = updateReservationDto.LodgingTaxAmount.Value;
            if (updateReservationDto.TotalTaxAmount.HasValue) reservation.TotalTaxAmount = updateReservationDto.TotalTaxAmount.Value;
            if (updateReservationDto.TotalExtra.HasValue) reservation.TotalExtra = updateReservationDto.TotalExtra.Value;
            if (updateReservationDto.TotalAmount.HasValue) reservation.TotalAmount = updateReservationDto.TotalAmount.Value;
            if (updateReservationDto.AmountPaid.HasValue) reservation.AmountPaid = updateReservationDto.AmountPaid.Value;
            if (updateReservationDto.BalanceAmount.HasValue) reservation.BalanceAmount = updateReservationDto.BalanceAmount.Value;
            if (updateReservationDto.CheckInDate.HasValue) reservation.CheckInDate = updateReservationDto.CheckInDate.Value;
            if (updateReservationDto.CheckOutDate.HasValue) reservation.CheckOutDate = updateReservationDto.CheckOutDate.Value;
            if (updateReservationDto.DepartureDate.HasValue) reservation.DepartureDate = updateReservationDto.DepartureDate.Value;
            if (updateReservationDto.IsAutoExtend.HasValue) reservation.IsAutoExtend = updateReservationDto.IsAutoExtend.Value;
            if (updateReservationDto.PriceTypeId.HasValue) reservation.PriceTypeId = updateReservationDto.PriceTypeId.Value;

            await _reservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await GenerateDayRatesAsync(reservation.ReservationId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GenerateDayRatesAsync failed for ReservationId={ReservationId} during update by number. Continuing without day rates.", reservation.ReservationId);
            }

            try
            {
                await _customerLedgerService.SyncReservationAsync(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SyncReservationAsync failed for ReservationId={ReservationId}. Continuing without ledger update.", reservation.ReservationId);
            }

            // Update apartment statuses based on reservation units (outside transaction)
            // This runs after save to ensure data consistency
            try
            {
                await UpdateApartmentStatusFromReservationUnitsAsync(reservation.ReservationId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "UpdateApartmentStatusFromReservationUnitsAsync failed for ReservationId={ReservationId}. Continuing without apartment status update.", reservation.ReservationId);
            }

            var updateByNumberResponse = _mapper.Map<ZaaerReservationResponseDto>(reservation);
            // await LoadDayRatesIntoResponseAsync(updateByNumberResponse);
            return updateByNumberResponse;
        }

        public async Task<ZaaerReservationResponseDto?> UpdateReservationByZaaerIdAsync(int zaaerId, ZaaerUpdateReservationDto updateReservationDto)
        {
            // Find reservation by ZaaerId, optionally filtering by HotelId if provided
            var existing = await _reservationRepository.FindAsync(r => r.ZaaerId == zaaerId);
            
            // If HotelId is provided in the DTO, filter by it for multi-tenancy
            IEnumerable<Reservation> filtered = existing;
            if (updateReservationDto.HotelId.HasValue)
            {
                filtered = existing.Where(r => r.HotelId == updateReservationDto.HotelId.Value);
            }
            
            var reservation = filtered.FirstOrDefault();

            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ZaaerId {ZaaerId} not found. HotelId filter: {HotelId}", zaaerId, updateReservationDto.HotelId);
                return null;
            }

            _mapper.Map(updateReservationDto, reservation);
            
            // Explicitly set ZaaerId and ExternalRefNo to ensure they're saved
            if (updateReservationDto.ZaaerId.HasValue)
            {
                reservation.ZaaerId = updateReservationDto.ZaaerId.Value;
            }
            if (updateReservationDto.ExternalRefNo.HasValue)
            {
                reservation.ExternalRefNo = updateReservationDto.ExternalRefNo.Value;
            }
            
            if (!string.IsNullOrWhiteSpace(updateReservationDto.RentalType))
            {
                reservation.RentalType = updateReservationDto.RentalType!;
            }
            if (!string.IsNullOrWhiteSpace(updateReservationDto.ReservationType))
            {
                reservation.ReservationType = updateReservationDto.ReservationType!;
            }
            if (string.IsNullOrWhiteSpace(reservation.ReservationType))
            {
                reservation.ReservationType = "Individual";
            }
            if (!string.IsNullOrWhiteSpace(updateReservationDto.Status))
            {
                reservation.Status = updateReservationDto.Status!;
            }
            if (string.IsNullOrWhiteSpace(reservation.Status))
            {
                reservation.Status = "Unconfirmed";
            }
            if (updateReservationDto.NumberOfMonths.HasValue)
            {
                reservation.NumberOfMonths = updateReservationDto.NumberOfMonths.Value;
            }
            if (updateReservationDto.TotalPenalties.HasValue)
            {
                reservation.TotalPenalties = updateReservationDto.TotalPenalties.Value;
            }
            if (updateReservationDto.TotalDiscounts.HasValue)
            {
                reservation.TotalDiscounts = updateReservationDto.TotalDiscounts.Value;
            }

            await _reservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            if (updateReservationDto.ReservationUnits != null && updateReservationDto.ReservationUnits.Any())
            {
                await UpdateReservationUnitsAsync(reservation.ReservationId, updateReservationDto.ReservationUnits);
            }
            else
            {
                _logger.LogDebug("UpdateReservationByZaaerIdAsync: ReservationUnits payload empty for ReservationId={ReservationId}", reservation.ReservationId);
            }

            // Trust Zaaer totals; do not recompute
            if (updateReservationDto.Subtotal.HasValue) reservation.Subtotal = updateReservationDto.Subtotal.Value;
            if (updateReservationDto.VatRate.HasValue) reservation.VatRate = updateReservationDto.VatRate.Value;
            if (updateReservationDto.VatAmount.HasValue) reservation.VatAmount = updateReservationDto.VatAmount.Value;
            if (updateReservationDto.LodgingTaxRate.HasValue) reservation.LodgingTaxRate = updateReservationDto.LodgingTaxRate.Value;
            if (updateReservationDto.LodgingTaxAmount.HasValue) reservation.LodgingTaxAmount = updateReservationDto.LodgingTaxAmount.Value;
            if (updateReservationDto.TotalTaxAmount.HasValue) reservation.TotalTaxAmount = updateReservationDto.TotalTaxAmount.Value;
            if (updateReservationDto.TotalExtra.HasValue) reservation.TotalExtra = updateReservationDto.TotalExtra.Value;
            if (updateReservationDto.TotalAmount.HasValue) reservation.TotalAmount = updateReservationDto.TotalAmount.Value;
            if (updateReservationDto.AmountPaid.HasValue) reservation.AmountPaid = updateReservationDto.AmountPaid.Value;
            if (updateReservationDto.BalanceAmount.HasValue) reservation.BalanceAmount = updateReservationDto.BalanceAmount.Value;
            if (updateReservationDto.TotalNights.HasValue) reservation.TotalNights = updateReservationDto.TotalNights.Value;
            if (updateReservationDto.CheckInDate.HasValue) reservation.CheckInDate = updateReservationDto.CheckInDate.Value;
            if (updateReservationDto.CheckOutDate.HasValue) reservation.CheckOutDate = updateReservationDto.CheckOutDate.Value;
            if (updateReservationDto.DepartureDate.HasValue) reservation.DepartureDate = updateReservationDto.DepartureDate.Value;
            if (updateReservationDto.IsAutoExtend.HasValue) reservation.IsAutoExtend = updateReservationDto.IsAutoExtend.Value;
            if (updateReservationDto.PriceTypeId.HasValue) reservation.PriceTypeId = updateReservationDto.PriceTypeId.Value;

            await _reservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await GenerateDayRatesAsync(reservation.ReservationId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GenerateDayRatesAsync failed for ReservationId={ReservationId} during update by ZaaerId. Continuing without day rates.", reservation.ReservationId);
            }

            try
            {
                await _customerLedgerService.SyncReservationAsync(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SyncReservationAsync failed for ReservationId={ReservationId}. Continuing without ledger update.", reservation.ReservationId);
            }

            // Update apartment statuses based on reservation units (outside transaction)
            // This runs after save to ensure data consistency
            try
            {
                await UpdateApartmentStatusFromReservationUnitsAsync(reservation.ReservationId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "UpdateApartmentStatusFromReservationUnitsAsync failed for ReservationId={ReservationId}. Continuing without apartment status update.", reservation.ReservationId);
            }

            var response = _mapper.Map<ZaaerReservationResponseDto>(reservation);
            // await LoadDayRatesIntoResponseAsync(response);
            return response;
        }

        public async Task<ZaaerReservationResponseDto?> GetReservationByIdAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                return null;
            }

            var response = _mapper.Map<ZaaerReservationResponseDto>(reservation);
            // await LoadDayRatesIntoResponseAsync(response);
            return response;
        }

        public async Task<ZaaerReservationResponseDto?> GetReservationByNumberAsync(string reservationNo)
        {
            var reservations = await _reservationRepository.FindAsync(r => r.ReservationNo == reservationNo);
            var reservation = reservations.FirstOrDefault();
            
            if (reservation == null)
            {
                return null;
            }

            var response = _mapper.Map<ZaaerReservationResponseDto>(reservation);
            // await LoadDayRatesIntoResponseAsync(response);
            return response;
        }

        public async Task<IEnumerable<ZaaerReservationResponseDto>> GetReservationsByHotelIdAsync(int hotelId)
        {
            var reservations = await _reservationRepository.FindAsync(r => r.HotelId == hotelId);
            var responses = _mapper.Map<IEnumerable<ZaaerReservationResponseDto>>(reservations).ToList();
            
            foreach (var response in responses)
            {
                // await LoadDayRatesIntoResponseAsync(response);
            }
            
            return responses;
        }

        public async Task<bool> DeleteReservationAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                return false;
            }

            // Get reservation units
            var units = await _reservationUnitRepository.FindAsync(ru => ru.ReservationId == reservationId);
            
            // For each unit, handle related invoices first
            foreach (var unit in units)
            {
                // Find invoices related to this unit
                var invoices = await _invoiceRepository.FindAsync(i => i.UnitId == unit.UnitId);
                
                // Set UnitId to null for related invoices (soft delete approach)
                foreach (var invoice in invoices)
                {
                    invoice.UnitId = null;
                    await _invoiceRepository.UpdateAsync(invoice);
                }
                
                // Now delete the reservation unit
                await _reservationUnitRepository.DeleteAsync(unit);
            }

            // Delete reservation
            await _reservationRepository.DeleteAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Updates reservation units by trusting Zaaer-provided values (no server-side calculations).
        /// </summary>
        private async Task UpdateReservationUnitsAsync(int reservationId, List<ZaaerReservationUnitDto> newUnits)
        {
            // Get the reservation to find its ZaaerId for use in units
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            var reservationIdForUnits = reservation?.ZaaerId.HasValue == true 
                ? reservation.ZaaerId.Value 
                : reservationId;
            
            // Find existing units for this reservation (either stored by zaaer_id or internal reservation_id)
            var existingUnits = await _reservationUnitRepository.FindAsync(ru => ru.ReservationId == reservationIdForUnits || ru.ReservationId == reservationId);
            // Key by ZaaerId when available, otherwise by UnitId to keep a stable key
            var existingUnitsDict = existingUnits.ToDictionary(ru => ru.ZaaerId ?? ru.UnitId);
            var removedUnitIds = new List<int>();

            foreach (var unitDto in newUnits)
            {
                // Resolve key for matching: prefer unit zaaerId from the payload
                var matchKey = unitDto.ZaaerId ?? 0;
				var effectiveCheckOut = unitDto.DepartureDate ?? unitDto.CheckOutDate;

                // If we have a matching existing unit (by zaaerId), update it
                if (unitDto.ZaaerId.HasValue && existingUnitsDict.ContainsKey(matchKey))
                            {
                    var existingUnit = existingUnitsDict[matchKey];
                    existingUnit.ApartmentId = unitDto.ApartmentId;
                    existingUnit.CheckInDate = unitDto.CheckInDate;
                    existingUnit.CheckOutDate = unitDto.CheckOutDate;
					// Use DepartureDate from DTO if provided, otherwise fall back to CheckOutDate
					existingUnit.DepartureDate = unitDto.DepartureDate ?? unitDto.CheckOutDate;
					existingUnit.NumberOfNights = unitDto.NumberOfNights ?? (int)Math.Max(1, Math.Ceiling((effectiveCheckOut - unitDto.CheckInDate).TotalDays));
                    if (unitDto.RentAmount.HasValue) existingUnit.RentAmount = unitDto.RentAmount.Value;
                    if (unitDto.VatRate.HasValue) existingUnit.VatRate = unitDto.VatRate.Value;
                    existingUnit.VatAmount = unitDto.VatAmount;
                    if (unitDto.LodgingTaxRate.HasValue) existingUnit.LodgingTaxRate = unitDto.LodgingTaxRate.Value;
                    existingUnit.LodgingTaxAmount = unitDto.LodgingTaxAmount;
                    if (unitDto.TotalAmount.HasValue) existingUnit.TotalAmount = unitDto.TotalAmount.Value;
                    if (!string.IsNullOrWhiteSpace(unitDto.Status)) existingUnit.Status = unitDto.Status!;
                    existingUnit.ZaaerId = unitDto.ZaaerId;
                    await _reservationUnitRepository.UpdateAsync(existingUnit);
                    existingUnitsDict.Remove(matchKey);
                    }
                    else
                    {
                    var newUnit = new ReservationUnit
                    {
                        ReservationId = reservationIdForUnits,
                        ApartmentId = unitDto.ApartmentId,
                        CheckInDate = unitDto.CheckInDate,
                        CheckOutDate = unitDto.CheckOutDate,
						DepartureDate = unitDto.DepartureDate ?? unitDto.CheckOutDate,
						NumberOfNights = unitDto.NumberOfNights ?? (int)Math.Max(1, Math.Ceiling((effectiveCheckOut - unitDto.CheckInDate).TotalDays)),
                        RentAmount = unitDto.RentAmount ?? 0m,
                        VatAmount = unitDto.VatAmount,
                        LodgingTaxAmount = unitDto.LodgingTaxAmount,
                        TotalAmount = unitDto.TotalAmount ?? 0m,
                        Status = string.IsNullOrWhiteSpace(unitDto.Status) ? "Reserved" : unitDto.Status!,
                        CreatedAt = KsaTime.Now,
                        ZaaerId = unitDto.ZaaerId
                    };
                    if (unitDto.VatRate.HasValue) newUnit.VatRate = unitDto.VatRate.Value;
                    if (unitDto.LodgingTaxRate.HasValue) newUnit.LodgingTaxRate = unitDto.LodgingTaxRate.Value;
                    await _reservationUnitRepository.AddAsync(newUnit);
                }
            }

            // Any leftover in the dict are units not present in the incoming payload: delete them
            foreach (var unitToDelete in existingUnitsDict.Values)
            {
                var invoices = await _invoiceRepository.FindAsync(i => i.UnitId == unitToDelete.UnitId);
                foreach (var invoice in invoices)
                {
                    invoice.UnitId = null;
                    await _invoiceRepository.UpdateAsync(invoice);
                }
                
                await _reservationUnitRepository.DeleteAsync(unitToDelete);
                removedUnitIds.Add(unitToDelete.UnitId);
                _logger.LogInformation("UpdateReservationUnitsAsync: Removing unit UnitId={UnitId} (ReservationId={ReservationId}) and its day rates", unitToDelete.UnitId, reservationIdForUnits);
            }
            
            await _unitOfWork.SaveChangesAsync();

            // Day-rate generation handled separately after reservation update.
        }

        private async Task GenerateDayRatesAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                _logger.LogWarning("GenerateDayRatesAsync: ReservationId={ReservationId} not found.", reservationId);
                return;
            }

            var reservationIdForUnits = reservation.ZaaerId.HasValue
                ? reservation.ZaaerId.Value
                : reservationId;

            var units = await _reservationUnitRepository.FindAsync(ru =>
                ru.ReservationId == reservationIdForUnits || ru.ReservationId == reservationId);

            var unitList = units.ToList();
            if (unitList.Count == 0)
            {
                _logger.LogDebug("GenerateDayRatesAsync: No units found for ReservationId={ReservationId}", reservationId);
                await _reservationRatesService.ReplaceRatesAsync(reservationIdForUnits, Array.Empty<ZaaerReservationUnitDayRateItem>());
                return;
            }

            var items = BuildDayRateItems(reservation, unitList);
            if (items.Count == 0)
            {
                _logger.LogDebug("GenerateDayRatesAsync: Computed zero day-rate rows for ReservationId={ReservationId}", reservationId);
                await _reservationRatesService.ReplaceRatesAsync(reservationIdForUnits, Array.Empty<ZaaerReservationUnitDayRateItem>());
                return;
            }

            await _reservationRatesService.ReplaceRatesAsync(reservationIdForUnits, items);
            _logger.LogInformation("GenerateDayRatesAsync: Generated {Count} day-rate rows for ReservationId={ReservationId}", items.Count, reservationId);
        }

        private List<ZaaerReservationUnitDayRateItem> BuildDayRateItems(Reservation reservation, List<ReservationUnit> units)
        {
            var rentalType = (reservation.RentalType ?? "daily").Trim().ToLowerInvariant();
            var items = new List<ZaaerReservationUnitDayRateItem>();
            var isMonthly = rentalType == "monthly";

            foreach (var unit in units)
            {
                var nights = unit.NumberOfNights;
                if (!nights.HasValue || nights.Value <= 0)
                {
                    nights = (int)Math.Max(1, Math.Ceiling((unit.CheckOutDate - unit.CheckInDate).TotalDays));
                }

                var divisor = Math.Max(1, nights.Value);
                if (isMonthly)
                {
                    var nightDate = unit.CheckInDate.Date;
                    items.Add(CreateDayRateItem(unit, nightDate,
                        unit.TotalAmount,
                        unit.LodgingTaxAmount ?? 0m,
                        unit.VatAmount ?? 0m,
                        unit.RentAmount));
                    continue;
                }

                var perGross = Math.Round(unit.TotalAmount / divisor, 2);
                var perEwa = Math.Round((unit.LodgingTaxAmount ?? 0m) / divisor, 2);
                var perVat = Math.Round((unit.VatAmount ?? 0m) / divisor, 2);
                var perNet = Math.Round(unit.RentAmount / divisor, 2);

                var startDate = unit.CheckInDate.Date;
                for (var i = 0; i < divisor; i++)
                {
                    var nightDate = startDate.AddDays(i);
                    items.Add(CreateDayRateItem(unit, nightDate, perGross, perEwa, perVat, perNet));
                }
            }

            return items;
        }

        private static ZaaerReservationUnitDayRateItem CreateDayRateItem(ReservationUnit unit, DateTime nightDate, decimal grossRate, decimal ewaAmount, decimal vatAmount, decimal netAmount)
        {
            return new ZaaerReservationUnitDayRateItem
            {
                UnitId = unit.ApartmentId,
                NightDate = nightDate,
                GrossRate = grossRate,
                EwaAmount = ewaAmount,
                VatAmount = vatAmount,
                NetAmount = netAmount
            };
        }

        private static ZaaerUpdateReservationDto MapCreateToUpdate(ZaaerCreateReservationDto createDto)
        {
            return new ZaaerUpdateReservationDto
            {
                ZaaerId = createDto.ZaaerId,
                ReservationNo = createDto.ReservationNo,
                HotelId = createDto.HotelId,
                CustomerId = createDto.CustomerId,
                ReservationDate = createDto.ReservationDate,
                RentalType = createDto.RentalType,
                NumberOfMonths = createDto.NumberOfMonths,
                TotalPenalties = createDto.TotalPenalties,
                TotalDiscounts = createDto.TotalDiscounts,
                Subtotal = createDto.Subtotal,
                TotalTaxAmount = createDto.TotalTaxAmount,
                TotalAmount = createDto.TotalAmount,
                CorporateId = createDto.CorporateId,
                ReservationType = createDto.ReservationType,
                VisitPurposeId = createDto.VisitPurposeId,
                ReservationUnits = createDto.ReservationUnits,
                TotalNights = createDto.TotalNights,
                AmountPaid = createDto.AmountPaid,
                BalanceAmount = createDto.BalanceAmount,
                Status = createDto.Status,
                CreatedBy = createDto.CreatedBy,
                ExternalRefNo = createDto.ExternalRefNo,
                VatRate = createDto.VatRate,
                VatAmount = createDto.VatAmount,
                LodgingTaxRate = createDto.LodgingTaxRate,
                LodgingTaxAmount = createDto.LodgingTaxAmount,
                TotalExtra = createDto.TotalExtra,
                CheckInDate = createDto.CheckInDate,
                CheckOutDate = createDto.CheckOutDate,
                DepartureDate = createDto.DepartureDate,
                IsAutoExtend = createDto.IsAutoExtend,
                PriceTypeId = createDto.PriceTypeId
            };
        }

        private async Task<ZaaerReservationResponseDto?> TryHandleDuplicateOnCreateAsync(ZaaerCreateReservationDto createReservationDto, Exception ex)
        {
            if (ex is DbUpdateException dbEx && dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
            {
                _logger.LogInformation("CreateReservationAsync detected duplicate ReservationNo={ReservationNo}; attempting idempotent update.", createReservationDto.ReservationNo);
                return await UpdateReservationByNumberAsync(createReservationDto.ReservationNo, MapCreateToUpdate(createReservationDto));
            }

            return null;
        }

        /// <summary>
        /// Updates apartment status based on reservation units status.
        /// This method runs OUTSIDE of transactions to avoid blocking and ensure data consistency.
        /// Maps reservation_units.apartment_id to apartments.zaaer_id and updates apartment status accordingly.
        /// </summary>
        /// <param name="reservationId">The reservation ID (can be internal ID or zaaer_id)</param>
        private async Task UpdateApartmentStatusFromReservationUnitsAsync(int reservationId)
        {
            try
            {
                // Get reservation to determine if we should use zaaer_id or reservation_id
                var reservation = await _reservationRepository.GetByIdAsync(reservationId);
                if (reservation == null)
                {
                    _logger.LogWarning("Reservation with ID {ReservationId} not found for apartment status update", reservationId);
                    return;
                }

                // Determine which reservation_id value to use (zaaer_id or internal reservation_id)
                var reservationIdForUnits = reservation.ZaaerId.HasValue 
                    ? reservation.ZaaerId.Value 
                    : reservationId;

                // Get all reservation units for this reservation
                // Check both zaaer_id and internal reservation_id to handle all cases
                var reservationUnits = await _reservationUnitRepository.FindAsync(ru => 
                    ru.ReservationId == reservationIdForUnits || ru.ReservationId == reservationId);

                if (!reservationUnits.Any())
                {
                    _logger.LogDebug("No reservation units found for reservation {ReservationId}", reservationId);
                    return;
                }

                // Group units by apartment_id to handle multiple units for same apartment
                // Use the most recent status if multiple units exist for same apartment
                var apartmentStatusMap = reservationUnits
                    .Where(ru => ru.ApartmentId > 0) // Ensure apartment_id is valid
                    .GroupBy(ru => ru.ApartmentId)
                    .Select(g => new
                    {
                        ApartmentId = g.Key,
                        Status = g.OrderByDescending(ru => ru.CreatedAt)
                                  .FirstOrDefault()?.Status ?? "Reserved"
                    })
                    .ToList();

                // Update each apartment's status based on its reservation units
                foreach (var apartmentStatus in apartmentStatusMap)
                {
                    try
                    {
                        // Find apartment by zaaer_id (apartment_id from reservation_units maps to zaaer_id in apartments)
                        var apartments = await _unitOfWork.Apartments.FindAsync(a => a.ZaaerId == apartmentStatus.ApartmentId);
                        var apartment = apartments.FirstOrDefault();

                        if (apartment == null)
                        {
                            _logger.LogWarning(
                                "Apartment with zaaer_id {ZaaerId} not found for reservation {ReservationId}. " +
                                "Skipping apartment status update.",
                                apartmentStatus.ApartmentId, reservationId);
                            continue;
                        }

                        // Map reservation unit status to apartment status
                        string newApartmentStatus = apartmentStatus.Status.ToLowerInvariant() switch
                        {
                            "checked_in" or "checkedin" => "rented",
                            "checked_out" or "checkedout" => "vacant",
                            "cancelled" or "canceled" => "vacant",
                            "no_show" or "noshow" => "vacant",
                            _ => apartment.Status // Keep existing status for other states (Reserved, etc.)
                        };

                        // Only update if status actually changed
                        if (apartment.Status != newApartmentStatus)
                        {
                            var oldStatus = apartment.Status;
                            apartment.Status = newApartmentStatus;
                            await _unitOfWork.Apartments.UpdateAsync(apartment);
                            await _unitOfWork.SaveChangesAsync();

                            _logger.LogInformation(
                                "Updated apartment {ApartmentId} (zaaer_id: {ZaaerId}) status from '{OldStatus}' to '{NewStatus}' " +
                                "based on reservation unit status '{UnitStatus}' for reservation {ReservationId}",
                                apartment.ApartmentId, apartment.ZaaerId, oldStatus, newApartmentStatus, 
                                apartmentStatus.Status, reservationId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error updating apartment status for apartment_id {ApartmentId} in reservation {ReservationId}. " +
                            "Continuing with other apartments.",
                            apartmentStatus.ApartmentId, reservationId);
                        // Continue processing other apartments even if one fails
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in UpdateApartmentStatusFromReservationUnitsAsync for reservation {ReservationId}. " +
                    "This is a non-critical operation and will not fail the request.",
                    reservationId);
                // Don't throw - this is a background operation that shouldn't fail the main request
            }
        }

        /// <summary>
        /// Helper method to load day rates from database and populate them in the response DTO
        /// </summary>
        private async Task LoadDayRatesIntoResponseAsync(ZaaerReservationResponseDto response)
        {
            if (response?.ReservationUnits == null || !response.ReservationUnits.Any())
                return;

            try
            {
                // Get all day rates for this reservation directly from database
                // Using UnitOfWork to access the DbContext
                var reservation = await _reservationRepository.GetByIdAsync(response.ReservationId);
                if (reservation == null) return;

                // Access DbContext through a repository or directly if available
                // For now, using the service method and mapping what we can
                var allDayRates = await _reservationRatesService.GetByReservationAsync(response.ReservationId);
                var dayRatesList = allDayRates.ToList();

                // Group day rates by UnitId and populate each unit's DayRates list
                foreach (var unit in response.ReservationUnits)
                {
                    unit.DayRates = dayRatesList
                        .Where(dr => dr.UnitId == unit.UnitId)
                        .OrderBy(dr => dr.NightDate)
                        .Select(dr => new ZaaerDayRateResponseDto
                        {
                            RateId = dr.RateId,
                            UnitId = dr.UnitId,
                            NightDate = dr.NightDate,
                            GrossRate = dr.GrossRate,
                            EwaAmount = dr.EwaAmount,
                            VatAmount = dr.VatAmount,
                            NetAmount = dr.NetAmount,
                            IsManual = true, // Will be loaded from DB if service is extended
                            CreatedAt = KsaTime.Now, // Will be loaded from DB if service is extended
                            UpdatedAt = null // Will be loaded from DB if service is extended
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load day rates for reservation {ReservationId}", response.ReservationId);
                // Continue without day rates if loading fails
            }
        }
    }
}

