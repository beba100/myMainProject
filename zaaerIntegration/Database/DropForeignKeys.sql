-- Drop Foreign Key Constraints Script
-- سكريبت حذف قيود المفاتيح الخارجية

-- This script will drop all foreign key constraints that reference the hotels table
-- هذا السكريبت سيقوم بحذف جميع قيود المفاتيح الخارجية التي تشير إلى جدول الفنادق

PRINT 'Starting Foreign Key Constraints cleanup...'
PRINT 'بدء تنظيف قيود المفاتيح الخارجية...'

-- Get all foreign key constraints that reference the hotels table
-- الحصول على جميع قيود المفاتيح الخارجية التي تشير إلى جدول الفنادق

DECLARE @sql NVARCHAR(MAX) = ''
DECLARE @constraintName NVARCHAR(128)
DECLARE @tableName NVARCHAR(128)

-- Cursor to iterate through all foreign key constraints
DECLARE fk_cursor CURSOR FOR
SELECT 
    fk.name AS constraint_name,
    t.name AS table_name
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
INNER JOIN sys.tables rt ON fk.referenced_object_id = rt.object_id
WHERE rt.name = 'hotels'

OPEN fk_cursor
FETCH NEXT FROM fk_cursor INTO @constraintName, @tableName

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'ALTER TABLE [dbo].[' + @tableName + '] DROP CONSTRAINT [' + @constraintName + ']'
    
    PRINT 'Dropping constraint: ' + @constraintName + ' from table: ' + @tableName
    
    BEGIN TRY
        EXEC sp_executesql @sql
        PRINT 'Successfully dropped constraint: ' + @constraintName
    END TRY
    BEGIN CATCH
        PRINT 'Error dropping constraint ' + @constraintName + ': ' + ERROR_MESSAGE()
    END CATCH
    
    FETCH NEXT FROM fk_cursor INTO @constraintName, @tableName
END

CLOSE fk_cursor
DEALLOCATE fk_cursor

-- Check if there are any remaining foreign key constraints
DECLARE @remainingFKCount INT
SELECT @remainingFKCount = COUNT(*)
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.referenced_object_id = t.object_id
WHERE t.name = 'hotels'

IF @remainingFKCount = 0
BEGIN
    PRINT 'All foreign key constraints have been successfully dropped!'
    PRINT 'تم حذف جميع قيود المفاتيح الخارجية بنجاح!'
END
ELSE
BEGIN
    PRINT 'Warning: ' + CAST(@remainingFKCount AS VARCHAR(10)) + ' foreign key constraints still remain.'
    PRINT 'تحذير: لا يزال هناك ' + CAST(@remainingFKCount AS VARCHAR(10)) + ' قيود مفاتيح خارجية.'
END

PRINT 'Foreign Key Constraints cleanup completed!'
PRINT 'اكتمل تنظيف قيود المفاتيح الخارجية!'
