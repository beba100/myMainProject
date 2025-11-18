# =============================================
# Script: إضافة 70 مستخدم في Master DB
# Database: Master DB (db32357)
# Server: s800.public.eu.machineasp.net
# =============================================

param(
    [string]$ConnectionString = "Server=s800.public.eu.machineasp.net; Database=db32357; User Id=admin; Password=vS6FWjgGoHcjwbcQaFby1pcx; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;"
)

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "إضافة 70 مستخدم في Master DB" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# تحميل مكتبة BCrypt من NuGet package
# يجب تثبيت BCrypt.Net-Next package أولاً: Install-Package BCrypt.Net-Next
$bcryptDllPath = Join-Path $PSScriptRoot "..\bin\Debug\net8.0\BCrypt.Net-Next.dll"
if (Test-Path $bcryptDllPath) {
    Add-Type -Path $bcryptDllPath -ErrorAction SilentlyContinue
}

# إذا لم تكن المكتبة موجودة، سنستخدم طريقة بديلة
function HashPassword {
    param([string]$password)
    
    # استخدام BCrypt.Net-Next عبر .NET
    try {
        if ([BCrypt.Net.BCrypt] -ne $null) {
            $bcrypt = [BCrypt.Net.BCrypt]::HashPassword($password, [BCrypt.Net.BCrypt]::GenerateSalt(12))
            return $bcrypt
        }
    }
    catch {
        # إذا فشل، نستخدم hash بسيط (للاستخدام في التطوير فقط)
        Write-Warning "BCrypt not available, using simple hash (NOT SECURE FOR PRODUCTION)"
        Write-Warning "Please install BCrypt.Net-Next package or use Python script instead"
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
        $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
        return [Convert]::ToBase64String($hash)
    }
    
    # Fallback
    Write-Warning "BCrypt not available, using simple hash (NOT SECURE FOR PRODUCTION)"
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
    $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
    return [Convert]::ToBase64String($hash)
}

# الاتصال بقاعدة البيانات
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    Write-Host "✅ تم الاتصال بقاعدة البيانات بنجاح" -ForegroundColor Green
}
catch {
    Write-Host "❌ فشل الاتصال بقاعدة البيانات: $_" -ForegroundColor Red
    exit 1
}

# الحصول على قائمة Tenants
$tenantsQuery = "SELECT Id, Code, Name FROM Tenants"
$tenantsCommand = New-Object System.Data.SqlClient.SqlCommand($tenantsQuery, $connection)
$tenantsAdapter = New-Object System.Data.SqlClient.SqlDataAdapter($tenantsCommand)
$tenantsTable = New-Object System.Data.DataTable
$tenantsAdapter.Fill($tenantsTable) | Out-Null

if ($tenantsTable.Rows.Count -eq 0) {
    Write-Host "❌ لا توجد فنادق في قاعدة البيانات. يرجى إضافة فنادق أولاً." -ForegroundColor Red
    $connection.Close()
    exit 1
}

Write-Host "✅ تم العثور على $($tenantsTable.Rows.Count) فندق" -ForegroundColor Green
Write-Host ""

# الحصول على قائمة Roles
$rolesQuery = "SELECT Id, Code FROM Roles"
$rolesCommand = New-Object System.Data.SqlClient.SqlCommand($rolesQuery, $connection)
$rolesAdapter = New-Object System.Data.SqlClient.SqlDataAdapter($rolesCommand)
$rolesTable = New-Object System.Data.DataTable
$rolesAdapter.Fill($rolesTable) | Out-Null

if ($rolesTable.Rows.Count -eq 0) {
    Write-Host "❌ لا توجد أدوار في قاعدة البيانات. يرجى تشغيل CreateMasterUsersTables.sql أولاً." -ForegroundColor Red
    $connection.Close()
    exit 1
}

Write-Host "✅ تم العثور على $($rolesTable.Rows.Count) دور" -ForegroundColor Green
Write-Host ""

# إنشاء 70 مستخدم
$defaultPassword = "123" # كلمة مرور افتراضية
$passwordHash = HashPassword $defaultPassword

$successCount = 0
$errorCount = 0

# توزيع المستخدمين على الفنادق
$tenantIndex = 0
$roleIndex = 0

for ($i = 1; $i -le 70; $i++) {
    try {
        # اختيار Tenant (توزيع دوري)
        $tenant = $tenantsTable.Rows[$tenantIndex]
        $tenantId = $tenant["Id"]
        $tenantCode = $tenant["Code"]
        
        # اختيار Role (توزيع دوري)
        $role = $rolesTable.Rows[$roleIndex]
        $roleId = $role["Id"]
        
        # إنشاء اسم مستخدم فريد
        $username = "user$i"
        
        # التحقق من عدم وجود مستخدم بنفس الاسم
        $checkQuery = "SELECT COUNT(*) FROM MasterUsers WHERE Username = @username"
        $checkCommand = New-Object System.Data.SqlClient.SqlCommand($checkQuery, $connection)
        $checkCommand.Parameters.AddWithValue("@username", $username) | Out-Null
        $exists = $checkCommand.ExecuteScalar()
        
        if ($exists -gt 0) {
            Write-Host "⚠️  المستخدم $username موجود بالفعل، يتم التخطي..." -ForegroundColor Yellow
            $errorCount++
            continue
        }
        
        # إدراج المستخدم
        $insertUserQuery = @"
            INSERT INTO MasterUsers (Username, PasswordHash, TenantId, IsActive, CreatedAt)
            VALUES (@username, @passwordHash, @tenantId, 1, GETUTCDATE());
            SELECT SCOPE_IDENTITY();
"@
        
        $insertUserCommand = New-Object System.Data.SqlClient.SqlCommand($insertUserQuery, $connection)
        $insertUserCommand.Parameters.AddWithValue("@username", $username) | Out-Null
        $insertUserCommand.Parameters.AddWithValue("@passwordHash", $passwordHash) | Out-Null
        $insertUserCommand.Parameters.AddWithValue("@tenantId", $tenantId) | Out-Null
        
        $userId = $insertUserCommand.ExecuteScalar()
        
        # إضافة الدور للمستخدم
        $insertRoleQuery = "INSERT INTO UserRoles (UserId, RoleId) VALUES (@userId, @roleId)"
        $insertRoleCommand = New-Object System.Data.SqlClient.SqlCommand($insertRoleQuery, $connection)
        $insertRoleCommand.Parameters.AddWithValue("@userId", $userId) | Out-Null
        $insertRoleCommand.Parameters.AddWithValue("@roleId", $roleId) | Out-Null
        $insertRoleCommand.ExecuteNonQuery() | Out-Null
        
        Write-Host "✅ تم إضافة المستخدم: $username (Tenant: $tenantCode, Role: $($role['Code']))" -ForegroundColor Green
        $successCount++
        
        # تحديث المؤشرات
        $tenantIndex = ($tenantIndex + 1) % $tenantsTable.Rows.Count
        $roleIndex = ($roleIndex + 1) % $rolesTable.Rows.Count
    }
    catch {
        Write-Host "❌ خطأ في إضافة المستخدم $i : $_" -ForegroundColor Red
        $errorCount++
    }
}

$connection.Close()

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "تم الانتهاء!" -ForegroundColor Cyan
Write-Host "✅ نجح: $successCount" -ForegroundColor Green
Write-Host "❌ فشل: $errorCount" -ForegroundColor Red
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "كلمة المرور الافتراضية لجميع المستخدمين: $defaultPassword" -ForegroundColor Yellow
Write-Host "يُنصح بتغيير كلمات المرور بعد أول تسجيل دخول" -ForegroundColor Yellow

