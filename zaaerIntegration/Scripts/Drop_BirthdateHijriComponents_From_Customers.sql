-- =====================================================
-- Remove Hijri Birthdate Component Fields from customers table
-- =====================================================
-- Description: Drops the following columns from customers table:
--   - birthdate_hijri_day
--   - birthdate_hijri_month
--   - birthdate_hijri_year
--   - birthdate_hijri_string
--
-- Note: These fields are no longer needed. Only birthdate_hijri (string) and birthdate_gregorian (date) are kept.
-- =====================================================

USE [YourDatabaseName]; -- Change this to your actual database name
GO

-- Drop birthdate_hijri_day column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.customers') AND name = 'birthdate_hijri_day')
BEGIN
    ALTER TABLE [dbo].[customers]
    DROP COLUMN [birthdate_hijri_day];
    PRINT 'Column birthdate_hijri_day dropped successfully.';
END
ELSE
    PRINT 'Column birthdate_hijri_day does not exist.';
GO

-- Drop birthdate_hijri_month column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.customers') AND name = 'birthdate_hijri_month')
BEGIN
    ALTER TABLE [dbo].[customers]
    DROP COLUMN [birthdate_hijri_month];
    PRINT 'Column birthdate_hijri_month dropped successfully.';
END
ELSE
    PRINT 'Column birthdate_hijri_month does not exist.';
GO

-- Drop birthdate_hijri_year column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.customers') AND name = 'birthdate_hijri_year')
BEGIN
    ALTER TABLE [dbo].[customers]
    DROP COLUMN [birthdate_hijri_year];
    PRINT 'Column birthdate_hijri_year dropped successfully.';
END
ELSE
    PRINT 'Column birthdate_hijri_year does not exist.';
GO

-- Drop birthdate_hijri_string column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.customers') AND name = 'birthdate_hijri_string')
BEGIN
    ALTER TABLE [dbo].[customers]
    DROP COLUMN [birthdate_hijri_string];
    PRINT 'Column birthdate_hijri_string dropped successfully.';
END
ELSE
    PRINT 'Column birthdate_hijri_string does not exist.';
GO

PRINT 'All Hijri birthdate component columns have been processed.';
GO

