-- Script to create maintenances table
-- This script is idempotent and can be run multiple times safely

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'maintenances')
BEGIN
    CREATE TABLE dbo.maintenances (
        id INT IDENTITY(1,1) PRIMARY KEY,
        zaaer_id INT NULL,
        hotel_id INT NOT NULL,
        unit_id INT NOT NULL,
        user_id INT NOT NULL,
        from_date DATE NOT NULL,
        to_date DATE NOT NULL,
        reason NVARCHAR(100) NOT NULL,
        comment NVARCHAR(500) NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'active',
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2 NULL,
        
        CONSTRAINT FK_Maintenances_HotelSettings FOREIGN KEY (hotel_id) 
            REFERENCES dbo.hotel_settings(hotel_id) ON DELETE RESTRICT,
        CONSTRAINT FK_Maintenances_Apartments FOREIGN KEY (unit_id) 
            REFERENCES dbo.apartments(apartment_id) ON DELETE RESTRICT,
        CONSTRAINT FK_Maintenances_Users FOREIGN KEY (user_id) 
            REFERENCES dbo.users(user_id) ON DELETE RESTRICT
    );
    
    -- Create index on zaaer_id for faster lookups
    CREATE INDEX IX_Maintenances_ZaaerId ON dbo.maintenances(zaaer_id);
    CREATE INDEX IX_Maintenances_UnitId ON dbo.maintenances(unit_id);
    CREATE INDEX IX_Maintenances_HotelId ON dbo.maintenances(hotel_id);
    
    PRINT 'Table maintenances created successfully.';
END
ELSE
BEGIN
    PRINT 'Table maintenances already exists.';
END
GO

