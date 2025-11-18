-- SQL INSERT Statements for expense_categories table
-- Based on the data from dbo.expense_categories table
-- Date: 2025-11-17
-- Hotel ID: 11 (Dammam2)

-- Note: All categories have hotel_id = 11, is_active = True (1), and created_at = '2025-11-12 13:22:50'

INSERT INTO expense_categories (
    expense_category_id,
    hotel_id,
    category_name,
    description,
    is_active,
    created_at,
    updated_at
) VALUES
(1, 11, N'مصروفات صيانة', NULL, 1, '2025-11-12 13:22:50', NULL),
(2, 11, N'مصروفات تجديدات وتأسيس', NULL, 1, '2025-11-12 13:22:50', NULL),
(3, 11, N'مصروفات مصاعد', NULL, 1, '2025-11-12 13:22:50', NULL),
(4, 11, N'مصروفات كهرباء', NULL, 1, '2025-11-12 13:22:50', NULL),
(5, 11, N'مصروفات مياة', NULL, 1, '2025-11-12 13:22:50', NULL),
(6, 11, N'مصروفات هاتف', NULL, 1, '2025-11-12 13:22:50', NULL),
(7, 11, N'مصروفات ورشة الفرع', NULL, 1, '2025-11-12 13:22:50', NULL),
(8, 11, N'صيانه أدوات سلامة', NULL, 1, '2025-11-12 13:22:50', NULL),
(9, 11, N'مصروفات الحراسات الأمنية', NULL, 1, '2025-11-12 13:22:50', NULL),
(10, 11, N'مصروفات تجديد السجلات والرخص', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, 11, N'مصروفات رسم الاشغال والبلدية', NULL, 1, '2025-11-12 13:22:50', NULL),
(12, 11, N'جاري العييري', NULL, 1, '2025-11-12 13:22:50', NULL);

-- Alternative: If expense_category_id is an IDENTITY column, use this version instead:
/*
INSERT INTO expense_categories (
    hotel_id,
    category_name,
    description,
    is_active,
    created_at,
    updated_at
) VALUES
(11, N'مصروفات صيانة', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'مصروفات تجديدات وتأسيس', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'مصروفات مصاعد', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'مصروفات كهرباء', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'مصروفات مياة', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'مصروفات هاتف', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'مصروفات ورشة الفرع', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'صيانه أدوات سلامة', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'مصروفات الحراسات الأمنية', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'مصروفات تجديد السجلات والرخص', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'مصروفات رسم الاشغال والبلدية', NULL, 1, '2025-11-12 13:22:50', NULL),
(11, N'جاري العييري', NULL, 1, '2025-11-12 13:22:50', NULL);
*/

-- Notes:
-- 1. All categories use hotel_id = 11 (which corresponds to Dammam2 based on hotel_settings table)
-- 2. All categories have is_active = 1 (True)
-- 3. All categories have the same created_at timestamp: '2025-11-12 13:22:50'
-- 4. All descriptions are NULL
-- 5. All updated_at are NULL
-- 6. Arabic text fields use N prefix for Unicode support (NVARCHAR)
-- 7. If expense_category_id is IDENTITY, remove it from INSERT and let SQL Server auto-generate it

