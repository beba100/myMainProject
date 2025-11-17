-- =====================================================
-- Fix reservations table for Zaaer integration
-- =====================================================
-- Description: 
--   1. Adds external_ref_no column to reservations table
--   2. Drops CK_reservations_status CHECK constraint to allow any status value
--
-- This fixes two issues:
--   - Missing external_ref_no column causing 500 errors
--   - CHECK constraint preventing "checked_in" status
-- =====================================================

USE [YourDatabaseName]; -- Change this to your actual database name
GO

PRINT 'Starting fixes for reservations table...';
GO

-- Step 1: Add external_ref_no column if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.reservations') AND name = 'external_ref_no')
BEGIN
    ALTER TABLE [dbo].[reservations]
    ADD [external_ref_no] [int] NULL;
    PRINT '✓ Column external_ref_no added successfully to reservations table.';
END
ELSE
BEGIN
    PRINT '✓ Column external_ref_no already exists in reservations table.';
END
GO

-- Step 2: Drop the CHECK constraint on status column
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_reservations_status' AND object_id = OBJECT_ID('dbo.reservations'))
BEGIN
    ALTER TABLE [dbo].[reservations]
    DROP CONSTRAINT [CK_reservations_status];
    PRINT '✓ CHECK constraint CK_reservations_status dropped successfully.';
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
        PRINT '✓ CHECK constraint ' + @ConstraintName + ' dropped successfully.';
    END
    ELSE
    BEGIN
        PRINT 'ℹ No CHECK constraint found on reservations.status column.';
    END
END
GO

PRINT '';
PRINT '========================================';
PRINT 'All fixes completed successfully!';
PRINT '========================================';
GO

