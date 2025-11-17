-- SQL Script to add building_id column to floors table
-- This script should be run on each tenant database (not the master DB)

-- Check if building_id column doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'floors' AND COLUMN_NAME = 'building_id')
BEGIN
    -- Add the building_id column
    ALTER TABLE floors 
    ADD building_id INT NOT NULL DEFAULT 1;
    
    -- Add foreign key constraint
    ALTER TABLE floors
    ADD CONSTRAINT FK_Floors_Buildings
    FOREIGN KEY (building_id) REFERENCES buildings(building_id);
    
    PRINT 'building_id column added to floors table successfully.';
END
ELSE
BEGIN
    PRINT 'building_id column already exists in floors table.';
END

-- Optional: Update existing floors to reference a default building
-- You may want to create a default building first if none exists
-- UPDATE floors SET building_id = 1 WHERE building_id IS NULL;
