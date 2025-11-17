-- =====================================================
-- Check and Drop Foreign Key on reservation_units.reservation_id
-- =====================================================
-- Description: Checks for foreign key constraints on reservation_units.reservation_id
--   and drops them if they exist, allowing the column to store zaaer_id values instead
--
-- Note: This allows reservation_id to store zaaer_id values instead of database primary keys
-- =====================================================

USE [YourDatabaseName]; -- Change this to your actual database name
GO

-- Find and drop foreign key constraints on reservation_units.reservation_id
DECLARE @FKName NVARCHAR(128);
DECLARE @SQL NVARCHAR(MAX);

-- Find foreign key constraints referencing reservation_units.reservation_id
SELECT @FKName = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.tables t ON fkc.parent_object_id = t.object_id
INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
WHERE t.name = 'reservation_units' AND c.name = 'reservation_id';

IF @FKName IS NOT NULL
BEGIN
    SET @SQL = N'ALTER TABLE [dbo].[reservation_units] DROP CONSTRAINT [' + @FKName + ']';
    EXEC sp_executesql @SQL;
    PRINT '✓ Foreign key constraint ' + @FKName + ' dropped successfully.';
END
ELSE
BEGIN
    PRINT 'ℹ No foreign key constraint found on reservation_units.reservation_id';
END
GO

PRINT 'Script completed.';
GO

