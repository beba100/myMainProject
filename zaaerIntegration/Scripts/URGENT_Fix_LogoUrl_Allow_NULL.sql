-- URGENT FIX: Make logo_url column allow NULL values
-- This script MUST be run to fix the "Cannot insert NULL" error
-- Run this script IMMEDIATELY in SQL Server Management Studio

USE [db30471];
GO

PRINT '========================================';
PRINT 'URGENT FIX: Making logo_url nullable';
PRINT '========================================';
PRINT '';

-- Step 1: Find and drop default constraint on logo_url
DECLARE @constraint_name NVARCHAR(200);
SELECT @constraint_name = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('dbo.hotel_settings') 
AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.hotel_settings') AND name = 'logo_url');

IF @constraint_name IS NOT NULL
BEGIN
    DECLARE @sql_drop NVARCHAR(MAX) = 'ALTER TABLE dbo.hotel_settings DROP CONSTRAINT [' + @constraint_name + '];';
    EXEC sp_executesql @sql_drop;
    PRINT '✓ Default constraint dropped: ' + @constraint_name;
END
ELSE
BEGIN
    PRINT '✓ No default constraint found on logo_url (this is OK)';
END
GO

-- Step 2: Update existing NULL values to empty string (if any exist)
-- This is a safety measure before changing the column
UPDATE dbo.hotel_settings 
SET logo_url = '' 
WHERE logo_url IS NULL;
GO

-- Step 3: Change the column to allow NULL
ALTER TABLE dbo.hotel_settings 
ALTER COLUMN logo_url NVARCHAR(500) NULL;
GO

-- Step 4: Verify the change
IF EXISTS (SELECT 1 FROM sys.columns 
           WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') 
           AND name = 'logo_url' 
           AND is_nullable = 1)
BEGIN
    PRINT '';
    PRINT '========================================';
    PRINT '✓ SUCCESS! logo_url column now allows NULL';
    PRINT '========================================';
    PRINT '';
    PRINT 'The column has been successfully updated.';
    PRINT 'You can now send logoUrl: null in API requests.';
END
ELSE
BEGIN
    PRINT '';
    PRINT '========================================';
    PRINT '✗ ERROR: Column still does not allow NULL';
    PRINT '========================================';
    PRINT '';
    PRINT 'Please check the error messages above and try again.';
END
GO

PRINT '';
PRINT 'Script execution completed.';
PRINT '';

