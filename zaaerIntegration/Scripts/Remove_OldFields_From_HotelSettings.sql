-- Script to remove old fields from hotel_settings table
-- This script removes fields that are no longer needed based on the new structure
-- WARNING: This will permanently delete data in these columns. Make sure to backup first!

-- Helper function to drop default constraints
-- Drop default constraint for vat_percent column
DECLARE @constraint_name_vat NVARCHAR(200);
SELECT @constraint_name_vat = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('dbo.hotel_settings') 
AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.hotel_settings') AND name = 'vat_percent');

IF @constraint_name_vat IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.hotel_settings DROP CONSTRAINT ' + @constraint_name_vat);
    PRINT 'Default constraint for vat_percent dropped successfully.';
END
GO

-- Remove vat_percent column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'vat_percent')
BEGIN
    ALTER TABLE dbo.hotel_settings DROP COLUMN vat_percent;
    PRINT 'Column vat_percent removed successfully from hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column vat_percent does not exist in hotel_settings table.';
END
GO

-- Drop default constraint for lodging_tax column
DECLARE @constraint_name_lodging NVARCHAR(200);
SELECT @constraint_name_lodging = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('dbo.hotel_settings') 
AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.hotel_settings') AND name = 'lodging_tax');

IF @constraint_name_lodging IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.hotel_settings DROP CONSTRAINT ' + @constraint_name_lodging);
    PRINT 'Default constraint for lodging_tax dropped successfully.';
END
GO

-- Remove lodging_tax column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'lodging_tax')
BEGIN
    ALTER TABLE dbo.hotel_settings DROP COLUMN lodging_tax;
    PRINT 'Column lodging_tax removed successfully from hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column lodging_tax does not exist in hotel_settings table.';
END
GO

-- Drop default constraint for including_tax column
DECLARE @constraint_name_including NVARCHAR(200);
SELECT @constraint_name_including = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('dbo.hotel_settings') 
AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.hotel_settings') AND name = 'including_tax');

IF @constraint_name_including IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.hotel_settings DROP CONSTRAINT ' + @constraint_name_including);
    PRINT 'Default constraint for including_tax dropped successfully.';
END
GO

-- Remove including_tax column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'including_tax')
BEGIN
    ALTER TABLE dbo.hotel_settings DROP COLUMN including_tax;
    PRINT 'Column including_tax removed successfully from hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column including_tax does not exist in hotel_settings table.';
END
GO

-- Remove company_vatno column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'company_vatno')
BEGIN
    ALTER TABLE dbo.hotel_settings DROP COLUMN company_vatno;
    PRINT 'Column company_vatno removed successfully from hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column company_vatno does not exist in hotel_settings table.';
END
GO

-- Remove company_crn column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'company_crn')
BEGIN
    ALTER TABLE dbo.hotel_settings DROP COLUMN company_crn;
    PRINT 'Column company_crn removed successfully from hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column company_crn does not exist in hotel_settings table.';
END
GO

-- Remove webhooks_hotel_code column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'webhooks_hotel_code')
BEGIN
    ALTER TABLE dbo.hotel_settings DROP COLUMN webhooks_hotel_code;
    PRINT 'Column webhooks_hotel_code removed successfully from hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column webhooks_hotel_code does not exist in hotel_settings table.';
END
GO

-- Remove location column (if it exists separately from address)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND name = 'location')
BEGIN
    ALTER TABLE dbo.hotel_settings DROP COLUMN location;
    PRINT 'Column location removed successfully from hotel_settings table.';
END
ELSE
BEGIN
    PRINT 'Column location does not exist in hotel_settings table.';
END
GO

PRINT 'Script completed. All old fields have been processed.';
GO

