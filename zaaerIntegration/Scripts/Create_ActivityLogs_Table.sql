-- =============================================
-- Script to Create activity_logs Table
-- =============================================

/* 
================================================================================
PARTNER NOTE / ملاحظة للشريك:
================================================================================
✓ NEW: Added zaaer_id field to this table (and all other tables) for Zaaer 
      system integration tracking.
✓ REMOVED: externalRefNo field has been deleted from all tables:
      - customers.external_ref_no
      - reservations.externalrefno
      - invoices.externalrefno
      - payment_receipts.externalrefno
      - refunds.externalrefno
      - credit_notes.externalrefno
================================================================================
*/

-- Check if table exists and drop it if needed (optional - comment out if you want to keep existing data)
-- IF OBJECT_ID('dbo.activity_logs', 'U') IS NOT NULL
-- BEGIN
--     DROP TABLE [dbo].[activity_logs];
--     PRINT 'Table [dbo].[activity_logs] dropped';
-- END
-- GO

-- Create activity_logs table if it doesn't exist
IF OBJECT_ID('dbo.activity_logs', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[activity_logs] (
        [log_id] INT IDENTITY(1,1) NOT NULL,
        [hotel_id] INT NOT NULL,
        [event_key] NVARCHAR(100) NOT NULL,
        [message] NVARCHAR(1000) NOT NULL,
        [reservation_id] INT NULL,
        [unit_id] INT NULL,
        [ref_type] NVARCHAR(50) NULL,
        [ref_id] INT NULL,
        [ref_no] NVARCHAR(100) NULL,
        [amount_from] DECIMAL(12, 2) NULL,
        [amount_to] DECIMAL(12, 2) NULL,
        [created_by] NVARCHAR(200) NULL,
        [created_at] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [zaaer_id] INT NULL,
        CONSTRAINT [PK_activity_logs] PRIMARY KEY ([log_id])
    );
    
    -- Create indexes
    CREATE INDEX [IX_ActivityLogs_HotelId] ON [dbo].[activity_logs]([hotel_id]);
    CREATE INDEX [IX_ActivityLogs_CreatedAt] ON [dbo].[activity_logs]([created_at]);
    CREATE INDEX [IX_ActivityLogs_ReservationId] ON [dbo].[activity_logs]([reservation_id]) WHERE [reservation_id] IS NOT NULL;
    CREATE INDEX [IX_ActivityLogs_EventKey] ON [dbo].[activity_logs]([event_key]);
    
    PRINT 'Table [dbo].[activity_logs] created successfully';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[activity_logs] already exists';
END
GO

-- Add Foreign Key constraint to hotel_settings if it doesn't exist
IF OBJECT_ID('dbo.hotel_settings', 'U') IS NOT NULL
    AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ActivityLogs_HotelSettings')
BEGIN
    ALTER TABLE [dbo].[activity_logs]
    ADD CONSTRAINT [FK_ActivityLogs_HotelSettings] 
    FOREIGN KEY ([hotel_id]) 
    REFERENCES [dbo].[hotel_settings]([id]) 
    ON DELETE RESTRICT;
    
    PRINT 'Foreign key constraint [FK_ActivityLogs_HotelSettings] added';
END
ELSE
BEGIN
    IF OBJECT_ID('dbo.hotel_settings', 'U') IS NULL
        PRINT 'Table [dbo].[hotel_settings] does not exist. Skipping foreign key constraint.';
    ELSE
        PRINT 'Foreign key constraint [FK_ActivityLogs_HotelSettings] already exists';
END
GO

-- Add Foreign Key constraint to reservations if it doesn't exist
IF OBJECT_ID('dbo.reservations', 'U') IS NOT NULL
    AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ActivityLogs_Reservations')
BEGIN
    ALTER TABLE [dbo].[activity_logs]
    ADD CONSTRAINT [FK_ActivityLogs_Reservations] 
    FOREIGN KEY ([reservation_id]) 
    REFERENCES [dbo].[reservations]([reservation_id]) 
    ON DELETE SET NULL;
    
    PRINT 'Foreign key constraint [FK_ActivityLogs_Reservations] added';
END
ELSE
BEGIN
    IF OBJECT_ID('dbo.reservations', 'U') IS NULL
        PRINT 'Table [dbo].[reservations] does not exist. Skipping foreign key constraint.';
    ELSE
        PRINT 'Foreign key constraint [FK_ActivityLogs_Reservations] already exists';
END
GO

-- Add Foreign Key constraint to reservation_units if it doesn't exist
IF OBJECT_ID('dbo.reservation_units', 'U') IS NOT NULL
    AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ActivityLogs_ReservationUnits')
BEGIN
    ALTER TABLE [dbo].[activity_logs]
    ADD CONSTRAINT [FK_ActivityLogs_ReservationUnits] 
    FOREIGN KEY ([unit_id]) 
    REFERENCES [dbo].[reservation_units]([unit_id]) 
    ON DELETE SET NULL;
    
    PRINT 'Foreign key constraint [FK_ActivityLogs_ReservationUnits] added';
END
ELSE
BEGIN
    IF OBJECT_ID('dbo.reservation_units', 'U') IS NULL
        PRINT 'Table [dbo].[reservation_units] does not exist. Skipping foreign key constraint.';
    ELSE
        PRINT 'Foreign key constraint [FK_ActivityLogs_ReservationUnits] already exists';
END
GO

PRINT 'Script completed successfully.';
GO

