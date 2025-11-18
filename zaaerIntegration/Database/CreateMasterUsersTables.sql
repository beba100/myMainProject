-- =============================================
-- Script: إنشاء جداول المستخدمين والأدوار في Master DB
-- Database: Master DB (db32357)
-- Server: s800.public.eu.machineasp.net
-- =============================================

USE [db32357];
GO

-- =============================================
-- 1. إنشاء جدول Roles
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Roles] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Code] NVARCHAR(50) NOT NULL UNIQUE
    );
    
    PRINT '✅ Table Roles created successfully';
END
ELSE
BEGIN
    PRINT '⚠️ Table Roles already exists';
END
GO

-- =============================================
-- 2. إضافة الأدوار الأساسية
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[Roles] WHERE [Code] = 'Admin')
BEGIN
    INSERT INTO [dbo].[Roles] ([Name], [Code]) VALUES ('Administrator', 'Admin');
    PRINT '✅ Role Admin inserted';
END

IF NOT EXISTS (SELECT * FROM [dbo].[Roles] WHERE [Code] = 'Manager')
BEGIN
    INSERT INTO [dbo].[Roles] ([Name], [Code]) VALUES ('General Manager', 'Manager');
    PRINT '✅ Role Manager inserted';
END

IF NOT EXISTS (SELECT * FROM [dbo].[Roles] WHERE [Code] = 'Supervisor')
BEGIN
    INSERT INTO [dbo].[Roles] ([Name], [Code]) VALUES ('Supervisor', 'Supervisor');
    PRINT '✅ Role Supervisor inserted';
END

IF NOT EXISTS (SELECT * FROM [dbo].[Roles] WHERE [Code] = 'Staff')
BEGIN
    INSERT INTO [dbo].[Roles] ([Name], [Code]) VALUES ('Reception Staff', 'Staff');
    PRINT '✅ Role Staff inserted';
END

IF NOT EXISTS (SELECT * FROM [dbo].[Roles] WHERE [Code] = 'Accountant')
BEGIN
    INSERT INTO [dbo].[Roles] ([Name], [Code]) VALUES ('Accountant', 'Accountant');
    PRINT '✅ Role Accountant inserted';
END

IF NOT EXISTS (SELECT * FROM [dbo].[Roles] WHERE [Code] = 'ReadOnly')
BEGIN
    INSERT INTO [dbo].[Roles] ([Name], [Code]) VALUES ('Read Only', 'ReadOnly');
    PRINT '✅ Role ReadOnly inserted';
END
GO

-- =============================================
-- 3. إنشاء جدول MasterUsers
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MasterUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MasterUsers] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Username] NVARCHAR(100) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [TenantId] INT NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [FK_MasterUsers_Tenants] FOREIGN KEY ([TenantId]) 
            REFERENCES [dbo].[Tenants]([Id]) ON DELETE NO ACTION
    );
    
    -- Index على Username للبحث السريع
    CREATE INDEX [IX_MasterUsers_Username] ON [dbo].[MasterUsers]([Username]);
    
    -- Index على TenantId
    CREATE INDEX [IX_MasterUsers_TenantId] ON [dbo].[MasterUsers]([TenantId]);
    
    PRINT '✅ Table MasterUsers created successfully';
END
ELSE
BEGIN
    PRINT '⚠️ Table MasterUsers already exists';
END
GO

-- =============================================
-- 4. إنشاء جدول UserRoles
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserRoles] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [RoleId] INT NOT NULL,
        CONSTRAINT [FK_UserRoles_MasterUsers] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[MasterUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY ([RoleId]) 
            REFERENCES [dbo].[Roles]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [UQ_UserRoles_UserId_RoleId] UNIQUE ([UserId], [RoleId])
    );
    
    -- Index على UserId
    CREATE INDEX [IX_UserRoles_UserId] ON [dbo].[UserRoles]([UserId]);
    
    -- Index على RoleId
    CREATE INDEX [IX_UserRoles_RoleId] ON [dbo].[UserRoles]([RoleId]);
    
    PRINT '✅ Table UserRoles created successfully';
END
ELSE
BEGIN
    PRINT '⚠️ Table UserRoles already exists';
END
GO

PRINT '=============================================';
PRINT '✅ All tables created successfully!';
PRINT '=============================================';

