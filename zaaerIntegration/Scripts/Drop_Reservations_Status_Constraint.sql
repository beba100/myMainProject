-- =====================================================
-- Drop CHECK constraint on reservations.status column
-- =====================================================
-- Description: Drops the CK_reservations_status CHECK constraint
--   to allow any status value to be inserted, including "checked_in"
--
-- Note: This constraint was preventing certain status values from being saved
-- =====================================================

USE [YourDatabaseName]; -- Change this to your actual database name
GO

-- Drop the CHECK constraint if it exists
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_reservations_status' AND object_id = OBJECT_ID('dbo.reservations'))
BEGIN
    ALTER TABLE [dbo].[reservations]
    DROP CONSTRAINT [CK_reservations_status];
    PRINT 'CHECK constraint CK_reservations_status dropped successfully.';
END
ELSE
BEGIN
    -- Try to find the constraint with a different name pattern
    DECLARE @ConstraintName NVARCHAR(128);
    SELECT @ConstraintName = name 
    FROM sys.check_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.reservations') 
      AND definition LIKE '%status%';
    
    IF @ConstraintName IS NOT NULL
    BEGIN
        DECLARE @SQL NVARCHAR(MAX) = N'ALTER TABLE [dbo].[reservations] DROP CONSTRAINT [' + @ConstraintName + ']';
        EXEC sp_executesql @SQL;
        PRINT 'CHECK constraint ' + @ConstraintName + ' dropped successfully.';
    END
    ELSE
    BEGIN
        PRINT 'No CHECK constraint found on reservations.status column.';
    END
END
GO

PRINT 'Script completed successfully.';
GO

