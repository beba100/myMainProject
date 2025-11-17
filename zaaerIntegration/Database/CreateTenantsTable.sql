-- =============================================
-- Multi-Tenant System - Master Database Setup
-- نظام متعدد الفنادق - إعداد قاعدة البيانات المركزية
-- =============================================

USE db29328;
GO

-- =============================================
-- 1. إنشاء جدول Tenants (الفنادق)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tenants')
BEGIN
    CREATE TABLE [dbo].[Tenants] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Code] NVARCHAR(50) NOT NULL UNIQUE,
        [Name] NVARCHAR(200) NOT NULL,
        [ConnectionString] NVARCHAR(500) NOT NULL,
        [BaseUrl] NVARCHAR(200) NULL,
        [CreatedDate] DATETIME2 DEFAULT GETDATE(),
        [UpdatedDate] DATETIME2 DEFAULT GETDATE(),
        [IsActive] BIT DEFAULT 1
    );

    PRINT '✅ تم إنشاء جدول Tenants بنجاح';
END
ELSE
BEGIN
    PRINT '⚠️ جدول Tenants موجود بالفعل';
END
GO

-- =============================================
-- 2. إضافة Index على Code للبحث السريع
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tenants_Code')
BEGIN
    CREATE UNIQUE INDEX IX_Tenants_Code ON [dbo].[Tenants]([Code]);
    PRINT '✅ تم إنشاء Index على Code';
END
GO

-- =============================================
-- 3. إدخال بيانات الفنادق الأساسية
-- =============================================

IF NOT EXISTS (SELECT * FROM [dbo].[Tenants] WHERE Code = 'Dammam1')
BEGIN
    INSERT INTO [dbo].[Tenants] ([Code], [Name], [ConnectionString], [BaseUrl], [IsActive])
    VALUES (
        'Dammam1',
        N'الدمام 1',
        'Server=db30471.public.databaseasp.net; Database=db30471; User Id=db30471; Password=p+3C9qH-%G6g; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;',
        'https://alcairy.premiumasp.net/',
        1
    );
    
    PRINT '✅ تم إضافة فندق الدمام 1';
END
ELSE
BEGIN
    PRINT '⚠️ فندق الدمام 1 موجود بالفعل';
END
GO

-- =============================================
-- 4. عرض جميع الفنادق المسجلة
-- =============================================

SELECT 
    Id,
    Code,
    Name,
    CASE WHEN IsActive = 1 THEN N'✅ نشط' ELSE N'❌ غير نشط' END AS [Status],
    BaseUrl,
    CreatedDate
FROM [dbo].[Tenants]
ORDER BY Id;
GO

-- =============================================
-- 5. Template لإضافة فندق جديد
-- =============================================

/*
-- قم بتعديل القيم التالية وإزالة التعليق لإضافة فندق جديد

INSERT INTO [dbo].[Tenants] ([Code], [Name], [ConnectionString], [BaseUrl], [IsActive])
VALUES (
    'Dammam2',                          -- كود الفندق
    N'الدمام 2',                        -- اسم الفندق
    'YOUR_CONNECTION_STRING_HERE',      -- Connection String
    'https://your-hotel-url.com/',      -- رابط الفندق (اختياري)
    1                                   -- نشط = 1, غير نشط = 0
);
GO
*/

-- =============================================
-- 6. Stored Procedure لإضافة فندق جديد
-- =============================================

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_AddNewTenant')
    DROP PROCEDURE sp_AddNewTenant;
GO

CREATE PROCEDURE [dbo].[sp_AddNewTenant]
    @Code NVARCHAR(50),
    @Name NVARCHAR(200),
    @ConnectionString NVARCHAR(500),
    @BaseUrl NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- التحقق من عدم وجود الكود مسبقاً
    IF EXISTS (SELECT 1 FROM [dbo].[Tenants] WHERE Code = @Code)
    BEGIN
        PRINT '❌ خطأ: كود الفندق موجود بالفعل';
        RETURN -1;
    END
    
    -- إضافة الفندق الجديد
    INSERT INTO [dbo].[Tenants] ([Code], [Name], [ConnectionString], [BaseUrl], [IsActive])
    VALUES (@Code, @Name, @ConnectionString, @BaseUrl, 1);
    
    PRINT '✅ تم إضافة الفندق بنجاح';
    
    -- عرض معلومات الفندق المضاف
    SELECT * FROM [dbo].[Tenants] WHERE Code = @Code;
    
    RETURN 0;
END
GO

-- =============================================
-- 7. مثال على استخدام الـ Stored Procedure
-- =============================================

/*
EXEC sp_AddNewTenant 
    @Code = 'Dammam2',
    @Name = N'الدمام 2',
    @ConnectionString = 'YOUR_CONNECTION_STRING',
    @BaseUrl = 'https://dammam2.example.com/';
GO
*/

PRINT '';
PRINT '========================================';
PRINT '✅ اكتمل إعداد قاعدة البيانات المركزية';
PRINT '========================================';
GO

