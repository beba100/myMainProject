-- Drop ALL Foreign Key Constraints Script
-- سكريبت حذف جميع قيود المفاتيح الخارجية
--
-- This script drops ALL foreign key constraints in the entire database
-- تم إنشاء هذا السكريبت لحذف جميع قيود المفاتيح الخارجية في قاعدة البيانات بالكامل
-- ⚠️ WARNING: This will remove all referential integrity constraints
-- ⚠️ تحذير: سيتم إزالة جميع قيود سلامة المرجعية
--
-- USE WITH CAUTION!
-- استخدم بحذر!

PRINT '=================================================='
PRINT 'Starting ALL Foreign Key Constraints cleanup...'
PRINT 'بدء تنظيف جميع قيود المفاتيح الخارجية...'
PRINT '=================================================='
PRINT ''

DECLARE @sql NVARCHAR(MAX) = ''
DECLARE @constraintName NVARCHAR(128)
DECLARE @tableName NVARCHAR(128)
DECLARE @referencedTable NVARCHAR(128)
DECLARE @count INT = 0

-- Get ALL foreign key constraints in the database
PRINT 'Finding all foreign key constraints...'
PRINT 'البحث عن جميع قيود المفاتيح الخارجية...'

DECLARE fk_cursor CURSOR FOR
SELECT 
    fk.name AS constraint_name,
    t.name AS table_name,
    rt.name AS referenced_table
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
INNER JOIN sys.tables rt ON fk.referenced_object_id = rt.object_id
ORDER BY t.name, fk.name

OPEN fk_cursor
FETCH NEXT FROM fk_cursor INTO @constraintName, @tableName, @referencedTable

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'ALTER TABLE [dbo].[' + @tableName + '] DROP CONSTRAINT [' + @constraintName + ']'
    
    PRINT CAST(@count + 1 AS VARCHAR(10)) + '. Dropping: ' + @constraintName + 
          ' from [' + @tableName + '] → [' + @referencedTable + ']'
    
    BEGIN TRY
        EXEC sp_executesql @sql
        SET @count = @count + 1
        PRINT '   ✓ Successfully dropped'
    END TRY
    BEGIN CATCH
        PRINT '   ✗ Error: ' + ERROR_MESSAGE()
    END CATCH
    
    FETCH NEXT FROM fk_cursor INTO @constraintName, @tableName, @referencedTable
END

CLOSE fk_cursor
DEALLOCATE fk_cursor

-- Verify all constraints are dropped
PRINT ''
PRINT '=================================================='
DECLARE @remainingCount INT
SELECT @remainingCount = COUNT(*)
FROM sys.foreign_keys

IF @remainingCount = 0
BEGIN
    PRINT '✓ ALL foreign key constraints have been successfully dropped!'
    PRINT '✓ تم حذف جميع قيود المفاتيح الخارجية بنجاح!'
END
ELSE
BEGIN
    PRINT '⚠ Warning: ' + CAST(@remainingCount AS VARCHAR(10)) + ' foreign key constraints still remain.'
    PRINT '⚠ تحذير: لا يزال هناك ' + CAST(@remainingCount AS VARCHAR(10)) + ' قيود مفاتيح خارجية.'
END

PRINT ''
PRINT 'Total constraints dropped: ' + CAST(@count AS VARCHAR(10))
PRINT 'إجمالي القيود المحذوفة: ' + CAST(@count AS VARCHAR(10))
PRINT '=================================================='
PRINT 'Foreign Key Constraints cleanup completed!'
PRINT 'اكتمل تنظيف قيود المفاتيح الخارجية!'

