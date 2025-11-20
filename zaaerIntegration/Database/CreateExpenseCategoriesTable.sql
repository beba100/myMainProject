-- =============================================
-- Script: إنشاء جدول فئات المصروفات في Master DB
-- Database: Master DB (db32357)
-- Server: s800.public.eu.machineasp.net
-- =============================================

USE [db32357];
GO

-- =============================================
-- إنشاء جدول ExpenseCategories
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseCategories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExpenseCategories] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [MainCategory] NVARCHAR(200) NOT NULL, -- البند الرئيسي
        [Details] NVARCHAR(1000) NULL, -- التفصيل
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL
    );
    
    -- Index على MainCategory للبحث السريع
    CREATE INDEX [IX_ExpenseCategories_MainCategory] ON [dbo].[ExpenseCategories]([MainCategory]);
    
    -- Index على IsActive
    CREATE INDEX [IX_ExpenseCategories_IsActive] ON [dbo].[ExpenseCategories]([IsActive]);
    
    PRINT '✅ Table ExpenseCategories created successfully';
END
ELSE
BEGIN
    PRINT '⚠️ Table ExpenseCategories already exists';
END
GO

-- =============================================
-- إضافة البيانات الافتراضية
-- =============================================
-- Delete existing data if re-running script
DELETE FROM [dbo].[ExpenseCategories];
GO

-- Insert expense categories
INSERT INTO [dbo].[ExpenseCategories] ([MainCategory], [Details], [IsActive], [CreatedAt]) VALUES
(N'مصروفات الصيانة', N'مصاعد - شبكات - صيانة اثاث - تكييف - دهانات - غسالات - ثلاجات - حاسب الي - كهربائية اخرى', 1, GETUTCDATE()),
(N'مصروفات خدميه', N'انترنت - كروت شحن - هاتف - مياه - كهرباء - نزح بيارة - غاز', 1, GETUTCDATE()),
(N'مصروفات نظافة', N'مغاسل - مكافحة حشرات - إزالة مخلفات - أدوات نظافة', 1, GETUTCDATE()),
(N'مصروفات طريق', N'شحن - انتقالات - مصاريف سفر - بريد ومراسلات', 1, GETUTCDATE()),
(N'مصروفات سيارات', N'محروقات - زيوت - قطع غيار - تامين سيارات - تأجير سيارات', 1, GETUTCDATE()),
(N'مصاريف حكومية', N'ضبط اداري - شهادات صحية - اشتراكات - تصديقات - غرامات مرورية - فحص وتراخيص سيارات ومعدات - تصاريح', 1, GETUTCDATE()),
(N'مصروفات ضيافة', N'بوفيه واغراض الثلاجة والغرف - أغراض المطبخ', 1, GETUTCDATE()),
(N'مصروفات قرطاسية', N'احبار - أدوات كتابية', 1, GETUTCDATE()),
(N'رواتب وحوافز', N'رواتب التعقيب - حراسات - شغل عمالة خارجية', 1, GETUTCDATE()),
(N'تجهيزات وتاسيس', N'شراء اثاث - شراء أجهزة كهربائية وحاسب الي - اعمال ديكور - أبواب واخشاب - اعمال دهانات - أعمال وأدوات سباكة وكهرباء', 1, GETUTCDATE());
GO

PRINT '=============================================';
PRINT '✅ ExpenseCategories table created and populated successfully!';
PRINT '=============================================';

