-- SQL Script to create Users, Roles, Permissions, and RolePermissions tables
-- Note: Tables must be created in dependency order to avoid foreign key reference errors

-- Step 1: Create Permissions table first (no dependencies)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='permissions' and xtype='U')
BEGIN
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
    
    PRINT 'Permissions table created successfully.';
END
ELSE
BEGIN
    PRINT 'Permissions table already exists.';
END

-- Step 2: Create Roles table (depends on hotel_settings, but not on users yet)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='roles' and xtype='U')
BEGIN
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
        -- Note: Foreign keys to users table will be added after users table is created
    );
    
    PRINT 'Roles table created successfully.';
END
ELSE
BEGIN
    PRINT 'Roles table already exists.';
END

-- Step 3: Create Users table (depends on hotel_settings and roles)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='users' and xtype='U')
BEGIN
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
    
    PRINT 'Users table created successfully.';
END
ELSE
BEGIN
    PRINT 'Users table already exists.';
END

-- Step 4: Add foreign key constraints to roles table (now that users table exists)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Roles_CreatedBy_Users')
BEGIN
    ALTER TABLE roles
    ADD CONSTRAINT FK_Roles_CreatedBy_Users FOREIGN KEY (created_by) REFERENCES users(user_id);
    PRINT 'Added FK_Roles_CreatedBy_Users constraint.';
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Roles_UpdatedBy_Users')
BEGIN
    ALTER TABLE roles
    ADD CONSTRAINT FK_Roles_UpdatedBy_Users FOREIGN KEY (updated_by) REFERENCES users(user_id);
    PRINT 'Added FK_Roles_UpdatedBy_Users constraint.';
END

-- Step 5: Create RolePermissions table (depends on roles, permissions, and users)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='role_permissions' and xtype='U')
BEGIN
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
    
    PRINT 'RolePermissions table created successfully.';
END
ELSE
BEGIN
    PRINT 'RolePermissions table already exists.';
END

