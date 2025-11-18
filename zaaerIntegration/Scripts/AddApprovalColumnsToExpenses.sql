-- =============================================
-- إضافة أعمدة نظام الموافقات على المصروفات
-- Add Approval Workflow Columns to Expenses Table
-- =============================================

-- إضافة عمود حالة الموافقة
-- approval_status: auto-approved, pending, accepted, rejected
ALTER TABLE dbo.expenses
ADD approval_status VARCHAR(20) NOT NULL DEFAULT 'auto-approved';

-- إضافة عمود معرف المشرف الذي وافق/رفض
ALTER TABLE dbo.expenses
ADD approved_by INT NULL;

-- إضافة عمود تاريخ ووقت الموافقة/الرفض
ALTER TABLE dbo.expenses
ADD approved_at DATETIME NULL;

-- إضافة فهرس لتحسين الأداء عند البحث حسب حالة الموافقة
CREATE INDEX IX_Expenses_ApprovalStatus ON dbo.expenses(approval_status);

-- إضافة فهرس لتحسين الأداء عند البحث حسب المشرف
CREATE INDEX IX_Expenses_ApprovedBy ON dbo.expenses(approved_by);

-- =============================================
-- ملاحظات:
-- Notes:
-- =============================================
-- approval_status القيم الممكنة:
-- Possible values for approval_status:
--   - 'auto-approved': المبلغ <= 50 (موافقة تلقائية)
--   - 'pending': المبلغ > 50 (في انتظار الموافقة)
--   - 'accepted': وافق المشرف
--   - 'rejected': رفض المشرف
--
-- approved_by: معرف المستخدم (User ID) الذي وافق/رفض المصروف
-- approved_at: تاريخ ووقت الموافقة/الرفض
-- =============================================

