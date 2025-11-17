IF COL_LENGTH('dbo.reservations', 'total_extra') IS NULL
BEGIN
    ALTER TABLE dbo.reservations
        ADD total_extra DECIMAL(18, 2) NULL;
END;
GO

IF COL_LENGTH('dbo.reservations', 'check_in_date') IS NULL
BEGIN
    ALTER TABLE dbo.reservations
        ADD check_in_date DATETIME2(7) NULL;
END;
GO

IF COL_LENGTH('dbo.reservations', 'check_out_date') IS NULL
BEGIN
    ALTER TABLE dbo.reservations
        ADD check_out_date DATETIME2(7) NULL;
END;
GO

IF COL_LENGTH('dbo.reservations', 'departure_date') IS NULL
BEGIN
    ALTER TABLE dbo.reservations
        ADD departure_date DATETIME2(7) NULL;
END;
GO

