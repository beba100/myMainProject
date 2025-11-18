# 📝 تعليمات استخدام SQL Script لإضافة المستخدمين

## ⚠️ ملاحظة مهمة

**SQL Server لا يحتوي على BCrypt مدمج!**

لذلك لديك خياران:

---

## ✅ الخيار 1: استخدام Python/PowerShell Script (موصى به)

هذا هو الخيار الأفضل والأسهل:

### Python Script:
```bash
cd zaaerIntegration/Scripts
pip install pyodbc bcrypt
python Add70Users.py
```

### PowerShell Script:
```powershell
cd zaaerIntegration/Scripts
.\Add70Users.ps1
```

**هذا الخيار:**
- ✅ يحسب BCrypt hash تلقائياً
- ✅ يعمل مباشرة
- ✅ آمن ومضمون

---

## ⚠️ الخيار 2: استخدام SQL Script (يتطلب خطوات إضافية)

إذا كنت تريد استخدام SQL Script مباشرة:

### الخطوة 1: حساب BCrypt Hash

احسب BCrypt hash لكلمة المرور "123" باستخدام أحد الطرق التالية:

#### الطريقة 1: Python (الأسهل)
```bash
python -c "import bcrypt; print(bcrypt.hashpw(b'123', bcrypt.gensalt(rounds=12)).decode('utf-8'))"
```

#### الطريقة 2: PowerShell Script
```powershell
.\GetBCryptHash.ps1
```

#### الطريقة 3: C# Code
```csharp
BCrypt.Net.BCrypt.HashPassword("123", BCrypt.Net.BCrypt.GenerateSalt(12))
```

### الخطوة 2: تحديث SQL Script

افتح ملف `Add70Users.sql` وابحث عن السطر:
```sql
DECLARE @DefaultPasswordHash NVARCHAR(500) = '$2a$12$...';
```

استبدل القيمة بـ Hash الذي حصلت عليه في الخطوة 1.

### الخطوة 3: تشغيل SQL Script

في SQL Server Management Studio:
1. افتح ملف `Add70Users.sql`
2. تأكد من الاتصال بقاعدة البيانات الصحيحة (db32357)
3. اضغط F5 لتشغيل Script

---

## 📋 الملفات المتوفرة

1. **Add70Users.sql** - SQL Script كامل (يتطلب حساب hash يدوياً)
2. **Add70Users_Simple.sql** - SQL Script مبسط (يستخدم SHA256 - غير آمن للإنتاج)
3. **Add70Users.py** - Python Script (موصى به) ✅
4. **Add70Users.ps1** - PowerShell Script ✅
5. **GetBCryptHash.ps1** - Script لحساب BCrypt hash

---

## 🎯 التوصية

**استخدم Python Script (`Add70Users.py`)** لأنه:
- ✅ سهل الاستخدام
- ✅ يحسب BCrypt hash تلقائياً
- ✅ يعمل مباشرة بدون خطوات إضافية
- ✅ آمن ومضمون

---

## 📝 مثال على Hash صحيح

BCrypt hash لكلمة المرور "123" يبدو هكذا:
```
$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYqJqZqZqZq
```

**ملاحظة:** هذا hash يتغير في كل مرة بسبب salt، لذلك يجب حساب hash جديد في كل مرة.

---

## ❓ استكشاف الأخطاء

### خطأ: "تسجيل الدخول لا يعمل"
- تأكد من استخدام BCrypt hash وليس SHA256
- احسب hash جديد واستبدله في SQL Script
- أو استخدم Python/PowerShell script بدلاً من SQL

### خطأ: "BCrypt hash غير صحيح"
- احسب hash جديد باستخدام Python
- تأكد من استخدام rounds=12
- تأكد من نسخ Hash كاملاً بدون أخطاء

---

## 📞 الدعم

للمزيد من المعلومات:
- راجع `AUTHENTICATION_GUIDE.md`
- راجع `README.md` في مجلد Scripts

