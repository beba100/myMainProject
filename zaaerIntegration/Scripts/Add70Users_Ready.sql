-- =============================================
-- Script: إضافة 70 مستخدم في Master DB
-- Database: Master DB (db32357)
-- Server: s800.public.eu.machineasp.net
-- كلمة المرور: 123
-- =============================================
-- 
-- ⚠️  مهم: قبل تشغيل هذا Script
-- 1. تأكد من تشغيل CreateMasterUsersTables.sql أولاً
-- 2. احسب BCrypt hash لكلمة المرور "123" (راجع التعليمات أدناه)
-- 3. استبدل @DefaultPasswordHash بالـ hash الصحيح
-- =============================================

USE [db32357];
GO

-- =============================================
-- حساب BCrypt Hash لكلمة المرور "123"
-- =============================================
-- للحصول على hash صحيح، استخدم أحد الطرق التالية:
--
-- الطريقة 1 (Python - الأسهل):
--   python -c "import bcrypt; print(bcrypt.hashpw(b'123', bcrypt.gensalt(rounds=12)).decode('utf-8'))"
--
-- الطريقة 2 (Python Script):
--   python Add70Users.py
--
-- الطريقة 3 (PowerShell):
--   .\GetBCryptHash.ps1
--
-- ثم انسخ Hash وضعه في المتغير @DefaultPasswordHash أدناه
-- =============================================

-- ⚠️  استبدل هذا Hash بـ hash صحيح محسوب من Python
-- Hash محسوب مسبقاً (مثال - يجب استبداله)
DECLARE @DefaultPasswordHash NVARCHAR(500) = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYqJqZqZqZq';

-- =============================================
-- التحقق من وجود الجداول
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

IF NOT EXISTS (SELECT * FROM Tenants)
BEGIN
    PRINT '❌ لا توجد فنادق في قاعدة البيانات. يرجى إضافة فنادق أولاً.';
    RETURN;
END

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

-- إنشاء جداول مؤقتة
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

-- حلقة لإضافة المستخدمين
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
PRINT '⚠️  ملاحظة:';
PRINT 'إذا لم يعمل تسجيل الدخول، تأكد من حساب BCrypt hash صحيح';
PRINT 'واستبدل @DefaultPasswordHash في بداية هذا Script';
PRINT '';
GO

