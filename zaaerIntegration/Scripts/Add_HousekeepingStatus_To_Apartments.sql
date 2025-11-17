-- Script to add housekeeping_status column to apartments table
-- This script is idempotent and can be run multiple times safely

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[apartments]') AND name = 'housekeeping_status')
BEGIN
    ALTER TABLE dbo.apartments ADD housekeeping_status NVARCHAR(50) NULL;
    
    PRINT 'Column housekeeping_status added successfully to apartments table.';
END
ELSE
BEGIN
    PRINT 'Column housekeeping_status already exists in apartments table.';
END
GO

