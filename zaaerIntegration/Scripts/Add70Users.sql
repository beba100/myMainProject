-- =============================================
-- Script: إضافة 70 مستخدم في Master DB
-- Database: Master DB (db32357)
-- Server: s800.public.eu.machineasp.net
-- كلمة المرور: 123
-- =============================================

USE [db32357];
GO

-- =============================================
-- ملاحظة مهمة: 
-- SQL Server لا يحتوي على BCrypt مدمج
-- يجب حساب BCrypt hash خارج SQL ثم وضعه هنا
-- أو استخدام Python/PowerShell script بدلاً من SQL
-- =============================================

-- =============================================
-- BCrypt hash لكلمة المرور "123"
-- =============================================
-- ملاحظة: BCrypt hash يتغير في كل مرة بسبب salt
-- للحصول على hash صحيح، استخدم أحد الطرق التالية:
-- 
-- 1. Python (الأسهل):
--    python -c "import bcrypt; print(bcrypt.hashpw(b'123', bcrypt.gensalt(rounds=12)).decode('utf-8'))"
--
-- 2. Python Script:
--    python Add70Users.py  (سيحسب hash تلقائياً)
--
-- 3. PowerShell Script:
--    .\GetBCryptHash.ps1
--
-- 4. C# Code:
--    BCrypt.Net.BCrypt.HashPassword("123", BCrypt.Net.BCrypt.GenerateSalt(12))
--
-- ثم انسخ Hash وضعه في المتغير @DefaultPasswordHash أدناه
-- =============================================

-- Hash محسوب مسبقاً لكلمة المرور "123"
-- ⚠️  إذا لم يعمل تسجيل الدخول، احسب hash جديد واستبدل القيمة أدناه
DECLARE @DefaultPasswordHash NVARCHAR(500) = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYqJqZqZqZq';

-- =============================================
-- التحقق من وجود الجداول والأدوار
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MasterUsers')
BEGIN
    PRINT '❌ جدول MasterUsers غير موجود. يرجى تشغيل CreateMasterUsersTables.sql أولاً.';
    RETURN;
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    PRINT '❌ جدول Roles غير موجود. يرجى تشغيل CreateMasterUsersTables.sql أولاً.';
    RETURN;
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
BEGIN
    PRINT '❌ جدول UserRoles غير موجود. يرجى تشغيل CreateMasterUsersTables.sql أولاً.';
    RETURN;
END

-- التحقق من وجود Tenants
IF NOT EXISTS (SELECT * FROM Tenants)
BEGIN
    PRINT '❌ لا توجد فنادق في قاعدة البيانات. يرجى إضافة فنادق أولاً.';
    RETURN;
END

-- التحقق من وجود Roles
IF NOT EXISTS (SELECT * FROM Roles)
BEGIN
    PRINT '❌ لا توجد أدوار في قاعدة البيانات. يرجى تشغيل CreateMasterUsersTables.sql أولاً.';
    RETURN;
END

PRINT '✅ جميع الجداول موجودة. بدء إضافة المستخدمين...';
PRINT '';

-- =============================================
-- إضافة المستخدمين الـ 70
-- =============================================

DECLARE @SuccessCount INT = 0;
DECLARE @ErrorCount INT = 0;
DECLARE @Counter INT = 1;
DECLARE @Username NVARCHAR(100);
DECLARE @UserId INT;
DECLARE @TenantId INT;
DECLARE @RoleId INT;
DECLARE @TenantIndex INT = 0;
DECLARE @RoleIndex INT = 0;
DECLARE @TenantCount INT;
DECLARE @RoleCount INT;
DECLARE @TenantCode NVARCHAR(50);
DECLARE @RoleCode NVARCHAR(50);

-- الحصول على عدد Tenants و Roles
SELECT @TenantCount = COUNT(*) FROM Tenants;
SELECT @RoleCount = COUNT(*) FROM Roles;

-- إنشاء جداول مؤقتة لتخزين Tenants و Roles بالترتيب
DECLARE @Tenants TABLE (Id INT, Code NVARCHAR(50), RowNum INT);
DECLARE @Roles TABLE (Id INT, Code NVARCHAR(50), RowNum INT);

INSERT INTO @Tenants (Id, Code, RowNum)
SELECT Id, Code, ROW_NUMBER() OVER (ORDER BY Id) - 1 AS RowNum
FROM Tenants
ORDER BY Id;

