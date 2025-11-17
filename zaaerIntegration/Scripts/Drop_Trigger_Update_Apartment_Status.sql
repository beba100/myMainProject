-- =============================================
-- Drop Trigger: Update Apartment Status from Reservation Units
-- =============================================
-- This script drops the trigger TRG_Update_Apartment_Status_From_Reservation_Units
-- The application-level logic (UpdateApartmentStatusFromReservationUnitsAsync) will handle
-- apartment status updates instead.
-- =============================================

PRINT '========================================';
PRINT 'Dropping Trigger: TRG_Update_Apartment_Status_From_Reservation_Units';
PRINT '========================================';
PRINT '';

-- Drop trigger if it exists
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TRG_Update_Apartment_Status_From_Reservation_Units')
BEGIN
    DROP TRIGGER [dbo].[TRG_Update_Apartment_Status_From_Reservation_Units];
    PRINT '✓ Trigger TRG_Update_Apartment_Status_From_Reservation_Units dropped successfully.';
    PRINT '';
    PRINT 'Note: Apartment status updates will now be handled by application-level logic';
    PRINT '      (UpdateApartmentStatusFromReservationUnitsAsync method in ZaaerReservationService).';
END
ELSE
BEGIN
    PRINT 'ℹ Trigger TRG_Update_Apartment_Status_From_Reservation_Units does not exist.';
    PRINT '  Nothing to drop.';
END

PRINT '';
PRINT '========================================';
PRINT 'Script completed!';
PRINT '========================================';
GO

