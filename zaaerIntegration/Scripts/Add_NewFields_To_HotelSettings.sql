-- Script to add new fields to hotel_settings table
-- This script is idempotent and can be run multiple times safely

-- Add tax_number column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'tax_number')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD tax_number NVARCHAR(50) NULL;
    PRINT 'Column tax_number added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column tax_number already exists in hotel_settings table.';
END
GO

-- Add cr_number column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'cr_number')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD cr_number NVARCHAR(50) NULL;
    PRINT 'Column cr_number added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column cr_number already exists in hotel_settings table.';
END
GO

-- Add country_code column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'country_code')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD country_code NVARCHAR(10) NULL;
    PRINT 'Column country_code added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column country_code already exists in hotel_settings table.';
END
GO

-- Add city column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'city')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD city NVARCHAR(100) NULL;
    PRINT 'Column city added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column city already exists in hotel_settings table.';
END
GO

-- Add webhooks_hotel_code column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'webhooks_hotel_code')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD webhooks_hotel_code NVARCHAR(50) NULL;
    PRINT 'Column webhooks_hotel_code added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column webhooks_hotel_code already exists in hotel_settings table.';
END
GO

-- Add contact_person column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'contact_person')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD contact_person NVARCHAR(100) NULL;
    PRINT 'Column contact_person added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column contact_person already exists in hotel_settings table.';
END
GO

-- Add location column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'location')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD location NVARCHAR(500) NULL;
    PRINT 'Column location added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column location already exists in hotel_settings table.';
END
GO

-- Add latitude column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'latitude')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD latitude NVARCHAR(50) NULL;
    PRINT 'Column latitude added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column latitude already exists in hotel_settings table.';
END
GO

-- Add longitude column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'longitude')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD longitude NVARCHAR(50) NULL;
    PRINT 'Column longitude added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column longitude already exists in hotel_settings table.';
END
GO

-- Add enabled column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'enabled')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD enabled INT NOT NULL DEFAULT 1;
    PRINT 'Column enabled added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column enabled already exists in hotel_settings table.';
END
GO

-- Add total_rooms column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'total_rooms')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD total_rooms INT NOT NULL DEFAULT 0;
    PRINT 'Column total_rooms added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column total_rooms already exists in hotel_settings table.';
END
GO

-- Add property_type column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'property_type')
BEGIN
    ALTER TABLE dbo.hotel_settings ADD property_type NVARCHAR(50) NULL;
    PRINT 'Column property_type added successfully to hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column property_type already exists in hotel_settings table.';
END
GO

