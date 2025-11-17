-- Quick fix script for logo_url NULL constraint issue
-- This script specifically fixes the logo_url column to allow NULL values
-- Run this script to fix the immediate error

PRINT 'Fixing logo_url column to allow NULL values...';
PRINT 'إصلاح عمود logo_url للسماح بقيم NULL...';
GO

-- Step 1: Drop default constraint on logo_url if it exists
DECLARE @constraint_name NVARCHAR(200);
SELECT @constraint_name = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('dbo.hotel_settings') 
AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.hotel_settings') AND name = 'logo_url');

IF @constraint_name IS NOT NULL
BEGIN
    DECLARE @sql_drop NVARCHAR(MAX) = 'ALTER TABLE dbo.hotel_settings DROP CONSTRAINT [' + @constraint_name + '];';
    EXEC sp_executesql @sql_drop;
    PRINT 'Default constraint dropped: ' + @constraint_name;
END
ELSE
BEGIN
    PRINT 'No default constraint found on logo_url.';
END
GO

-- Step 2: Check current NULL status and update if needed
IF EXISTS (SELECT 1 FROM sys.columns 
           WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') 
           AND name = 'logo_url' 
           AND is_nullable = 0)
BEGIN
    -- Column exists and is NOT NULL, change it to NULL
    ALTER TABLE dbo.hotel_settings ALTER COLUMN logo_url NVARCHAR(500) NULL;
    PRINT 'Column logo_url successfully changed to allow NULL values.';
END
ELSE IF EXISTS (SELECT 1 FROM sys.columns 
                WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') 
                AND name = 'logo_url' 
                AND is_nullable = 1)
BEGIN
    PRINT 'Column logo_url already allows NULL values.';
END
ELSE
BEGIN
    PRINT 'ERROR: Column logo_url does not exist in hotel_settings table!';
END
GO

PRINT '';
PRINT 'Script completed. logo_url column should now accept NULL values.';
PRINT 'تم إكمال السكريبت. يجب أن يقبل عمود logo_url الآن قيم NULL.';
GO

