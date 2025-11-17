-- =============================================
-- Verify Trigger and Indexes Status
-- =============================================
-- Run this to check if trigger and indexes are properly set up
-- =============================================

PRINT '========================================';
PRINT 'Verifying Trigger and Indexes Status';
PRINT '========================================';
PRINT '';

-- =============================================
-- Check Trigger Status
-- =============================================
PRINT '=== TRIGGER STATUS ===';
PRINT '';

IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TRG_Update_Apartment_Status_From_Reservation_Units')
BEGIN
    DECLARE @TriggerEnabled BIT;
    SELECT @TriggerEnabled = is_disabled FROM sys.triggers WHERE name = 'TRG_Update_Apartment_Status_From_Reservation_Units';
    
    IF @TriggerEnabled = 0
        PRINT '✓ Trigger TRG_Update_Apartment_Status_From_Reservation_Units EXISTS and is ENABLED';
    ELSE
        PRINT '⚠ Trigger TRG_Update_Apartment_Status_From_Reservation_Units EXISTS but is DISABLED';
END
ELSE
BEGIN
    PRINT '✗ Trigger TRG_Update_Apartment_Status_From_Reservation_Units DOES NOT EXIST';
    PRINT '  → Run: Create_Trigger_Update_Apartment_Status_From_Reservation_Units.sql';
END

PRINT '';
GO

-- =============================================
-- Check Indexes Status
-- =============================================
PRINT '=== INDEXES STATUS ===';
PRINT '';

-- Check each index
DECLARE @IndexCount INT = 0;
DECLARE @MissingIndexes NVARCHAR(MAX) = '';

-- IX_Apartments_ZaaerId
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Apartments_ZaaerId' AND object_id = OBJECT_ID('dbo.apartments'))
BEGIN
    PRINT '✓ IX_Apartments_ZaaerId exists';
    SET @IndexCount = @IndexCount + 1;
END
ELSE
BEGIN
    PRINT '✗ IX_Apartments_ZaaerId MISSING';
    SET @MissingIndexes = @MissingIndexes + 'IX_Apartments_ZaaerId, ';
END

-- IX_ReservationUnits_ApartmentId
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ApartmentId' AND object_id = OBJECT_ID('dbo.reservation_units'))
BEGIN
    PRINT '✓ IX_ReservationUnits_ApartmentId exists';
    SET @IndexCount = @IndexCount + 1;
END
ELSE
BEGIN
    PRINT '✗ IX_ReservationUnits_ApartmentId MISSING';
    SET @MissingIndexes = @MissingIndexes + 'IX_ReservationUnits_ApartmentId, ';
END

-- IX_ReservationUnits_ReservationId
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ReservationId' AND object_id = OBJECT_ID('dbo.reservation_units'))
BEGIN
    PRINT '✓ IX_ReservationUnits_ReservationId exists';
    SET @IndexCount = @IndexCount + 1;
END
ELSE
BEGIN
    PRINT '✗ IX_ReservationUnits_ReservationId MISSING';
    SET @MissingIndexes = @MissingIndexes + 'IX_ReservationUnits_ReservationId, ';
END

-- IX_ReservationUnits_ApartmentId_Status
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ApartmentId_Status' AND object_id = OBJECT_ID('dbo.reservation_units'))
BEGIN
    PRINT '✓ IX_ReservationUnits_ApartmentId_Status exists';
    SET @IndexCount = @IndexCount + 1;
END
ELSE
BEGIN
    PRINT '✗ IX_ReservationUnits_ApartmentId_Status MISSING';
    SET @MissingIndexes = @MissingIndexes + 'IX_ReservationUnits_ApartmentId_Status, ';
END

-- IX_Apartments_Status
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Apartments_Status' AND object_id = OBJECT_ID('dbo.apartments'))
BEGIN
    PRINT '✓ IX_Apartments_Status exists';
    SET @IndexCount = @IndexCount + 1;
END
ELSE
BEGIN
    PRINT '✗ IX_Apartments_Status MISSING';
    SET @MissingIndexes = @MissingIndexes + 'IX_Apartments_Status, ';
END

PRINT '';
PRINT 'Summary: ' + CAST(@IndexCount AS NVARCHAR(10)) + ' out of 5 indexes exist';

IF LEN(@MissingIndexes) > 0
BEGIN
    SET @MissingIndexes = LEFT(@MissingIndexes, LEN(@MissingIndexes) - 2); -- Remove trailing comma
    PRINT '';
    PRINT '⚠ Missing indexes: ' + @MissingIndexes;
    PRINT '  → Run: Create_Indexes_For_Apartment_Status_Update.sql';
END
ELSE
BEGIN
    PRINT '';
    PRINT '✓ All indexes exist!';
END

PRINT '';
GO

-- =============================================
-- Check Application Code Status
-- =============================================
PRINT '=== APPLICATION CODE STATUS ===';
PRINT '';
PRINT 'Please verify manually:';
PRINT '1. Check if ZaaerReservationService.cs has UpdateApartmentStatusFromReservationUnitsAsync method';
PRINT '2. Check if it''s called in CreateReservationAsync (after transaction commit)';
PRINT '3. Check if it''s called in UpdateReservationByZaaerIdAsync (after save)';
PRINT '';

-- =============================================
-- Recommendations
-- =============================================
PRINT '=== RECOMMENDATIONS ===';
PRINT '';

IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TRG_Update_Apartment_Status_From_Reservation_Units')
    AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Apartments_ZaaerId' AND object_id = OBJECT_ID('dbo.apartments'))
    AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ApartmentId' AND object_id = OBJECT_ID('dbo.reservation_units'))
BEGIN
    PRINT '✓ Setup looks good!';
    PRINT '';
    PRINT 'Next steps:';
    PRINT '1. Test the trigger manually (see Test_Apartment_Status_Update.sql)';
    PRINT '2. Test the application API endpoints';
    PRINT '3. Monitor logs for any errors';
END
ELSE
BEGIN
    PRINT '⚠ Setup incomplete. Please:';
    IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TRG_Update_Apartment_Status_From_Reservation_Units')
        PRINT '  - Create trigger: Create_Trigger_Update_Apartment_Status_From_Reservation_Units.sql';
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Apartments_ZaaerId' AND object_id = OBJECT_ID('dbo.apartments'))
        OR NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ApartmentId' AND object_id = OBJECT_ID('dbo.reservation_units'))
        PRINT '  - Create indexes: Create_Indexes_For_Apartment_Status_Update.sql';
END

PRINT '';
PRINT '========================================';
PRINT 'Verification completed!';
PRINT '========================================';
GO

