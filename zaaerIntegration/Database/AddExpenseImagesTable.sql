-- Create expense_images table for storing multiple images per expense
-- إنشاء جدول expense_images لحفظ صور متعددة لكل مصروف

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'expense_images') AND type in (N'U'))
BEGIN
    CREATE TABLE expense_images (
        expense_image_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        expense_id INT NOT NULL,
        image_path NVARCHAR(500) NOT NULL,
        original_filename NVARCHAR(255) NULL,
        file_size BIGINT NULL,
        content_type NVARCHAR(100) NULL,
        display_order INT NOT NULL DEFAULT 0,
        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ExpenseImages_Expenses 
            FOREIGN KEY (expense_id) REFERENCES expenses(expense_id) 
            ON DELETE CASCADE
    );
    
    -- Create index for performance
    CREATE INDEX IX_ExpenseImages_ExpenseId ON expense_images(expense_id);
    CREATE INDEX IX_ExpenseImages_DisplayOrder ON expense_images(expense_id, display_order);
    
    PRINT 'Table expense_images created successfully.';
END
ELSE
BEGIN
    PRINT 'Table expense_images already exists.';
END;
GO

