# 🏨 نظام Multi-Tenant للفنادق
## Zaaer Integration API - Multi-Tenant Architecture

---

## 📖 المحتويات

1. [نظرة عامة](#نظرة-عامة)
2. [الميزات الرئيسية](#الميزات-الرئيسية)
3. [البنية التقنية](#البنية-التقنية)
4. [التثبيت والإعداد](#التثبيت-والإعداد)
5. [الاستخدام](#الاستخدام)
6. [إضافة فندق جديد](#إضافة-فندق-جديد)
7. [أمثلة عملية](#أمثلة-عملية)
8. [الأسئلة الشائعة](#الأسئلة-الشائعة)

---

## 🎯 نظرة عامة

هذا النظام يوفر **Multi-Tenant Architecture احترافي** لإدارة عدة فنادق من خلال API واحد، حيث:

- 🏢 كل فندق له قاعدة بيانات منفصلة تماماً
- 🔐 فصل كامل بين بيانات الفنادق
- 🚀 سهولة التوسع بإضافة فنادق جديدة
- 📊 قاعدة بيانات مركزية لإدارة معلومات الفنادق

---

## ✨ الميزات الرئيسية

| الميزة | الوصف |
|--------|-------|
| **Database-per-Tenant** | كل فندق له قاعدة بيانات خاصة |
| **Dynamic Context** | إنشاء DbContext ديناميكي حسب الفندق |
| **Automatic Resolution** | تحديد الفندق تلقائياً من HTTP Header |
| **Security** | فصل تام بين البيانات |
| **Scalability** | سهولة إضافة فنادق جديدة |
| **Repository Pattern** | استخدام Unit of Work مع Repository Pattern |

---

## 🏗️ البنية التقنية

### هيكل المشروع

```
zaaerIntegration/
│
├── Models/
│   └── Tenant.cs                    # Model للفندق
│
├── Data/
│   ├── MasterDbContext.cs           # Context للقاعدة المركزية
│   ├── ApplicationDbContext.cs      # Context للفنادق
│   └── TenantDbContextResolver.cs   # إنشاء Context ديناميكي
│
├── Services/
│   ├── Interfaces/
│   │   └── ITenantService.cs        # Interface للخدمة
│   └── Implementations/
│       └── TenantService.cs         # تطبيق الخدمة
│
├── Middleware/
│   └── TenantMiddleware.cs          # التحقق من X-Hotel-Code
│
├── Repositories/
│   ├── Interfaces/
│   │   └── IUnitOfWork.cs
│   └── Implementations/
│       └── UnitOfWork.cs
│
└── Database/
    └── CreateTenantsTable.sql       # SQL Script للإعداد
```

### تدفق البيانات (Data Flow)

```
┌─────────────────┐
│  HTTP Request   │
│  + X-Hotel-Code │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ TenantMiddleware│ ◄── يتحقق من وجود Header
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  TenantService  │ ◄── يبحث عن الفندق في Master DB
└────────┬────────┘
         │
         ▼
┌─────────────────────┐
│ TenantDbContext     │ ◄── ينشئ DbContext للفندق
│ Resolver            │
└────────┬────────────┘
         │
         ▼
┌─────────────────┐
│   UnitOfWork    │ ◄── يدير العمليات على قاعدة الفندق
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Tenant Database │ ◄── قاعدة بيانات الفندق المحدد
└─────────────────┘
```

---

## 🚀 التثبيت والإعداد

### المتطلبات

- .NET 8.0 SDK
- SQL Server 2019 أو أحدث
- Visual Studio 2022 أو VS Code

### خطوات التثبيت

#### 1️⃣ إعداد قاعدة البيانات المركزية

قم بتنفيذ SQL Script الموجود في:
```bash
Database/CreateTenantsTable.sql
```

هذا Script سيقوم بـ:
- ✅ إنشاء جدول `Tenants`
- ✅ إضافة فندق Dammam1 الأساسي
- ✅ إنشاء Stored Procedures مساعدة

#### 2️⃣ تحديث Connection Strings

في ملف `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MasterDb": "Server=db29328.public.databaseasp.net; Database=db29328; User Id=db29328; Password=***; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;",
    "DefaultConnection": "..."
  }
}
```

#### 3️⃣ بناء المشروع

```bash
dotnet build
```

#### 4️⃣ تشغيل المشروع

```bash
dotnet run
```

---

## 📚 الاستخدام

### في Swagger UI

1. افتح Swagger:
   ```
   https://localhost:PORT/swagger
   ```

2. اضغط على زر **Authorize** 🔒

3. أدخل كود الفندق:
   ```
   Dammam1
   ```

4. اضغط **Authorize**

5. الآن جميع Requests ستستخدم قاعدة بيانات Dammam1

### في Postman

أضف Header لكل Request:

```
Key: X-Hotel-Code
Value: Dammam1
```

### في الكود

#### C# مع HttpClient

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Hotel-Code", "Dammam1");

var response = await client.GetAsync("https://your-api.com/api/customers");
var content = await response.Content.ReadAsStringAsync();
```

#### JavaScript/Fetch

```javascript
const response = await fetch('https://your-api.com/api/customers', {
    headers: {
        'X-Hotel-Code': 'Dammam1',
        'Content-Type': 'application/json'
    }
});

const data = await response.json();
console.log(data);
```

#### jQuery/Ajax

```javascript
$.ajax({
    url: 'https://your-api.com/api/customers',
    method: 'GET',
    headers: {
        'X-Hotel-Code': 'Dammam1'
    },
    success: function(data) {
        console.log(data);
    }
});
```

---

## ➕ إضافة فندق جديد

### الطريقة 1: باستخدام SQL

```sql
EXEC sp_AddNewTenant 
    @Code = 'Dammam2',
    @Name = N'الدمام 2',
    @ConnectionString = 'Server=YOUR_SERVER; Database=YOUR_DB; User Id=YOUR_USER; Password=YOUR_PASSWORD; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;',
    @BaseUrl = 'https://dammam2.example.com/';
```

### الطريقة 2: باستخدام SQL Insert مباشرة

```sql
INSERT INTO Tenants (Code, Name, ConnectionString, BaseUrl, IsActive)
VALUES (
    'Riyadh1',
    N'الرياض 1',
    'YOUR_CONNECTION_STRING',
    'https://riyadh1.example.com/',
    1
);
```

### الطريقة 3: من الكود (برمجياً)

في `Program.cs` أو في Controller مخصص:

```csharp
var masterContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();

masterContext.Tenants.Add(new Tenant
{
    Code = "Jeddah1",
    Name = "جدة 1",
    ConnectionString = "YOUR_CONNECTION_STRING",
    BaseUrl = "https://jeddah1.example.com/",
    IsActive = true
});

await masterContext.SaveChangesAsync();
```

---

## 💡 أمثلة عملية

### مثال 1: جلب عملاء من فندقين مختلفين

```bash
# Dammam1
curl -H "X-Hotel-Code: Dammam1" https://localhost:5000/api/customers

# Dammam2
curl -H "X-Hotel-Code: Dammam2" https://localhost:5000/api/customers
```

**النتيجة:** ستحصل على بيانات مختلفة لكل فندق! ✅

### مثال 2: إنشاء حجز لفندق محدد

```http
POST /api/reservation HTTP/1.1
Host: localhost:5000
X-Hotel-Code: Dammam1
Content-Type: application/json

{
  "customerId": 1,
  "checkInDate": "2024-12-01",
  "checkOutDate": "2024-12-05",
  "apartmentId": 101
}
```

### مثال 3: استعلام بدون Hotel Code (خطأ)

```bash
curl https://localhost:5000/api/customers
```

**النتيجة:**
```json
{
  "error": "Unauthorized",
  "message": "Missing X-Hotel-Code header",
  "hint": "Please provide 'X-Hotel-Code' header with a valid hotel code"
}
```

---

## 🔒 الأمان

### الفصل بين البيانات

- ✅ كل فندق له قاعدة بيانات منفصلة تماماً
- ✅ لا يمكن لفندق الوصول لبيانات فندق آخر
- ✅ الـ Connection String يُحفظ بشكل آمن في Master DB

### معالجة الأخطاء

| الحالة | الكود | الرسالة |
|--------|------|---------|
| بدون Header | 401 | Missing X-Hotel-Code header |
| كود خاطئ | 404 | Tenant not found |
| خطأ في الاتصال | 500 | Internal Server Error |

---

## ⚡ الأداء

### Caching

- الـ Tenant يتم Cache في نفس الـ Request
- لا يتم الاستعلام عن المعلومات إلا مرة واحدة لكل Request

### Connection Pooling

- كل قاعدة بيانات لها Connection Pool خاص بها
- يتم إدارة الاتصالات بكفاءة عالية

### Scoped Lifetime

- يتم إنشاء DbContext جديد لكل Request
- يتم Dispose تلقائياً بعد انتهاء Request

---

## 🔧 استكشاف الأخطاء

### المشكلة: 401 Unauthorized

**السبب:** لم يتم إرسال `X-Hotel-Code` Header

**الحل:** تأكد من إضافة Header في كل Request

### المشكلة: 404 Tenant Not Found

**السبب:** كود الفندق غير موجود في Master DB

**الحل:** 
1. تحقق من جدول `Tenants` في Master DB
2. أضف الفندق باستخدام SQL Script

### المشكلة: خطأ في الاتصال بقاعدة البيانات

**السبب:** Connection String غير صحيح

**الحل:**
1. تحقق من Connection String في جدول `Tenants`
2. تأكد من صحة Username/Password
3. تأكد من أن قاعدة البيانات متاحة

---

## 📊 الإحصائيات

### الملفات التي تم إنشاؤها/تعديلها

| الملف | الحالة | الوصف |
|------|--------|-------|
| `Models/Tenant.cs` | ✅ جديد | Model للفندق |
| `Data/MasterDbContext.cs` | ✅ جديد | Context للقاعدة المركزية |
| `Data/TenantDbContextResolver.cs` | ✅ جديد | إنشاء Context ديناميكي |
| `Services/ITenantService.cs` | ✅ جديد | Interface للخدمة |
| `Services/TenantService.cs` | ✅ جديد | تطبيق الخدمة |
| `Middleware/TenantMiddleware.cs` | ✅ جديد | Middleware للتحقق |
| `Program.cs` | ✅ محدّث | إضافة Multi-Tenant services |
| `appsettings.json` | ✅ محدّث | إضافة MasterDb |

---

## 🎓 الأسئلة الشائعة

### هل يمكن مشاركة بيانات بين فندقين؟

لا، كل فندق معزول تماماً. إذا كنت تحتاج لمشاركة بيانات، يجب إنشاء API منفصل لذلك.

### هل يمكن تغيير Connection String لفندق موجود؟

نعم، يمكنك تحديث `ConnectionString` في جدول `Tenants` في Master DB.

### كم عدد الفنادق التي يدعمها النظام؟

النظام قابل للتوسع لعدد غير محدود من الفنادق، لكن يعتمد على موارد السيرفر.

### هل يمكن استخدام قاعدة بيانات واحدة لأكثر من فندق؟

نعم تقنياً، لكن غير مُنصح به. الفكرة من Multi-Tenant هي الفصل التام.

---

## 📞 الدعم والمساعدة

إذا واجهت أي مشاكل:

1. 📖 راجع ملف `MULTI_TENANT_GUIDE.md`
2. 🔍 تحقق من Logs في مجلد `logs/`
3. 🧪 استخدم ملف `multi-tenant-test.http` للاختبار
4. 🗂️ راجع SQL Script في `Database/CreateTenantsTable.sql`

---

## 🎉 الخلاصة

النظام جاهز للعمل بشكل احترافي مع:

✅ Multi-Tenant Architecture  
✅ Database-per-Tenant  
✅ Dynamic DbContext  
✅ Repository Pattern + UoW  
✅ Middleware للتحقق  
✅ Swagger Support  
✅ Error Handling  
✅ Logging  

**مبروك! نظام Multi-Tenant جاهز للاستخدام الفوري! 🚀**

---

## 📝 الترخيص

© 2024 Zaaer Integration API - All Rights Reserved

---

**تم بناءه بـ ❤️ باستخدام ASP.NET Core 8.0**

