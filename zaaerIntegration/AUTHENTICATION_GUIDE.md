# 🔐 دليل نظام المصادقة والصلاحيات

## 📋 نظرة عامة

تم إضافة نظام مصادقة متكامل باستخدام JWT Tokens مع دعم الأدوار (Roles) والربط التلقائي بالفنادق (Tenants).

---

## ✅ ما تم إنجازه

### 1. الجداول في Master DB
- ✅ `MasterUsers` - جدول المستخدمين
- ✅ `Roles` - جدول الأدوار
- ✅ `UserRoles` - جدول ربط المستخدمين بالأدوار

### 2. Services
- ✅ `IMasterUserService` / `MasterUserService` - إدارة المستخدمين
- ✅ `IJwtService` / `JwtService` - إدارة JWT Tokens

### 3. Middleware
- ✅ `MasterUserResolverMiddleware` - يقرأ JWT Token ويضع TenantId في HttpContext

### 4. Controllers
- ✅ `AuthController` - `/api/auth/login` و `/api/auth/validate`

### 5. Frontend
- ✅ صفحة Login في `/login.html`

---

## 🚀 خطوات الإعداد

### 1. إنشاء الجداول في Master DB

قم بتشغيل SQL Script:
```sql
-- تشغيل الملف
zaaerIntegration/Database/CreateMasterUsersTables.sql
```

هذا سينشئ:
- جدول `Roles` مع 6 أدوار أساسية
- جدول `MasterUsers`
- جدول `UserRoles`

### 2. إضافة المستخدمين

#### الطريقة 1: PowerShell Script
```powershell
cd zaaerIntegration/Scripts
.\Add70Users.ps1
```

#### الطريقة 2: Python Script
```bash
cd zaaerIntegration/Scripts
python Add70Users.py
```

**ملاحظة:** كلمة المرور الافتراضية لجميع المستخدمين: `Password123!`

### 3. إعدادات JWT في appsettings.json

تم إضافة الإعدادات التالية:
```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForSecurity!",
    "Issuer": "ZaaerIntegration",
    "Audience": "ZaaerIntegration",
    "ExpirationMinutes": 1440
  }
}
```

**⚠️ مهم:** قم بتغيير `SecretKey` في بيئة الإنتاج!

---

## 📖 كيفية الاستخدام

### 1. تسجيل الدخول

افتح المتصفح وانتقل إلى:
```
http://localhost:5000/login.html
```

أو أرسل POST request:
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "user1",
  "password": "Password123!"
}
```

**الاستجابة:**
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

### 2. استخدام Token في الطلبات

بعد تسجيل الدخول، احفظ Token في localStorage (يتم تلقائياً في صفحة Login).

أرسل Token في Header:
```http
GET /api/customers
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**✅ المهم:** TenantId يتم تحديده تلقائياً من Token - لا حاجة لإرسال `X-Hotel-Code` header!

---

## 🔄 كيف يعمل النظام

### Flow تسجيل الدخول:

1. المستخدم يرسل `username` و `password` إلى `/api/auth/login`
2. `AuthController` يتحقق من البيانات عبر `MasterUserService`
3. يتم إنشاء JWT Token يحتوي على:
   - `userId`
   - `tenantId` (من المستخدم نفسه) ✅
   - `username`
   - `roles`
4. يتم إرجاع Token للمستخدم

### Flow الطلب العادي:

1. المستخدم يرسل Request مع `Authorization: Bearer {token}`
2. `MasterUserResolverMiddleware` يقرأ Token ويضع `TenantId` في `HttpContext.Items`
3. `TenantMiddleware` يقرأ `TenantId` من `HttpContext.Items` (أو من `X-Hotel-Code` header للتوافق)
4. `TenantService` يحصل على Tenant من Master DB
5. يتم إنشاء `ApplicationDbContext` للفندق المحدد
6. الطلب يتم معالجته

---

## 🎯 المميزات

### ✅ تحديد Tenant تلقائياً
- بعد تسجيل الدخول، TenantId يأتي من المستخدم نفسه
- لا حاجة لاختيار Tenant من dropdown
- كل مستخدم مرتبط بفندق محدد

### ✅ التوافق مع النظام الحالي
- لا يزال يمكن استخدام `X-Hotel-Code` header
- النظام يدعم كلا الطريقتين:
  1. JWT Token (الأولوية)
  2. X-Hotel-Code header (للتوافق)

### ✅ أمان
- كلمات المرور مشفرة بـ BCrypt
- JWT Tokens موقعة ومشفرة
- صلاحيات قائمة على الأدوار

---

## 📝 الأدوار المتاحة

1. **Admin** - Administrator
2. **Manager** - General Manager
3. **Supervisor** - Supervisor
4. **Staff** - Reception Staff
5. **Accountant** - Accountant
6. **ReadOnly** - Read Only

---

## 🔧 API Endpoints

### POST /api/auth/login
تسجيل الدخول والحصول على Token

**Request:**
```json
{
  "username": "user1",
  "password": "Password123!"
}
```

**Response:**
```json
{
  "token": "...",
  "userId": 1,
  "username": "user1",
  "tenantId": 1,
  "tenantCode": "Dammam1",
  "tenantName": "الدمام 1",
  "roles": ["Admin"],
  "expiresAt": "2024-01-02T12:00:00Z"
}
```

### POST /api/auth/validate
التحقق من صحة Token (للاختبار)

**Headers:**
```
Authorization: Bearer {token}
```

**Response:**
```json
{
  "valid": true,
  "userId": "1",
  "tenantId": "1",
  "username": "user1",
  "roles": ["Admin"]
}
```

---

## ⚠️ ملاحظات مهمة

1. **لا تعدل TenantMiddleware أو TenantService** - تم الحفاظ على التوافق الكامل
2. **جميع الجداول في Master DB فقط** - لا توجد تعديلات على قواعد بيانات الفنادق
3. **Middleware الجديد قبل القديم** - `MasterUserResolverMiddleware` قبل `TenantMiddleware`
4. **كلمة المرور الافتراضية** - يجب تغييرها بعد أول تسجيل دخول

---

## 🐛 استكشاف الأخطاء

### خطأ: "Invalid username or password"
- تأكد من صحة اسم المستخدم وكلمة المرور
- تأكد من تشغيل SQL Script لإنشاء الجداول
- تأكد من إضافة المستخدمين

### خطأ: "Missing tenant information"
- تأكد من إرسال Token في Authorization Header
- تأكد من أن Token صالح ولم ينتهِ
- تأكد من أن المستخدم مرتبط بـ Tenant

### خطأ: "Tenant not found"
- تأكد من وجود Tenant في Master DB
- تأكد من أن TenantId في Token صحيح

---

## 📞 الدعم

للمزيد من المعلومات، راجع:
- `zaaerIntegration/Database/CreateMasterUsersTables.sql`
- `zaaerIntegration/Scripts/Add70Users.ps1`
- `zaaerIntegration/wwwroot/login.html`

