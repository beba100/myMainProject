-- =============================================
-- Test Script: Apartment Status Update
-- =============================================
-- This script helps verify that the apartment status update functionality works correctly
-- Run this AFTER creating the indexes and trigger
-- =============================================

PRINT '========================================';
PRINT 'Testing Apartment Status Update Functionality';
PRINT '========================================';
PRINT '';

-- =============================================
-- Step 1: Check Indexes Exist
-- =============================================
PRINT 'Step 1: Verifying indexes exist...';
PRINT '';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Apartments_ZaaerId' AND object_id = OBJECT_ID('dbo.apartments'))
    PRINT '✓ IX_Apartments_ZaaerId exists';
ELSE
    PRINT '✗ IX_Apartments_ZaaerId MISSING';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ApartmentId' AND object_id = OBJECT_ID('dbo.reservation_units'))
    PRINT '✓ IX_ReservationUnits_ApartmentId exists';
ELSE
    PRINT '✗ IX_ReservationUnits_ApartmentId MISSING';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ReservationId' AND object_id = OBJECT_ID('dbo.reservation_units'))
    PRINT '✓ IX_ReservationUnits_ReservationId exists';
ELSE
    PRINT '✗ IX_ReservationUnits_ReservationId MISSING';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ApartmentId_Status' AND object_id = OBJECT_ID('dbo.reservation_units'))
    PRINT '✓ IX_ReservationUnits_ApartmentId_Status exists';
ELSE
    PRINT '✗ IX_ReservationUnits_ApartmentId_Status MISSING';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Apartments_Status' AND object_id = OBJECT_ID('dbo.apartments'))
    PRINT '✓ IX_Apartments_Status exists';
ELSE
    PRINT '✗ IX_Apartments_Status MISSING';

PRINT '';
GO

-- =============================================
-- Step 2: Check Trigger Exists
-- =============================================
PRINT 'Step 2: Verifying trigger exists...';
PRINT '';

IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TRG_Update_Apartment_Status_From_Reservation_Units')
    PRINT '✓ Trigger TRG_Update_Apartment_Status_From_Reservation_Units exists';
ELSE
    PRINT '✗ Trigger TRG_Update_Apartment_Status_From_Reservation_Units MISSING';

PRINT '';
GO

-- =============================================
-- Step 3: Check Table Sizes (for performance reference)
-- =============================================
PRINT 'Step 3: Checking table sizes...';
PRINT '';

SELECT 
    'apartments' AS TableName,
    COUNT(*) AS RowCount,
    SUM(CASE WHEN zaaer_id IS NOT NULL THEN 1 ELSE 0 END) AS RowsWithZaaerId
FROM apartments;

SELECT 
    'reservation_units' AS TableName,
    COUNT(*) AS RowCount,
    SUM(CASE WHEN apartment_id IS NOT NULL AND apartment_id > 0 THEN 1 ELSE 0 END) AS RowsWithApartmentId,
    COUNT(DISTINCT apartment_id) AS DistinctApartmentIds
FROM reservation_units;

PRINT '';
GO

-- =============================================
-- Step 4: Sample Data Check
-- =============================================
PRINT 'Step 4: Checking sample relationships...';
PRINT '';

-- Check if there are any reservation_units with matching apartments
SELECT TOP 5
    ru.unit_id,
    ru.reservation_id,
    ru.apartment_id AS ReservationUnitApartmentId,
    ru.status AS ReservationUnitStatus,
    a.apartment_id,
    a.zaaer_id AS ApartmentZaaerId,
    a.status AS ApartmentStatus
FROM reservation_units ru
LEFT JOIN apartments a ON a.zaaer_id = ru.apartment_id
WHERE ru.apartment_id IS NOT NULL AND ru.apartment_id > 0
ORDER BY ru.unit_id DESC;

PRINT '';
GO

-- =============================================
-- Step 5: Test Trigger (Manual Test)
-- =============================================
PRINT 'Step 5: Manual Trigger Test Instructions';
PRINT '';
PRINT 'To test the trigger manually:';
PRINT '1. Find a reservation_unit with apartment_id that matches an apartment zaaer_id';
PRINT '2. Note the current apartment status';
PRINT '3. Update the reservation_unit status to "checked_in"';
PRINT '4. Check if apartment status changed to "rented"';
PRINT '5. Update the reservation_unit status to "checked_out"';
PRINT '6. Check if apartment status changed to "vacant"';
PRINT '';
PRINT 'Example test query:';
PRINT '-- First, find a test unit';
PRINT 'SELECT TOP 1 unit_id, apartment_id, status FROM reservation_units WHERE apartment_id IN (SELECT zaaer_id FROM apartments WHERE zaaer_id IS NOT NULL);';
PRINT '';
PRINT '-- Then update it (replace UNIT_ID with actual unit_id)';
PRINT 'UPDATE reservation_units SET status = ''checked_in'' WHERE unit_id = UNIT_ID;';
PRINT 'SELECT apartment_id, status FROM apartments WHERE zaaer_id = (SELECT apartment_id FROM reservation_units WHERE unit_id = UNIT_ID);';
PRINT '';
GO

-- =============================================
-- Step 6: Index Usage Statistics (if available)
-- =============================================
PRINT 'Step 6: Index usage statistics (run after some usage)...';
PRINT '';

SELECT 
    i.name AS IndexName,
    s.user_seeks AS Seeks,
    s.user_scans AS Scans,
    s.user_lookups AS Lookups,
    s.user_updates AS Updates,
    s.last_user_seek AS LastSeek,
    s.last_user_scan AS LastScan
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE s.database_id = DB_ID()
    AND i.name IN (
        'IX_Apartments_ZaaerId',
        'IX_ReservationUnits_ApartmentId',
        'IX_ReservationUnits_ReservationId',
        'IX_ReservationUnits_ApartmentId_Status',
        'IX_Apartments_Status'
    )
ORDER BY i.name;

PRINT '';
GO

PRINT '========================================';
PRINT 'Test script completed!';
PRINT '========================================';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Verify all indexes and trigger exist (✓ marks above)';
PRINT '2. Test the trigger manually using the instructions in Step 5';
PRINT '3. Monitor index usage statistics after some production usage';
PRINT '4. Check application logs for any errors in UpdateApartmentStatusFromReservationUnitsAsync';
PRINT '';
GO

