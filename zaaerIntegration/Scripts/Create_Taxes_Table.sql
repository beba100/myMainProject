-- Create taxes table for Zaaer integration
-- This table stores tax configurations for hotels

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[taxes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[taxes] (
        [id] INT IDENTITY(1,1) NOT NULL,
        [zaaer_id] INT NULL,
        [hotel_id] INT NOT NULL,
        [tax_id] INT NULL,
        [tax_name] NVARCHAR(100) NOT NULL,
        [tax_type] NVARCHAR(50) NOT NULL,
        [tax_rate] DECIMAL(5,2) NOT NULL,
        [method] NVARCHAR(50) NULL,
        [enabled] BIT NOT NULL DEFAULT 1,
        [tax_code] NVARCHAR(50) NULL,
        [apply_on] NVARCHAR(100) NULL,
        [status] NVARCHAR(50) NULL,
        [description] NVARCHAR(500) NULL,
        [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME2 NULL,
        CONSTRAINT [PK_taxes] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_taxes_hotel_settings] FOREIGN KEY ([hotel_id]) 
            REFERENCES [dbo].[hotel_settings] ([hotel_id]) 
            ON DELETE NO ACTION 
            ON UPDATE NO ACTION
    );

    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_taxes_hotel_id] ON [dbo].[taxes] ([hotel_id]);
    CREATE NONCLUSTERED INDEX [IX_taxes_zaaer_id] ON [dbo].[taxes] ([zaaer_id]) WHERE [zaaer_id] IS NOT NULL;
    CREATE NONCLUSTERED INDEX [IX_taxes_tax_id] ON [dbo].[taxes] ([tax_id]) WHERE [tax_id] IS NOT NULL;
    CREATE NONCLUSTERED INDEX [IX_taxes_enabled] ON [dbo].[taxes] ([enabled]);
    CREATE NONCLUSTERED INDEX [IX_taxes_tax_type] ON [dbo].[taxes] ([tax_type]);
    
    -- Add FK to tax_categories if it exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tax_categories]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_taxes_tax_categories')
        BEGIN
            ALTER TABLE [dbo].[taxes]
            WITH NOCHECK ADD CONSTRAINT [FK_taxes_tax_categories]
            FOREIGN KEY ([tax_id]) REFERENCES [dbo].[tax_categories]([id])
            ON DELETE SET NULL ON UPDATE NO ACTION;
        END
    END
    
    -- Unique constraint: one tax per zaaer_id per hotel
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_taxes_zaaer_id_hotel_id] 
        ON [dbo].[taxes] ([zaaer_id], [hotel_id]) 
        WHERE [zaaer_id] IS NOT NULL;

    PRINT 'Table [dbo].[taxes] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[taxes] already exists.';
    
    -- Add new columns if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[taxes]') AND name = 'tax_id')
    BEGIN
        ALTER TABLE [dbo].[taxes] ADD [tax_id] INT NULL;
        CREATE NONCLUSTERED INDEX [IX_taxes_tax_id] ON [dbo].[taxes] ([tax_id]) WHERE [tax_id] IS NOT NULL;
        PRINT 'Column [tax_id] added to [dbo].[taxes].';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[taxes]') AND name = 'method')
    BEGIN
        ALTER TABLE [dbo].[taxes] ADD [method] NVARCHAR(50) NULL;
        PRINT 'Column [method] added to [dbo].[taxes].';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[taxes]') AND name = 'enabled')
    BEGIN
        ALTER TABLE [dbo].[taxes] ADD [enabled] BIT NOT NULL DEFAULT 1;
        CREATE NONCLUSTERED INDEX [IX_taxes_enabled] ON [dbo].[taxes] ([enabled]);
        PRINT 'Column [enabled] added to [dbo].[taxes].';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[taxes]') AND name = 'tax_code')
    BEGIN
        ALTER TABLE [dbo].[taxes] ADD [tax_code] NVARCHAR(50) NULL;
        PRINT 'Column [tax_code] added to [dbo].[taxes].';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[taxes]') AND name = 'apply_on')
    BEGIN
        ALTER TABLE [dbo].[taxes] ADD [apply_on] NVARCHAR(100) NULL;
        PRINT 'Column [apply_on] added to [dbo].[taxes].';
    END
    
    -- Make status nullable if it's not already
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[taxes]') AND name = 'status' AND is_nullable = 0)
    BEGIN
        ALTER TABLE [dbo].[taxes] ALTER COLUMN [status] NVARCHAR(50) NULL;
        PRINT 'Column [status] made nullable in [dbo].[taxes].';
    END

    -- Ensure FK exists after adding new columns
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tax_categories]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_taxes_tax_categories')
        BEGIN
            ALTER TABLE [dbo].[taxes]
            WITH NOCHECK ADD CONSTRAINT [FK_taxes_tax_categories]
            FOREIGN KEY ([tax_id]) REFERENCES [dbo].[tax_categories]([id])
            ON DELETE SET NULL ON UPDATE NO ACTION;
            PRINT 'Foreign key FK_taxes_tax_categories created.';
        END
    END
END
GO

