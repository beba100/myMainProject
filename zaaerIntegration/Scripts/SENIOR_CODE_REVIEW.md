# Senior-Level Code Review: UpdateApartmentStatusFromReservationUnitsAsync

## ✅ What We Did Right (Senior-Level Practices)

### 1. **Separation of Concerns** ✅
- **Good**: Method is focused on a single responsibility (updating apartment status)
- **Good**: Runs outside transactions to avoid blocking
- **Good**: Non-blocking - won't fail main request if it errors

### 2. **Error Handling** ✅
- **Good**: Try-catch at method level prevents failures
- **Good**: Try-catch at apartment level allows processing to continue
- **Good**: Proper logging at different levels (Warning, Information, Error)
- **Good**: Errors don't propagate to caller

### 3. **Transaction Management** ✅
- **Excellent**: Runs OUTSIDE transactions (critical for performance)
- **Good**: Each apartment update is in its own save operation
- **Good**: Won't block main reservation transaction

### 4. **Edge Case Handling** ✅
- **Good**: Handles missing reservations
- **Good**: Handles missing apartments
- **Good**: Handles multiple units for same apartment (uses most recent)
- **Good**: Validates apartment_id > 0
- **Good**: Handles both zaaer_id and reservation_id scenarios

### 5. **Logging** ✅
- **Good**: Appropriate log levels (Debug, Warning, Information, Error)
- **Good**: Structured logging with context (ReservationId, ApartmentId, etc.)
- **Good**: Logs status changes for audit trail

### 6. **Performance Considerations** ✅
- **Good**: Uses FindAsync instead of GetAllAsync
- **Good**: Groups units efficiently
- **Good**: Only updates if status actually changed

---

## ⚠️ Areas for Improvement (Senior-Level Refinements)

### 1. **N+1 Query Problem** ⚠️
**Current Issue**:
```csharp
foreach (var apartmentStatus in apartmentStatusMap)
{
    var apartments = await _unitOfWork.Apartments.FindAsync(...); // Query per apartment
}
```

**Senior Solution**: Batch load all apartments in one query
```csharp
// Get all apartment IDs
var apartmentIds = apartmentStatusMap.Select(a => a.ApartmentId).ToList();

// Load all apartments in one query
var apartments = await _unitOfWork.Apartments.FindAsync(a => 
    apartmentIds.Contains(a.ZaaerId ?? 0));

// Create lookup dictionary
var apartmentDict = apartments.ToDictionary(a => a.ZaaerId ?? 0);
```

**Impact**: Reduces N queries to 1 query (significant performance improvement)

---

### 2. **Status Mapping Logic** ⚠️
**Current**: Hardcoded switch statement in method

**Senior Solution**: Extract to a service/helper class
```csharp
public interface IApartmentStatusMapper
{
    string MapReservationUnitStatusToApartmentStatus(string unitStatus, string currentApartmentStatus);
}

public class ApartmentStatusMapper : IApartmentStatusMapper
{
    private static readonly Dictionary<string, string> StatusMap = new()
    {
        { "checked_in", "rented" },
        { "checkedin", "rented" },
        { "checked_out", "vacant" },
        { "checkedout", "vacant" },
        { "cancelled", "vacant" },
        { "canceled", "vacant" },
        { "no_show", "vacant" },
        { "noshow", "vacant" }
    };

    public string MapReservationUnitStatusToApartmentStatus(string unitStatus, string currentApartmentStatus)
    {
        if (string.IsNullOrWhiteSpace(unitStatus))
            return currentApartmentStatus;

        var normalizedStatus = unitStatus.ToLowerInvariant().Trim();
        return StatusMap.TryGetValue(normalizedStatus, out var mappedStatus) 
            ? mappedStatus 
            : currentApartmentStatus;
    }
}
```

**Benefits**:
- Testable in isolation
- Reusable across codebase
- Easy to modify business rules
- Can be configured from database/config

---

### 3. **Multiple SaveChanges Calls** ⚠️
**Current**: Calls `SaveChangesAsync()` for each apartment

**Senior Solution**: Batch all updates, then single SaveChanges
```csharp
var apartmentsToUpdate = new List<Apartment>();

foreach (var apartmentStatus in apartmentStatusMap)
{
    // ... find apartment ...
    if (apartment.Status != newApartmentStatus)
    {
        apartment.Status = newApartmentStatus;
        apartmentsToUpdate.Add(apartment);
    }
}

// Single save for all updates
if (apartmentsToUpdate.Any())
{
    foreach (var apt in apartmentsToUpdate)
    {
        await _unitOfWork.Apartments.UpdateAsync(apt);
    }
    await _unitOfWork.SaveChangesAsync();
}
```

**Impact**: Reduces database round-trips from N to 1

---

### 4. **Magic Strings** ⚠️
**Current**: Hardcoded status strings like "rented", "vacant", "checked_in"

**Senior Solution**: Use constants or enums
```csharp
public static class ApartmentStatus
{
    public const string Rented = "rented";
    public const string Vacant = "vacant";
    public const string Available = "available";
    // ...
}

public static class ReservationUnitStatus
{
    public const string CheckedIn = "checked_in";
    public const string CheckedOut = "checked_out";
    // ...
}
```

**Benefits**: 
- Compile-time safety
- Refactoring-friendly
- No typos
- IDE autocomplete

---

### 5. **Method Complexity** ⚠️
**Current**: Method does multiple things (fetch, map, update)

