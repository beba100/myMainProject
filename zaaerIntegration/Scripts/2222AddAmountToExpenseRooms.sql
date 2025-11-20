-- =============================================
-- Add amount column to expense_rooms table
-- إضافة حقل amount في جدول expense_rooms
-- =============================================

-- Check if amount column exists
IF COL_LENGTH('dbo.expense_rooms', 'amount') IS NULL
BEGIN
    -- Add amount column
    ALTER TABLE dbo.expense_rooms
    ADD amount DECIMAL(12,2) NULL;
    
    PRINT '✅ Added amount column to expense_rooms table';
END
ELSE
BEGIN
    PRINT '⚠️ amount column already exists in expense_rooms table';
END
GO

PRINT '✅ Expense rooms table updated successfully!';
GO

