-- SQL Script to add number_of_months column to reservations table
-- This field is used for monthly rental type reservations

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE Name = N'number_of_months' AND Object_ID = Object_ID(N'reservations')
)
BEGIN
    ALTER TABLE reservations ADD number_of_months INT NULL;
    PRINT 'Column number_of_months added to reservations table.';
END
ELSE
BEGIN
    PRINT 'Column number_of_months already exists in reservations table.';
END

-- Optional: Set default value for existing monthly reservations (if rental_type = 'Monthly')
-- UPDATE reservations 
-- SET number_of_months = CASE 
--     WHEN rental_type = 'Monthly' AND total_nights IS NOT NULL THEN CEILING(total_nights / 30.0)
--     ELSE NULL 
-- END
-- WHERE rental_type = 'Monthly' AND number_of_months IS NULL;

PRINT 'number_of_months column added successfully.';

