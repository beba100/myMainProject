# 🔍 دليل اختبار تسجيل الدخول على Swagger

## 📋 خطوات الاختبار والتصحيح

---

## 🚀 الخطوة 1: تشغيل المشروع في وضع Debug

### في Visual Studio:
1. اضغط `F5` أو `Ctrl+F5` لتشغيل المشروع
2. تأكد من أن Profile هو `https` (يستخدم HTTPS)
3. سيفتح المتصفح تلقائياً على: `https://localhost:7131/swagger`

### في Command Line:
```bash
cd c:\myMainProject\zaaerIntegration
dotnet run
```

---

## 🔐 الخطوة 2: اختبار تسجيل الدخول

### 2.1. فتح Swagger UI
افتح المتصفح على:
```
https://localhost:7131/swagger
```

### 2.2. البحث عن Login Endpoint
1. ابحث عن `POST /api/auth/login`
2. اضغط على `POST /api/auth/login` لتوسيع التفاصيل
3. اضغط على `Try it out`

### 2.3. إدخال بيانات تسجيل الدخول
في حقل `Request body`، أدخل:
```json
{
  "username": "user1",
  "password": "123"
}
```

**ملاحظة:** تأكد من أن المستخدم موجود في Master DB وأن كلمة المرور صحيحة.

### 2.4. تنفيذ الطلب
1. اضغط على `Execute`
2. شاهد النتيجة في `Responses`

---

## ✅ النتيجة المتوقعة (نجاح)

### Response 200 OK:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "username": "user1",
  "tenantId": 1,
  "tenantCode": "Dammam1",
  "tenantName": "الدمام 1",
  "roles": ["Admin"],
  "expiresAt": "2024-01-02T12:00:00Z"
}
```

### الخطوات التالية:
1. **انسخ Token** من الاستجابة
2. اضغط على زر `Authorize` 🔒 في أعلى Swagger
3. في حقل `Value`، أدخل: `Bearer {your-token}`
   - مثال: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
4. اضغط `Authorize` ثم `Close`
5. الآن يمكنك استخدام أي endpoint محمي

---

## ❌ النتيجة المتوقعة (فشل)

### Response 401 Unauthorized:
```json
{
  "error": "Invalid username or password"
}
```

---

## 🐛 التصحيح (Debugging)

### 1. تفعيل Logging المفصل

#### في Visual Studio:
1. افتح `Output` window: `View` → `Output`
2. اختر `Show output from: Debug`
3. شاهد الـ logs أثناء تنفيذ الطلب

#### في Command Line:
الـ logs ستظهر مباشرة في الـ Console

### 2. وضع Breakpoints

#### في `AuthController.cs`:
ضع Breakpoint في:
- السطر 47: `var user = await _masterUserService.ValidateLoginAsync(...)`
- السطر 50: `if (user == null)`
- السطر 69: `var token = _jwtService.GenerateToken(...)`

#### في `MasterUserService.cs`:
ضع Breakpoint في:
- السطر 170: `var user = await GetByUsernameAsync(username)`
- السطر 178: `if (user.TenantId <= 0)`
- السطر 200: `if (!ValidatePassword(...))`

#### في `ValidatePassword`:
ضع Breakpoint في:
- السطر 82: `if (!passwordHash.StartsWith("$2a$")...)`
- السطر 92: `var isValid = BCrypt.Net.BCrypt.Verify(...)`

### 3. فحص البيانات في Debug

#### عند Breakpoint في `ValidateLoginAsync`:
افحص المتغيرات:
- `username` - يجب أن يكون "user1"
- `password` - يجب أن يكون "123"
- `user` - يجب أن يكون غير null
- `user.TenantId` - يجب أن يكون > 0
- `user.PasswordHash` - يجب أن يبدأ بـ `$2a$` أو `$2b$`
- `user.IsActive` - يجب أن يكون `true`

#### عند Breakpoint في `ValidatePassword`:
افحص المتغيرات:
- `password` - كلمة المرور المدخلة
- `passwordHash` - الـ hash من قاعدة البيانات
- `isValid` - نتيجة التحقق

### 4. فحص قاعدة البيانات

#### التحقق من المستخدم في Master DB:
```sql
SELECT 
    Id,
    Username,
    PasswordHash,
    TenantId,
    IsActive,
    CreatedAt
FROM MasterUsers
WHERE Username = 'user1'
```

**النتيجة المتوقعة:**
- `Username`: "user1"
- `PasswordHash`: يجب أن يبدأ بـ `$2a$` أو `$2b$`
- `TenantId`: يجب أن يكون > 0
- `IsActive`: 1 (true)

#### التحقق من Tenant:
```sql
SELECT 
    Id,
    Code,
    Name,
    DatabaseName
