-- =====================================================
-- Add ExternalRefNo column to reservations table
-- =====================================================
-- Description: Adds the external_ref_no column to the reservations table
--   to store external reference numbers from Zaaer integration system
--
-- Note: This column is usually the same as zaaer_id but kept separate for flexibility
-- =====================================================

USE [YourDatabaseName]; -- Change this to your actual database name
GO

-- Add external_ref_no column if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.reservations') AND name = 'external_ref_no')
BEGIN
    ALTER TABLE [dbo].[reservations]
    ADD [external_ref_no] [int] NULL;
    PRINT 'Column external_ref_no added successfully to reservations table.';
END
ELSE
    PRINT 'Column external_ref_no already exists in reservations table.';
GO

-- Optionally, you can copy existing zaaer_id values to external_ref_no for existing records
-- UPDATE [dbo].[reservations]
-- SET [external_ref_no] = [zaaer_id]
-- WHERE [zaaer_id] IS NOT NULL AND [external_ref_no] IS NULL;
-- GO

PRINT 'Script completed successfully.';
GO

