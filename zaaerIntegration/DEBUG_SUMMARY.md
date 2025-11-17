# 🎯 Debug Summary - Multi-Tenant System

## ❓ لماذا TenantDatabase Settings موجودة؟

### 📊 البنية المعمارية:

```
Master DB (db29328)                    Tenant Databases (Same Server)
┌──────────────────────┐              ┌──────────────────────────────┐
│ Tenants Table        │              │ Server: db31839...           │
│ ┌──────┬──────────┐  │              │ UserId: db31839              │
│ │ Code │DatabaseName│ │              │ Password: 3Sp#w6?D+P8t      │
│ ├──────┼──────────┤  │              │                              │
│ │Dammam1│ db30471 │  │─────────────→│ db30471 (Dammam1)           │
│ │Dammam2│ db31839 │  │─────────────→│ db31839 (Dammam2, Riyadh1)  │
│ │Riyadh1│ db31839 │  │─────────────→│ db31839 (Jeddah1)           │
│ └──────┴──────────┘  │              └──────────────────────────────┘
└──────────────────────┘
```

### 🎯 الفكرة:
- **Master DB** → يحتوي على معلومات جميع الفنادق (Code, DatabaseName, ...)
- **TenantDatabase Settings** → نفس Server/UserId/Password لجميع الفنادق
- **DatabaseName** → مختلف لكل فندق (يُقرأ من Master DB)

### 📝 مثال:
```json
// appsettings.json
"TenantDatabase": {
    "Server": "db31839.public.databaseasp.net",  // نفس السيرفر
    "UserId": "db31839",                          // نفس User
    "Password": "3Sp#w6?D+P8t"                   // نفس Password
}
```

**عند Request مع `X-Hotel-Code: Dammam1`:**
1. النظام يبحث في Master DB → يجد `DatabaseName = "db30471"`
2. يبني Connection String: `Server=db31839...; Database=db30471; User Id=db31839; Password=...`
3. يتصل بقاعدة البيانات `db30471` على نفس السيرفر

---

## 🚀 خطة Debug السريعة

### ✅ Step 1: اضبط Visual Studio
1. افتح المشروع في Visual Studio
2. اضغط **F5** لبدء Debug
3. افتح **Output Window** → View → Output → Debug

---

### ✅ Step 2: ضع Breakpoints التالية

#### 🔴 Breakpoint #1: TenantMiddleware (Line 19)
```csharp
// File: TenantMiddleware.cs
public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
{
    // 🔴 PUT BREAKPOINT HERE
```
**Watch:** `context.Request.Headers["X-Hotel-Code"]`

---

#### 🔴 Breakpoint #2: GetTenant() - قراءة Header (Line 65)
```csharp
// File: TenantService.cs
string hotelCode = hotelCodeValues.ToString().Trim();
// 🔴 PUT BREAKPOINT HERE
```
**Watch:** `hotelCode`

---

#### 🔴 Breakpoint #3: GetTenant() - البحث في Master DB (Line 76)
```csharp
// File: TenantService.cs
_currentTenant = _masterDbContext.Tenants
    .AsNoTracking()
    .FirstOrDefault(t => t.Code.ToLower() == hotelCode.ToLower());
// 🔴 PUT BREAKPOINT HERE
```
**Watch:** 
- `hotelCode`
- `_currentTenant` (بعد السطر)
- `_currentTenant?.DatabaseName` ⚠️ **مهم جداً!**

---

#### 🔴 Breakpoint #4: BuildConnectionStringForTenant() (Line 253)
```csharp
// File: TenantService.cs
var connectionString = $"Server={server}; Database={tenant.DatabaseName}; User Id={userId}; Password={password}; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";
// 🔴 PUT BREAKPOINT HERE
```
**Watch:**
- `tenant.DatabaseName`
- `server`
- `userId`
- `connectionString` ⚠️ **مهم جداً!**

---

#### 🔴 Breakpoint #5: GetCurrentDbContext() (Line 68)
```csharp
// File: TenantDbContextResolver.cs
var dbContext = new ApplicationDbContext(optionsBuilder.Options);
// 🔴 PUT BREAKPOINT HERE
```
**Watch:** `dbContext`
**Test:** `dbContext.Database.CanConnect()` في Immediate Window

---

#### 🔴 Breakpoint #6: Program.cs - Master DB Connection (Line 277)
```csharp
// File: Program.cs
var tenantsCount = await masterContext.Tenants.CountAsync();
// 🔴 PUT BREAKPOINT HERE
```
**Watch:** `tenantsCount`
**Test:** `masterContext.Tenants.ToList()` في Immediate Window

---

## 🧪 Test Scenarios

### Test 1: بدون Header
```
GET /api/customers
Headers: (لا يوجد X-Hotel-Code)
```
**Expected:** 401 Unauthorized

