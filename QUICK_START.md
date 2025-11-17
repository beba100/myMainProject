# 🚀 Quick Start Guide
## دليل البدء السريع - Multi-Tenant System

---

## ⏱️ 5 دقائق للتشغيل!

اتبع هذه الخطوات البسيطة لتشغيل النظام:

---

## 📋 المتطلبات

✅ .NET 8.0 SDK  
✅ SQL Server  
✅ Visual Studio 2022 أو VS Code  

---

## 🔧 خطوات التشغيل

### الخطوة 1: تشغيل SQL Script (دقيقتان) ⏱️

1. افتح SQL Server Management Studio
2. اتصل بـ Master Database:
   ```
   Server: db29328.public.databaseasp.net
   Database: db29328
   User: db29328
   Password: S@q9+o5QA-s7
   ```
3. افتح الملف:
   ```
   zaaerIntegration/Database/CreateTenantsTable.sql
   ```
4. اضغط F5 لتنفيذ Script
5. تأكد من رسالة: **✅ تم إضافة فندق الدمام 1**

---

### الخطوة 2: تشغيل المشروع (دقيقة واحدة) ⏱️

#### باستخدام Visual Studio:
```
1. افتح zaaerIntegration.csproj
2. اضغط F5
```

#### باستخدام Command Line:
```bash
cd "c:\BEBA_HOTEL\My API Project\‏‏zaaerIntegration - نسخة\zaaerIntegration"
dotnet run
```

---

### الخطوة 3: فتح Swagger (30 ثانية) ⏱️

1. سيفتح المتصفح تلقائياً على:
   ```
   https://localhost:7062/swagger
   ```

2. اضغط على زر **Authorize** 🔒

3. أدخل في حقل `Value`:
   ```
   Dammam1
   ```

4. اضغط **Authorize** ثم **Close**

---

### الخطوة 4: اختبار API (30 ثانية) ⏱️

1. اذهب إلى `GET /api/Customer`
2. اضغط **Try it out**
3. اضغط **Execute**
4. شاهد النتيجة! 🎉

إذا ظهرت لك بيانات العملاء → **النظام يعمل بنجاح!** ✅

---

## 🧪 طرق اختبار إضافية

### الطريقة 1: صفحة Demo التفاعلية

افتح في المتصفح:
```
https://localhost:7062/multi-tenant-demo.html
```

ستجد واجهة جميلة للاختبار! 🎨

### الطريقة 2: ملف HTTP

في VS Code، افتح:
```
multi-tenant-test.http
```

اضغط على **Send Request** بجانب أي طلب.

### الطريقة 3: Postman

1. أنشئ Request جديد
2. Method: `GET`
3. URL: `https://localhost:7062/api/Customer`
4. Headers:
   ```
   X-Hotel-Code: Dammam1
   ```
5. اضغط **Send**

---

## ✅ التحقق من النجاح

### علامات النجاح:

#### في Console:
```
✅ Master Database initialized with Tenants successfully
info: zaaerIntegration.Services.Implementations.TenantService[0]
      Tenant resolved successfully: الدمام 1 (Dammam1)
```

#### في Swagger:
```json
[
  {
    "id": 1,
    "name": "اسم العميل",
    "phoneNumber": "05xxxxxxxx",
    ...
  }
]
```

#### في Demo Page:
```
✅ نجح - 200
عدد النتائج: X
```

---

## ❌ استكشاف المشاكل الشائعة

### المشكلة 1: خطأ في الاتصال بـ Master DB

**الأعراض:**
```
Failed to initialize Master Database
```

**الحل:**
- تحقق من Connection String في `appsettings.json`
- تأكد من صحة Username/Password
- تحقق من أن السيرفر متاح

---

### المشكلة 2: 401 Unauthorized

**الأعراض:**
```json
{
  "error": "Unauthorized",
  "message": "Missing X-Hotel-Code header"
}
```

**الحل:**
- تأكد من إضافة Header: `X-Hotel-Code`
- في Swagger: اضغط Authorize أولاً
- تأكد من القيمة: `Dammam1`

---