**Senior Solution**: Break into smaller methods
```csharp
private async Task UpdateApartmentStatusFromReservationUnitsAsync(int reservationId)
{
    var reservationUnits = await GetReservationUnitsForReservationAsync(reservationId);
    if (!reservationUnits.Any()) return;

    var apartmentStatusMap = BuildApartmentStatusMap(reservationUnits);
    await UpdateApartmentsStatusAsync(apartmentStatusMap, reservationId);
}

private async Task<IEnumerable<ReservationUnit>> GetReservationUnitsForReservationAsync(int reservationId) { }
private Dictionary<int, string> BuildApartmentStatusMap(IEnumerable<ReservationUnit> units) { }
private async Task UpdateApartmentsStatusAsync(Dictionary<int, string> statusMap, int reservationId) { }
```

**Benefits**: 
- Easier to test
- Easier to understand
- Single Responsibility Principle

---

### 6. **Dependency Injection** ⚠️
**Current**: Uses `_unitOfWork` directly

**Senior Solution**: Inject `IApartmentRepository` directly
```csharp
private readonly IApartmentRepository _apartmentRepository;

// Then use:
var apartments = await _apartmentRepository.FindAsync(...);
```

**Benefits**: 
- Better testability
- Clearer dependencies
- Follows Dependency Inversion Principle

---

### 7. **Async/Await Best Practices** ⚠️
**Current**: Uses `Any()` on IEnumerable (synchronous)

**Senior Solution**: Use async-compatible methods
```csharp
// Instead of:
if (!reservationUnits.Any())

// Use:
if (!await reservationUnits.AnyAsync()) // If available
// Or:
var unitsList = reservationUnits.ToList();
if (!unitsList.Any())
```

---

## 📊 Overall Assessment

### Current Level: **Mid-to-Senior** ✅

**Strengths**:
- ✅ Good error handling
- ✅ Transaction safety
- ✅ Edge case handling
- ✅ Proper logging
- ✅ Non-blocking design

**Areas for Growth**:
- ⚠️ Performance optimization (N+1 queries)
- ⚠️ Code organization (extract logic)
- ⚠️ Testability (dependency injection)
- ⚠️ Maintainability (magic strings, complexity)

---

## 🎯 Senior-Level Refactored Version

Here's how a senior developer might refactor it:

```csharp
private readonly IApartmentStatusMapper _statusMapper;
private readonly IApartmentRepository _apartmentRepository;

private async Task UpdateApartmentStatusFromReservationUnitsAsync(int reservationId)
{
    try
    {
        var reservationUnits = await GetReservationUnitsForReservationAsync(reservationId);
        if (!reservationUnits.Any()) return;

        var apartmentStatusMap = BuildApartmentStatusMap(reservationUnits);
        await UpdateApartmentsStatusBatchAsync(apartmentStatusMap, reservationId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating apartment status for reservation {ReservationId}", reservationId);
    }
}

private async Task<IEnumerable<ReservationUnit>> GetReservationUnitsForReservationAsync(int reservationId)
{
    var reservation = await _reservationRepository.GetByIdAsync(reservationId);
    if (reservation == null)
    {
        _logger.LogWarning("Reservation {ReservationId} not found", reservationId);
        return Enumerable.Empty<ReservationUnit>();
    }

    var reservationIdForUnits = reservation.ZaaerId ?? reservationId;
    return await _reservationUnitRepository.FindAsync(ru => 
        ru.ReservationId == reservationIdForUnits || ru.ReservationId == reservationId);
}

private Dictionary<int, string> BuildApartmentStatusMap(IEnumerable<ReservationUnit> units)
{
    return units
        .Where(ru => ru.ApartmentId > 0)
        .GroupBy(ru => ru.ApartmentId)
        .ToDictionary(
            g => g.Key,
            g => g.OrderByDescending(ru => ru.CreatedAt).First().Status
        );
}

private async Task UpdateApartmentsStatusBatchAsync(
    Dictionary<int, string> apartmentStatusMap, 
    int reservationId)
{
    var apartmentIds = apartmentStatusMap.Keys.ToList();
    var apartments = await _apartmentRepository.FindAsync(a => 
        apartmentIds.Contains(a.ZaaerId ?? 0));
    
    var apartmentDict = apartments.ToDictionary(a => a.ZaaerId ?? 0);
    var apartmentsToUpdate = new List<Apartment>();

    foreach (var (apartmentId, unitStatus) in apartmentStatusMap)
    {
        if (!apartmentDict.TryGetValue(apartmentId, out var apartment))
        {
            _logger.LogWarning("Apartment zaaer_id {ZaaerId} not found", apartmentId);
            continue;
        }

        var newStatus = _statusMapper.MapReservationUnitStatusToApartmentStatus(
            unitStatus, 
            apartment.Status);

        if (apartment.Status != newStatus)
        {
            apartment.Status = newStatus;
            apartmentsToUpdate.Add(apartment);
        }
    }

    if (apartmentsToUpdate.Any())
    {
        foreach (var apt in apartmentsToUpdate)
        {
            await _apartmentRepository.UpdateAsync(apt);
        }
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation(
            "Updated {Count} apartment statuses for reservation {ReservationId}",
            apartmentsToUpdate.Count, reservationId);
    }
}
```

---

## ✅ Conclusion

**Your current implementation**: **7/10** (Mid-to-Senior level)

**What makes it senior**:
- ✅ Error handling
- ✅ Transaction safety
- ✅ Non-blocking design
- ✅ Edge case handling

**What would make it more senior**:
- ⚠️ Performance optimization (batch queries)
- ⚠️ Code organization (extract methods/services)
- ⚠️ Testability improvements

**Verdict**: Your code is **solid and production-ready**. The improvements above are **optimizations**, not fixes. The current implementation works well and follows good practices. The refactored version would be more maintainable and performant, but your current code is definitely senior-level work! 🎯

