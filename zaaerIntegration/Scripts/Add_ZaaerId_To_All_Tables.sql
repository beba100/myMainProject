-- =============================================
-- Script to add zaaer_id column to all tables
-- =============================================
-- Note: customers, apartments, banks, and buildings already have zaaer_id
-- This script adds zaaer_id to all remaining tables

USE [YourDatabaseName]; -- Replace with your database name
GO

-- Reservations
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.reservations') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[reservations]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[reservations]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[reservations]';
END
GO

-- Invoices
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.invoices') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[invoices]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[invoices]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[invoices]';
END
GO

-- Payment Receipts
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.payment_receipts') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[payment_receipts]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[payment_receipts]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[payment_receipts]';
END
GO

-- Refunds
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.refunds') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[refunds]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[refunds]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[refunds]';
END
GO

-- Credit Notes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.credit_notes') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[credit_notes]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[credit_notes]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[credit_notes]';
END
GO

-- Floors
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.floors') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[floors]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[floors]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[floors]';
END
GO

-- Room Types
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.room_types') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[room_types]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[room_types]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[room_types]';
END
GO

-- Room Type Rates
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.room_type_rates') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[room_type_rates]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[room_type_rates]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[room_type_rates]';
END
GO

-- Reservation Units
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.reservation_units') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[reservation_units]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[reservation_units]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[reservation_units]';
END
GO

-- Reservation Unit Day Rates
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.reservation_unit_day_rates') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[reservation_unit_day_rates]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[reservation_unit_day_rates]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[reservation_unit_day_rates]';
END
GO

-- Reservation Unit Swaps
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.reservation_unit_swaps') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[reservation_unit_swaps]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[reservation_unit_swaps]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[reservation_unit_swaps]';
END
GO

-- Corporate Customers
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.corporate_customers') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[corporate_customers]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[corporate_customers]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[corporate_customers]';
END
GO

-- Customer Accounts
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.customer_accounts') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[customer_accounts]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[customer_accounts]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[customer_accounts]';
END
GO

-- Customer Identifications
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.customer_identifications') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[customer_identifications]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[customer_identifications]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[customer_identifications]';
END
GO

-- Customer Transactions
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.customer_transactions') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[customer_transactions]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[customer_transactions]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[customer_transactions]';
END
GO

-- Discounts
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.discounts') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[discounts]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[discounts]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[discounts]';
END
GO

-- Expenses (check if not already added)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.expenses') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[expenses]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[expenses]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[expenses]';
END
GO

-- Hotel Settings
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.hotel_settings') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[hotel_settings]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[hotel_settings]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[hotel_settings]';
END
GO

-- Seasonal Rates
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.seasonal_rates') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[seasonal_rates]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[seasonal_rates]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[seasonal_rates]';
END
GO

-- Seasonal Rate Items
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.seasonal_rate_items') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[seasonal_rate_items]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[seasonal_rate_items]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[seasonal_rate_items]';
END
GO

-- Rate Types
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.rate_types') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[rate_types]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[rate_types]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[rate_types]';
END
GO

-- Rate Type Unit Items
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.rate_type_unit_items') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[rate_type_unit_items]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[rate_type_unit_items]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[rate_type_unit_items]';
END
GO

-- ZATCA Details
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.zatca_details') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[zatca_details]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[zatca_details]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[zatca_details]';
END
GO

-- NTMP Details
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ntmp_details') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[ntmp_details]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[ntmp_details]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[ntmp_details]';
END
GO

-- Shomoos Details
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.shomoos_details') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[shomoos_details]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[shomoos_details]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[shomoos_details]';
END
GO

-- Integration Responses
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.integration_responses') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[integration_responses]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[integration_responses]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[integration_responses]';
END
GO

-- Activity Logs
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.activity_logs') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[activity_logs]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[activity_logs]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[activity_logs]';
END
GO

-- Users
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.users') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[users]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[users]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[users]';
END
GO

-- Roles
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.roles') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[roles]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[roles]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[roles]';
END
GO

-- Permissions
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.permissions') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[permissions]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[permissions]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[permissions]';
END
GO

-- Role Permissions
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.role_permissions') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[role_permissions]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[role_permissions]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[role_permissions]';
END
GO

-- Payment Methods
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.payment_methods') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[payment_methods]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[payment_methods]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[payment_methods]';
END
GO

-- Penalties
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.penalties') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[penalties]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[penalties]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[penalties]';
END
GO

-- Visit Purposes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.visitpurpose') AND name = 'zaaer_id')
BEGIN
    ALTER TABLE [dbo].[visitpurpose]
    ADD [zaaer_id] INT NULL;
    PRINT 'Column [zaaer_id] added to [dbo].[visitpurpose]';
END
ELSE
BEGIN
    PRINT 'Column [zaaer_id] already exists in [dbo].[visitpurpose]';
END
GO

PRINT '========================================';
PRINT 'Script completed successfully!';
PRINT '========================================';
GO