FROM Tenants
WHERE Id = (SELECT TenantId FROM MasterUsers WHERE Username = 'user1')
```

---

## 🔍 فحص الأخطاء الشائعة

### ❌ الخطأ 1: "User not found in Master DB"
**السبب:** المستخدم غير موجود في جدول `MasterUsers`

**الحل:**
1. تحقق من أن المستخدم موجود في Master DB
2. تحقق من أن `Username` مطابق تماماً (case-sensitive)

### ❌ الخطأ 2: "Password hash is not a valid BCrypt hash"
**السبب:** `PasswordHash` في قاعدة البيانات ليس BCrypt hash

**الحل:**
1. تحقق من أن `PasswordHash` يبدأ بـ `$2a$` أو `$2b$`
2. إذا كان SHA256 أو Base64، يجب إعادة تشفير كلمة المرور بـ BCrypt

### ❌ الخطأ 3: "User has invalid TenantId"
**السبب:** `TenantId` في جدول `MasterUsers` هو 0 أو null

**الحل:**
1. تحقق من أن `TenantId` موجود وصحيح
2. تحقق من أن Tenant موجود في جدول `Tenants`

### ❌ الخطأ 4: "Invalid password"
**السبب:** كلمة المرور المدخلة لا تطابق الـ hash في قاعدة البيانات

**الحل:**
1. تحقق من كلمة المرور الصحيحة
2. إذا نسيت كلمة المرور، قم بإعادة تشفيرها:
   ```csharp
   var hash = BCrypt.Net.BCrypt.HashPassword("123", BCrypt.Net.BCrypt.GenerateSalt(12));
   ```

### ❌ الخطأ 5: "User is inactive"
**السبب:** `IsActive` في جدول `MasterUsers` هو 0 (false)

**الحل:**
```sql
UPDATE MasterUsers
SET IsActive = 1
WHERE Username = 'user1'
```

---

## 📊 فحص الـ Logs

### في Visual Studio:
1. افتح `Output` window
2. ابحث عن:
   - `❌ Login failed:` - فشل تسجيل الدخول
   - `✅ Login successful:` - نجاح تسجيل الدخول
   - `❌ Password hash is not a valid BCrypt hash` - مشكلة في الـ hash

### في ملفات الـ Logs:
افتح ملف:
```
logs/log-YYYYMMDD.txt
```

ابحث عن:
- `Login attempt with invalid username`
- `Login failed: User not found in Master DB`
- `Login failed: Invalid password`
- `Login successful`

---

## 🧪 اختبارات إضافية

### 1. اختبار Validate Token Endpoint
1. بعد تسجيل الدخول، احصل على Token
2. ابحث عن `POST /api/auth/validate`
3. اضغط `Authorize` وأدخل Token
4. اضغط `Execute`
5. يجب أن ترى:
   ```json
   {
     "valid": true,
     "userId": "1",
     "tenantId": "1",
     "username": "user1",
     "roles": ["Admin"]
   }
   ```

### 2. اختبار Endpoint محمي
1. بعد تسجيل الدخول والتأكيد (Authorize)
2. ابحث عن أي endpoint محمي (مثل `GET /api/customers`)
3. اضغط `Try it out` ثم `Execute`
4. يجب أن يعمل بدون `X-Hotel-Code` header (لأن TenantId يأتي من Token)

---

## 💡 نصائح للتصحيح

1. **استخدم Breakpoints** - ضع Breakpoints في جميع النقاط المهمة
2. **راقب الـ Logs** - شاهد الـ logs أثناء التنفيذ
3. **افحص قاعدة البيانات** - تأكد من أن البيانات صحيحة
4. **اختبر خطوة بخطوة** - اختبر كل جزء على حدة
5. **استخدم Swagger** - أسهل طريقة للاختبار

---

## 🎯 Checklist للتصحيح

- [ ] المستخدم موجود في `MasterUsers` table
- [ ] `PasswordHash` يبدأ بـ `$2a$` أو `$2b$`
- [ ] `TenantId` > 0
- [ ] `IsActive` = 1
- [ ] Tenant موجود في `Tenants` table
- [ ] كلمة المرور صحيحة
- [ ] Token يتم إنشاؤه بنجاح
- [ ] `TenantId` موجود في Token

---

## 📞 إذا استمرت المشكلة

1. تحقق من الـ Logs المفصلة
2. استخدم Breakpoints لفحص البيانات
3. تحقق من قاعدة البيانات مباشرة
4. تأكد من أن جميع الـ Services مُسجلة في `Program.cs`

