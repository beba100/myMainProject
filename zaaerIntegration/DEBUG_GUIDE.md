# 🐛 Debug Guide - Multi-Tenant System

## 📋 لماذا TenantDatabase Settings موجودة؟

### ✅ الفكرة الأساسية:
جميع الفنادق (Tenants) موجودة على **نفس SQL Server Instance** لكن كل فندق له **قاعدة بيانات مختلفة**.

```
Master DB (db29328) → يحتوي على جدول Tenants
├── Dammam1 → DatabaseName: "db30471"
├── Dammam2 → DatabaseName: "db31839"
├── Riyadh1 → DatabaseName: "db31839"
└── Jeddah1 → DatabaseName: "db31839"
```

### 🔑 TenantDatabase Settings:
```json
"TenantDatabase": {
    "Server": "db31839.public.databaseasp.net",    // نفس السيرفر لجميع الفنادق
    "UserId": "db31839",                            // نفس User Id
    "Password": "3Sp#w6?D+P8t"                      // نفس Password
}
```

### 🎯 كيف يعمل النظام:
1. **Master DB (db29328)** → يحتوي على جدول `Tenants` مع `DatabaseName` لكل فندق
2. عند Request مع `X-Hotel-Code: Dammam1`:
   - النظام يبحث في Master DB عن Tenant بـ Code = "Dammam1"
   - يجد `DatabaseName = "db30471"`
   - يبني Connection String: `Server=db31839...; Database=db30471; User Id=db31839; Password=...`
3. كل فندق يستخدم **نفس Server/UserId/Password** لكن **Database مختلف**

---

## 🎯 خطة Debug - Step by Step

### ✅ الخطوة 1: إعداد Visual Studio

1. **افتح المشروع في Visual Studio**
2. **اضبط Launch Settings:**
   - اضغط F5 أو Run
   - تأكد من أن الـ Project يبدأ بشكل صحيح

3. **افتح Output Window:**
   - View → Output
   - اختر "Debug" من القائمة المنسدلة

---

### ✅ الخطوة 2: Breakpoints الرئيسية

#### 🔴 Breakpoint #1: TenantMiddleware.InvokeAsync()
**الموقع:** `zaaerIntegration/Middleware/TenantMiddleware.cs:19`

```csharp
public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
{
    // 🔴 PUT BREAKPOINT HERE - أول نقطة في الـ Pipeline
    var path = context.Request.Path.Value?.ToLower() ?? "";
    
    // Watch Variables:
    // - context.Request.Path
    // - context.Request.Headers["X-Hotel-Code"]
```

**ما تفحصه هنا:**
- ✅ هل Request وصل للمiddleware؟
- ✅ ما هو Path؟
- ✅ هل X-Hotel-Code موجود في Headers؟

---

#### 🔴 Breakpoint #2: TenantService.GetTenant()
**الموقع:** `zaaerIntegration/Services/Implementations/TenantService.cs:43`

```csharp
public Tenant? GetTenant()
{
    // 🔴 PUT BREAKPOINT HERE
    if (_currentTenant != null)
        return _currentTenant;

    var httpContext = _httpContextAccessor.HttpContext;
    // Watch: httpContext
```

**ما تفحصه هنا:**
- ✅ هل HttpContext موجود؟
- ✅ ما قيمة X-Hotel-Code header؟

**Breakpoint #2.1: بعد قراءة hotelCode**
**الموقع:** `TenantService.cs:65`

```csharp
string hotelCode = hotelCodeValues.ToString().Trim();
// 🔴 PUT BREAKPOINT HERE
// Watch: hotelCode
```

**Breakpoint #2.2: قبل البحث في Master DB**
**الموقع:** `TenantService.cs:76`

```csharp
// 🔴 PUT BREAKPOINT HERE
_currentTenant = _masterDbContext.Tenants
    .AsNoTracking()
    .FirstOrDefault(t => t.Code.ToLower() == hotelCode.ToLower());

// Watch Variables:
// - hotelCode
// - _masterDbContext (تأكد من أنه متصل بـ Master DB)
```

**Breakpoint #2.3: بعد العثور على Tenant**
**الموقع:** `TenantService.cs:96`

```csharp
_logger.LogInformation("✅ Tenant resolved successfully: {TenantName} ({TenantCode}), Database: {DatabaseName}", 
    _currentTenant.Name, _currentTenant.Code, _currentTenant.DatabaseName);

// 🔴 PUT BREAKPOINT HERE
// Watch Variables:
// - _currentTenant.Id
// - _currentTenant.Code
// - _currentTenant.Name
// - _currentTenant.DatabaseName  // ⚠️ مهم جداً!
// - _currentTenant.ConnectionString
```

---

#### 🔴 Breakpoint #3: TenantService.GetTenantConnectionString()
**الموقع:** `zaaerIntegration/Services/Implementations/TenantService.cs:161`

```csharp
public string GetTenantConnectionString()
{
    try
    {
        var tenant = GetTenant();
        // 🔴 PUT BREAKPOINT HERE
        // Watch: tenant
```

