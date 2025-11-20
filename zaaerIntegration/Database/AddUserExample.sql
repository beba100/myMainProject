-- =============================================
-- مثال: إضافة مستخدم جديد (جاهز للاستخدام)
-- Example: Add New User (Ready to Use)
-- =============================================

USE [db32357_MasterDB]; -- Change to your Master DB name
GO

-- =============================================
-- الخطوة 1: إضافة المستخدم في MasterUsers
-- Step 1: Add User to MasterUsers
-- =============================================

-- مثال 1: موظف استقبال (Staff)
DECLARE @NewUserId INT;
DECLARE @Username NVARCHAR(100) = 'sara_staff';
DECLARE @Password NVARCHAR(500) = '123456';  -- كلمة المرور
DECLARE @DefaultTenantId INT = 9;  -- Riyadh3 (الفندق الافتراضي)

-- التحقق من عدم وجود المستخدم
IF NOT EXISTS (SELECT 1 FROM MasterUsers WHERE Username = @Username)
BEGIN
    INSERT INTO MasterUsers (Username, PasswordHash, TenantId, IsActive, CreatedAt)
    VALUES (@Username, @Password, @DefaultTenantId, 1, GETDATE());
    
    SET @NewUserId = SCOPE_IDENTITY();
    PRINT '✅ User created successfully. UserId: ' + CAST(@NewUserId AS NVARCHAR(10));
END
ELSE
BEGIN
    SELECT @NewUserId = Id FROM MasterUsers WHERE Username = @Username;
    PRINT '⚠️ User already exists. UserId: ' + CAST(@NewUserId AS NVARCHAR(10));
END
GO

-- =============================================
-- الخطوة 2: إضافة الدور في UserRoles
-- Step 2: Add Role to UserRoles
-- =============================================

DECLARE @UserId INT = (SELECT Id FROM MasterUsers WHERE Username = 'sara_staff');
DECLARE @RoleCode NVARCHAR(50) = 'Staff';  -- Staff, Supervisor, Admin, Manager, Accountant, ReadOnly

-- الحصول على RoleId
DECLARE @RoleId INT;
SELECT @RoleId = Id FROM Roles WHERE Code = @RoleCode;

IF @RoleId IS NULL
BEGIN
    PRINT '❌ Role not found: ' + @RoleCode;
    PRINT 'Available roles:';
    SELECT Code, Name FROM Roles;
END
ELSE
BEGIN
    -- التحقق من عدم وجود الدور للمستخدم
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
        PRINT '✅ Role added successfully: ' + @RoleCode;
    END
    ELSE
    BEGIN
        PRINT '⚠️ User already has this role: ' + @RoleCode;
    END
END
GO

-- =============================================
-- الخطوة 3: إضافة الفنادق في UserTenants (حسب الدور)
-- Step 3: Add Hotels to UserTenants (Based on Role)
-- =============================================

DECLARE @UserId INT = (SELECT Id FROM MasterUsers WHERE Username = 'sara_staff');
DECLARE @RoleCode NVARCHAR(50) = 'Staff';  -- Staff, Supervisor, Admin, Manager, Accountant

-- التحقق من الدور
DECLARE @RoleId INT;
SELECT @RoleId = Id FROM Roles WHERE Code = @RoleCode;

IF @RoleCode IN ('Admin', 'Manager', 'Accountant')
BEGIN
    PRINT '✅ User has full access role (' + @RoleCode + ') - No need to add UserTenants';
    PRINT '   User will see all hotels automatically.';
END
ELSE IF @RoleCode = 'Staff'
BEGIN
    -- Staff: فندق واحد فقط
    DECLARE @TenantId INT = 9;  -- Riyadh3
    
    IF NOT EXISTS (SELECT 1 FROM UserTenants WHERE UserId = @UserId AND TenantId = @TenantId)
    BEGIN
        INSERT INTO UserTenants (UserId, TenantId)
        VALUES (@UserId, @TenantId);
        PRINT '✅ Hotel added for Staff user: TenantId = ' + CAST(@TenantId AS NVARCHAR(10));
    END
    ELSE
    BEGIN
        PRINT '⚠️ Hotel already assigned to user.';
    END
