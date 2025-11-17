-- Drop Unit Id Foreign Key Constraints
-- حذف قيود المفاتيح الخارجية لـ unit_id
--
-- This script drops the foreign key constraints related to unit_id in:
-- payment_receipts, refunds, and invoices tables
--
-- سكريبت حذف القيود الخارجية المتعلقة بـ unit_id في:
-- payment_receipts, refunds, and invoices

PRINT 'Starting UnitId Foreign Key Constraints cleanup...'
PRINT 'بدء تنظيف قيود المفاتيح الخارجية لـ unit_id...'

DECLARE @sql NVARCHAR(MAX) = ''
DECLARE @constraintName NVARCHAR(128)
DECLARE @tableName NVARCHAR(128)

-- List of foreign key constraints to drop based on images
DECLARE @ConstraintsToDrop TABLE (
    TableName NVARCHAR(128),
    ConstraintName NVARCHAR(128)
)

INSERT INTO @ConstraintsToDrop VALUES
    -- Payment Receipts constraints
    ('payment_receipts', 'FK_payment_receipts_reservation_units_unit_id'),
    
    -- Refunds constraints
    ('refunds', 'FK_refunds_reservation_units_unit_id'),
    
    -- Invoices constraints  
    ('invoices', 'FK_invoices_reservation_units_unit_id')

-- Iterate through the constraints and drop them
DECLARE constraint_cursor CURSOR FOR
SELECT TableName, ConstraintName FROM @ConstraintsToDrop

OPEN constraint_cursor
FETCH NEXT FROM constraint_cursor INTO @tableName, @constraintName

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Check if constraint exists before trying to drop it
    IF EXISTS (
        SELECT 1 
        FROM sys.foreign_keys fk
        INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
        WHERE t.name = @tableName AND fk.name = @constraintName
    )
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
    END
    ELSE
    BEGIN
        PRINT 'Constraint ' + @constraintName + ' does not exist in table ' + @tableName + ', skipping...'
    END
    
    FETCH NEXT FROM constraint_cursor INTO @tableName, @constraintName
END

CLOSE constraint_cursor
DEALLOCATE constraint_cursor

-- Verify remaining constraints
PRINT ''
PRINT 'Verifying dropped constraints...'
PRINT 'التحقق من القيود المحذوفة...'

DECLARE @remainingCount INT
SELECT @remainingCount = COUNT(*)
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
WHERE t.name IN ('payment_receipts', 'refunds', 'invoices')
AND fk.name LIKE '%unit_id%'

IF @remainingCount = 0
BEGIN
    PRINT 'All unit_id foreign key constraints have been successfully dropped!'
    PRINT 'تم حذف جميع قيود المفاتيح الخارجية لـ unit_id بنجاح!'
END
ELSE
BEGIN
    PRINT 'Warning: ' + CAST(@remainingCount AS VARCHAR(10)) + ' unit_id foreign key constraints still remain.'
    PRINT 'تحذير: لا يزال هناك ' + CAST(@remainingCount AS VARCHAR(10)) + ' قيود مفاتيح خارجية لـ unit_id.'
END

PRINT ''
PRINT 'UnitId Foreign Key Constraints cleanup completed!'
PRINT 'اكتمل تنظيف قيود المفاتيح الخارجية لـ unit_id!'

