# 🔐 كيفية الحصول على BCrypt Hash لكلمة المرور "123"

## 📋 الطرق المتاحة

### ✅ الطريقة 1: Python (الأسهل والأسرع)

```bash
python -c "import bcrypt; print(bcrypt.hashpw(b'123', bcrypt.gensalt(rounds=12)).decode('utf-8'))"
```

**النتيجة:** ستحصل على hash مثل:
```
$2a$12$abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUV
```

**انسخ هذا Hash وضعه في SQL Script:**
```sql
DECLARE @DefaultPasswordHash NVARCHAR(500) = '$2a$12$abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUV';
```

---

### ✅ الطريقة 2: Python Script

```bash
cd zaaerIntegration/Scripts
python Add70Users.py
```

هذا Script سيحسب hash تلقائياً ويضيف المستخدمين مباشرة.

---

### ✅ الطريقة 3: PowerShell Script

```powershell
cd zaaerIntegration/Scripts
.\GetBCryptHash.ps1
```

---

### ✅ الطريقة 4: C# Code

```csharp
using BCrypt.Net;

var password = "123";
var hash = BCrypt.HashPassword(password, BCrypt.GenerateSalt(12));
Console.WriteLine(hash);
```

---

## 📝 خطوات استخدام SQL Script

### الخطوة 1: حساب Hash

استخدم أحد الطرق أعلاه لحساب BCrypt hash لكلمة المرور "123".

### الخطوة 2: تحديث SQL Script

افتح ملف `Add70Users_Ready.sql` وابحث عن السطر:
```sql
DECLARE @DefaultPasswordHash NVARCHAR(500) = '$2a$12$...';
```

استبدل القيمة بـ Hash الذي حصلت عليه.

### الخطوة 3: تشغيل SQL Script

في SQL Server Management Studio:
1. افتح ملف `Add70Users_Ready.sql`
2. تأكد من الاتصال بقاعدة البيانات الصحيحة (db32357)
3. اضغط F5 لتشغيل Script

---

## ⚠️ ملاحظات مهمة

1. **BCrypt hash يتغير في كل مرة** بسبب salt
   - كل مرة تحسب hash جديد، ستحصل على قيمة مختلفة
   - كل hash صحيح وسيعمل مع كلمة المرور "123"

2. **استخدم rounds=12** للحصول على hash متوافق مع التطبيق

3. **انسخ Hash كاملاً** بدون أخطاء

---

## 🎯 التوصية

**استخدم Python Script (`Add70Users.py`)** لأنه:
- ✅ يحسب hash تلقائياً
- ✅ يضيف المستخدمين مباشرة
- ✅ لا يحتاج خطوات إضافية
- ✅ أسهل وأسرع

---

## 📞 الدعم

للمزيد من المعلومات:
- راجع `SQL_SCRIPT_INSTRUCTIONS.md`
- راجع `README.md`

