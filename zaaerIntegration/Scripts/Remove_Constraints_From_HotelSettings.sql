-- Script to remove database constraints from hotel_settings table
-- This script removes constraints that might prevent updates
-- WARNING: This will permanently remove constraints. Make sure to backup first!

PRINT 'Starting constraint removal for hotel_settings table...';
PRINT 'بدء إزالة القيود من جدول hotel_settings...';
GO

-- ============================================
-- Remove Foreign Key Constraints (from other tables referencing hotel_settings)
-- ============================================

-- Get all foreign key constraints that reference hotel_settings.hotel_id
DECLARE @sql NVARCHAR(MAX) = '';
DECLARE @constraintName NVARCHAR(200);
DECLARE @tableName NVARCHAR(200);

DECLARE fk_cursor CURSOR FOR
SELECT 
    fk.name AS ConstraintName,
    OBJECT_NAME(fk.parent_object_id) AS TableName
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.referenced_column_id = c.column_id AND fkc.referenced_object_id = c.object_id
WHERE OBJECT_NAME(fkc.referenced_object_id) = 'hotel_settings'
    AND c.name = 'hotel_id';

OPEN fk_cursor;
FETCH NEXT FROM fk_cursor INTO @constraintName, @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'ALTER TABLE [dbo].[' + @tableName + '] DROP CONSTRAINT [' + @constraintName + '];';
    PRINT 'Dropping foreign key constraint: ' + @constraintName + ' from table: ' + @tableName;
    EXEC sp_executesql @sql;
    FETCH NEXT FROM fk_cursor INTO @constraintName, @tableName;
END;

CLOSE fk_cursor;
DEALLOCATE fk_cursor;

GO

-- ============================================
-- Remove Check Constraints
-- ============================================

DECLARE @checkConstraintName NVARCHAR(200);
DECLARE check_cursor CURSOR FOR
SELECT name
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('dbo.hotel_settings');

OPEN check_cursor;
FETCH NEXT FROM check_cursor INTO @checkConstraintName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'ALTER TABLE [dbo].[hotel_settings] DROP CONSTRAINT [' + @checkConstraintName + '];';
    PRINT 'Dropping check constraint: ' + @checkConstraintName;
    EXEC sp_executesql @sql;
    FETCH NEXT FROM check_cursor INTO @checkConstraintName;
END;

CLOSE check_cursor;
DEALLOCATE check_cursor;

GO

-- ============================================
-- Remove Unique Constraints (except primary key)
-- ============================================

DECLARE @uniqueConstraintName NVARCHAR(200);
DECLARE unique_cursor CURSOR FOR
SELECT k.name
FROM sys.key_constraints k
WHERE k.parent_object_id = OBJECT_ID('dbo.hotel_settings')
    AND k.type = 'UQ'; -- Unique constraint

OPEN unique_cursor;
FETCH NEXT FROM unique_cursor INTO @uniqueConstraintName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'ALTER TABLE [dbo].[hotel_settings] DROP CONSTRAINT [' + @uniqueConstraintName + '];';
    PRINT 'Dropping unique constraint: ' + @uniqueConstraintName;
    EXEC sp_executesql @sql;
    FETCH NEXT FROM unique_cursor INTO @uniqueConstraintName;
END;

CLOSE unique_cursor;
DEALLOCATE unique_cursor;

GO

-- ============================================
-- Remove Default Constraints (if any remain)
-- ============================================

DECLARE @defaultConstraintName NVARCHAR(200);
DECLARE default_cursor CURSOR FOR
SELECT dc.name
FROM sys.default_constraints dc
WHERE dc.parent_object_id = OBJECT_ID('dbo.hotel_settings');

OPEN default_cursor;
FETCH NEXT FROM default_cursor INTO @defaultConstraintName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'ALTER TABLE [dbo].[hotel_settings] DROP CONSTRAINT [' + @defaultConstraintName + '];';
    PRINT 'Dropping default constraint: ' + @defaultConstraintName;
    EXEC sp_executesql @sql;
    FETCH NEXT FROM default_cursor INTO @defaultConstraintName;
END;

CLOSE default_cursor;
DEALLOCATE default_cursor;

GO

-- ============================================
-- Summary
-- ============================================

PRINT '';
PRINT 'Constraint removal completed.';
PRINT 'تم إكمال إزالة القيود.';
PRINT '';
PRINT 'Note: Primary key constraint (PK_hotel_settings) was NOT removed.';
PRINT 'ملاحظة: لم يتم إزالة قيد المفتاح الأساسي (PK_hotel_settings).';
GO

