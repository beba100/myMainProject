-- SQL Script to Drop and Recreate Users, Roles, Permissions, and RolePermissions tables
-- with Arabic-friendly column names

-- Step 1: Drop foreign key constraints first
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RolePermissions_CreatedBy_Users')
BEGIN
    ALTER TABLE role_permissions DROP CONSTRAINT FK_RolePermissions_CreatedBy_Users;
    PRINT 'FK_RolePermissions_CreatedBy_Users constraint dropped.';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RolePermissions_Permissions')
BEGIN
    ALTER TABLE role_permissions DROP CONSTRAINT FK_RolePermissions_Permissions;
    PRINT 'FK_RolePermissions_Permissions constraint dropped.';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RolePermissions_Roles')
BEGIN
    ALTER TABLE role_permissions DROP CONSTRAINT FK_RolePermissions_Roles;
    PRINT 'FK_RolePermissions_Roles constraint dropped.';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Roles')
BEGIN
    ALTER TABLE users DROP CONSTRAINT FK_Users_Roles;
    PRINT 'FK_Users_Roles constraint dropped.';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_HotelSettings')
BEGIN
    ALTER TABLE users DROP CONSTRAINT FK_Users_HotelSettings;
    PRINT 'FK_Users_HotelSettings constraint dropped.';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Roles_UpdatedBy_Users')
BEGIN
    ALTER TABLE roles DROP CONSTRAINT FK_Roles_UpdatedBy_Users;
    PRINT 'FK_Roles_UpdatedBy_Users constraint dropped.';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Roles_CreatedBy_Users')
BEGIN
    ALTER TABLE roles DROP CONSTRAINT FK_Roles_CreatedBy_Users;
    PRINT 'FK_Roles_CreatedBy_Users constraint dropped.';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Roles_HotelSettings')
BEGIN
    ALTER TABLE roles DROP CONSTRAINT FK_Roles_HotelSettings;
    PRINT 'FK_Roles_HotelSettings constraint dropped.';
END

-- Step 2: Drop existing tables in reverse dependency order
IF EXISTS (SELECT * FROM sysobjects WHERE name='role_permissions' and xtype='U')
BEGIN
    DROP TABLE role_permissions;
    PRINT 'role_permissions table dropped successfully.';
END

IF EXISTS (SELECT * FROM sysobjects WHERE name='users' and xtype='U')
BEGIN
    DROP TABLE users;
    PRINT 'users table dropped successfully.';
END

IF EXISTS (SELECT * FROM sysobjects WHERE name='roles' and xtype='U')
BEGIN
    DROP TABLE roles;
    PRINT 'roles table dropped successfully.';
END

IF EXISTS (SELECT * FROM sysobjects WHERE name='permissions' and xtype='U')
BEGIN
    DROP TABLE permissions;
    PRINT 'permissions table dropped successfully.';
END

-- Step 3: Create Permissions table first (no dependencies)
CREATE TABLE permissions (
    permission_id INT PRIMARY KEY IDENTITY(1,1),
    permission_name NVARCHAR(100) NOT NULL,
    permission_code NVARCHAR(100) NOT NULL,
    module_name NVARCHAR(50) NOT NULL,
    action_name NVARCHAR(50) NOT NULL,
    description NVARCHAR(500),
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT GETDATE()
);

PRINT 'permissions table created successfully.';

-- Step 4: Create Roles table (depends on hotel_settings)
CREATE TABLE roles (
    role_id INT PRIMARY KEY IDENTITY(1,1),
    hotel_id INT NOT NULL,
    role_name NVARCHAR(100) NOT NULL,
    role_description NVARCHAR(500),
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME,
    created_by INT,
    updated_by INT,
    
    CONSTRAINT FK_Roles_HotelSettings FOREIGN KEY (hotel_id) REFERENCES hotel_settings(hotel_id)
);

PRINT 'roles table created successfully.';

