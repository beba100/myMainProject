-- Create hotel_settings table
-- إنشاء جدول إعدادات الفندق الجديد

-- First, check if hotel_settings table already exists and drop it if it does
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping existing hotel_settings table...'
    DROP TABLE [dbo].[hotel_settings]
END
GO

CREATE TABLE [dbo].[hotel_settings](
    [hotel_id] [int] IDENTITY(1,1) NOT NULL,
    [hotel_code] [nvarchar](50) NULL,
    [hotel_name] [nvarchar](50) NULL,
    [vat_percent] [decimal](5, 2) NOT NULL DEFAULT(15.00),
    [lodging_tax] [decimal](5, 2) NOT NULL DEFAULT(2.50),
    [default_currency] [nvarchar](10) NOT NULL DEFAULT('SAR'),
    [company_name] [nvarchar](200) NOT NULL,
    [company_vatno] [nvarchar](50) NOT NULL,
    [company_crn] [nvarchar](50) NULL,
    [logo_url] [nvarchar](500) NOT NULL DEFAULT(''),
    [address] [nvarchar](500) NOT NULL,
    [phone] [nvarchar](50) NOT NULL,
    [email] [nvarchar](100) NOT NULL,
    [created_at] [datetime2](7) NULL,
    CONSTRAINT [PK_hotel_settings] PRIMARY KEY CLUSTERED
    (
        [hotel_id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Insert sample data from existing hotels table (if it exists)
-- إدراج بيانات تجريبية من جدول الفنادق الموجود (إذا كان موجوداً)

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[hotels]') AND type in (N'U'))
BEGIN
    PRINT 'Inserting data from hotels table...'
    INSERT INTO [dbo].[hotel_settings] (
        [hotel_code],
        [hotel_name],
        [vat_percent],
        [lodging_tax],
        [default_currency],
        [company_name],
        [company_vatno],
        [company_crn],
        [logo_url],
        [address],
        [phone],
        [email],
        [created_at]
    )
    SELECT 
        h.[hotel_code],
        h.[hotel_name],
        h.[vat_percent],
        h.[lodging_tax],
        h.[default_currency],
        h.[company_name],
        h.[company_vatno],
        h.[company_crn],
        h.[logo_url],
        h.[address],
        h.[phone],
        h.[email],
        h.[created_at]
    FROM [dbo].[hotels] h
    PRINT 'Data inserted from hotels table successfully!'
END
ELSE
BEGIN
    PRINT 'Hotels table does not exist, inserting default data...'
    INSERT INTO [dbo].[hotel_settings] (
        [hotel_code],
        [hotel_name],
        [vat_percent],
        [lodging_tax],
        [default_currency],
        [company_name],
        [company_vatno],
        [company_crn],
        [logo_url],
        [address],
        [phone],
        [email],
        [created_at]
    )
    VALUES (
        'HTL001',
        'Default Hotel',
        15.00,
        2.50,
        'SAR',
        'Default Hotel Company',
        '123456789012345',
        '1234567890',
        'https://example.com/logo.png',
        'Default Hotel Address',
        '+966501234567',
        'info@defaulthotel.com',
        GETDATE()
    )
    PRINT 'Default data inserted successfully!'
END
GO

-- Update all foreign key references to point to hotel_settings instead of hotels
-- تحديث جميع المراجع الخارجية لتشير إلى hotel_settings بدلاً من hotels

-- Update customers table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[customers]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[customers]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating customers table...'
    UPDATE [dbo].[customers] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[customers] c
    INNER JOIN [dbo].[hotels] h ON c.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = c.[hotel_id])
    PRINT 'Customers table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Customers table or hotel_id column does not exist, skipping...'
END
GO

-- Update reservations table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[reservations]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[reservations]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating reservations table...'
    UPDATE [dbo].[reservations] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[reservations] r
    INNER JOIN [dbo].[hotels] h ON r.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = r.[hotel_id])
    PRINT 'Reservations table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Reservations table or hotel_id column does not exist, skipping...'
END
GO

-- Update invoices table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[invoices]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[invoices]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating invoices table...'
    UPDATE [dbo].[invoices] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[invoices] i
    INNER JOIN [dbo].[hotels] h ON i.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = i.[hotel_id])
    PRINT 'Invoices table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Invoices table or hotel_id column does not exist, skipping...'
END
GO

-- Update payment_receipts table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[payment_receipts]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[payment_receipts]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating payment_receipts table...'
    UPDATE [dbo].[payment_receipts] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[payment_receipts] pr
    INNER JOIN [dbo].[hotels] h ON pr.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = pr.[hotel_id])
    PRINT 'Payment_receipts table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Payment_receipts table or hotel_id column does not exist, skipping...'
END
GO

-- Update refunds table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[refunds]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[refunds]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating refunds table...'
    UPDATE [dbo].[refunds] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[refunds] rf
    INNER JOIN [dbo].[hotels] h ON rf.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = rf.[hotel_id])
    PRINT 'Refunds table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Refunds table or hotel_id column does not exist, skipping...'
END
GO

