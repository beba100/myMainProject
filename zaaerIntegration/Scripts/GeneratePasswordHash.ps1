# 🔧 Script PowerShell لإنشاء BCrypt Hash لكلمة المرور
# استخدم هذا Script لإنشاء hash جديد لكلمة المرور

# تثبيت BCrypt.Net-Next إذا لم يكن مثبتاً
# Install-Package BCrypt.Net-Next -Force

# تحميل مكتبة BCrypt
Add-Type -Path ".\packages\BCrypt.Net-Next.4.0.3\lib\netstandard2.0\BCrypt.Net-Next.dll" -ErrorAction SilentlyContinue

# إذا لم تعمل الطريقة السابقة، استخدم هذا:
# Install-Module -Name BCrypt.Net-Next -Force
# Import-Module BCrypt.Net-Next

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔧 BCrypt Password Hash Generator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# كلمة المرور الافتراضية
$password = "123"

Write-Host "كلمة المرور: $password" -ForegroundColor Yellow
Write-Host ""

# محاولة إنشاء hash باستخدام BCrypt.Net-Next
try {
    # إذا كان BCrypt.Net-Next مثبتاً كـ NuGet package
    $bcryptAssembly = [System.Reflection.Assembly]::LoadFrom("$PSScriptRoot\..\packages\BCrypt.Net-Next.4.0.3\lib\netstandard2.0\BCrypt.Net-Next.dll")
    $bcryptType = $bcryptAssembly.GetType("BCrypt.Net.BCrypt")
    $hashMethod = $bcryptType.GetMethod("HashPassword", [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
    $saltMethod = $bcryptType.GetMethod("GenerateSalt", [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
    
    $salt = $saltMethod.Invoke($null, @(12))
    $hash = $hashMethod.Invoke($null, @($password, $salt))
    
    Write-Host "✅ تم إنشاء BCrypt Hash بنجاح!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Hash:" -ForegroundColor Cyan
    Write-Host $hash -ForegroundColor White
    Write-Host ""
    Write-Host "استخدم هذا الـ hash في SQL Script:" -ForegroundColor Yellow
    Write-Host "UPDATE MasterUsers SET PasswordHash = '$hash' WHERE Username = 'user1';" -ForegroundColor Green
}
catch {
    Write-Host "❌ لم يتم العثور على BCrypt.Net-Next" -ForegroundColor Red
    Write-Host ""
    Write-Host "الطريقة البديلة:" -ForegroundColor Yellow
    Write-Host "1. افتح Visual Studio" -ForegroundColor White
    Write-Host "2. أنشئ Console Application جديد" -ForegroundColor White
    Write-Host "3. ثبت BCrypt.Net-Next package:" -ForegroundColor White
    Write-Host "   Install-Package BCrypt.Net-Next" -ForegroundColor Cyan
    Write-Host "4. استخدم هذا الكود:" -ForegroundColor White
    Write-Host ""
    Write-Host "using BCrypt.Net;" -ForegroundColor Cyan
    Write-Host "var hash = BCrypt.HashPassword(`"123`", BCrypt.GenerateSalt(12));" -ForegroundColor Cyan
    Write-Host "Console.WriteLine(hash);" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "5. انسخ الـ hash واستخدمه في SQL Script" -ForegroundColor White
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

