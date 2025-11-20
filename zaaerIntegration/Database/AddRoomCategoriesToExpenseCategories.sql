-- =============================================
-- Script: إضافة فئات الغرف (Room Categories) إلى جدول ExpenseCategories
-- Database: Master DB (db32357)
-- Description: إضافة عمود CategoryCode وإضافة 3 فئات للغرف
-- =============================================

USE [db32357];
GO

-- =============================================
-- إضافة عمود CategoryCode إذا لم يكن موجوداً
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'CategoryCode' AND Object_ID = Object_ID(N'[dbo].[ExpenseCategories]'))
BEGIN
    ALTER TABLE [dbo].[ExpenseCategories]
    ADD [CategoryCode] NVARCHAR(50) NULL;
    
    PRINT '✅ Column CategoryCode added to ExpenseCategories table.';
END
ELSE
BEGIN
    PRINT '⚠️ Column CategoryCode already exists in ExpenseCategories table.';
END
GO

-- =============================================
-- إضافة فئات الغرف (Room Categories)
-- =============================================

-- التحقق من وجود الفئات قبل الإضافة
IF NOT EXISTS (SELECT * FROM [dbo].[ExpenseCategories] WHERE [CategoryCode] = 'CAT_BUILDING')
BEGIN
    INSERT INTO [dbo].[ExpenseCategories] ([MainCategory], [Details], [CategoryCode], [IsActive], [CreatedAt])
    VALUES (N'مبنى كامل', N'فئة خاصة للغرف - يشمل المبنى بالكامل', 'CAT_BUILDING', 1, GETUTCDATE());
    PRINT '✅ Added category: مبنى كامل (CAT_BUILDING)';
END
ELSE
BEGIN
    PRINT '⚠️ Category CAT_BUILDING already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[ExpenseCategories] WHERE [CategoryCode] = 'CAT_RECEPTION')
BEGIN
    INSERT INTO [dbo].[ExpenseCategories] ([MainCategory], [Details], [CategoryCode], [IsActive], [CreatedAt])
    VALUES (N'الاستقبال', N'فئة خاصة للغرف - منطقة الاستقبال', 'CAT_RECEPTION', 1, GETUTCDATE());
    PRINT '✅ Added category: الاستقبال (CAT_RECEPTION)';
END
ELSE
BEGIN
    PRINT '⚠️ Category CAT_RECEPTION already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[ExpenseCategories] WHERE [CategoryCode] = 'CAT_CORRIDORS')
BEGIN
    INSERT INTO [dbo].[ExpenseCategories] ([MainCategory], [Details], [CategoryCode], [IsActive], [CreatedAt])
    VALUES (N'الممرات', N'فئة خاصة للغرف - الممرات والدهاليز', 'CAT_CORRIDORS', 1, GETUTCDATE());
    PRINT '✅ Added category: الممرات (CAT_CORRIDORS)';
END
ELSE
BEGIN
    PRINT '⚠️ Category CAT_CORRIDORS already exists.';
END
GO

-- =============================================
-- إنشاء Index على CategoryCode للبحث السريع
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExpenseCategories_CategoryCode' AND object_id = OBJECT_ID('dbo.ExpenseCategories'))
BEGIN
    CREATE INDEX [IX_ExpenseCategories_CategoryCode] ON [dbo].[ExpenseCategories]([CategoryCode]);
    PRINT '✅ Index IX_ExpenseCategories_CategoryCode created.';
END
ELSE
BEGIN
    PRINT '⚠️ Index IX_ExpenseCategories_CategoryCode already exists.';
END
GO

PRINT '=============================================';
PRINT '✅ Room Categories added successfully!';
PRINT '=============================================';