END
ELSE IF @RoleCode = 'Supervisor'
BEGIN
    -- Supervisor: عدة فنادق
    -- إضافة عدة فنادق
    IF NOT EXISTS (SELECT 1 FROM UserTenants WHERE UserId = @UserId AND TenantId = 1)
        INSERT INTO UserTenants (UserId, TenantId) VALUES (@UserId, 1);  -- Dammam1
    
    IF NOT EXISTS (SELECT 1 FROM UserTenants WHERE UserId = @UserId AND TenantId = 2)
        INSERT INTO UserTenants (UserId, TenantId) VALUES (@UserId, 2);  -- Dammam2
    
    IF NOT EXISTS (SELECT 1 FROM UserTenants WHERE UserId = @UserId AND TenantId = 9)
        INSERT INTO UserTenants (UserId, TenantId) VALUES (@UserId, 9);  -- Riyadh3
    
    PRINT '✅ Hotels added for Supervisor user.';
END
GO

-- =============================================
-- التحقق من النتيجة
-- Verify Result
-- =============================================

DECLARE @Username NVARCHAR(100) = 'sara_staff';

-- عرض معلومات المستخدم
SELECT 
    'User Info' AS InfoType,
    u.Id AS UserId,
    u.Username,
    u.TenantId AS DefaultTenantId,
    t.Code AS DefaultTenantCode,
    u.IsActive
FROM MasterUsers u
LEFT JOIN Tenants t ON u.TenantId = t.Id
WHERE u.Username = @Username;

-- عرض أدوار المستخدم
SELECT 
    'User Roles' AS InfoType,
    r.Code AS RoleCode,
    r.Name AS RoleName
FROM MasterUsers u
INNER JOIN UserRoles ur ON u.Id = ur.UserId
INNER JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Username = @Username;

-- عرض فنادق المستخدم
SELECT 
    'User Hotels' AS InfoType,
    t.Code AS HotelCode,
    t.Name AS HotelName
FROM MasterUsers u
INNER JOIN UserTenants ut ON u.Id = ut.UserId
INNER JOIN Tenants t ON ut.TenantId = t.Id
WHERE u.Username = @Username;
GO

-- =============================================
-- أمثلة سريعة لإضافة مستخدمين مختلفين
-- Quick Examples for Different User Types
-- =============================================

/*
-- ============================================
-- مثال: إضافة Admin (يرى كل الفنادق)
-- ============================================
INSERT INTO MasterUsers (Username, PasswordHash, TenantId, IsActive, CreatedAt)
VALUES ('admin_user', 'admin123', 1, 1, GETDATE());

DECLARE @AdminUserId INT = SCOPE_IDENTITY();
DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Code = 'Admin');

INSERT INTO UserRoles (UserId, RoleId)
VALUES (@AdminUserId, @AdminRoleId);

-- لا حاجة لإضافة UserTenants - Admin يرى كل الفنادق
*/

/*
-- ============================================
-- مثال: إضافة Supervisor (يرى عدة فنادق)
-- ============================================
INSERT INTO MasterUsers (Username, PasswordHash, TenantId, IsActive, CreatedAt)
VALUES ('supervisor_user', 'super123', 1, 1, GETDATE());

DECLARE @SupervisorUserId INT = SCOPE_IDENTITY();
DECLARE @SupervisorRoleId INT = (SELECT Id FROM Roles WHERE Code = 'Supervisor');

INSERT INTO UserRoles (UserId, RoleId)
VALUES (@SupervisorUserId, @SupervisorRoleId);

-- إضافة عدة فنادق
INSERT INTO UserTenants (UserId, TenantId)
VALUES 
    (@SupervisorUserId, 1),  -- Dammam1
    (@SupervisorUserId, 2),  -- Dammam2
    (@SupervisorUserId, 9);  -- Riyadh3
*/

/*
-- ============================================
-- مثال: إضافة Staff (يرى فندق واحد فقط)
-- ============================================
INSERT INTO MasterUsers (Username, PasswordHash, TenantId, IsActive, CreatedAt)
VALUES ('staff_user', 'staff123', 9, 1, GETDATE());

DECLARE @StaffUserId INT = SCOPE_IDENTITY();
DECLARE @StaffRoleId INT = (SELECT Id FROM Roles WHERE Code = 'Staff');

INSERT INTO UserRoles (UserId, RoleId)
VALUES (@StaffUserId, @StaffRoleId);

-- إضافة فندق واحد فقط
INSERT INTO UserTenants (UserId, TenantId)
VALUES (@StaffUserId, 9);  -- Riyadh3
*/

PRINT '✅ Script completed!';
GO

