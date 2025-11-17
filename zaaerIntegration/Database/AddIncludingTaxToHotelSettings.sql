-- SQL Script to add including_tax column to hotel_settings table
-- This field determines if prices include tax (including tax) or exclude tax (exclusive tax)

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE Name = N'including_tax' AND Object_ID = Object_ID(N'hotel_settings')
)
BEGIN
    ALTER TABLE hotel_settings ADD including_tax BIT NOT NULL DEFAULT 0;
    PRINT 'Column including_tax added to hotel_settings table with default value 0 (exclusive tax).';
END
ELSE
BEGIN
    PRINT 'Column including_tax already exists in hotel_settings table.';
END

-- Optional: Update existing hotels to set including_tax = 1 if needed
-- UPDATE hotel_settings SET including_tax = 1 WHERE hotel_id = 1;

PRINT 'including_tax column added successfully.';

