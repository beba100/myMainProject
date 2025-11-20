-- =============================================
-- Create UserTenants Table
-- إنشاء جدول UserTenants لربط المستخدمين بالفنادق (للصلاحيات)
-- =============================================

USE [YourDatabaseName]; -- Change this to your actual Master DB name
GO

-- Check if UserTenants table exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserTenants]') AND type in (N'U'))
BEGIN
    -- Create UserTenants table
    CREATE TABLE [dbo].[UserTenants] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] INT NOT NULL,
        [TenantId] INT NOT NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        
        -- Foreign Key to MasterUsers
        CONSTRAINT [FK_UserTenants_MasterUsers] 
            FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[MasterUsers]([Id]) 
            ON DELETE CASCADE,
        
        -- Foreign Key to Tenants
        CONSTRAINT [FK_UserTenants_Tenants] 
            FOREIGN KEY ([TenantId]) 
            REFERENCES [dbo].[Tenants]([Id]) 
            ON DELETE NO ACTION,
        
        -- Unique constraint: User can't have same tenant twice
        CONSTRAINT [UQ_UserTenants_UserId_TenantId] 
            UNIQUE ([UserId], [TenantId])
    );
    
    PRINT '✅ UserTenants table created successfully.';
END
ELSE
BEGIN
    PRINT '⚠️ UserTenants table already exists.';
END
GO

-- Add indexes for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserTenants_UserId' AND object_id = OBJECT_ID('dbo.UserTenants'))
BEGIN
    CREATE INDEX [IX_UserTenants_UserId] ON [dbo].[UserTenants]([UserId]);
    PRINT '✅ Index IX_UserTenants_UserId created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserTenants_TenantId' AND object_id = OBJECT_ID('dbo.UserTenants'))
BEGIN
    CREATE INDEX [IX_UserTenants_TenantId] ON [dbo].[UserTenants]([TenantId]);
    PRINT '✅ Index IX_UserTenants_TenantId created.';
END
GO

-- =============================================
-- Example: Add hotels for Reception Staff (Staff role)
-- مثال: إضافة فندق واحد لموظف استقبال
-- =============================================
-- Example: user12 is Reception Staff in Riyadh3 (TenantId = 9)
-- INSERT INTO UserTenants (UserId, TenantId)
-- VALUES (12, 9);   -- 9 = Riyadh3 (from Tenants table)
-- GO

-- =============================================
-- Example: Add multiple hotels for Supervisor
-- مثال: إضافة عدة فنادق لمشرف
-- =============================================
-- Example: user15 is Supervisor for multiple hotels
-- INSERT INTO UserTenants (UserId, TenantId)
-- VALUES 
--     (15, 1),  -- Dammam1
--     (15, 2),  -- Dammam2
--     (15, 9);  -- Riyadh3
-- GO

PRINT '✅ UserTenants table setup completed!';
GO

