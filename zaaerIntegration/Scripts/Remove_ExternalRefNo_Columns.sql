-- =============================================
-- Script to Remove external_ref_no columns from all tables
-- =============================================

-- Remove index and column from customers table
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.customers') AND name = 'IX_customers_external_ref_no')
BEGIN
    DROP INDEX [IX_customers_external_ref_no] ON [dbo].[customers];
    PRINT 'Index [IX_customers_external_ref_no] dropped from [dbo].[customers]';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.customers') AND name = 'external_ref_no')
BEGIN
    ALTER TABLE [dbo].[customers] DROP COLUMN [external_ref_no];
    PRINT 'Column [external_ref_no] removed from [dbo].[customers]';
END
ELSE
BEGIN
    PRINT 'Column [external_ref_no] does not exist in [dbo].[customers]';
END
GO

-- Remove index and column from reservations table
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.reservations') AND name = 'IX_reservations_externalrefno')
BEGIN
    DROP INDEX [IX_reservations_externalrefno] ON [dbo].[reservations];
    PRINT 'Index [IX_reservations_externalrefno] dropped from [dbo].[reservations]';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.reservations') AND name = 'externalrefno')
BEGIN
    ALTER TABLE [dbo].[reservations] DROP COLUMN [externalrefno];
    PRINT 'Column [externalrefno] removed from [dbo].[reservations]';
END
ELSE
BEGIN
    PRINT 'Column [externalrefno] does not exist in [dbo].[reservations]';
END
GO

-- Remove index and column from invoices table
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.invoices') AND name = 'IX_invoices_externalrefno')
BEGIN
    DROP INDEX [IX_invoices_externalrefno] ON [dbo].[invoices];
    PRINT 'Index [IX_invoices_externalrefno] dropped from [dbo].[invoices]';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.invoices') AND name = 'externalrefno')
BEGIN
    ALTER TABLE [dbo].[invoices] DROP COLUMN [externalrefno];
    PRINT 'Column [externalrefno] removed from [dbo].[invoices]';
END
ELSE
BEGIN
    PRINT 'Column [externalrefno] does not exist in [dbo].[invoices]';
END
GO

-- Remove index and column from payment_receipts table
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.payment_receipts') AND name = 'IX_payment_receipts_externalrefno')
BEGIN
    DROP INDEX [IX_payment_receipts_externalrefno] ON [dbo].[payment_receipts];
    PRINT 'Index [IX_payment_receipts_externalrefno] dropped from [dbo].[payment_receipts]';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.payment_receipts') AND name = 'externalrefno')
BEGIN
    ALTER TABLE [dbo].[payment_receipts] DROP COLUMN [externalrefno];
    PRINT 'Column [externalrefno] removed from [dbo].[payment_receipts]';
END
ELSE
BEGIN
    PRINT 'Column [externalrefno] does not exist in [dbo].[payment_receipts]';
END
GO

-- Remove index and column from refunds table
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.refunds') AND name = 'IX_refunds_externalrefno')
BEGIN
    DROP INDEX [IX_refunds_externalrefno] ON [dbo].[refunds];
    PRINT 'Index [IX_refunds_externalrefno] dropped from [dbo].[refunds]';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.refunds') AND name = 'externalrefno')
BEGIN
    ALTER TABLE [dbo].[refunds] DROP COLUMN [externalrefno];
    PRINT 'Column [externalrefno] removed from [dbo].[refunds]';
END
ELSE
BEGIN
    PRINT 'Column [externalrefno] does not exist in [dbo].[refunds]';
END
GO

-- Remove index and column from credit_notes table
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.credit_notes') AND name = 'IX_credit_notes_externalrefno')
BEGIN
    DROP INDEX [IX_credit_notes_externalrefno] ON [dbo].[credit_notes];
    PRINT 'Index [IX_credit_notes_externalrefno] dropped from [dbo].[credit_notes]';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.credit_notes') AND name = 'externalrefno')
BEGIN
    ALTER TABLE [dbo].[credit_notes] DROP COLUMN [externalrefno];
    PRINT 'Column [externalrefno] removed from [dbo].[credit_notes]';
END
ELSE
BEGIN
    PRINT 'Column [externalrefno] does not exist in [dbo].[credit_notes]';
END
GO

PRINT 'Script completed successfully.';

