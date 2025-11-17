-- Script to allow NULL values for columns in hotel_settings table
-- This script aligns the database schema with the model definition
-- This script is idempotent and can be run multiple times safely

PRINT 'Starting to allow NULL values for hotel_settings columns...';
PRINT 'بدء السماح بقيم NULL لأعمدة hotel_settings...';
GO

-- Allow NULL for hotel_code
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'hotel_code' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN hotel_code NVARCHAR(50) NULL;
    PRINT 'Column hotel_code now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column hotel_code already allows NULL values.';
END
GO

-- Allow NULL for hotel_name
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'hotel_name' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN hotel_name NVARCHAR(50) NULL;
    PRINT 'Column hotel_name now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column hotel_name already allows NULL values.';
END
GO

-- Allow NULL for default_currency
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'default_currency' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN default_currency NVARCHAR(10) NULL;
    PRINT 'Column default_currency now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column default_currency already allows NULL values.';
END
GO

-- Allow NULL for company_name
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'company_name' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN company_name NVARCHAR(200) NULL;
    PRINT 'Column company_name now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column company_name already allows NULL values.';
END
GO

-- Allow NULL for logo_url (THIS IS THE CRITICAL ONE)
-- First, drop any default constraint on logo_url if it exists
DECLARE @logo_url_constraint_name NVARCHAR(200);
SELECT @logo_url_constraint_name = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('dbo.hotel_settings') 
AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.hotel_settings') AND name = 'logo_url');

IF @logo_url_constraint_name IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.hotel_settings DROP CONSTRAINT ' + @logo_url_constraint_name);
    PRINT 'Default constraint for logo_url dropped: ' + @logo_url_constraint_name;
END
GO

-- Now allow NULL for logo_url
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'logo_url' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN logo_url NVARCHAR(500) NULL;
    PRINT 'Column logo_url now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column logo_url already allows NULL values.';
END
GO

-- Allow NULL for phone
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'phone' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN phone NVARCHAR(50) NULL;
    PRINT 'Column phone now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column phone already allows NULL values.';
END
GO

-- Allow NULL for email
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'email' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN email NVARCHAR(100) NULL;
    PRINT 'Column email now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column email already allows NULL values.';
END
GO

-- Allow NULL for address
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'address' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN address NVARCHAR(500) NULL;
    PRINT 'Column address now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column address already allows NULL values.';
END
GO

-- Ensure tax_number allows NULL (should already be nullable, but just in case)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'tax_number' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN tax_number NVARCHAR(50) NULL;
    PRINT 'Column tax_number now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column tax_number already allows NULL values.';
END
GO

-- Ensure cr_number allows NULL
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'cr_number' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN cr_number NVARCHAR(50) NULL;
    PRINT 'Column cr_number now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column cr_number already allows NULL values.';
END
GO

-- Ensure country_code allows NULL
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'country_code' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN country_code NVARCHAR(10) NULL;
    PRINT 'Column country_code now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column country_code already allows NULL values.';
END
GO

-- Ensure city allows NULL
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'city' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN city NVARCHAR(100) NULL;
    PRINT 'Column city now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column city already allows NULL values.';
END
GO

-- Ensure contact_person allows NULL
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'contact_person' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN contact_person NVARCHAR(100) NULL;
    PRINT 'Column contact_person now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column contact_person already allows NULL values.';
END
GO

-- Ensure latitude allows NULL
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'latitude' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN latitude NVARCHAR(50) NULL;
    PRINT 'Column latitude now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column latitude already allows NULL values.';
END
GO

-- Ensure longitude allows NULL
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'longitude' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN longitude NVARCHAR(50) NULL;
    PRINT 'Column longitude now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column longitude already allows NULL values.';
END
GO

-- Ensure property_type allows NULL
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'property_type' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN property_type NVARCHAR(50) NULL;
    PRINT 'Column property_type now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column property_type already allows NULL values.';
END
GO

-- Ensure created_at allows NULL
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'created_at' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN created_at DATETIME2 NULL;
    PRINT 'Column created_at now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column created_at already allows NULL values.';
END
GO

-- Ensure zaaer_id allows NULL
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'zaaer_id' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.hotel_settings ALTER COLUMN zaaer_id INT NULL;
    PRINT 'Column zaaer_id now allows NULL values.';
END
ELSE
BEGIN
    PRINT 'Column zaaer_id already allows NULL values.';
END
GO

PRINT '';
PRINT 'Script completed. All columns now allow NULL values where specified in the model.';
PRINT 'تم إكمال السكريبت. جميع الأعمدة الآن تسمح بقيم NULL كما هو محدد في النموذج.';
GO