-- Step 5: Create Users table (depends on hotel_settings and roles)
CREATE TABLE users (
    user_id INT PRIMARY KEY IDENTITY(1,1),
    hotel_id INT NOT NULL,
    status BIT NOT NULL DEFAULT 1,
    user_type NVARCHAR(50),
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    title NVARCHAR(100),
    profile_picture_url NVARCHAR(500),
    signature_url NVARCHAR(500),
    date_of_birth DATETIME,
    gender NVARCHAR(20),
    department NVARCHAR(100),
    description NVARCHAR(1000),
    email NVARCHAR(255) NOT NULL,
    phone_number NVARCHAR(20),
    business_phone_number NVARCHAR(20),
    address NVARCHAR(500),
    password_hash NVARCHAR(255) NOT NULL,
    change_password BIT NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME,
    last_login DATETIME,
    is_active BIT NOT NULL DEFAULT 1,
    role_id INT,
    
    CONSTRAINT FK_Users_HotelSettings FOREIGN KEY (hotel_id) REFERENCES hotel_settings(hotel_id),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (role_id) REFERENCES roles(role_id)
);

PRINT 'users table created successfully.';

-- Step 6: Add foreign key constraints to roles table (now that users table exists)
ALTER TABLE roles
ADD CONSTRAINT FK_Roles_CreatedBy_Users FOREIGN KEY (created_by) REFERENCES users(user_id);

ALTER TABLE roles
ADD CONSTRAINT FK_Roles_UpdatedBy_Users FOREIGN KEY (updated_by) REFERENCES users(user_id);

PRINT 'Added foreign key constraints to roles table.';

-- Step 7: Create RolePermissions table (depends on roles, permissions, and users)
CREATE TABLE role_permissions (
    role_permission_id INT PRIMARY KEY IDENTITY(1,1),
    role_id INT NOT NULL,
    permission_id INT NOT NULL,
    granted BIT NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    created_by INT,
    
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (role_id) REFERENCES roles(role_id) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (permission_id) REFERENCES permissions(permission_id) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_CreatedBy_Users FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT IX_RolePermissions_RoleId_PermissionId UNIQUE (role_id, permission_id)
);

PRINT 'role_permissions table created successfully.';

-- Step 8: Seed initial permissions based on the Zaaer system
INSERT INTO permissions (permission_name, permission_code, module_name, action_name, description) VALUES
-- Users Module
(N'قائمة المستخدمين', 'users.list', 'users', 'list', 'View users list'),
(N'إنشاء مستخدم', 'users.create', 'users', 'create', 'Create new user'),
(N'تعديل مستخدم', 'users.edit', 'users', 'edit', 'Edit existing user'),
(N'حذف مستخدم', 'users.delete', 'users', 'delete', 'Delete user'),
(N'تغيير المناوبة', 'users.change_shift', 'users', 'change_shift', 'Change user shift'),

-- Roles Module
(N'قائمة الأدوار', 'roles.list', 'roles', 'list', 'View roles list'),
(N'إنشاء دور', 'roles.create', 'roles', 'create', 'Create new role'),
(N'تعديل دور', 'roles.edit', 'roles', 'edit', 'Edit existing role'),
(N'حذف دور', 'roles.delete', 'roles', 'delete', 'Delete role'),

-- Branches Module
(N'قائمة الفروع', 'branches.list', 'branches', 'list', 'View branches list'),
(N'إنشاء فرع', 'branches.create', 'branches', 'create', 'Create new branch'),
(N'تعديل فرع', 'branches.edit', 'branches', 'edit', 'Edit existing branch'),
(N'معاينة فرع', 'branches.view', 'branches', 'view', 'View branch details'),

-- Units Module
(N'قائمة الوحدات', 'units.list', 'units', 'list', 'View units list'),
(N'إنشاء وحدة', 'units.create', 'units', 'create', 'Create new unit'),
(N'تعديل وحدة', 'units.edit', 'units', 'edit', 'Edit existing unit'),
(N'معاينة وحدة', 'units.view', 'units', 'view', 'View unit details'),
(N'حذف وحدة', 'units.delete', 'units', 'delete', 'Delete unit'),
(N'تغيير حالة الوحدة', 'units.change_status', 'units', 'change_status', 'Change unit status'),
(N'وحدات متعدده', 'units.multiple', 'units', 'multiple', 'Multiple units operations'),

-- Blocks Module
(N'قائمة البلوكات', 'blocks.list', 'blocks', 'list', 'View blocks list'),
(N'إنشاء بلوك', 'blocks.create', 'blocks', 'create', 'Create new block'),
(N'تعديل بلوك', 'blocks.edit', 'blocks', 'edit', 'Edit existing block'),
(N'معاينة بلوك', 'blocks.view', 'blocks', 'view', 'View block details'),
(N'حذف بلوك', 'blocks.delete', 'blocks', 'delete', 'Delete block'),

