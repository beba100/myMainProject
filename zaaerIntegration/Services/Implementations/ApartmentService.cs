using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Repositories.Interfaces;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Services.Implementations
{
    /// <summary>
    /// Service implementation for Apartment operations
    /// يستخدم ITenantService للحصول على HotelId من X-Hotel-Code header
    /// </summary>
    public class ApartmentService : IApartmentService
    {
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly ILogger<ApartmentService> _logger;

        public ApartmentService(
            IApartmentRepository apartmentRepository, 
            IMapper mapper,
            ApplicationDbContext context,
            ITenantService tenantService,
            ILogger<ApartmentService> logger)
        {
            _apartmentRepository = apartmentRepository ?? throw new ArgumentNullException(nameof(apartmentRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على HotelId من Tenant (يُقرأ من X-Hotel-Code header)
        /// 1. يحصل على Tenant.Code من Master DB
        /// 2. يبحث عن HotelSettings في Tenant DB باستخدام HotelCode == Tenant.Code
        /// 3. يستخدم HotelSettings.HotelId في الاستعلامات
        /// </summary>
        private async Task<int> GetCurrentHotelIdAsync()
        {
            var tenant = _tenantService.GetTenant();
            if (tenant == null)
            {
                _logger.LogError("❌ [GetCurrentHotelIdAsync] Tenant not resolved in ApartmentService.");
                throw new UnauthorizedAccessException("Tenant not resolved. Please ensure X-Hotel-Code header is provided.");
            }

            _logger.LogInformation("🔍 [GetCurrentHotelIdAsync] Looking for HotelSettings with HotelCode='{TenantCode}' (case-insensitive)", tenant.Code);

            // ✅ البحث عن HotelSettings في Tenant DB باستخدام Tenant.Code من Master DB (case-insensitive)
            var hotelSettings = await _context.HotelSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.HotelCode != null && h.HotelCode.ToLower() == tenant.Code.ToLower());

            if (hotelSettings == null)
            {
                // ✅ DEBUG: Log all available HotelSettings
                var allHotelSettings = await _context.HotelSettings
                    .AsNoTracking()
                    .Select(h => new { h.HotelId, h.HotelCode })
                    .ToListAsync();
                
                _logger.LogError("❌ [GetCurrentHotelIdAsync] HotelSettings not found for Tenant Code: '{TenantCode}' in Tenant DB", tenant.Code);
                _logger.LogError("❌ [GetCurrentHotelIdAsync] Available HotelSettings in tenant DB: {Available}", 
                    string.Join(", ", allHotelSettings.Select(h => $"HotelId={h.HotelId}, HotelCode='{h.HotelCode}'")));
                
                throw new InvalidOperationException(
                    $"HotelSettings not found for hotel code: {tenant.Code}. " +
                    "Please ensure hotel settings are configured in the tenant database with matching HotelCode.");
            }

            _logger.LogInformation("✅ [GetCurrentHotelIdAsync] Found HotelSettings: HotelId={HotelId}, HotelCode='{HotelCode}' for Tenant Code: '{TenantCode}'", 
                hotelSettings.HotelId, hotelSettings.HotelCode, tenant.Code);
            
            // ✅ Return HotelId - but we'll use HotelCode for filtering in GetAllApartmentsAsync
            return hotelSettings.HotelId;
        }

        public async Task<(IEnumerable<ApartmentResponseDto> Apartments, int TotalCount)> GetAllApartmentsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null)
        {
            try
            {
                // Get current HotelId from X-Hotel-Code header (Tenant.Code from Master DB)
                var tenant = _tenantService.GetTenant();
                var hotelSettings = await _context.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelCode != null && h.HotelCode.ToLower() == tenant.Code.ToLower());
                
                if (hotelSettings == null)
                {
                    throw new InvalidOperationException($"HotelSettings not found for hotel code: {tenant.Code}.");
                }
                
                var hotelCode = hotelSettings.HotelCode ?? tenant.Code; // Fallback to tenant.Code if null
                
                // ✅ DEBUG: Log ALL HotelSettings in the database to see what we have
                var allHotelSettingsInDb = await _context.HotelSettings
                    .AsNoTracking()
                    .Select(h => new { h.HotelId, h.HotelCode })
                    .ToListAsync();
                _logger.LogInformation("🔍 [GetAllApartmentsAsync] ALL HotelSettings in tenant DB: {AllSettings}", 
                    string.Join(", ", allHotelSettingsInDb.Select(h => $"HotelId={h.HotelId}, HotelCode='{h.HotelCode}'")));
                
                // ✅ SOLUTION: Find ALL HotelSettings with the same HotelCode, then use ALL their HotelIds
                // This handles cases where data is linked to different HotelIds but same HotelCode
                var allHotelIdsWithSameCode = await _context.HotelSettings
                    .AsNoTracking()
                    .Where(h => h.HotelCode != null && h.HotelCode.ToLower() == hotelCode.ToLower())
                    .Select(h => h.HotelId)
                    .ToListAsync();
                
                _logger.LogInformation("📋 [GetAllApartmentsAsync] Fetching apartments for Tenant Code: {TenantCode} (HotelCode: '{HotelCode}') from Master DB, PageNumber: {PageNumber}, PageSize: {PageSize}", 
                    tenant?.Code ?? "Unknown", hotelCode, pageNumber, pageSize);
                _logger.LogInformation("🔍 [GetAllApartmentsAsync] All HotelIds with HotelCode='{HotelCode}': {HotelIds}", 
                    hotelCode, string.Join(", ", allHotelIdsWithSameCode));
                
                // ✅ CRITICAL FIX: If data is linked to HotelId=11 but we only found HotelId=1,
                // we need to also check if there's a HotelSettings with HotelId=11 that should have the same HotelCode
                // OR we need to include HotelId=11 in our search if apartments are linked to it
                // Let's check what HotelIds are actually used in the apartments table
                var hotelIdsInApartments = await _context.Apartments
                    .AsNoTracking()
                    .Select(a => a.HotelId)
                    .Distinct()
                    .ToListAsync();
                _logger.LogInformation("🔍 [GetAllApartmentsAsync] HotelIds actually used in apartments table: {HotelIds}", 
                    string.Join(", ", hotelIdsInApartments));
                
                // ✅ If apartments are linked to HotelId=11 but we're searching with HotelId=1,
                // we need to include HotelId=11 in our search
                // Check if there's a HotelSettings with HotelId=11
                var hotelSettingsWithId11 = await _context.HotelSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelId == 11);
                
                if (hotelSettingsWithId11 != null)
                {
                    _logger.LogInformation("🔍 [GetAllApartmentsAsync] Found HotelSettings with HotelId=11: HotelCode='{HotelCode}'", 
                        hotelSettingsWithId11.HotelCode);
                    
                    // ✅ If HotelId=11 has the same HotelCode, add it to the list
                    if (hotelSettingsWithId11.HotelCode != null && 
                        hotelSettingsWithId11.HotelCode.ToLower() == hotelCode.ToLower())
                    {
                        if (!allHotelIdsWithSameCode.Contains(11))
                        {
                            allHotelIdsWithSameCode.Add(11);
                            _logger.LogInformation("✅ [GetAllApartmentsAsync] Added HotelId=11 to search list (same HotelCode)");
                        }
                    }
                    // ✅ If HotelId=11 is used in apartments but has different HotelCode, still include it
                    // This handles data migration scenarios
                    else if (hotelIdsInApartments.Contains(11))
                    {
                        allHotelIdsWithSameCode.Add(11);
                        _logger.LogWarning("⚠️ [GetAllApartmentsAsync] Added HotelId=11 to search list (data exists but different HotelCode: '{DifferentCode}')", 
                            hotelSettingsWithId11.HotelCode);
                    }
                }
                else if (hotelIdsInApartments.Contains(11))
                {
                    // ✅ If HotelId=11 is used in apartments but no HotelSettings exists for it,
                    // we still need to include it in the search
                    allHotelIdsWithSameCode.Add(11);
                    _logger.LogWarning("⚠️ [GetAllApartmentsAsync] Added HotelId=11 to search list (data exists but no HotelSettings record)");
                }
                
                _logger.LogInformation("🔍 [GetAllApartmentsAsync] Final HotelIds to search: {HotelIds}", 
                    string.Join(", ", allHotelIdsWithSameCode));

                // ✅ DEBUG: Check total apartments in DB before filtering
                var totalApartmentsInDb = await _context.Apartments.CountAsync();
                var apartmentsForHotelIds = await _context.Apartments
                    .Where(a => allHotelIdsWithSameCode.Contains(a.HotelId))
                    .Select(a => new { a.ApartmentId, a.ApartmentCode, a.ApartmentName, a.HotelId, a.ZaaerId })
                    .ToListAsync();
                
                _logger.LogInformation("🔍 [GetAllApartmentsAsync] DEBUG - Total apartments in tenant DB: {TotalCount}", totalApartmentsInDb);
                _logger.LogInformation("🔍 [GetAllApartmentsAsync] DEBUG - Apartments with HotelCode='{HotelCode}' (HotelIds: {HotelIds}): {Count} apartments", 
                    hotelCode, string.Join(", ", allHotelIdsWithSameCode), apartmentsForHotelIds.Count);
                if (apartmentsForHotelIds.Any())
                {
                    _logger.LogInformation("🔍 [GetAllApartmentsAsync] DEBUG - Sample apartments: {Sample}", 
                        string.Join(", ", apartmentsForHotelIds.Take(5).Select(a => $"Id={a.ApartmentId}, Code='{a.ApartmentCode}', Name='{a.ApartmentName}', HotelId={a.HotelId}, ZaaerId={a.ZaaerId}")));
                }
                else
                {
                    // ✅ Check all HotelIds in apartments table
                    var allHotelIds = await _context.Apartments
                        .Select(a => a.HotelId)
                        .Distinct()
                        .ToListAsync();
                    _logger.LogWarning("⚠️ [GetAllApartmentsAsync] No apartments found with HotelCode='{HotelCode}' (HotelIds: {HotelIds}). Available HotelIds in DB: {AvailableHotelIds}", 
                        hotelCode, string.Join(", ", allHotelIdsWithSameCode), string.Join(", ", allHotelIds));
                }

                // ✅ Build filter: filter by ALL HotelIds that have the same HotelCode
                System.Linq.Expressions.Expression<Func<Apartment, bool>>? filter = a => allHotelIdsWithSameCode.Contains(a.HotelId);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // ✅ Only search in direct properties to avoid EF Core Include issues
                    // Navigation properties will be loaded via Include, but we can't use them in Where clause
                    filter = a => allHotelIdsWithSameCode.Contains(a.HotelId) && (
                        a.ApartmentName != null && a.ApartmentName.Contains(searchTerm) ||
                        a.ApartmentCode != null && a.ApartmentCode.Contains(searchTerm) ||
                        a.Status != null && a.Status.Contains(searchTerm)
                    );
                    _logger.LogInformation("🔍 [GetAllApartmentsAsync] Using search filter with term: {SearchTerm}", searchTerm);
                }

                var (apartments, totalCount) = await _apartmentRepository.GetPagedAsync(
                    pageNumber, 
                    pageSize, 
                    filter);

                _logger.LogInformation("✅ [GetAllApartmentsAsync] Successfully retrieved {Count} apartments (Total: {TotalCount}) for HotelCode: '{HotelCode}' (HotelIds: {HotelIds})", 
                    apartments.Count(), totalCount, hotelCode, string.Join(", ", allHotelIdsWithSameCode));

                // ✅ DEBUG: Log raw apartments before mapping
                if (apartments.Any())
                {
                    _logger.LogInformation("🔍 [GetAllApartmentsAsync] DEBUG - Raw apartments before mapping: {Count} apartments", apartments.Count());
                    var sampleApartments = apartments.Take(3).Select(a => $"Id={a.ApartmentId}, Code='{a.ApartmentCode}', Name='{a.ApartmentName}', HotelId={a.HotelId}, ZaaerId={a.ZaaerId}");
                    _logger.LogInformation("🔍 [GetAllApartmentsAsync] DEBUG - Sample raw apartments: {Sample}", string.Join(", ", sampleApartments));
                }
                else if (totalCount > 0)
                {
                    _logger.LogWarning("⚠️ [GetAllApartmentsAsync] WARNING: totalCount={TotalCount} but apartments.Count=0. This may indicate a filter or Include issue.", totalCount);
                }

                var apartmentDtos = _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
                
                // ✅ DEBUG: Log mapped DTOs
                if (apartmentDtos.Any())
                {
                    _logger.LogInformation("🔍 [GetAllApartmentsAsync] DEBUG - Mapped DTOs: {Count} DTOs", apartmentDtos.Count());
                    _logger.LogInformation("🔍 [GetAllApartmentsAsync] DEBUG - Mapped DTOs sample: {Sample}", 
                        string.Join(", ", apartmentDtos.Take(3).Select(dto => $"Id={dto.ApartmentId}, Code='{dto.ApartmentCode}', Name='{dto.ApartmentName}', ZaaerId={dto.ZaaerId}")));
                }
                else if (apartments.Any())
                {
                    _logger.LogWarning("⚠️ [GetAllApartmentsAsync] WARNING: {Count} raw apartments but 0 DTOs after mapping. Mapping may have failed.", apartments.Count());
                }
                
                return (apartmentDtos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetAllApartmentsAsync] Error retrieving apartments: {Message}", ex.Message);
                throw new InvalidOperationException($"Error retrieving apartments: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentLookupDto>> GetApartmentLookupAsync()
        {
            try
            {
                var tenant = _tenantService.GetTenant();
                if (tenant == null)
                {
                    throw new UnauthorizedAccessException("Tenant not resolved. Please ensure X-Hotel-Code header is provided.");
                }

                var tenantCode = tenant.Code ?? throw new InvalidOperationException("Tenant code is missing.");

                var hotelSettingsSnapshot = await _context.HotelSettings
                    .AsNoTracking()
                    .ToListAsync();

                var matchingSettings = hotelSettingsSnapshot
                    .Where(h => !string.IsNullOrWhiteSpace(h.HotelCode) && h.HotelCode!.Equals(tenantCode, StringComparison.OrdinalIgnoreCase))
                    .Select(h => new { h.HotelId, h.HotelCode })
                    .ToList();

                if (matchingSettings.Count == 0)
                {
                    throw new InvalidOperationException($"HotelSettings not found for hotel code: {tenantCode}.");
                }

                var hotelCode = matchingSettings.First().HotelCode ?? tenantCode;
                var hotelIds = new HashSet<int>(matchingSettings.Select(h => h.HotelId));

                // Include any additional HotelSettings entries that share the same HotelCode (case insensitive)
                foreach (var id in hotelSettingsSnapshot
                             .Where(h => !string.IsNullOrWhiteSpace(h.HotelCode) && h.HotelCode!.Equals(hotelCode, StringComparison.OrdinalIgnoreCase))
                             .Select(h => h.HotelId))
                {
                    hotelIds.Add(id);
                }

                // Safety net for data that might be linked to other HotelIds (legacy records)
                var distinctHotelIdsInApartments = await _context.Apartments
                    .AsNoTracking()
                    .Select(a => a.HotelId)
                    .Distinct()
                    .ToListAsync();

                // If any apartment uses a HotelId that shares the same code but was missed above, include it
                foreach (var apartmentHotelId in distinctHotelIdsInApartments)
                {
                    if (hotelIds.Contains(apartmentHotelId))
                    {
                        continue;
                    }

                    var matchingSetting = hotelSettingsSnapshot.FirstOrDefault(h => h.HotelId == apartmentHotelId);

                    if (matchingSetting != null &&
                        !string.IsNullOrWhiteSpace(matchingSetting.HotelCode) &&
                        matchingSetting.HotelCode!.Equals(hotelCode, StringComparison.OrdinalIgnoreCase))
                    {
                        hotelIds.Add(apartmentHotelId);
                    }
                }

                var lookups = await _context.Apartments
                    .AsNoTracking()
                    .Where(a => hotelIds.Contains(a.HotelId))
                    .OrderBy(a => a.ApartmentCode)
                    .Select(a => new ApartmentLookupDto
                    {
                        ApartmentId = a.ApartmentId,
                        ZaaerId = a.ZaaerId,
                        ApartmentCode = a.ApartmentCode,
                        ApartmentName = a.ApartmentName,
                        HotelId = a.HotelId,
                        Status = a.Status
                    })
                    .ToListAsync();

                _logger.LogInformation("✅ [GetApartmentLookupAsync] Returned {Count} apartments for HotelCode '{HotelCode}' (Tenant: {TenantCode})",
                    lookups.Count, hotelCode, tenantCode);

                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetApartmentLookupAsync] Error retrieving apartment lookup list: {Message}", ex.Message);
                throw new InvalidOperationException($"Error retrieving apartment lookup list: {ex.Message}", ex);
            }
        }

        public async Task<ApartmentResponseDto?> GetApartmentByIdAsync(int id)
        {
            try
            {
                var apartment = await _apartmentRepository.GetWithDetailsAsync(id);
                return apartment != null ? _mapper.Map<ApartmentResponseDto>(apartment) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<ApartmentResponseDto?> GetApartmentByCodeAsync(string apartmentCode)
        {
            try
            {
                var apartment = await _apartmentRepository.GetByApartmentCodeAsync(apartmentCode);
                return apartment != null ? _mapper.Map<ApartmentResponseDto>(apartment) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment with code {apartmentCode}: {ex.Message}", ex);
            }
        }

        public async Task<ApartmentResponseDto> CreateApartmentAsync(CreateApartmentDto createApartmentDto)
        {
            try
            {
                // Check if apartment code already exists
                if (await _apartmentRepository.ApartmentCodeExistsAsync(createApartmentDto.ApartmentCode))
                {
                    throw new InvalidOperationException($"Apartment with code '{createApartmentDto.ApartmentCode}' already exists.");
                }

                var apartment = _mapper.Map<Apartment>(createApartmentDto);

                var createdApartment = await _apartmentRepository.AddAsync(apartment);
                return _mapper.Map<ApartmentResponseDto>(createdApartment);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating apartment: {ex.Message}", ex);
            }
        }

        public async Task<ApartmentResponseDto?> UpdateApartmentAsync(int id, UpdateApartmentDto updateApartmentDto)
        {
            try
            {
                var existingApartment = await _apartmentRepository.GetByIdAsync(id);
                if (existingApartment == null)
                {
                    return null;
                }

                // Check if apartment code already exists (excluding current apartment)
                if (await _apartmentRepository.ApartmentCodeExistsAsync(updateApartmentDto.ApartmentCode, id))
                {
                    throw new InvalidOperationException($"Apartment with code '{updateApartmentDto.ApartmentCode}' already exists.");
                }

                _mapper.Map(updateApartmentDto, existingApartment);

                await _apartmentRepository.UpdateAsync(existingApartment);

                return _mapper.Map<ApartmentResponseDto>(existingApartment);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating apartment with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteApartmentAsync(int id)
        {
            try
            {
                var apartment = await _apartmentRepository.GetByIdAsync(id);
                if (apartment == null)
                {
                    return false;
                }

                await _apartmentRepository.DeleteAsync(apartment);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting apartment with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelIdAsync(int hotelId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithDetailsByHotelIdAsync(hotelId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments for hotel {hotelId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByBuildingIdAsync(int buildingId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithDetailsByBuildingIdAsync(buildingId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments for building {buildingId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByFloorIdAsync(int floorId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithDetailsByFloorIdAsync(floorId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments for floor {floorId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByRoomTypeIdAsync(int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithDetailsByRoomTypeIdAsync(roomTypeId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments for room type {roomTypeId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByStatusAsync(string status)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByStatusAsync(status);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by status {status}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetAvailableApartmentsAsync()
        {
            try
            {
                var apartments = await _apartmentRepository.GetAvailableAsync();
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving available apartments: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetOccupiedApartmentsAsync()
        {
            try
            {
                var apartments = await _apartmentRepository.GetOccupiedAsync();
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving occupied apartments: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetMaintenanceApartmentsAsync()
        {
            try
            {
                var apartments = await _apartmentRepository.GetMaintenanceAsync();
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving maintenance apartments: {ex.Message}", ex);
            }
        }

        public async Task<object> GetApartmentStatisticsAsync()
        {
            try
            {
                return await _apartmentRepository.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment statistics: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByNameAsync(string name)
        {
            try
            {
                var apartments = await _apartmentRepository.SearchByNameAsync(name);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByCodeAsync(string code)
        {
            try
            {
                var apartments = await _apartmentRepository.SearchByCodeAsync(code);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by code: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByHotelNameAsync(string hotelName)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByHotelNameAsync(hotelName);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by hotel name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByBuildingNameAsync(string buildingName)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByBuildingNameAsync(buildingName);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by building name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByFloorNameAsync(string floorName)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByFloorNameAsync(floorName);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by floor name: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> SearchApartmentsByRoomTypeNameAsync(string roomTypeName)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByRoomTypeNameAsync(roomTypeName);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching apartments by room type name: {ex.Message}", ex);
            }
        }

        public async Task<bool> ApartmentCodeExistsAsync(string apartmentCode, int? excludeId = null)
        {
            try
            {
                return await _apartmentRepository.ApartmentCodeExistsAsync(apartmentCode, excludeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking apartment code existence: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsWithReservationsAsync()
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithReservationsAsync();
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments with reservations: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsWithoutReservationsAsync()
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithoutReservationsAsync();
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments without reservations: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByReservationCountRangeAsync(int minCount, int maxCount)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByReservationCountRangeAsync(minCount, maxCount);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by reservation count range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetTopApartmentsByReservationCountAsync(int topCount = 10)
        {
            try
            {
                var apartments = await _apartmentRepository.GetTopByReservationCountAsync(topCount);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top apartments by reservation count: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByRevenueRangeAsync(decimal minRevenue, decimal maxRevenue)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByRevenueRangeAsync(minRevenue, maxRevenue);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by revenue range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetTopApartmentsByRevenueAsync(int topCount = 10)
        {
            try
            {
                var apartments = await _apartmentRepository.GetTopByRevenueAsync(topCount);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving top apartments by revenue: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetAvailableApartmentsForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var apartments = await _apartmentRepository.GetAvailableForDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving available apartments for date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsWithOverlappingReservationsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var apartments = await _apartmentRepository.GetWithOverlappingReservationsAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments with overlapping reservations: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelAndBuildingAsync(int hotelId, int buildingId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByHotelAndBuildingAsync(hotelId, buildingId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by hotel and building: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelAndFloorAsync(int hotelId, int floorId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByHotelAndFloorAsync(hotelId, floorId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by hotel and floor: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByHotelAndRoomTypeAsync(int hotelId, int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByHotelAndRoomTypeAsync(hotelId, roomTypeId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by hotel and room type: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByBuildingAndFloorAsync(int buildingId, int floorId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByBuildingAndFloorAsync(buildingId, floorId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by building and floor: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByBuildingAndRoomTypeAsync(int buildingId, int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByBuildingAndRoomTypeAsync(buildingId, roomTypeId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by building and room type: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByFloorAndRoomTypeAsync(int floorId, int roomTypeId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByFloorAndRoomTypeAsync(floorId, roomTypeId);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by floor and room type: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ApartmentResponseDto>> GetApartmentsByMultipleCriteriaAsync(int? hotelId = null, int? buildingId = null, int? floorId = null, int? roomTypeId = null, string? status = null)
        {
            try
            {
                var apartments = await _apartmentRepository.GetByMultipleCriteriaAsync(hotelId, buildingId, floorId, roomTypeId, status);
                return _mapper.Map<IEnumerable<ApartmentResponseDto>>(apartments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartments by multiple criteria: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetApartmentOccupancyRateAsync(int apartmentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _apartmentRepository.GetOccupancyRateAsync(apartmentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment occupancy rate: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetApartmentRevenueAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _apartmentRepository.GetRevenueAsync(apartmentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment revenue: {ex.Message}", ex);
            }
        }

        public async Task<int> GetApartmentReservationCountAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _apartmentRepository.GetReservationCountAsync(apartmentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment reservation count: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetApartmentAverageStayDurationAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _apartmentRepository.GetAverageStayDurationAsync(apartmentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment average stay duration: {ex.Message}", ex);
            }
        }

        public async Task<object> GetApartmentUtilizationStatisticsAsync(int apartmentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _apartmentRepository.GetUtilizationStatisticsAsync(apartmentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving apartment utilization statistics: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateApartmentStatusAsync(int id, string status)
        {
            try
            {
                var apartment = await _apartmentRepository.GetByIdAsync(id);
                if (apartment == null)
                {
                    return false;
                }

                apartment.Status = status;
                await _apartmentRepository.UpdateAsync(apartment);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating apartment status: {ex.Message}", ex);
            }
        }

        public async Task<bool> CheckApartmentAvailabilityAsync(int apartmentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var availableApartments = await _apartmentRepository.GetAvailableForDateRangeAsync(startDate, endDate);
                return availableApartments.Any(a => a.ApartmentId == apartmentId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking apartment availability: {ex.Message}", ex);
            }
        }
    }
}