**Breakpoint #3.1: في BuildConnectionStringForTenant()**
**الموقع:** `TenantService.cs:218`

```csharp
public string BuildConnectionStringForTenant(Tenant tenant)
{
    // 🔴 PUT BREAKPOINT HERE
    // Watch Variables:
    // - tenant.DatabaseName
    // - server (من appsettings.json)
    // - userId
    // - password
```

**Breakpoint #3.2: بعد بناء Connection String**
**الموقع:** `TenantService.cs:253`

```csharp
var connectionString = $"Server={server}; Database={tenant.DatabaseName}; User Id={userId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

// 🔴 PUT BREAKPOINT HERE
// Watch: connectionString
// تأكد من أن:
// - Server = "db31839.public.databaseasp.net"
// - Database = tenant.DatabaseName (مثلاً "db30471")
// - User Id = "db31839"
```

---

#### 🔴 Breakpoint #4: TenantDbContextResolver.GetCurrentDbContext()
**الموقع:** `zaaerIntegration/Data/TenantDbContextResolver.cs:30`

```csharp
public ApplicationDbContext GetCurrentDbContext()
{
    try
    {
        var tenant = _tenantService.GetTenant();
        // 🔴 PUT BREAKPOINT HERE
        // Watch: tenant
```

**Breakpoint #4.1: بعد الحصول على Connection String**
**الموقع:** `TenantDbContextResolver.cs:44`

```csharp
connectionString = _tenantService.GetTenantConnectionString();
// 🔴 PUT BREAKPOINT HERE
// Watch: connectionString
```

**Breakpoint #4.2: بعد إنشاء DbContext**
**الموقع:** `TenantDbContextResolver.cs:68`

```csharp
var dbContext = new ApplicationDbContext(optionsBuilder.Options);
// 🔴 PUT BREAKPOINT HERE
// Watch: dbContext
// Test: dbContext.Database.CanConnect() // اختبار الاتصال
```

---

### ✅ الخطوة 3: اختبار السيناريوهات

#### 🧪 Test Case 1: Request بدون X-Hotel-Code Header
```
GET /api/customers
Headers: (لا يوجد X-Hotel-Code)
```

**Expected Result:**
- ✅ Breakpoint #1 (TenantMiddleware) → يمر
- ✅ Breakpoint #2 (GetTenant) → يرمي UnauthorizedAccessException
- ✅ Response: 401 Unauthorized

---

#### 🧪 Test Case 2: Request مع X-Hotel-Code غير موجود
```
GET /api/customers
Headers: X-Hotel-Code: InvalidHotel
```

**Expected Result:**
- ✅ Breakpoint #2.2 → يبحث في Master DB
- ✅ Breakpoint #2.3 → لا يصل هنا (Tenant = null)
- ✅ Response: 404 Not Found

---

#### 🧪 Test Case 3: Request مع X-Hotel-Code صحيح
```
GET /api/customers
Headers: X-Hotel-Code: Dammam1
```

**Expected Flow:**
1. ✅ Breakpoint #1 → TenantMiddleware يبدأ
2. ✅ Breakpoint #2 → GetTenant() يبدأ
3. ✅ Breakpoint #2.1 → hotelCode = "Dammam1"
4. ✅ Breakpoint #2.2 → يبحث في Master DB
5. ✅ Breakpoint #2.3 → Tenant موجود مع DatabaseName = "db30471"
6. ✅ Breakpoint #3 → GetTenantConnectionString()
7. ✅ Breakpoint #3.1 → BuildConnectionStringForTenant()
8. ✅ Breakpoint #3.2 → Connection String = "Server=db31839...; Database=db30471;..."
9. ✅ Breakpoint #4 → GetCurrentDbContext()
10. ✅ Breakpoint #4.1 → Connection String جاهز
11. ✅ Breakpoint #4.2 → DbContext تم إنشاؤه

---

#### 🧪 Test Case 4: Request مع Tenant بدون DatabaseName
```
(في Master DB: Code = "TestHotel", DatabaseName = NULL)
GET /api/customers
Headers: X-Hotel-Code: TestHotel
```

**Expected Result:**
- ✅ Breakpoint #2.2 → يجد Tenant
- ✅ Breakpoint #2.3 → لا يصل هنا (يرمي InvalidOperationException)
- ✅ Response: 500 Internal Server Error مع رسالة "DatabaseName is not configured"

---

### ✅ الخطوة 4: استخدام Debug Tools

#### 🔍 Watch Window:
أضف هذه المتغيرات في Watch Window:

```
// في TenantService.GetTenant()
hotelCode
_currentTenant
_currentTenant?.DatabaseName
_masterDbContext.Tenants.Count()

// في BuildConnectionStringForTenant()
tenant.DatabaseName
server
userId
password
connectionString

// في TenantDbContextResolver
connectionString
dbContext.Database.Connection.ConnectionString
```

#### 🔍 Immediate Window:
يمكنك اختبار في Immediate Window:

