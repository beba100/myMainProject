-- Simplified Hotel Settings Migration Script
-- سكريبت ترحيل إعدادات الفندق المبسط

-- This script works when hotels and config tables are already deleted
-- هذا السكريبت يعمل عندما تكون جداول الفنادق والكونفيج قد تم حذفها بالفعل

PRINT 'Starting Simplified Hotel Settings Migration...'
PRINT 'بدء ترحيل إعدادات الفندق المبسط...'

-- First, check if hotel_settings table already exists and drop it if it does
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping existing hotel_settings table...'
    DROP TABLE [dbo].[hotel_settings]
END
GO

-- Create hotel_settings table
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

-- Insert default hotel settings data
-- إدراج بيانات إعدادات الفندق الافتراضية

PRINT 'Inserting default hotel settings data...'
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
GO

-- Update existing tables to use hotel_id = 1 (the default hotel we just created)
-- تحديث الجداول الموجودة لاستخدام hotel_id = 1 (الفندق الافتراضي الذي أنشأناه للتو)

-- Update customers table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[customers]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[customers]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating customers table to use default hotel_id = 1...'
    UPDATE [dbo].[customers] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating reservations table to use default hotel_id = 1...'
    UPDATE [dbo].[reservations] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating invoices table to use default hotel_id = 1...'
    UPDATE [dbo].[invoices] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating payment_receipts table to use default hotel_id = 1...'
    UPDATE [dbo].[payment_receipts] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating refunds table to use default hotel_id = 1...'
    UPDATE [dbo].[refunds] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating customer_accounts table to use default hotel_id = 1...'
    UPDATE [dbo].[customer_accounts] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating room_types table to use default hotel_id = 1...'
    UPDATE [dbo].[room_types] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating apartments table to use default hotel_id = 1...'
    UPDATE [dbo].[apartments] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating buildings table to use default hotel_id = 1...'
    UPDATE [dbo].[buildings] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating floors table to use default hotel_id = 1...'
    UPDATE [dbo].[floors] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating corporate_customers table to use default hotel_id = 1...'
    UPDATE [dbo].[corporate_customers] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating credit_notes table to use default hotel_id = 1...'
    UPDATE [dbo].[credit_notes] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
    PRINT 'Credit_notes table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Credit_notes table or hotel_id column does not exist, skipping...'
END
GO

-- Update discounts table (if it exists and has hotel_id column)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[discounts]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[discounts]') AND name = 'hotel_id')
BEGIN
    PRINT 'Updating discounts table to use default hotel_id = 1...'
    UPDATE [dbo].[discounts] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
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
    PRINT 'Updating penalties table to use default hotel_id = 1...'
    UPDATE [dbo].[penalties] SET [hotel_id] = 1 WHERE [hotel_id] IS NOT NULL
    PRINT 'Penalties table updated successfully!'
END
ELSE
BEGIN
    PRINT 'Penalties table or hotel_id column does not exist, skipping...'
END
GO

-- Verify the hotel_settings table was created successfully
-- التحقق من إنشاء جدول hotel_settings بنجاح

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[hotel_settings]') AND type in (N'U'))
BEGIN
    DECLARE @RecordCount INT
    SELECT @RecordCount = COUNT(*) FROM [dbo].[hotel_settings]
    PRINT 'Hotel_settings table created successfully with ' + CAST(@RecordCount AS VARCHAR(10)) + ' record(s)!'
    PRINT 'تم إنشاء جدول hotel_settings بنجاح مع ' + CAST(@RecordCount AS VARCHAR(10)) + ' سجل!'
END
ELSE
BEGIN
    PRINT 'ERROR: Hotel_settings table was not created!'
    PRINT 'خطأ: لم يتم إنشاء جدول hotel_settings!'
END
GO

PRINT 'Simplified Hotel Settings migration completed successfully!'
PRINT 'تم إكمال ترحيل إعدادات الفندق المبسط بنجاح!'
