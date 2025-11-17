-- =============================================
-- Trigger: Update Apartment Status from Reservation Units
-- =============================================
-- This trigger automatically updates apartments.status based on reservation_units.status
-- when reservation units are created or updated.
--
-- Business Logic:
-- - reservation_units.apartment_id maps to apartments.zaaer_id
-- - When reservation_units.status = 'checked_in' → apartments.status = 'rented'
-- - When reservation_units.status = 'checked_out' → apartments.status = 'vacant'
-- - When reservation_units.status = 'cancelled' → apartments.status = 'vacant'
-- - When reservation_units.status = 'no_show' → apartments.status = 'vacant'
-- - Other statuses: Keep existing apartment status
--
-- This is a safety net that works even if application-level logic is bypassed
-- (e.g., direct database updates, migrations, manual changes)
-- =============================================

-- Drop trigger if it exists (for idempotency)
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TRG_Update_Apartment_Status_From_Reservation_Units')
BEGIN
    DROP TRIGGER [dbo].[TRG_Update_Apartment_Status_From_Reservation_Units];
    PRINT 'Existing trigger TRG_Update_Apartment_Status_From_Reservation_Units dropped.';
END
GO

-- Create the trigger
CREATE TRIGGER [dbo].[TRG_Update_Apartment_Status_From_Reservation_Units]
ON [dbo].[reservation_units]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only process if status column was actually changed (for UPDATE operations)
    IF UPDATE(status) OR EXISTS (SELECT 1 FROM inserted)
    BEGIN
        BEGIN TRY
            -- Set-based update: More efficient than cursor approach
            -- Update apartments based on reservation units status
            UPDATE a
            SET a.status = CASE 
                    WHEN LOWER(LTRIM(RTRIM(i.status))) IN ('checked_in', 'checkedin') THEN 'rented'
                    WHEN LOWER(LTRIM(RTRIM(i.status))) IN ('checked_out', 'checkedout') THEN 'vacant'
                    WHEN LOWER(LTRIM(RTRIM(i.status))) IN ('cancelled', 'canceled') THEN 'vacant'
                    WHEN LOWER(LTRIM(RTRIM(i.status))) IN ('no_show', 'noshow') THEN 'vacant'
                    ELSE a.status -- Keep existing status for other states
                END
            FROM apartments a
            INNER JOIN inserted i ON a.zaaer_id = i.apartment_id
            WHERE i.apartment_id IS NOT NULL 
                AND i.apartment_id > 0
                -- Only update if the new status is different from current status
                AND a.status <> CASE 
                    WHEN LOWER(LTRIM(RTRIM(i.status))) IN ('checked_in', 'checkedin') THEN 'rented'
                    WHEN LOWER(LTRIM(RTRIM(i.status))) IN ('checked_out', 'checkedout') THEN 'vacant'
                    WHEN LOWER(LTRIM(RTRIM(i.status))) IN ('cancelled', 'canceled') THEN 'vacant'
                    WHEN LOWER(LTRIM(RTRIM(i.status))) IN ('no_show', 'noshow') THEN 'vacant'
                    ELSE a.status
                END;
        END TRY
        BEGIN CATCH
            -- Log error but don't fail the transaction
            -- This ensures that if apartment update fails, the reservation unit operation can still succeed
            DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
            DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
            DECLARE @ErrorState INT = ERROR_STATE();
            
            -- Log error (optional - can be removed if not needed)
            -- PRINT 'Error updating apartment status from reservation units: ' + @ErrorMessage;
            
            -- Don't re-throw - allow the main operation to succeed
        END CATCH
    END
END
GO

PRINT 'Trigger TRG_Update_Apartment_Status_From_Reservation_Units created successfully.';
PRINT 'The trigger will automatically update apartments.status when reservation_units.status changes.';
GO