-- Unit Types Module
(N'قائمة أنواع الوحدات', 'unit_types.list', 'unit_types', 'list', 'View unit types list'),
(N'إنشاء نوع وحدة', 'unit_types.create', 'unit_types', 'create', 'Create new unit type'),
(N'تعديل نوع وحدة', 'unit_types.edit', 'unit_types', 'edit', 'Edit existing unit type'),
(N'حذف نوع وحدة', 'unit_types.delete', 'unit_types', 'delete', 'Delete unit type'),

-- Facilities Module
(N'قائمة المرافق', 'facilities.list', 'facilities', 'list', 'View facilities list'),
(N'إنشاء مرفق', 'facilities.create', 'facilities', 'create', 'Create new facility'),
(N'تعديل مرفق', 'facilities.edit', 'facilities', 'edit', 'Edit existing facility'),
(N'معاينة مرفق', 'facilities.view', 'facilities', 'view', 'View facility details'),
(N'حذف مرفق', 'facilities.delete', 'facilities', 'delete', 'Delete facility'),

-- Bookings Module
(N'قائمة الحجوزات', 'bookings.list', 'bookings', 'list', 'View bookings list'),
(N'إنشاء حجز', 'bookings.create', 'bookings', 'create', 'Create new booking'),
(N'تعديل حجز', 'bookings.edit', 'bookings', 'edit', 'Edit existing booking'),
(N'تسجيل الوصول', 'bookings.checkin', 'bookings', 'checkin', 'Check-in booking'),
(N'تسجيل المغادرة', 'bookings.checkout', 'bookings', 'checkout', 'Check-out booking'),
(N'التراجع عن تسجيل الوصول', 'bookings.undo_checkin', 'bookings', 'undo_checkin', 'Undo check-in'),
(N'تغيير الوحدة', 'bookings.change_unit', 'bookings', 'change_unit', 'Change booking unit'),
(N'الغاء الحجوزات', 'bookings.cancel', 'bookings', 'cancel', 'Cancel booking'),
(N'إعادة فتح الحجز', 'bookings.reopen', 'bookings', 'reopen', 'Reopen booking'),
(N'عدم الحضور', 'bookings.no_show', 'bookings', 'no_show', 'Mark as no show'),
(N'ملخص الحجز', 'bookings.summary', 'bookings', 'summary', 'View booking summary'),
(N'عقد التسكين', 'bookings.contract', 'bookings', 'contract', 'View accommodation contract'),
(N'تسجيل خروج متأخر', 'bookings.late_checkout', 'bookings', 'late_checkout', 'Late check-out'),
(N'تعديل الضريبة', 'bookings.modify_tax', 'bookings', 'modify_tax', 'Modify booking tax'),
(N'إضافة خصم', 'bookings.add_discount', 'bookings', 'add_discount', 'Add discount to booking'),
(N'التراجع عن الغاء الحجز', 'bookings.undo_cancel', 'bookings', 'undo_cancel', 'Undo booking cancellation'),
(N'تعديل المبلغ الإجمالي للإيجار', 'bookings.modify_total', 'bookings', 'modify_total', 'Modify total rental amount'),
(N'خفض السعر إلى ما دون الحد الأدنى', 'bookings.reduce_price', 'bookings', 'reduce_price', 'Reduce price below minimum'),
(N'انشاء حجز بالجمله', 'bookings.bulk_create', 'bookings', 'bulk_create', 'Create bulk booking'),
(N'إضافة شركة جديدة', 'bookings.add_company', 'bookings', 'add_company', 'Add new company'),
(N'تعديل نوع السعر', 'bookings.modify_price_type', 'bookings', 'modify_price_type', 'Modify price type'),
(N'عرض الملخص المالي الإجمالي', 'bookings.financial_summary', 'bookings', 'financial_summary', 'View total financial summary'),
(N'توفر الوحدات', 'bookings.unit_availability', 'bookings', 'unit_availability', 'Unit availability on dashboard'),
(N'تفاصيل الحجز', 'bookings.details', 'bookings', 'details', 'View booking details'),

-- Reviews Module
(N'قائمة المراجعات', 'reviews.list', 'reviews', 'list', 'View reviews list'),
(N'رد على مراجعة', 'reviews.reply', 'reviews', 'reply', 'Reply to review'),

