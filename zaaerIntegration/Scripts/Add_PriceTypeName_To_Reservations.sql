-- =============================================
-- Add price_type_name column to reservations table
-- إضافة عمود نوع السعر لجدول الحجوزات
-- =============================================

-- Check if column already exists, then add it if not
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'reservations' 
    AND COLUMN_NAME = 'price_type_name'
)
BEGIN
    ALTER TABLE [dbo].[reservations]
    ADD [price_type_name] NVARCHAR(255) NULL;
    
    PRINT 'Column price_type_name added successfully to reservations table.';
END
ELSE
BEGIN
    PRINT 'Column price_type_name already exists in reservations table.';
END
GO