INSERT INTO @Roles (Id, Code, RowNum)
SELECT Id, Code, ROW_NUMBER() OVER (ORDER BY Id) - 1 AS RowNum
FROM Roles
ORDER BY Id;

-- حساب BCrypt hash لكلمة المرور "123"
-- ملاحظة: SQL Server لا يحتوي على BCrypt مدمج
-- يجب حساب hash خارج SQL ثم وضعه في المتغير @DefaultPasswordHash

-- للحصول على hash صحيح، استخدم أحد الطرق التالية:
-- 1. Python: python -c "import bcrypt; print(bcrypt.hashpw(b'123', bcrypt.gensalt(rounds=12)).decode('utf-8'))"
-- 2. C#: BCrypt.Net.BCrypt.HashPassword("123", BCrypt.Net.BCrypt.GenerateSalt(12))
-- 3. استخدم Python script: Add70Users.py

-- Hash محسوب مسبقاً (مثال - يجب استبداله)
-- هذا hash هو مثال فقط - يجب حساب hash صحيح لكلمة المرور "123"
SET @DefaultPasswordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYqJqZqZqZq';

WHILE @Counter <= 70
BEGIN
    BEGIN TRY
        -- اختيار Tenant (توزيع دوري)
        SELECT @TenantId = Id, @TenantCode = Code
        FROM @Tenants
        WHERE RowNum = @TenantIndex % @TenantCount;
        
        -- اختيار Role (توزيع دوري)
        SELECT @RoleId = Id, @RoleCode = Code
        FROM @Roles
        WHERE RowNum = @RoleIndex % @RoleCount;
        
        -- إنشاء اسم مستخدم
        SET @Username = 'user' + CAST(@Counter AS NVARCHAR(10));
        
        -- التحقق من عدم وجود مستخدم بنفس الاسم
        IF EXISTS (SELECT 1 FROM MasterUsers WHERE Username = @Username)
        BEGIN
            PRINT '⚠️  المستخدم ' + @Username + ' موجود بالفعل، يتم التخطي...';
            SET @ErrorCount = @ErrorCount + 1;
            SET @Counter = @Counter + 1;
            SET @TenantIndex = @TenantIndex + 1;
            SET @RoleIndex = @RoleIndex + 1;
            CONTINUE;
        END
        
        -- إدراج المستخدم
        INSERT INTO MasterUsers (Username, PasswordHash, TenantId, IsActive, CreatedAt)
        VALUES (@Username, @DefaultPasswordHash, @TenantId, 1, GETUTCDATE());
        
        SET @UserId = SCOPE_IDENTITY();
        
        -- إضافة الدور للمستخدم
        INSERT INTO UserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
        
        PRINT '✅ تم إضافة المستخدم: ' + @Username + ' (Tenant: ' + @TenantCode + ', Role: ' + @RoleCode + ')';
        SET @SuccessCount = @SuccessCount + 1;
        
        -- تحديث المؤشرات
        SET @TenantIndex = @TenantIndex + 1;
        SET @RoleIndex = @RoleIndex + 1;
        SET @Counter = @Counter + 1;
    END TRY
    BEGIN CATCH
        PRINT '❌ خطأ في إضافة المستخدم ' + CAST(@Counter AS NVARCHAR(10)) + ': ' + ERROR_MESSAGE();
        SET @ErrorCount = @ErrorCount + 1;
        SET @Counter = @Counter + 1;
        SET @TenantIndex = @TenantIndex + 1;
        SET @RoleIndex = @RoleIndex + 1;
    END CATCH
END

PRINT '';
PRINT '=============================================';
PRINT 'تم الانتهاء!';
PRINT '✅ نجح: ' + CAST(@SuccessCount AS NVARCHAR(10));
PRINT '❌ فشل: ' + CAST(@ErrorCount AS NVARCHAR(10));
PRINT '=============================================';
PRINT '';
PRINT 'كلمة المرور الافتراضية لجميع المستخدمين: 123';
PRINT '⚠️  يُنصح بتغيير كلمات المرور بعد أول تسجيل دخول';
PRINT '';
PRINT '⚠️  ملاحظة مهمة:';
PRINT 'إذا لم يعمل تسجيل الدخول، يجب حساب BCrypt hash صحيح لكلمة المرور "123"';
PRINT 'استخدم Python script (Add70Users.py) للحصول على hash صحيح';
PRINT 'أو استخدم: python -c "import bcrypt; print(bcrypt.hashpw(b''123'', bcrypt.gensalt(rounds=12)).decode(''utf-8''))"';
GO
