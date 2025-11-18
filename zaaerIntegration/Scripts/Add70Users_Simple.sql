-- =============================================
-- Script: إضافة 70 مستخدم في Master DB (نسخة مبسطة)
-- Database: Master DB (db32357)
-- Server: s800.public.eu.machineasp.net
-- كلمة المرور: 123
-- =============================================
-- 
-- ⚠️  ملاحظة مهمة:
-- هذا Script يستخدم hash بسيط (SHA256) وليس BCrypt
-- للاستخدام في التطوير فقط - غير آمن للإنتاج
-- 
-- للحصول على BCrypt hash صحيح، استخدم:
-- 1. Python script: Add70Users.py (موصى به)
-- 2. PowerShell script: Add70Users.ps1
-- 3. أو احسب hash يدوياً وضعه في Add70Users.sql
-- =============================================

USE [db32357];
GO

-- التحقق من وجود الجداول
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

IF NOT EXISTS (SELECT * FROM Tenants) OR NOT EXISTS (SELECT * FROM Roles)
BEGIN
    PRINT '❌ لا توجد فنادق أو أدوار في قاعدة البيانات.';
    RETURN;
END

PRINT '✅ جميع الجداول موجودة. بدء إضافة المستخدمين...';
PRINT '⚠️  تحذير: هذا Script يستخدم hash بسيط - غير آمن للإنتاج!';
PRINT '';

-- =============================================
-- حساب hash بسيط لكلمة المرور "123" (SHA256)
-- ⚠️  هذا hash لن يعمل مع BCrypt في التطبيق!
-- =============================================
DECLARE @Password NVARCHAR(100) = '123';
DECLARE @DefaultPasswordHash NVARCHAR(500) = CONVERT(NVARCHAR(500), HASHBYTES('SHA2_256', @Password), 2);

PRINT '⚠️  تحذير: هذا Script يستخدم SHA256 hash وليس BCrypt!';
PRINT '⚠️  تسجيل الدخول لن يعمل مع هذا hash!';
PRINT '⚠️  استخدم Python script (Add70Users.py) للحصول على BCrypt hash صحيح!';
PRINT '';

-- =============================================
-- إضافة المستخدمين
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

SELECT @TenantCount = COUNT(*) FROM Tenants;
SELECT @RoleCount = COUNT(*) FROM Roles;

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

WHILE @Counter <= 70
BEGIN
    BEGIN TRY
        SELECT @TenantId = Id, @TenantCode = Code
        FROM @Tenants
        WHERE RowNum = @TenantIndex % @TenantCount;
        
        SELECT @RoleId = Id, @RoleCode = Code
        FROM @Roles
        WHERE RowNum = @RoleIndex % @RoleCount;
        
        SET @Username = 'user' + CAST(@Counter AS NVARCHAR(10));
        
        IF EXISTS (SELECT 1 FROM MasterUsers WHERE Username = @Username)
        BEGIN
            PRINT '⚠️  المستخدم ' + @Username + ' موجود بالفعل، يتم التخطي...';
            SET @ErrorCount = @ErrorCount + 1;
            SET @Counter = @Counter + 1;
            SET @TenantIndex = @TenantIndex + 1;
            SET @RoleIndex = @RoleIndex + 1;
            CONTINUE;
        END
        
        INSERT INTO MasterUsers (Username, PasswordHash, TenantId, IsActive, CreatedAt)
        VALUES (@Username, @DefaultPasswordHash, @TenantId, 1, GETUTCDATE());
        
        SET @UserId = SCOPE_IDENTITY();
        
        INSERT INTO UserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
        
        PRINT '✅ تم إضافة المستخدم: ' + @Username + ' (Tenant: ' + @TenantCode + ', Role: ' + @RoleCode + ')';
        SET @SuccessCount = @SuccessCount + 1;
        
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
PRINT '⚠️  تحذير مهم:';
PRINT 'هذا Script يستخدم SHA256 hash وليس BCrypt!';
PRINT 'تسجيل الدخول لن يعمل مع هذا hash!';
PRINT '';
PRINT '✅ للحصول على BCrypt hash صحيح، استخدم:';
PRINT '   python Add70Users.py';
PRINT '   أو';
PRINT '   .\Add70Users.ps1';
PRINT '';
GO

