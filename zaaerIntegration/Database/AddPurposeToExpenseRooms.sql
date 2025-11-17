-- =============================================
-- Add purpose column to expense_rooms table
-- إضافة حقل purpose في جدول expense_rooms
-- =============================================

USE [YourDatabaseName]; -- Change this to your actual database name
GO

-- Check if purpose column exists
IF COL_LENGTH('dbo.expense_rooms', 'purpose') IS NULL
BEGIN
    -- Add purpose column
    ALTER TABLE dbo.expense_rooms
    ADD purpose NVARCHAR(500) NULL;
    
    PRINT '✅ Added purpose column to expense_rooms table';
END
ELSE
BEGIN
    PRINT '⚠️ purpose column already exists in expense_rooms table';
END
GO

-- Add index for performance (optional)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_expense_rooms_expense_id' AND object_id = OBJECT_ID('dbo.expense_rooms'))
BEGIN
    CREATE INDEX IX_expense_rooms_expense_id ON dbo.expense_rooms(expense_id);
    PRINT '✅ Created index IX_expense_rooms_expense_id';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_expense_rooms_apartment_id' AND object_id = OBJECT_ID('dbo.expense_rooms'))
BEGIN
    CREATE INDEX IX_expense_rooms_apartment_id ON dbo.expense_rooms(apartment_id);
    PRINT '✅ Created index IX_expense_rooms_apartment_id';
END
GO

PRINT '✅ Expense rooms table updated successfully!';
GO