-- OTA Messages Module
(N'قائمة رسائل OTA', 'ota_messages.list', 'ota_messages', 'list', 'View OTA messages list'),
(N'رد على رسالة OTA', 'ota_messages.reply', 'ota_messages', 'reply', 'Reply to OTA message'),

-- Receipt Voucher Module
(N'قائمة إيصالات الاستلام', 'receipt_vouchers.list', 'receipt_vouchers', 'list', 'View receipt vouchers list'),
(N'إنشاء إيصال استلام', 'receipt_vouchers.create', 'receipt_vouchers', 'create', 'Create receipt voucher'),
(N'الغاء إيصال استلام', 'receipt_vouchers.cancel', 'receipt_vouchers', 'cancel', 'Cancel receipt voucher'),
(N'معاينة إيصال استلام', 'receipt_vouchers.preview', 'receipt_vouchers', 'preview', 'Preview receipt voucher'),
(N'إنشاء إيصال من رصيد المحفظة', 'receipt_vouchers.from_wallet', 'receipt_vouchers', 'from_wallet', 'Create receipt from wallet balance'),
(N'السماح بالسحب مع الإيصال', 'receipt_vouchers.withdraw_with_receipt', 'receipt_vouchers', 'withdraw_with_receipt', 'Allow withdrawal with receipt'),

-- Refund Voucher Module
(N'قائمة إيصالات الاسترداد', 'refund_vouchers.list', 'refund_vouchers', 'list', 'View refund vouchers list'),
(N'إنشاء إيصال استرداد', 'refund_vouchers.create', 'refund_vouchers', 'create', 'Create refund voucher'),
(N'معاينة إيصال استرداد', 'refund_vouchers.preview', 'refund_vouchers', 'preview', 'Preview refund voucher'),
(N'الغاء إيصال استرداد', 'refund_vouchers.cancel', 'refund_vouchers', 'cancel', 'Cancel refund voucher'),

-- Forms/Templates Module
(N'قائمة النماذج', 'forms.list', 'forms', 'list', 'View forms list'),
(N'إنشاء نموذج', 'forms.create', 'forms', 'create', 'Create new form'),
(N'معاينة نموذج', 'forms.preview', 'forms', 'preview', 'Preview form'),
(N'تعديل نموذج', 'forms.edit', 'forms', 'edit', 'Edit existing form'),
(N'تحصيل نموذج', 'forms.collect', 'forms', 'collect', 'Collect form'),
(N'الغاء نموذج', 'forms.cancel', 'forms', 'cancel', 'Cancel form'),

-- e-Payment Link Module
(N'قائمة روابط الدفع الإلكتروني', 'epayment_links.list', 'epayment_links', 'list', 'View e-payment links list'),
(N'إنشاء رابط دفع إلكتروني', 'epayment_links.create', 'epayment_links', 'create', 'Create e-payment link'),

-- Invoice Module
(N'قائمة الفواتير', 'invoices.list', 'invoices', 'list', 'View invoices list'),
(N'إنشاء فاتورة', 'invoices.create', 'invoices', 'create', 'Create new invoice'),
(N'معاينة فاتورة', 'invoices.preview', 'invoices', 'preview', 'Preview invoice'),
(N'إشعار دائن', 'invoices.credit_note', 'invoices', 'credit_note', 'Create credit note'),
(N'إشعار مدين', 'invoices.debit_note', 'invoices', 'debit_note', 'Create debit note'),

-- Order Module
(N'قائمة الطلبات', 'orders.list', 'orders', 'list', 'View orders list'),
(N'حذف طلب', 'orders.delete', 'orders', 'delete', 'Delete order'),

-- Sub-booking Module
(N'حجز فرعي', 'sub_bookings', 'sub_bookings', 'manage', 'Manage sub-bookings');

PRINT 'Initial permissions seeded successfully.';

-- Step 9: Create a default admin role for each hotel
INSERT INTO roles (hotel_id, role_name, role_description, is_active)
SELECT hotel_id, N'مدير النظام', N'مدير النظام مع جميع الصلاحيات', 1
FROM hotel_settings;

PRINT 'Default admin roles created for all hotels.';

-- Step 10: Assign all permissions to admin roles
INSERT INTO role_permissions (role_id, permission_id, granted, created_at)
SELECT r.role_id, p.permission_id, 1, GETDATE()
FROM roles r
CROSS JOIN permissions p
WHERE r.role_name = N'مدير النظام';

PRINT 'All permissions assigned to admin roles.';

PRINT 'All tables created and seeded successfully!';
