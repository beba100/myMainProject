-- Create tax_categories table (referenced by taxes.tax_id)

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tax_categories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tax_categories] (
        [id] INT IDENTITY(1,1) NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        [name_ar] NVARCHAR(100) NULL,
        [description] NVARCHAR(500) NULL,
        [status] NVARCHAR(50) NULL DEFAULT 'active',
        [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME2 NULL,
        CONSTRAINT [PK_tax_categories] PRIMARY KEY CLUSTERED ([id] ASC)
    );

    PRINT 'Table [dbo].[tax_categories] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[tax_categories] already exists.';
END
GO

-- Ensure foreign key from taxes.tax_id to tax_categories.id exists (create after both tables)
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_taxes_tax_categories'
)
BEGIN
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[taxes]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[taxes]
        WITH NOCHECK ADD CONSTRAINT [FK_taxes_tax_categories]
        FOREIGN KEY ([tax_id]) REFERENCES [dbo].[tax_categories]([id])
        ON DELETE SET NULL ON UPDATE NO ACTION;

        PRINT 'Foreign key FK_taxes_tax_categories created.';
    END
END
ELSE
BEGIN
    PRINT 'Foreign key FK_taxes_tax_categories already exists.';
END
GO

