# Script لحساب BCrypt hash لكلمة المرور "123"
# استخدم هذا لحساب hash ثم ضعه في SQL Script

# تأكد من تثبيت BCrypt: pip install bcrypt
# ثم استخدم: python -c "import bcrypt; print(bcrypt.hashpw(b'123', bcrypt.gensalt(rounds=12)).decode('utf-8'))"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "حساب BCrypt Hash لكلمة المرور '123'" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# محاولة استخدام Python
$pythonCmd = Get-Command python -ErrorAction SilentlyContinue
if ($pythonCmd) {
    Write-Host "✅ تم العثور على Python" -ForegroundColor Green
    Write-Host "جاري حساب Hash..." -ForegroundColor Yellow
    Write-Host ""
    
    $hash = python -c "import bcrypt; print(bcrypt.hashpw(b'123', bcrypt.gensalt(rounds=12)).decode('utf-8'))"
    
    Write-Host "BCrypt Hash لكلمة المرور '123':" -ForegroundColor Green
    Write-Host $hash -ForegroundColor White
    Write-Host ""
    Write-Host "انسخ هذا Hash وضعه في SQL Script (Add70Users.sql)" -ForegroundColor Yellow
    Write-Host "في السطر: DECLARE @DefaultPasswordHash NVARCHAR(500) = '...';" -ForegroundColor Yellow
}
else {
    Write-Host "❌ Python غير مثبت" -ForegroundColor Red
    Write-Host ""
    Write-Host "لحساب Hash، استخدم أحد الطرق التالية:" -ForegroundColor Yellow
    Write-Host "1. تثبيت Python ثم تشغيل:" -ForegroundColor White
    Write-Host "   python -c `"import bcrypt; print(bcrypt.hashpw(b'123', bcrypt.gensalt(rounds=12)).decode('utf-8'))`"" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "2. أو استخدم Python script: Add70Users.py" -ForegroundColor White
    Write-Host ""
    Write-Host "3. أو استخدم C# code:" -ForegroundColor White
    Write-Host "   BCrypt.Net.BCrypt.HashPassword(`"123`", BCrypt.Net.BCrypt.GenerateSalt(12))" -ForegroundColor Cyan
}

