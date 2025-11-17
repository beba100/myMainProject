-- SQL Script to create room_type_rates table
-- جدول أسعار أنواع الغرف

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='room_type_rates' and xtype='U')
BEGIN
    CREATE TABLE room_type_rates (
        rate_id INT PRIMARY KEY IDENTITY(1,1),
        roomtype_id INT NOT NULL,
        hotel_id INT NOT NULL,
        
        -- Daily Rates
        daily_rate_low_weekdays DECIMAL(12,2) NULL,
        daily_rate_high_weekdays DECIMAL(12,2) NULL,
        daily_rate_min DECIMAL(12,2) NULL,
        
        -- Monthly Rates
        monthly_rate DECIMAL(12,2) NULL,
        monthly_rate_min DECIMAL(12,2) NULL,
        
        -- OTA Rates
        ota_rate_low_weekdays DECIMAL(12,2) NULL,
        ota_rate_high_weekdays DECIMAL(12,2) NULL,
        
        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        updated_at DATETIME NULL,
        
        CONSTRAINT FK_RoomTypeRates_RoomTypes FOREIGN KEY (roomtype_id) REFERENCES room_types(roomtype_id) ON DELETE CASCADE,
        CONSTRAINT FK_RoomTypeRates_HotelSettings FOREIGN KEY (hotel_id) REFERENCES hotel_settings(hotel_id) ON DELETE CASCADE,
        CONSTRAINT IX_RoomTypeRates_RoomTypeId_HotelId UNIQUE (roomtype_id, hotel_id)
    );
    
    PRINT 'room_type_rates table created successfully.';
END
ELSE
BEGIN
    PRINT 'room_type_rates table already exists.';
END

-- Banks table: add missing fields required for Zaaer integration (safe/idempotent)
IF COL_LENGTH('dbo.banks', 'account_number') IS NULL
BEGIN
    ALTER TABLE dbo.banks ADD account_number NVARCHAR(50) NULL;
END
IF COL_LENGTH('dbo.banks', 'iban') IS NULL
BEGIN
    ALTER TABLE dbo.banks ADD iban NVARCHAR(50) NULL;
END
IF COL_LENGTH('dbo.banks', 'currency_code') IS NULL
BEGIN
    ALTER TABLE dbo.banks ADD currency_code NVARCHAR(10) NULL;
END
IF COL_LENGTH('dbo.banks', 'description') IS NULL
BEGIN
    ALTER TABLE dbo.banks ADD description NVARCHAR(500) NULL;
END

-- Create expenses table (idempotent)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='expenses' and xtype='U')
BEGIN
    CREATE TABLE expenses (
        expense_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        date_time DATETIME NOT NULL,
        voucher_type NVARCHAR(100) NOT NULL,
        paid_to NVARCHAR(200) NOT NULL,
        received_by NVARCHAR(200) NOT NULL,
        amount DECIMAL(12,2) NOT NULL,
        payment_method_id INT NULL,
        purpose NVARCHAR(500) NULL,
        comment NVARCHAR(500) NULL,
        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        updated_at DATETIME NULL,
        CONSTRAINT FK_Expenses_PaymentMethods FOREIGN KEY (payment_method_id) REFERENCES payment_methods(payment_method_id)
    );
    PRINT 'expenses table created successfully.';
END

-- Add index for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RoomTypeRates_RoomTypeId' AND object_id = OBJECT_ID('room_type_rates'))
BEGIN
    CREATE INDEX IX_RoomTypeRates_RoomTypeId ON room_type_rates(roomtype_id);
    PRINT 'Index IX_RoomTypeRates_RoomTypeId created.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RoomTypeRates_HotelId' AND object_id = OBJECT_ID('room_type_rates'))
BEGIN
    CREATE INDEX IX_RoomTypeRates_HotelId ON room_type_rates(hotel_id);
    PRINT 'Index IX_RoomTypeRates_HotelId created.';
END

-- Seed mock data (idempotent): create a few base-rate rows for existing room types
-- ملاحظة: يتم تجنب التكرار عبر شرط NOT EXISTS
IF EXISTS (SELECT 1 FROM room_types)
BEGIN
    DECLARE @targetHotelId INT;
    -- Pick a hotel id that has room types (fallback to smallest hotel id)
    SELECT TOP 1 @targetHotelId = hotel_id
    FROM room_types
    ORDER BY hotel_id;

    ;WITH top_room_types AS (
        SELECT TOP (4) roomtype_id, hotel_id
        FROM room_types
        WHERE hotel_id = @targetHotelId
        ORDER BY roomtype_id
    )
    INSERT INTO room_type_rates (
        roomtype_id,
        hotel_id,
        daily_rate_low_weekdays,
        daily_rate_high_weekdays,
        daily_rate_min,
        monthly_rate,
        monthly_rate_min,
        ota_rate_low_weekdays,
        ota_rate_high_weekdays
    )
    SELECT
        t.roomtype_id,
        t.hotel_id,
        CAST(100.00 AS DECIMAL(12,2)) AS daily_rate_low_weekdays,
        CAST(150.00 AS DECIMAL(12,2)) AS daily_rate_high_weekdays,
        CAST(100.00 AS DECIMAL(12,2)) AS daily_rate_min,
        CAST(3000.00 AS DECIMAL(12,2)) AS monthly_rate,
        CAST(2500.00 AS DECIMAL(12,2)) AS monthly_rate_min,
        CAST(100.00 AS DECIMAL(12,2)) AS ota_rate_low_weekdays,
        CAST(200.00 AS DECIMAL(12,2)) AS ota_rate_high_weekdays
    FROM top_room_types t
    WHERE NOT EXISTS (
        SELECT 1 FROM room_type_rates r
        WHERE r.roomtype_id = t.roomtype_id
          AND r.hotel_id = t.hotel_id
    );

    PRINT 'Mock room_type_rates data inserted (if missing).';
END

