-- =============================================
-- Indexes for Apartment Status Update Performance
-- =============================================
-- This script creates indexes to optimize the performance of:
-- 1. Application-level apartment status updates
-- 2. Database trigger for apartment status updates
--
-- Indexes created:
-- - apartments.zaaer_id: Used in JOIN with reservation_units.apartment_id
-- - reservation_units.apartment_id: Used in JOIN with apartments.zaaer_id
-- - reservation_units.reservation_id: Used in WHERE clauses to find units by reservation
-- - reservation_units.status: Used in WHERE clauses and CASE statements
--
-- These indexes significantly improve query performance, especially for:
-- - Finding apartments by zaaer_id (application code)
-- - Joining reservation_units with apartments (trigger)
-- - Finding reservation units by reservation_id (application code)
-- =============================================

-- Check if indexes already exist before creating (idempotent script)
PRINT '========================================';
PRINT 'Creating indexes for apartment status update performance...';
PRINT '========================================';
GO

-- =============================================
-- Index on apartments.zaaer_id
-- =============================================
-- Purpose: Optimize JOIN between apartments and reservation_units
-- Used in: Trigger (JOIN apartments ON zaaer_id = apartment_id)
--          Application code (FindAsync apartments by zaaer_id)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Apartments_ZaaerId' AND object_id = OBJECT_ID('dbo.apartments'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Apartments_ZaaerId]
    ON [dbo].[apartments] ([zaaer_id])
    WHERE [zaaer_id] IS NOT NULL; -- Filtered index (only index non-null values)
    
    PRINT '✓ Index IX_Apartments_ZaaerId created successfully.';
END
ELSE
BEGIN
    PRINT 'ℹ Index IX_Apartments_ZaaerId already exists.';
END
GO

-- =============================================
-- Index on reservation_units.apartment_id
-- =============================================
-- Purpose: Optimize JOIN between reservation_units and apartments
-- Used in: Trigger (JOIN apartments ON zaaer_id = apartment_id)
--          Application code (Grouping units by apartment_id)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ApartmentId' AND object_id = OBJECT_ID('dbo.reservation_units'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReservationUnits_ApartmentId]
    ON [dbo].[reservation_units] ([apartment_id])
    WHERE [apartment_id] IS NOT NULL AND [apartment_id] > 0; -- Filtered index (only valid apartment_ids)
    
    PRINT '✓ Index IX_ReservationUnits_ApartmentId created successfully.';
END
ELSE
BEGIN
    PRINT 'ℹ Index IX_ReservationUnits_ApartmentId already exists.';
END
GO

-- =============================================
-- Index on reservation_units.reservation_id
-- =============================================
-- Purpose: Optimize queries to find units by reservation
-- Used in: Application code (FindAsync units by reservation_id)
-- Note: This is critical for the UpdateApartmentStatusFromReservationUnitsAsync method
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ReservationId' AND object_id = OBJECT_ID('dbo.reservation_units'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReservationUnits_ReservationId]
    ON [dbo].[reservation_units] ([reservation_id]);
    
    PRINT '✓ Index IX_ReservationUnits_ReservationId created successfully.';
END
ELSE
BEGIN
    PRINT 'ℹ Index IX_ReservationUnits_ReservationId already exists.';
END
GO

-- =============================================
-- Composite Index on reservation_units (apartment_id, status)
-- =============================================
-- Purpose: Optimize queries that filter/group by apartment_id and status
-- Used in: Application code (GroupBy apartment_id, OrderBy status)
-- Note: This covers both the JOIN and status filtering in one index
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationUnits_ApartmentId_Status' AND object_id = OBJECT_ID('dbo.reservation_units'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReservationUnits_ApartmentId_Status]
    ON [dbo].[reservation_units] ([apartment_id], [status])
    INCLUDE ([created_at]) -- Include created_at for OrderBy operations
    WHERE [apartment_id] IS NOT NULL AND [apartment_id] > 0; -- Filtered index
    
    PRINT '✓ Index IX_ReservationUnits_ApartmentId_Status created successfully.';
END
ELSE
BEGIN
    PRINT 'ℹ Index IX_ReservationUnits_ApartmentId_Status already exists.';
END
GO

-- =============================================
-- Index on apartments.status (optional but recommended)
-- =============================================
-- Purpose: Optimize queries that filter apartments by status
-- Used in: General apartment queries, status-based filtering
-- Note: This is useful for other parts of the application too
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Apartments_Status' AND object_id = OBJECT_ID('dbo.apartments'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Apartments_Status]
    ON [dbo].[apartments] ([status])
    WHERE [status] IS NOT NULL; -- Filtered index
    
    PRINT '✓ Index IX_Apartments_Status created successfully.';
END
ELSE
BEGIN
    PRINT 'ℹ Index IX_Apartments_Status already exists.';
END
GO

PRINT '========================================';
PRINT 'Index creation completed!';
PRINT '========================================';
PRINT '';
PRINT 'Performance Benefits:';
PRINT '  - Faster JOINs between apartments and reservation_units';
PRINT '  - Faster queries to find units by reservation_id';
PRINT '  - Faster grouping and filtering by apartment_id and status';
PRINT '  - Improved trigger performance';
PRINT '  - Improved application-level query performance';
PRINT '';
GO

