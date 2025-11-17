# 🏨 Multi-Tenant System Guide
## دليل استخدام النظام متعدد الفنادق

---

## 📋 نظرة عامة (Overview)

هذا النظام يدعم **Multi-Tenant Architecture** بحيث يمكن لعدة فنادق استخدام نفس الـ API مع قواعد بيانات منفصلة لكل فندق.

### ✨ المميزات

✅ قاعدة بيانات مركزية (Master DB) تحتوي على معلومات كل الفنادق  
✅ قاعدة بيانات منفصلة لكل فندق (Database-per-Tenant)  
✅ تحديد الفندق تلقائياً من خلال HTTP Header: `X-Hotel-Code`  
✅ فصل تام بين بيانات الفنادق المختلفة  
✅ سهولة إضافة فنادق جديدة  

---

## 🗄️ هيكل قواعد البيانات

### 1. Master Database (db29328)
قاعدة البيانات المركزية التي تحتوي على:

| Column Name       | Type          | Description                    |
|-------------------|---------------|--------------------------------|
| Id                | int           | معرف الفندق                    |
| Code              | string(50)    | كود الفندق (Dammam1, Dammam2) |
| Name              | string(200)   | اسم الفندق                     |
| ConnectionString  | string(500)   | Connection String للفندق      |
| BaseUrl           | string(200)   | URL الفندق (اختياري)           |

### 2. Tenant Databases
كل فندق له قاعدة بيانات خاصة تحتوي على:
- Customers (العملاء)
- Reservations (الحجوزات)
- Invoices (الفواتير)
- Apartments (الشقق)
- وجميع الجداول الأخرى الخاصة بالفندق

---

## 🚀 كيفية الاستخدام

### 1️⃣ في Swagger UI

1. افتح Swagger: `https://localhost:YOUR_PORT/swagger`
2. اضغط على زر **Authorize** (🔒)
3. أدخل كود الفندق في حقل `X-Hotel-Code`:
   ```
   Dammam1
   ```
4. اضغط **Authorize**
5. الآن جميع Requests ستستخدم قاعدة بيانات فندق Dammam1

### 2️⃣ في Postman

أضف Header لكل Request:

```
Key: X-Hotel-Code
Value: Dammam1
```

### 3️⃣ في C# HttpClient

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Hotel-Code", "Dammam1");

var response = await client.GetAsync("https://your-api.com/api/customers");
```

### 4️⃣ في JavaScript/Fetch

```javascript
fetch('https://your-api.com/api/customers', {
    headers: {
        'X-Hotel-Code': 'Dammam1'
    }
})
```

---

## 🔧 إضافة فندق جديد

### الطريقة 1: من خلال قاعدة البيانات المركزية

قم بإضافة سطر جديد في جدول `Tenants` في قاعدة البيانات المركزية:

```sql
INSERT INTO Tenants (Code, Name, ConnectionString, BaseUrl)
VALUES (
    'Dammam2',
    'الدمام 2',
    'Server=YOUR_SERVER; Database=YOUR_DB; User Id=YOUR_USER; Password=YOUR_PASSWORD; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;',
    'https://dammam2.example.com/'
)
```

### الطريقة 2: برمجياً من الكود

في `Program.cs`، أضف فندق جديد:

```csharp
masterContext.Tenants.Add(
    new Tenant
    {
        Code = "Dammam2",
        Name = "الدمام 2",
        ConnectionString = "YOUR_CONNECTION_STRING",
        BaseUrl = "https://dammam2.example.com/"
    }
);
await masterContext.SaveChangesAsync();
```

---

## 🏗️ البنية التقنية (Architecture)

### Flow Diagram

```
1. HTTP Request يصل للـ API
                ↓
2. TenantMiddleware يتحقق من وجود X-Hotel-Code
                ↓
3. TenantService يبحث عن الفندق في Master DB
                ↓
4. TenantDbContextResolver ينشئ DbContext للفندق
                ↓
5. Controller يستخدم UnitOfWork للوصول للبيانات
                ↓
6. البيانات تُقرأ من قاعدة بيانات الفندق المحدد
```

### الملفات الرئيسية

| File                                      | Purpose                                   |
|-------------------------------------------|-------------------------------------------|
| `Models/Tenant.cs`                        | Model للفندق                              |
| `Data/MasterDbContext.cs`                 | Context لقاعدة البيانات المركزية          |
| `Services/ITenantService.cs`              | Interface لخدمة الفندق                    |
| `Services/TenantService.cs`               | تطبيق خدمة الحصول على معلومات الفندق      |
| `Data/TenantDbContextResolver.cs`         | إنشاء DbContext ديناميكي حسب الفندق      |
| `Middleware/TenantMiddleware.cs`          | التحقق من X-Hotel-Code في كل Request     |

---

## ⚠️ ملاحظات هامة

### 1. أمان البيانات
- كل فندق مفصول تماماً عن الآخر
- لا يمكن لفندق الوصول لبيانات فندق آخر
- الـ Connection String يُحفظ في Master DB فقط

### 2. الأداء
- يتم Cache الـ Tenant في نفس الـ Request لتجنب الاستعلامات المتكررة
- كل Request يُنشئ DbContext جديد (Scoped lifetime)

### 3. معالجة الأخطاء
- إذا لم يتم إرسال `X-Hotel-Code` → **401 Unauthorized**
- إذا كان الكود غير موجود → **404 Not Found**

---

## 📊 مثال عملي (Full Example)

### Request
```http
GET /api/customers HTTP/1.1
Host: localhost:5000
X-Hotel-Code: Dammam1
```

### Response
```json
[
  {
    "id": 1,
    "name": "أحمد محمد",
    "phoneNumber": "0501234567",
    ...
  }
]
```

---

## 🧪 الاختبار (Testing)

### اختبار بفندق مختلف

```bash
# Dammam1
curl -H "X-Hotel-Code: Dammam1" https://localhost:5000/api/customers

# Dammam2
curl -H "X-Hotel-Code: Dammam2" https://localhost:5000/api/customers
```

ستحصل على نتائج مختلفة لكل فندق! ✅

---

## 📞 الدعم

إذا واجهت أي مشاكل:
1. تأكد من إضافة الفندق في Master DB
2. تأكد من صحة Connection String
3. تحقق من Logs في مجلد `logs/`

---

## 🎯 الخلاصة

✅ النظام جاهز للعمل مع عدة فنادق  
✅ يكفي إرسال `X-Hotel-Code` في Header  
✅ كل فندق له بياناته المستقلة تماماً  
✅ سهولة إضافة فنادق جديدة بدون تعديل الكود  

**🎉 مبروك! النظام Multi-Tenant جاهز للاستخدام!**

