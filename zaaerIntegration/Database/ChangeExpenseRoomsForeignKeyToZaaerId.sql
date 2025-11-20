-- =============================================
-- Change Foreign Key in expense_rooms from apartment_id to zaaer_id
-- تغيير المفتاح الأجنبي في expense_rooms من apartment_id إلى zaaer_id
-- =============================================

-- Step 1: Drop the old foreign key constraint if it exists
IF EXISTS (SELECT * FROM sys.foreign_keys 
           WHERE name = 'FK_ExpenseRooms_Apartments' 
           AND parent_object_id = OBJECT_ID('expense_rooms'))
BEGIN
    ALTER TABLE expense_rooms
    DROP CONSTRAINT FK_ExpenseRooms_Apartments;
    PRINT '✅ Dropped old FK_ExpenseRooms_Apartments constraint.';
END
GO

-- Step 2: Migrate data and rename apartment_id column to zaaer_id
IF EXISTS (SELECT * FROM sys.columns 
           WHERE Name = N'apartment_id' 
           AND Object_ID = Object_ID(N'expense_rooms'))
BEGIN
    -- Check if zaaer_id column already exists
    IF NOT EXISTS (SELECT * FROM sys.columns 
                   WHERE Name = N'zaaer_id' 
                   AND Object_ID = Object_ID(N'expense_rooms'))
    BEGIN
        -- Add zaaer_id column first
        ALTER TABLE expense_rooms
        ADD zaaer_id INT NULL;
        PRINT '✅ Added zaaer_id column.';
        
        -- Migrate data from apartment_id to zaaer_id
        UPDATE er
        SET er.zaaer_id = a.zaaer_id
        FROM expense_rooms er
        INNER JOIN apartments a ON er.apartment_id = a.apartment_id
        WHERE a.zaaer_id IS NOT NULL;
        
        PRINT '✅ Migrated data from apartment_id to zaaer_id.';
        
        -- Drop apartment_id column
        ALTER TABLE expense_rooms
        DROP COLUMN apartment_id;
        PRINT '✅ Dropped apartment_id column.';
    END
    ELSE
    BEGIN
        -- If zaaer_id already exists, migrate data from apartment_id
        UPDATE er
        SET er.zaaer_id = a.zaaer_id
        FROM expense_rooms er
        INNER JOIN apartments a ON er.apartment_id = a.apartment_id
        WHERE er.zaaer_id IS NULL AND a.zaaer_id IS NOT NULL;
        
        PRINT '✅ Migrated data from apartment_id to zaaer_id.';
        
        -- Drop apartment_id column
        ALTER TABLE expense_rooms
        DROP COLUMN apartment_id;
        PRINT '✅ Dropped apartment_id column.';
    END
END
ELSE IF NOT EXISTS (SELECT * FROM sys.columns 
                    WHERE Name = N'zaaer_id' 
                    AND Object_ID = Object_ID(N'expense_rooms'))
BEGIN
    -- If apartment_id doesn't exist and zaaer_id doesn't exist, create zaaer_id
    ALTER TABLE expense_rooms
    ADD zaaer_id INT NULL;
    PRINT '✅ Added zaaer_id column.';
END
GO

-- Step 3: Ensure apartments.zaaer_id has a unique index (required for foreign key)
-- Check if apartments.zaaer_id has a unique constraint or index
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE object_id = OBJECT_ID('apartments')
               AND (is_unique = 1 OR is_unique_constraint = 1)
               AND name IN (
                   SELECT name FROM sys.index_columns ic
                   INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                   WHERE ic.object_id = OBJECT_ID('apartments')
                   AND c.name = 'zaaer_id'
                   AND ic.key_ordinal = 1
               ))
BEGIN
    -- Create unique index on apartments.zaaer_id if it doesn't exist
    -- Note: This assumes zaaer_id should be unique. If not, you may need to adjust this.
    IF NOT EXISTS (SELECT * FROM sys.indexes 
                   WHERE name = 'IX_apartments_zaaer_id_unique' 
                   AND object_id = OBJECT_ID('apartments'))
    BEGIN
        -- First, check if there are duplicate zaaer_id values
        IF NOT EXISTS (SELECT zaaer_id FROM apartments WHERE zaaer_id IS NOT NULL GROUP BY zaaer_id HAVING COUNT(*) > 1)
        BEGIN
            CREATE UNIQUE NONCLUSTERED INDEX IX_apartments_zaaer_id_unique 
            ON apartments(zaaer_id) 
            WHERE zaaer_id IS NOT NULL; -- Filtered unique index (allows multiple NULLs)
            PRINT '✅ Created unique index IX_apartments_zaaer_id_unique on apartments.zaaer_id.';
        END
        ELSE
        BEGIN
            PRINT '⚠️ Cannot create unique index: duplicate zaaer_id values found in apartments table.';
            PRINT '⚠️ Foreign key constraint will not be created. Please fix duplicate zaaer_id values first.';
        END
    END
END
GO

-- Step 4: Create index on expense_rooms.zaaer_id (recommended for foreign keys)
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_expense_rooms_zaaer_id' 
               AND object_id = OBJECT_ID('expense_rooms'))
BEGIN
    CREATE INDEX IX_expense_rooms_zaaer_id ON expense_rooms(zaaer_id);
    PRINT '✅ Created index IX_expense_rooms_zaaer_id.';
END
GO

-- Step 5: Create new foreign key constraint: expense_rooms.zaaer_id -> apartments.zaaer_id
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_ExpenseRooms_Apartments_ZaaerId' 
               AND parent_object_id = OBJECT_ID('expense_rooms'))
BEGIN
    -- Check if unique index exists on apartments.zaaer_id
    IF EXISTS (SELECT * FROM sys.indexes 
               WHERE object_id = OBJECT_ID('apartments')
               AND (is_unique = 1 OR is_unique_constraint = 1)
               AND name IN (
                   SELECT name FROM sys.index_columns ic
                   INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                   WHERE ic.object_id = OBJECT_ID('apartments')
                   AND c.name = 'zaaer_id'
                   AND ic.key_ordinal = 1
               ))
    BEGIN
        -- Create foreign key constraint
        -- Note: SQL Server uses NO ACTION instead of RESTRICT
        ALTER TABLE expense_rooms
        ADD CONSTRAINT FK_ExpenseRooms_Apartments_ZaaerId
        FOREIGN KEY (zaaer_id) 
        REFERENCES apartments(zaaer_id)
        ON DELETE NO ACTION;
        
        PRINT '✅ Created FK_ExpenseRooms_Apartments_ZaaerId constraint (zaaer_id -> apartments.zaaer_id).';
    END
    ELSE
    BEGIN
        PRINT '⚠️ Cannot create foreign key: apartments.zaaer_id does not have a unique constraint/index.';
        PRINT '⚠️ Please ensure apartments.zaaer_id is unique before creating the foreign key.';
    END
END
ELSE
BEGIN
    PRINT '⚠️ FK_ExpenseRooms_Apartments_ZaaerId constraint already exists.';
END
GO

PRINT '✅ Expense rooms table updated successfully! Foreign Key changed from apartment_id to zaaer_id.';
GO