### Test 2: مع Header صحيح
```
GET /api/customers
Headers: X-Hotel-Code: Dammam1
```
**Expected Flow:**
1. Breakpoint #1 → TenantMiddleware
2. Breakpoint #2 → hotelCode = "Dammam1"
3. Breakpoint #3 → Tenant موجود, DatabaseName = "db30471"
4. Breakpoint #4 → Connection String = "Server=db31839...; Database=db30471;..."
5. Breakpoint #5 → DbContext تم إنشاؤه

---

## 🔍 ما تبحث عنه في Debug

### في Breakpoint #3:
```csharp
_currentTenant.Id           // > 0
_currentTenant.Code         // "Dammam1"
_currentTenant.DatabaseName // "db30471" ⚠️ ليس null!
_currentTenant.Name         // "الدمام 1"
```

### في Breakpoint #4:
```csharp
server           // "db31839.public.databaseasp.net"
userId           // "db31839"
password         // "3Sp#w6?D+P8t"
tenant.DatabaseName // "db30471"
connectionString    // "Server=db31839...; Database=db30471; User Id=db31839; Password=...;"
```

### في Breakpoint #5:
```csharp
// Test في Immediate Window:
dbContext.Database.CanConnect()  // true
dbContext.Database.Connection.Database  // "db30471"
```

---

## 🐛 Common Issues & Solutions

### ❌ Issue 1: "Tenant not found"
**الحل:**
- ✅ تحقق من Master DB (db29328)
- ✅ تأكد من وجود Tenant بـ Code = "Dammam1"
- ✅ تحقق من Case-insensitive comparison

### ❌ Issue 2: "DatabaseName is not configured"
**الحل:**
- ✅ افتح Master DB (db29328)
- ✅ تحقق من وجود DatabaseName في جدول Tenants
- ✅ تحديث Tenant record:
  ```sql
  UPDATE Tenants SET DatabaseName = 'db30471' WHERE Code = 'Dammam1';
  ```

### ❌ Issue 3: "TenantDatabase settings are missing"
**الحل:**
- ✅ تحقق من appsettings.json
- ✅ تأكد من وجود TenantDatabase:Server, UserId, Password

### ❌ Issue 4: "Cannot connect to database"
**الحل:**
- ✅ تحقق من Connection String في Breakpoint #4
- ✅ تأكد من أن Database موجود على Server
- ✅ تحقق من User Id و Password

---

## 📝 Debug Checklist

### ✅ قبل البدء:
- [ ] Master DB (db29328) متاح ومتصل
- [ ] Tenants موجودة في Master DB
- [ ] كل Tenant له DatabaseName
- [ ] TenantDatabase settings موجودة في appsettings.json

### ✅ أثناء Debug:
- [ ] Breakpoint #1 في TenantMiddleware
- [ ] Breakpoint #2 في GetTenant() - قراءة Header
- [ ] Breakpoint #3 في GetTenant() - البحث في Master DB
- [ ] Breakpoint #4 في BuildConnectionStringForTenant()
- [ ] Breakpoint #5 في GetCurrentDbContext()
- [ ] Breakpoint #6 في Program.cs - Master DB Connection

### ✅ ما تبحث عنه:
- [ ] X-Hotel-Code موجود في Headers
- [ ] Tenant موجود في Master DB
- [ ] DatabaseName موجود في Tenant
- [ ] Connection String صحيح
- [ ] DbContext متصل بقاعدة البيانات الصحيحة

---

## 🎯 Quick Test Script

### استخدام Postman أو Swagger:

1. **افتح Swagger:** `https://localhost:5001/swagger`
2. **أضف Header:**
   ```
   X-Hotel-Code: Dammam1
   ```
3. **أرسل Request:**
   ```
   GET /api/customers
   ```
4. **راقب Debug:**
   - Breakpoint #1 → #2 → #3 → #4 → #5

---

## 📊 Expected Values

### عند Request مع `X-Hotel-Code: Dammam1`:

```
Breakpoint #2:
  hotelCode = "Dammam1"

Breakpoint #3:
  _currentTenant.Id = 1
  _currentTenant.Code = "Dammam1"
  _currentTenant.DatabaseName = "db30471" ⚠️
  _currentTenant.Name = "الدمام 1"

Breakpoint #4:
  server = "db31839.public.databaseasp.net"
  userId = "db31839"
  password = "3Sp#w6?D+P8t"
  tenant.DatabaseName = "db30471"
  connectionString = "Server=db31839.public.databaseasp.net; Database=db30471; User Id=db31839; Password=3Sp#w6?D+P8t; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;"

Breakpoint #5:
  dbContext.Database.CanConnect() = true
  dbContext.Database.Connection.Database = "db30471"
```

---

**Happy Debugging! 🐛✨**

