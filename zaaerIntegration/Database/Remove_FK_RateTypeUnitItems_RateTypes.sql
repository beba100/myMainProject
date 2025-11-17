-- =============================================
-- Script: Remove Foreign Key Constraint
-- Description: Remove FK constraint between rate_types and rate_type_unit_items tables
-- Reason: Data comes from Zaaer system without enforced referential integrity
-- =============================================

-- Step 1: Find the Foreign Key constraint name
-- This query will show all FK constraints on rate_type_unit_items table
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS ParentTable,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys AS fk
INNER JOIN sys.tables AS t ON fk.parent_object_id = t.object_id
WHERE t.name = 'rate_type_unit_items'
    AND OBJECT_NAME(fk.referenced_object_id) = 'rate_types';

-- Step 2: Drop the Foreign Key constraint
-- Replace 'FK_rate_type_unit_items_rate_types_rate_type_id' with the actual constraint name from Step 1
-- Common constraint names:
--   - FK_rate_type_unit_items_rate_types_rate_type_id
--   - FK_RateTypeUnitItems_RateTypes
--   - FK_rate_type_unit_items_rate_types

-- Try dropping with common constraint names
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_rate_type_unit_items_rate_types_rate_type_id')
BEGIN
    ALTER TABLE [dbo].[rate_type_unit_items]
    DROP CONSTRAINT [FK_rate_type_unit_items_rate_types_rate_type_id];
    PRINT 'Foreign Key constraint [FK_rate_type_unit_items_rate_types_rate_type_id] dropped successfully.';
END
ELSE IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RateTypeUnitItems_RateTypes')
BEGIN
    ALTER TABLE [dbo].[rate_type_unit_items]
    DROP CONSTRAINT [FK_RateTypeUnitItems_RateTypes];
    PRINT 'Foreign Key constraint [FK_RateTypeUnitItems_RateTypes] dropped successfully.';
END
ELSE IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_rate_type_unit_items_rate_types')
BEGIN
    ALTER TABLE [dbo].[rate_type_unit_items]
    DROP CONSTRAINT [FK_rate_type_unit_items_rate_types];
    PRINT 'Foreign Key constraint [FK_rate_type_unit_items_rate_types] dropped successfully.';
END
ELSE
BEGIN
    -- If none of the common names exist, find and drop the constraint dynamically
    DECLARE @FKName NVARCHAR(128);
    SELECT @FKName = fk.name
    FROM sys.foreign_keys AS fk
    INNER JOIN sys.tables AS t ON fk.parent_object_id = t.object_id
    WHERE t.name = 'rate_type_unit_items'
        AND OBJECT_NAME(fk.referenced_object_id) = 'rate_types';
    
    IF @FKName IS NOT NULL
    BEGIN
        DECLARE @SQL NVARCHAR(MAX) = N'ALTER TABLE [dbo].[rate_type_unit_items] DROP CONSTRAINT [' + @FKName + N'];';
        EXEC sp_executesql @SQL;
        PRINT 'Foreign Key constraint [' + @FKName + '] dropped successfully.';
    END
    ELSE
    BEGIN
        PRINT 'No Foreign Key constraint found between rate_type_unit_items and rate_types tables.';
    END
END
GO

-- Step 3: Verify the constraint is removed
-- This query should return 0 rows if the FK constraint is successfully removed
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS ParentTable,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys AS fk
INNER JOIN sys.tables AS t ON fk.parent_object_id = t.object_id
WHERE t.name = 'rate_type_unit_items'
    AND OBJECT_NAME(fk.referenced_object_id) = 'rate_types';

PRINT 'Verification complete. If no rows are returned, the FK constraint has been successfully removed.';
GO

-- Note: The index on rate_type_id will remain for performance
-- The unique constraint on (rate_type_id, unit_type_name) will also remain
-- Only the Foreign Key constraint is removed to allow data insertion from Zaaer system