-- Seed initial permissions based on the Zaaer system
IF NOT EXISTS (SELECT 1 FROM permissions)
BEGIN
    INSERT INTO permissions (permission_name, permission_code, module_name, action_name, description) VALUES
    -- Users Module
    ('قائمة المستخدمين', 'users.list', 'users', 'list', 'View users list'),
    ('إنشاء مستخدم', 'users.create', 'users', 'create', 'Create new user'),
    ('تعديل مستخدم', 'users.edit', 'users', 'edit', 'Edit existing user'),
    ('حذف مستخدم', 'users.delete', 'users', 'delete', 'Delete user'),
    ('تغيير المناوبة', 'users.change_shift', 'users', 'change_shift', 'Change user shift'),
    
    -- Roles Module
    ('قائمة الأدوار', 'roles.list', 'roles', 'list', 'View roles list'),
    ('إنشاء دور', 'roles.create', 'roles', 'create', 'Create new role'),
    ('تعديل دور', 'roles.edit', 'roles', 'edit', 'Edit existing role'),
    ('حذف دور', 'roles.delete', 'roles', 'delete', 'Delete role'),
    
    -- Branches Module
    ('قائمة الفروع', 'branches.list', 'branches', 'list', 'View branches list'),
    ('إنشاء فرع', 'branches.create', 'branches', 'create', 'Create new branch'),
    ('تعديل فرع', 'branches.edit', 'branches', 'edit', 'Edit existing branch'),
    ('معاينة فرع', 'branches.view', 'branches', 'view', 'View branch details'),
    
    -- Units Module
    ('قائمة الوحدات', 'units.list', 'units', 'list', 'View units list'),
    ('إنشاء وحدة', 'units.create', 'units', 'create', 'Create new unit'),
    ('تعديل وحدة', 'units.edit', 'units', 'edit', 'Edit existing unit'),
    ('معاينة وحدة', 'units.view', 'units', 'view', 'View unit details'),
    ('حذف وحدة', 'units.delete', 'units', 'delete', 'Delete unit'),
    ('تغيير حالة الوحدة', 'units.change_status', 'units', 'change_status', 'Change unit status'),
    ('وحدات متعدده', 'units.multiple', 'units', 'multiple', 'Multiple units operations'),
    
    -- Blocks Module
    ('قائمة البلوكات', 'blocks.list', 'blocks', 'list', 'View blocks list'),
    ('إنشاء بلوك', 'blocks.create', 'blocks', 'create', 'Create new block'),
    ('تعديل بلوك', 'blocks.edit', 'blocks', 'edit', 'Edit existing block'),
    ('معاينة بلوك', 'blocks.view', 'blocks', 'view', 'View block details'),
    ('حذف بلوك', 'blocks.delete', 'blocks', 'delete', 'Delete block'),
    
    -- Unit Types Module
    ('قائمة أنواع الوحدات', 'unit_types.list', 'unit_types', 'list', 'View unit types list'),
    ('إنشاء نوع وحدة', 'unit_types.create', 'unit_types', 'create', 'Create new unit type'),
    ('تعديل نوع وحدة', 'unit_types.edit', 'unit_types', 'edit', 'Edit existing unit type'),
    ('حذف نوع وحدة', 'unit_types.delete', 'unit_types', 'delete', 'Delete unit type'),
    
    -- Facilities Module
    ('قائمة المرافق', 'facilities.list', 'facilities', 'list', 'View facilities list'),
    ('إنشاء مرفق', 'facilities.create', 'facilities', 'create', 'Create new facility'),
    ('تعديل مرفق', 'facilities.edit', 'facilities', 'edit', 'Edit existing facility'),
    ('معاينة مرفق', 'facilities.view', 'facilities', 'view', 'View facility details'),
    ('حذف مرفق', 'facilities.delete', 'facilities', 'delete', 'Delete facility'),
    
    -- Bookings Module
    ('قائمة الحجوزات', 'bookings.list', 'bookings', 'list', 'View bookings list'),
    ('إنشاء حجز', 'bookings.create', 'bookings', 'create', 'Create new booking'),
    ('تعديل حجز', 'bookings.edit', 'bookings', 'edit', 'Edit existing booking'),
    ('تسجيل الوصول', 'bookings.checkin', 'bookings', 'checkin', 'Check-in booking'),
    ('تسجيل المغادرة', 'bookings.checkout', 'bookings', 'checkout', 'Check-out booking'),
    ('التراجع عن تسجيل الوصول', 'bookings.undo_checkin', 'bookings', 'undo_checkin', 'Undo check-in'),
    ('تغيير الوحدة', 'bookings.change_unit', 'bookings', 'change_unit', 'Change booking unit'),
    ('الغاء الحجوزات', 'bookings.cancel', 'bookings', 'cancel', 'Cancel booking'),
    ('إعادة فتح الحجز', 'bookings.reopen', 'bookings', 'reopen', 'Reopen booking'),
    ('عدم الحضور', 'bookings.no_show', 'bookings', 'no_show', 'Mark as no show'),
    ('ملخص الحجز', 'bookings.summary', 'bookings', 'summary', 'View booking summary'),
    ('عقد التسكين', 'bookings.contract', 'bookings', 'contract', 'View accommodation contract'),
    ('تسجيل خروج متأخر', 'bookings.late_checkout', 'bookings', 'late_checkout', 'Late check-out'),
    ('تعديل الضريبة', 'bookings.modify_tax', 'bookings', 'modify_tax', 'Modify booking tax'),
    ('إضافة خصم', 'bookings.add_discount', 'bookings', 'add_discount', 'Add discount to booking'),
    ('التراجع عن الغاء الحجز', 'bookings.undo_cancel', 'bookings', 'undo_cancel', 'Undo booking cancellation'),
    ('تعديل المبلغ الإجمالي للإيجار', 'bookings.modify_total', 'bookings', 'modify_total', 'Modify total rental amount'),
    ('خفض السعر إلى ما دون الحد الأدنى', 'bookings.reduce_price', 'bookings', 'reduce_price', 'Reduce price below minimum'),
    ('انشاء حجز بالجمله', 'bookings.bulk_create', 'bookings', 'bulk_create', 'Create bulk booking'),
    ('إضافة شركة جديدة', 'bookings.add_company', 'bookings', 'add_company', 'Add new company'),
    ('تعديل نوع السعر', 'bookings.modify_price_type', 'bookings', 'modify_price_type', 'Modify price type'),
    ('عرض الملخص المالي الإجمالي', 'bookings.financial_summary', 'bookings', 'financial_summary', 'View total financial summary'),
    ('توفر الوحدات', 'bookings.unit_availability', 'bookings', 'unit_availability', 'Unit availability on dashboard'),
    ('تفاصيل الحجز', 'bookings.details', 'bookings', 'details', 'View booking details'),
    
    -- Reviews Module
    ('قائمة المراجعات', 'reviews.list', 'reviews', 'list', 'View reviews list'),
    ('رد على مراجعة', 'reviews.reply', 'reviews', 'reply', 'Reply to review'),
    
    -- OTA Messages Module
    ('قائمة رسائل OTA', 'ota_messages.list', 'ota_messages', 'list', 'View OTA messages list'),
    ('رد على رسالة OTA', 'ota_messages.reply', 'ota_messages', 'reply', 'Reply to OTA message'),
    
    -- Receipt Voucher Module
    ('قائمة إيصالات الاستلام', 'receipt_vouchers.list', 'receipt_vouchers', 'list', 'View receipt vouchers list'),
    ('إنشاء إيصال استلام', 'receipt_vouchers.create', 'receipt_vouchers', 'create', 'Create receipt voucher'),
    ('الغاء إيصال استلام', 'receipt_vouchers.cancel', 'receipt_vouchers', 'cancel', 'Cancel receipt voucher'),
    ('معاينة إيصال استلام', 'receipt_vouchers.preview', 'receipt_vouchers', 'preview', 'Preview receipt voucher'),
    ('إنشاء إيصال من رصيد المحفظة', 'receipt_vouchers.from_wallet', 'receipt_vouchers', 'from_wallet', 'Create receipt from wallet balance'),
    ('السماح بالسحب مع الإيصال', 'receipt_vouchers.withdraw_with_receipt', 'receipt_vouchers', 'withdraw_with_receipt', 'Allow withdrawal with receipt'),
    
    -- Refund Voucher Module
    ('قائمة إيصالات الاسترداد', 'refund_vouchers.list', 'refund_vouchers', 'list', 'View refund vouchers list'),
    ('إنشاء إيصال استرداد', 'refund_vouchers.create', 'refund_vouchers', 'create', 'Create refund voucher'),
    ('معاينة إيصال استرداد', 'refund_vouchers.preview', 'refund_vouchers', 'preview', 'Preview refund voucher'),
    ('الغاء إيصال استرداد', 'refund_vouchers.cancel', 'refund_vouchers', 'cancel', 'Cancel refund voucher'),
    
    -- Forms/Templates Module
    ('قائمة النماذج', 'forms.list', 'forms', 'list', 'View forms list'),
    ('إنشاء نموذج', 'forms.create', 'forms', 'create', 'Create new form'),
    ('معاينة نموذج', 'forms.preview', 'forms', 'preview', 'Preview form'),
    ('تعديل نموذج', 'forms.edit', 'forms', 'edit', 'Edit existing form'),
    ('تحصيل نموذج', 'forms.collect', 'forms', 'collect', 'Collect form'),
    ('الغاء نموذج', 'forms.cancel', 'forms', 'cancel', 'Cancel form'),
    
    -- e-Payment Link Module
    ('قائمة روابط الدفع الإلكتروني', 'epayment_links.list', 'epayment_links', 'list', 'View e-payment links list'),
    ('إنشاء رابط دفع إلكتروني', 'epayment_links.create', 'epayment_links', 'create', 'Create e-payment link'),
    
    -- Invoice Module
    ('قائمة الفواتير', 'invoices.list', 'invoices', 'list', 'View invoices list'),
    ('إنشاء فاتورة', 'invoices.create', 'invoices', 'create', 'Create new invoice'),
    ('معاينة فاتورة', 'invoices.preview', 'invoices', 'preview', 'Preview invoice'),
    ('إشعار دائن', 'invoices.credit_note', 'invoices', 'credit_note', 'Create credit note'),
    ('إشعار مدين', 'invoices.debit_note', 'invoices', 'debit_note', 'Create debit note'),
    
    -- Order Module
    ('قائمة الطلبات', 'orders.list', 'orders', 'list', 'View orders list'),
    ('حذف طلب', 'orders.delete', 'orders', 'delete', 'Delete order'),
    
    -- Sub-booking Module
    ('حجز فرعي', 'sub_bookings', 'sub_bookings', 'manage', 'Manage sub-bookings');
    
    PRINT 'Initial permissions seeded successfully.';
END
ELSE
BEGIN
    PRINT 'Permissions table already contains data.';
END

-- Create a default admin role for each hotel
IF NOT EXISTS (SELECT 1 FROM roles WHERE role_name = 'مدير النظام')
BEGIN
    INSERT INTO roles (hotel_id, role_name, role_description, is_active)
    SELECT hotel_id, 'مدير النظام', 'مدير النظام مع جميع الصلاحيات', 1
    FROM hotel_settings;
    
    PRINT 'Default admin roles created for all hotels.';
END
ELSE
BEGIN
    PRINT 'Admin roles already exist.';
END

-- Assign all permissions to admin roles
IF EXISTS (SELECT 1 FROM roles WHERE role_name = 'مدير النظام' AND role_id NOT IN (SELECT DISTINCT role_id FROM role_permissions))
BEGIN
    INSERT INTO role_permissions (role_id, permission_id, granted, created_at)
    SELECT r.role_id, p.permission_id, 1, GETDATE()
    FROM roles r
    CROSS JOIN permissions p
    WHERE r.role_name = 'مدير النظام';
    
    PRINT 'All permissions assigned to admin roles.';
END
ELSE
BEGIN
    PRINT 'Admin roles already have permissions assigned.';
END
