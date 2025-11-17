-- Add total_penalties and total_discounts columns to reservations table

IF NOT EXISTS (
    SELECT * FROM sys.columns WHERE Name = N'total_penalties' AND Object_ID = Object_ID(N'reservations')
)
BEGIN
    ALTER TABLE reservations ADD total_penalties DECIMAL(12,2) NULL;
    PRINT 'Column total_penalties added to reservations.';
END
ELSE
BEGIN
    PRINT 'Column total_penalties already exists.';
END

IF NOT EXISTS (
    SELECT * FROM sys.columns WHERE Name = N'total_discounts' AND Object_ID = Object_ID(N'reservations')
)
BEGIN
    ALTER TABLE reservations ADD total_discounts DECIMAL(12,2) NULL;
    PRINT 'Column total_discounts added to reservations.';
END
ELSE
BEGIN
    PRINT 'Column total_discounts already exists.';
END


