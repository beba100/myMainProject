IF COL_LENGTH('dbo.reservation_units', 'departure_date') IS NULL
BEGIN
    ALTER TABLE dbo.reservation_units
        ADD departure_date DATETIME2(7) NULL;
END;
GO

