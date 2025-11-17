-- SQL Script to add a default building to existing databases
-- This script should be run on each tenant database (not the master DB)

-- Check if any buildings exist
IF NOT EXISTS (SELECT * FROM buildings)
BEGIN
    -- Insert a default building
    INSERT INTO buildings (hotel_id, building_name, building_number, address)
    VALUES (1, 'Default Building', 'DEFAULT', 'Default Address');
    
    PRINT 'Default building created successfully.';
END
ELSE
BEGIN
    PRINT 'Buildings already exist in the database.';
END
