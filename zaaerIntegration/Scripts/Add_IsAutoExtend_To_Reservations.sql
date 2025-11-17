-- =============================================
-- Add is_auto_extend column to reservations table
-- إضافة عمود تمديد تلقائي لجدول الحجوزات
-- =============================================

-- Check if column already exists, then add it if not
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'reservations' 
    AND COLUMN_NAME = 'is_auto_extend'
)
BEGIN
    ALTER TABLE [dbo].[reservations]
    ADD [is_auto_extend] BIT NULL;
    
    PRINT 'Column is_auto_extend added successfully to reservations table.';
END
ELSE
BEGIN
    PRINT 'Column is_auto_extend already exists in reservations table.';
END
GO

