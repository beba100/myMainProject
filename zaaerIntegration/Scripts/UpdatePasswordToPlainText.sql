-- 🔧 Script لتحديث كلمة المرور إلى Plain Text
-- استخدم هذا Script لتحديث كلمة المرور في قاعدة البيانات إلى plain text

-- ============================================
-- 1. فحص المستخدم الحالي
-- ============================================
SELECT 
    Id,
    Username,
    PasswordHash,
    TenantId,
    IsActive
FROM MasterUsers
WHERE Username = 'user1';

-- ============================================
-- 2. تحديث كلمة المرور إلى Plain Text "123"
-- ============================================
UPDATE MasterUsers
SET PasswordHash = '123',
    UpdatedAt = GETUTCDATE()
WHERE Username = 'user1';

-- ============================================
-- 3. التحقق من التحديث
-- ============================================
SELECT 
    Username,
    PasswordHash,
    UpdatedAt
FROM MasterUsers
WHERE Username = 'user1';

-- ============================================
-- 4. تحديث جميع المستخدمين (اختياري)
-- ============================================
-- إذا كنت تريد تحديث جميع المستخدمين إلى كلمة مرور "123":
-- UPDATE MasterUsers
-- SET PasswordHash = '123',
--     UpdatedAt = GETUTCDATE()
-- WHERE PasswordHash LIKE '$2a$%' OR PasswordHash LIKE '$2b$%' OR PasswordHash LIKE '$2y$%';

