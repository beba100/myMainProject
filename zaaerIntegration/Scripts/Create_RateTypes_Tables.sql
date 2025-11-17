-- =============================================
-- Table: rate_types
-- Description: Stores general information about a rate type (from "Add new rate type" form).
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'rate_types' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[rate_types] (
        [id] INT IDENTITY(1,1) NOT NULL,
        [hotel_id] INT NOT NULL,
        [short_code] NVARCHAR(50) NOT NULL,
        [title] NVARCHAR(255) NOT NULL,
        [status] BIT NOT NULL DEFAULT 1,
        [created_at] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME2(7) NULL,
        CONSTRAINT [PK_rate_types] PRIMARY KEY ([id]),
        CONSTRAINT [FK_rate_types_hotel_settings_hotel_id] FOREIGN KEY ([hotel_id]) REFERENCES [dbo].[hotel_settings] ([id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_rate_types_hotel_id_short_code] UNIQUE ([hotel_id], [short_code])
    );
    
    CREATE INDEX [IX_rate_types_hotel_id] ON [dbo].[rate_types] ([hotel_id]);
    
    PRINT 'Table rate_types created successfully.';
END
ELSE
BEGIN
    PRINT 'Table rate_types already exists.';
END
GO

-- =============================================
-- Table: rate_type_unit_items
-- Description: Stores specific rates for different unit types within a rate type.
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'rate_type_unit_items' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[rate_type_unit_items] (
        [id] INT IDENTITY(1,1) NOT NULL,
        [rate_type_id] INT NOT NULL,
        [unit_type_name] NVARCHAR(100) NOT NULL,
        [rate] DECIMAL(18, 2) NOT NULL,
        [is_enabled] BIT NOT NULL DEFAULT 0,
        [created_at] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME2(7) NULL,
        CONSTRAINT [PK_rate_type_unit_items] PRIMARY KEY ([id]),
        CONSTRAINT [FK_rate_type_unit_items_rate_types_rate_type_id] FOREIGN KEY ([rate_type_id]) REFERENCES [dbo].[rate_types] ([id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_rate_type_unit_items_rate_type_id_unit_type_name] UNIQUE ([rate_type_id], [unit_type_name])
    );
    
    CREATE INDEX [IX_rate_type_unit_items_rate_type_id] ON [dbo].[rate_type_unit_items] ([rate_type_id]);
    
    PRINT 'Table rate_type_unit_items created successfully.';
END
ELSE
BEGIN
    PRINT 'Table rate_type_unit_items already exists.';
END
GO