```csharp
// اختبار الاتصال بـ Master DB
_masterDbContext.Tenants.Count()

// اختبار البحث عن Tenant
_masterDbContext.Tenants.FirstOrDefault(t => t.Code == "Dammam1")

// اختبار Connection String
_tenantService.GetTenantConnectionString()

// اختبار الاتصال بقاعدة بيانات Tenant
var dbContext = _tenantService.GetTenant();
dbContext.Database.CanConnect()
```

#### 🔍 Call Stack:
راقب Call Stack لترى:
1. TenantMiddleware.InvokeAsync()
2. TenantService.GetTenant()
3. TenantService.GetTenantConnectionString()
4. TenantService.BuildConnectionStringForTenant()
5. TenantDbContextResolver.GetCurrentDbContext()

---

### ✅ الخطوة 5: اختبار Master DB Connection

#### 🔴 Breakpoint في Program.cs
**الموقع:** `zaaerIntegration/Program.cs:276`

```csharp
var tenantsCount = await masterContext.Tenants.CountAsync();
// 🔴 PUT BREAKPOINT HERE
// Watch: tenantsCount
// Test: masterContext.Tenants.ToList() // عرض جميع Tenants
```

**ما تفحصه هنا:**
- ✅ هل Master DB متصل؟
- ✅ كم عدد Tenants في Master DB؟
- ✅ ما هي قيم DatabaseName لكل Tenant؟

---

## 🎯 Quick Debug Checklist

### ✅ قبل البدء:
- [ ] تأكد من أن Master DB (db29328) متاح
- [ ] تأكد من وجود Tenants في Master DB
- [ ] تأكد من أن كل Tenant له DatabaseName
- [ ] تأكد من إعدادات TenantDatabase في appsettings.json

### ✅ أثناء Debug:
- [ ] Breakpoint #1 في TenantMiddleware
- [ ] Breakpoint #2 في GetTenant()
- [ ] Breakpoint #3 في GetTenantConnectionString()
- [ ] Breakpoint #4 في GetCurrentDbContext()

### ✅ ما تبحث عنه:
- [ ] هل X-Hotel-Code موجود في Headers؟
- [ ] هل Tenant موجود في Master DB؟
- [ ] هل DatabaseName موجود في Tenant؟
- [ ] هل Connection String صحيح؟
- [ ] هل DbContext متصل بقاعدة البيانات الصحيحة？

---

## 🐛 Common Issues & Solutions

### ❌ Issue 1: "Tenant not found"
**الحل:**
- ✅ تحقق من Master DB (db29328)
- ✅ تحقق من Code في جدول Tenants
- ✅ تأكد من Case-insensitive comparison

### ❌ Issue 2: "DatabaseName is not configured"
**الحل:**
- ✅ تحقق من وجود DatabaseName في Master DB
- ✅ تحديث Tenant record في Master DB

### ❌ Issue 3: "TenantDatabase settings are missing"
**الحل:**
- ✅ تحقق من appsettings.json
- ✅ تأكد من وجود TenantDatabase:Server, UserId, Password

### ❌ Issue 4: "Cannot connect to database"
**الحل:**
- ✅ تحقق من Connection String
- ✅ تأكد من أن Database موجود على Server
- ✅ تأكد من User Id و Password صحيحين

---

## 📝 Debug Logs

راقب Logs في:
- **Console Output** (Visual Studio)
- **Output Window** → Debug
- **Log Files** → `logs/log-YYYYMMDD.txt`

ابحث عن:
- ✅ "Tenant resolved successfully"
- ✅ "Built connection string for tenant"
- ✅ "DbContext created successfully"
- ❌ أي Error messages

---

## 🎯 مثال كامل للـ Debug Session

```
1. اضغط F5 في Visual Studio
2. ضع Breakpoints في الأماكن المذكورة أعلاه
3. أرسل Request:
   GET /api/customers
   Headers: X-Hotel-Code: Dammam1

4. سيتم التوقف عند Breakpoint #1 (TenantMiddleware)
   - Watch: context.Request.Headers["X-Hotel-Code"] = "Dammam1"

5. Continue (F5) → Breakpoint #2 (GetTenant)
   - Watch: hotelCode = "Dammam1"

6. Continue (F5) → Breakpoint #2.2 (قبل البحث في Master DB)
   - Watch: _masterDbContext

7. Continue (F5) → Breakpoint #2.3 (بعد العثور على Tenant)
   - Watch: _currentTenant.DatabaseName = "db30471"

8. Continue (F5) → Breakpoint #3.2 (بعد بناء Connection String)
   - Watch: connectionString = "Server=db31839...; Database=db30471;..."

9. Continue (F5) → Breakpoint #4.2 (بعد إنشاء DbContext)
   - Watch: dbContext
   - Test: dbContext.Database.CanConnect() = true

10. Continue (F5) → Request مكتمل بنجاح ✅
```

---

## 🔗 Related Files

- `TenantMiddleware.cs` - أول نقطة في الـ Pipeline
- `TenantService.cs` - منطق Tenant Resolution
- `TenantDbContextResolver.cs` - إنشاء DbContext
- `MasterDbContext.cs` - Master DB Context
- `appsettings.json` - Configuration

---

**Happy Debugging! 🐛✨**

