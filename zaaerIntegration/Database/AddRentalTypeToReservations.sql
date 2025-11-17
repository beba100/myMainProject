-- Add rental_type column to reservations table if not exists
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE Name = N'rental_type' AND Object_ID = Object_ID(N'reservations')
)
BEGIN
    ALTER TABLE reservations ADD rental_type NVARCHAR(20) NULL;
    PRINT 'Column rental_type added to reservations table.';
END
ELSE
BEGIN
    PRINT 'Column rental_type already exists in reservations table.';
END

-- Optional: set default to Daily where NULL
UPDATE reservations SET rental_type = 'Daily' WHERE rental_type IS NULL;


