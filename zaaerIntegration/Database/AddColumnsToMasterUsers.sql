-- =============================================
-- Script: إضافة أعمدة جديدة إلى جدول MasterUsers
-- Database: Master DB (db32357)
-- Server: s800.public.eu.machineasp.net
-- =============================================

USE [db32357];
GO

-- =============================================
-- إضافة الأعمدة الجديدة إذا لم تكن موجودة
-- =============================================

-- إضافة عمود رقم الجوال
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MasterUsers]') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE [dbo].[MasterUsers]
    ADD [PhoneNumber] NVARCHAR(50) NULL;
    PRINT '✅ Column PhoneNumber added successfully';
END
ELSE
BEGIN
    PRINT '⚠️ Column PhoneNumber already exists';
END
GO

-- إضافة عمود الايميل
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MasterUsers]') AND name = 'Email')
BEGIN
    ALTER TABLE [dbo].[MasterUsers]
    ADD [Email] NVARCHAR(200) NULL;
    PRINT '✅ Column Email added successfully';
END
ELSE
BEGIN
    PRINT '⚠️ Column Email already exists';
END
GO

-- إضافة عمود الرقم الوظيفي
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MasterUsers]') AND name = 'EmployeeNumber')
BEGIN
    ALTER TABLE [dbo].[MasterUsers]
    ADD [EmployeeNumber] NVARCHAR(50) NULL;
    PRINT '✅ Column EmployeeNumber added successfully';
END
ELSE
BEGIN
    PRINT '⚠️ Column EmployeeNumber already exists';
END
GO

PRINT '=============================================';
PRINT '✅ All columns added successfully!';
PRINT '=============================================';

