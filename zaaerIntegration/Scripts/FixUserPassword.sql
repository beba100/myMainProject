-- 🔧 Script لفحص وإصلاح كلمة مرور المستخدم
-- استخدم هذا Script لفحص كلمة المرور الحالية وإصلاحها إذا لزم الأمر

-- ============================================
-- 1. فحص المستخدم الحالي
-- ============================================
SELECT 
    Id,
    Username,
    PasswordHash,
    TenantId,
    IsActive,
    CreatedAt,
    UpdatedAt,
    LEN(PasswordHash) AS HashLength,
    CASE 
        WHEN PasswordHash LIKE '$2a$%' THEN 'BCrypt ($2a$)'
        WHEN PasswordHash LIKE '$2b$%' THEN 'BCrypt ($2b$)'
        WHEN PasswordHash LIKE '$2y$%' THEN 'BCrypt ($2y$)'
        ELSE 'Unknown Format'
    END AS HashType
FROM MasterUsers
WHERE Username = 'user1';

-- ============================================
-- 2. إنشاء BCrypt Hash جديد لكلمة المرور "123"
-- ============================================
-- ⚠️ مهم: يجب إنشاء الـ hash من C# أو PowerShell
-- استخدم هذا الكود في C#:
-- var hash = BCrypt.Net.BCrypt.HashPassword("123", BCrypt.Net.BCrypt.GenerateSalt(12));
-- ثم انسخ الـ hash وضعه في المتغير @NewPasswordHash أدناه

-- ============================================
-- 3. تحديث كلمة المرور (استبدل @NewPasswordHash بالـ hash الصحيح)
-- ============================================
-- DECLARE @NewPasswordHash NVARCHAR(500) = '$2a$12$...'; -- ضع الـ hash هنا
-- 
-- UPDATE MasterUsers
-- SET PasswordHash = @NewPasswordHash,
--     UpdatedAt = GETUTCDATE()
-- WHERE Username = 'user1';
-- 
-- SELECT 'Password updated successfully' AS Result;

-- ============================================
-- 4. التحقق من التحديث
-- ============================================
-- SELECT 
--     Username,
--     PasswordHash,
--     UpdatedAt
-- FROM MasterUsers
-- WHERE Username = 'user1';

