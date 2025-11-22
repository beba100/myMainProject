-- =============================================
-- Script: إنشاء جدول ExpenseApprovalHistory لتتبع مسار طلب المصروف
-- Database: Tenant DB
-- =============================================

-- إنشاء جدول ExpenseApprovalHistory
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[expense_approval_history]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[expense_approval_history] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [expense_id] INT NOT NULL,
        [action] NVARCHAR(50) NOT NULL, -- created, approved, rejected, awaiting-manager, awaiting-accountant, awaiting-admin
        [action_by] INT NULL, -- UserId من MasterUsers
        [action_by_full_name] NVARCHAR(200) NULL, -- الاسم الكامل للمستخدم (للعرض السريع)
        [action_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [status] NVARCHAR(30) NOT NULL, -- الحالة بعد هذا الإجراء
        [rejection_reason] NVARCHAR(500) NULL, -- سبب الرفض (إذا كان الإجراء رفض)
        [comments] NVARCHAR(500) NULL, -- ملاحظات إضافية
        
        CONSTRAINT [FK_ExpenseApprovalHistory_Expenses] FOREIGN KEY ([expense_id]) 
            REFERENCES [dbo].[expenses]([expense_id]) ON DELETE CASCADE
    );
    
    -- Index على expense_id للبحث السريع
    CREATE INDEX [IX_ExpenseApprovalHistory_ExpenseId] ON [dbo].[expense_approval_history]([expense_id]);
    
    -- Index على action_at للترتيب
    CREATE INDEX [IX_ExpenseApprovalHistory_ActionAt] ON [dbo].[expense_approval_history]([action_at]);
    
    PRINT '✅ Table expense_approval_history created successfully';
END
ELSE
BEGIN
    PRINT '⚠️ Table expense_approval_history already exists';
END
GO

-- إضافة created_by إلى جدول expenses إذا لم يكن موجوداً
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[expenses]') AND name = 'created_by')
BEGIN
    ALTER TABLE [dbo].[expenses]
    ADD [created_by] INT NULL;
    
    PRINT '✅ Column created_by added to expenses table';
END
ELSE
BEGIN
    PRINT '⚠️ Column created_by already exists in expenses table';
END
GO