-- Update customer_accounts table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[customer_accounts]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[customer_accounts]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating customer_accounts table...'
    UPDATE [dbo].[customer_accounts] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[customer_accounts] ca
    INNER JOIN [dbo].[hotels] h ON ca.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = ca.[hotel_id])
    PRINT 'Customer_accounts table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Customer_accounts table or hotel_id column does not exist, skipping...'
END
GO

-- Update room_types table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[room_types]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[room_types]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating room_types table...'
    UPDATE [dbo].[room_types] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[room_types] rt
    INNER JOIN [dbo].[hotels] h ON rt.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = rt.[hotel_id])
    PRINT 'Room_types table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Room_types table or hotel_id column does not exist, skipping...'
END
GO

-- Update apartments table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[apartments]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[apartments]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating apartments table...'
    UPDATE [dbo].[apartments] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[apartments] a
    INNER JOIN [dbo].[hotels] h ON a.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = a.[hotel_id])
    PRINT 'Apartments table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Apartments table or hotel_id column does not exist, skipping...'
END
GO

-- Update buildings table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[buildings]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[buildings]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating buildings table...'
    UPDATE [dbo].[buildings] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[buildings] b
    INNER JOIN [dbo].[hotels] h ON b.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = b.[hotel_id])
    PRINT 'Buildings table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Buildings table or hotel_id column does not exist, skipping...'
END
GO

-- Update floors table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[floors]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[floors]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating floors table...'
    UPDATE [dbo].[floors] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[floors] f
    INNER JOIN [dbo].[hotels] h ON f.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = f.[hotel_id])
    PRINT 'Floors table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Floors table or hotel_id column does not exist, skipping...'
END
GO

-- Update corporate_customers table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[corporate_customers]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[corporate_customers]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating corporate_customers table...'
    UPDATE [dbo].[corporate_customers] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[corporate_customers] cc
    INNER JOIN [dbo].[hotels] h ON cc.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = cc.[hotel_id])
    PRINT 'Corporate_customers table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Corporate_customers table or hotel_id column does not exist, skipping...'
END
GO

-- Update credit_notes table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[credit_notes]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[credit_notes]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating credit_notes table...'
    UPDATE [dbo].[credit_notes] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[credit_notes] cn
    INNER JOIN [dbo].[hotels] h ON cn.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = cn.[hotel_id])
    PRINT 'Credit_notes table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Credit_notes table or hotel_id column does not exist, skipping...'
END
GO

-- Update config table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[config]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[config]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating config table...'
    UPDATE [dbo].[config] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[config] c
    INNER JOIN [dbo].[hotels] h ON c.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = c.[hotel_id])
    PRINT 'Config table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Config table or hotel_id column does not exist, skipping...'
END
GO

-- Update discounts table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[discounts]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[discounts]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating discounts table...'
    UPDATE [dbo].[discounts] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[discounts] d
    INNER JOIN [dbo].[hotels] h ON d.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = d.[hotel_id])
    PRINT 'Discounts table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Discounts table or hotel_id column does not exist, skipping...'
END
GO

-- Update penalties table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[penalties]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[penalties]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating penalties table...'
    UPDATE [dbo].[penalties] 
    SET [hotel_id] = hs.[hotel_id]
    FROM [dbo].[penalties] p
    INNER JOIN [dbo].[hotels] h ON p.[hotel_id] = h.[hotel_id]
    INNER JOIN [dbo].[hotel_settings] hs ON h.[hotel_id] = hs.[hotel_id]
    WHERE EXISTS (SELECT 1 FROM [dbo].[hotels] WHERE [hotel_id] = p.[hotel_id])
    PRINT 'Penalties table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Penalties table or hotel_id column does not exist, skipping...'
END
GO

-- Now drop the old hotels table (if it exists and no foreign key constraints)
-- الآن حذف جدول الفنادق القديم (إذا كان موجوداً ولا توجد قيود مفتاح خارجي)

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[hotels]') AND type in (N'U'))
BEGIN
    PRINT 'Attempting to drop hotels table...'
    
    -- Check for foreign key constraints
    DECLARE @FKCount INT
    SELECT @FKCount = COUNT(*)
    FROM sys.foreign_keys fk
    INNER JOIN sys.tables t ON fk.referenced_object_id = t.object_id
    WHERE t.name = 'hotels'
    
    IF @FKCount = 0
    BEGIN
        DROP TABLE [dbo].[hotels]
        PRINT 'Hotels table dropped successfully!'
    END
    ELSE
    BEGIN
        PRINT 'Cannot drop hotels table due to foreign key constraints. Please drop foreign keys manually first.'
        PRINT 'Number of foreign key constraints: ' + CAST(@FKCount AS VARCHAR(10))
    END
END
ELSE
BEGIN
    PRINT 'Hotels table does not exist, skipping drop...'
END
GO

-- Drop the old config table (if it exists)
-- حذف جدول config القديم (إذا كان موجوداً)

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[config]') AND type in (N'U'))
BEGIN
    PRINT 'Attempting to drop config table...'
    DROP TABLE [dbo].[config]
    PRINT 'Config table dropped successfully!'
END
ELSE
BEGIN
    PRINT 'Config table does not exist, skipping drop...'
END
GO

PRINT 'Hotel Settings migration completed successfully!'
PRINT 'تم إكمال ترحيل إعدادات الفندق بنجاح!'
