-- Add due_date column to expenses table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('expenses') AND name = 'due_date')
BEGIN
    ALTER TABLE expenses
    ADD due_date DATE NULL;
    PRINT 'due_date column added to expenses table successfully.';
END
ELSE
BEGIN
    PRINT 'due_date column already exists in expenses table.';
END

