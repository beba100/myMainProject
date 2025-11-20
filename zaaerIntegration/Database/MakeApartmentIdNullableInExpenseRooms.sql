-- =============================================
-- Make apartment_id nullable in expense_rooms table
-- جعل apartment_id nullable في جدول expense_rooms للسماح بالفئات (CAT_BUILDING, etc.)
-- =============================================

-- Check if apartment_id column exists and is NOT NULL
IF EXISTS (SELECT * FROM sys.columns 
           WHERE Name = N'apartment_id' 
           AND Object_ID = Object_ID(N'expense_rooms')
           AND is_nullable = 0)
BEGIN
    -- Drop the foreign key constraint first
    IF EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_ExpenseRooms_Apartments' 
               AND parent_object_id = OBJECT_ID('expense_rooms'))
    BEGIN
        ALTER TABLE expense_rooms
        DROP CONSTRAINT FK_ExpenseRooms_Apartments;
        PRINT '✅ Dropped FK_ExpenseRooms_Apartments constraint.';
    END

    -- Make apartment_id nullable
    ALTER TABLE expense_rooms
    ALTER COLUMN apartment_id INT NULL;
    
    PRINT '✅ Made apartment_id nullable in expense_rooms table.';
    
    -- Recreate the foreign key constraint with nullable support
    ALTER TABLE expense_rooms
    ADD CONSTRAINT FK_ExpenseRooms_Apartments
    FOREIGN KEY (apartment_id) 
    REFERENCES apartments(apartment_id)
    ON DELETE RESTRICT;
    
    PRINT '✅ Recreated FK_ExpenseRooms_Apartments constraint (allowing NULL).';
END
ELSE
BEGIN
    PRINT '⚠️ apartment_id column is already nullable or does not exist.';
END
GO

PRINT '✅ Expense rooms table updated successfully!';
GO

