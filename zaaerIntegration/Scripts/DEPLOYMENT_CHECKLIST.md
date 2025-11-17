# Deployment Checklist: Apartment Status Update Feature

## 📋 Pre-Deployment Checklist

### 1. Database Backup
- [ ] **CRITICAL**: Create full database backup before running any scripts
- [ ] Verify backup is successful and accessible
- [ ] Document backup location and timestamp

### 2. Environment Check
- [ ] Confirm you're running scripts on the correct database
- [ ] Check database connection and permissions
- [ ] Verify you have CREATE INDEX and CREATE TRIGGER permissions

### 3. Timing
- [ ] **Recommended**: Run during off-peak hours (if possible)
- [ ] Notify team members if this is a production database
- [ ] Plan for potential brief performance impact during index creation

---

## 🚀 Deployment Steps (In Order)

### Step 1: Create Indexes
**Script**: `Create_Indexes_For_Apartment_Status_Update.sql`

- [ ] Review the script
- [ ] Run the script
- [ ] Verify all indexes were created successfully
- [ ] Check for any errors in the output

**Expected Time**: 
- Small database (< 10K rows): 1-5 seconds
- Medium database (10K-100K rows): 5-30 seconds
- Large database (> 100K rows): 30 seconds - 5 minutes

**What to look for**:
```
✓ Index IX_Apartments_ZaaerId created successfully.
✓ Index IX_ReservationUnits_ApartmentId created successfully.
✓ Index IX_ReservationUnits_ReservationId created successfully.
✓ Index IX_ReservationUnits_ApartmentId_Status created successfully.
✓ Index IX_Apartments_Status created successfully.
```

### Step 2: Create Trigger
**Script**: `Create_Trigger_Update_Apartment_Status_From_Reservation_Units.sql`

- [ ] Review the script
- [ ] Run the script
- [ ] Verify trigger was created successfully

**Expected Time**: < 1 second

**What to look for**:
```
Trigger TRG_Update_Apartment_Status_From_Reservation_Units created successfully.
```

### Step 3: Verify Installation
**Script**: `Test_Apartment_Status_Update.sql`

- [ ] Run the test script
- [ ] Verify all indexes exist (✓ marks)
- [ ] Verify trigger exists (✓ mark)
- [ ] Review table sizes and sample data

---

## ✅ Post-Deployment Testing

### Test 1: Application-Level Logic
1. [ ] Create a new reservation via API: `POST /api/zaaer/ZaaerReservation`
2. [ ] Check application logs for `UpdateApartmentStatusFromReservationUnitsAsync`
3. [ ] Verify apartment status was updated correctly
4. [ ] Check for any errors or warnings in logs

### Test 2: Trigger (Database-Level)
1. [ ] Find a reservation_unit with a valid apartment_id
2. [ ] Note the current apartment status
3. [ ] Update reservation_unit status to `'checked_in'`
4. [ ] Verify apartment status changed to `'rented'`
5. [ ] Update reservation_unit status to `'checked_out'`
6. [ ] Verify apartment status changed to `'vacant'`

**Test Query**:
```sql
-- Find a test unit
SELECT TOP 1 
    ru.unit_id, 
    ru.apartment_id, 
    ru.status AS UnitStatus,
    a.status AS ApartmentStatus
FROM reservation_units ru
INNER JOIN apartments a ON a.zaaer_id = ru.apartment_id
WHERE ru.apartment_id IS NOT NULL;

-- Update unit status (replace UNIT_ID with actual ID)
UPDATE reservation_units 
SET status = 'checked_in' 
WHERE unit_id = UNIT_ID;

-- Check apartment status
SELECT a.apartment_id, a.zaaer_id, a.status 
FROM apartments a
WHERE a.zaaer_id = (
    SELECT apartment_id FROM reservation_units WHERE unit_id = UNIT_ID
);
```

### Test 3: Update Reservation
1. [ ] Update an existing reservation via API: `PUT /api/zaaer/ZaaerReservation/zaaer/{zaaerId}`
2. [ ] Verify apartment statuses were updated correctly
3. [ ] Check application logs for any errors

---

## 📊 Monitoring

### Immediate (First 24 Hours)
- [ ] Monitor application logs for errors
- [ ] Check database performance (CPU, memory)
- [ ] Verify no unexpected errors in trigger execution
- [ ] Monitor API response times

### Ongoing (First Week)
- [ ] Review index usage statistics
- [ ] Check trigger execution performance
- [ ] Monitor apartment status accuracy
- [ ] Review application logs for warnings

**Check Index Usage**:
```sql
SELECT 
    i.name AS IndexName,
    s.user_seeks AS Seeks,
    s.user_scans AS Scans,
    s.user_updates AS Updates
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE s.database_id = DB_ID()
    AND i.name LIKE 'IX_%Apartment%' OR i.name LIKE 'IX_%ReservationUnit%'
ORDER BY i.name;
```

---

## 🔧 Rollback Plan (If Needed)

If you need to rollback:

1. **Drop Trigger**:
```sql
DROP TRIGGER [dbo].[TRG_Update_Apartment_Status_From_Reservation_Units];
```

2. **Drop Indexes** (optional - they don't hurt if not used):
```sql
DROP INDEX [IX_Apartments_ZaaerId] ON [dbo].[apartments];
DROP INDEX [IX_ReservationUnits_ApartmentId] ON [dbo].[reservation_units];
DROP INDEX [IX_ReservationUnits_ReservationId] ON [dbo].[reservation_units];
DROP INDEX [IX_ReservationUnits_ApartmentId_Status] ON [dbo].[reservation_units];
DROP INDEX [IX_Apartments_Status] ON [dbo].[apartments];
```

3. **Remove Application Code**:
   - Remove calls to `UpdateApartmentStatusFromReservationUnitsAsync` in `ZaaerReservationService.cs`
   - Remove the method itself

---

## 📝 Notes

- **Indexes**: Safe to keep even if you remove the feature (they don't hurt performance)
- **Trigger**: Only remove if you're sure you don't need the safety net
- **Application Code**: The method is non-blocking and won't fail requests if it errors

---

## ✅ Sign-Off

- [ ] All scripts executed successfully
- [ ] All tests passed
- [ ] Monitoring in place
- [ ] Team notified
- [ ] Documentation updated

**Deployed by**: _________________  
**Date**: _________________  
**Time**: _________________  
**Database**: _________________