### المشكلة 3: 404 Tenant Not Found

**الأعراض:**
```json
{
  "error": "Not Found",
  "message": "Tenant not found for code: XXX"
}
```

**الحل:**
- تحقق من وجود الفندق في جدول `Tenants`
- تأكد من صحة كود الفندق (Case-sensitive)
- شغّل SQL Script مرة أخرى

---

### المشكلة 4: خطأ في قاعدة بيانات الفندق

**الأعراض:**
```
Cannot open database "db30471"
```

**الحل:**
- تحقق من Connection String للفندق في جدول `Tenants`
- تأكد من وجود قاعدة البيانات
- تأكد من صلاحيات المستخدم

---

## 📊 سيناريوهات الاختبار

### سيناريو 1: اختبار فندق واحد ✅
```
1. Header: X-Hotel-Code = Dammam1
2. GET /api/Customer
3. النتيجة المتوقعة: قائمة العملاء من db30471
```

### سيناريو 2: اختبار بدون Header ❌
```
1. بدون Header
2. GET /api/Customer
3. النتيجة المتوقعة: 401 Unauthorized
```

### سيناريو 3: اختبار بكود خاطئ ❌
```
1. Header: X-Hotel-Code = InvalidCode
2. GET /api/Customer
3. النتيجة المتوقعة: 404 Not Found
```

### سيناريو 4: اختبار Endpoints مختلفة ✅
```
1. Header: X-Hotel-Code = Dammam1
2. GET /api/Reservation
3. GET /api/Apartment
4. GET /api/Invoice
5. النتيجة المتوقعة: بيانات من db30471 لكل Endpoint
```

---

## 🎯 الخطوات التالية

بعد التأكد من أن كل شيء يعمل:

### 1️⃣ إضافة فندق جديد
```sql
EXEC sp_AddNewTenant 
    @Code = 'Dammam2',
    @Name = N'الدمام 2',
    @ConnectionString = 'YOUR_CONNECTION_STRING',
    @BaseUrl = 'https://dammam2.example.com/';
```

### 2️⃣ اختبار الفندق الجديد
```
Header: X-Hotel-Code = Dammam2
GET /api/Customer
```

### 3️⃣ دمج مع Frontend
```javascript
fetch('https://your-api.com/api/customers', {
    headers: {
        'X-Hotel-Code': 'Dammam1'
    }
})
```

---

## 📚 الموارد الإضافية

### التوثيق الكامل
```
📄 README_MULTI_TENANT.md     - توثيق شامل
📄 MULTI_TENANT_GUIDE.md      - دليل الاستخدام
📄 IMPLEMENTATION_SUMMARY.md  - ملخص التطبيق
```

### أدوات الاختبار
```
🧪 multi-tenant-test.http      - ملف HTTP
🎨 multi-tenant-demo.html      - صفحة تفاعلية
```

### Database
```
🗄️ Database/CreateTenantsTable.sql - SQL Script
```

---

## 🎊 مبروك!

إذا وصلت إلى هنا وكل شيء يعمل:

**✅ نظام Multi-Tenant جاهز للاستخدام!**

---

## 📞 تحتاج مساعدة؟

### راجع:
1. ملفات التوثيق في المشروع
2. Logs في مجلد `logs/`
3. Console output عند التشغيل

### تحقق من:
- [ ] Master DB متصل
- [ ] جدول Tenants موجود
- [ ] Dammam1 مضاف في Tenants
- [ ] Connection String صحيح
- [ ] X-Hotel-Code في Headers

---

## 🕐 الوقت الفعلي

- ✅ SQL Script: 2 دقيقة
- ✅ تشغيل المشروع: 1 دقيقة
- ✅ اختبار في Swagger: 1 دقيقة
- ✅ **إجمالي: 4 دقائق فقط!** ⚡

---

## 💡 نصيحة أخيرة

**استخدم Demo Page للاختبار السريع:**
```
https://localhost:7062/multi-tenant-demo.html
```

**أسهل طريقة للتحقق من أن كل شيء يعمل!** 🚀

---

**Happy Coding! 💻✨**

