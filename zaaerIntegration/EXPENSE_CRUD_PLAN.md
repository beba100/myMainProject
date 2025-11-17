# 📋 خطة العمل - Expense CRUD Operations مع X-Hotel-Code

## 🎯 المتطلبات:

1. ✅ حقل `purpose` موجود بالفعل في جدول `expenses`
2. ✅ إضافة ربط Expense بـ Room (Apartment) - إضافة `apartment_id` في `expenses`
3. ✅ إنشاء CRUD Operations جديدة تستخدم `X-Hotel-Code` header
4. ✅ لا نلمس Tenant/Middleware/Service/Program.cs logic

---

## 📊 البنية الحالية:

### ✅ ما هو موجود:
- ✅ `Expense` model موجود (يحتوي على `purpose`)
- ✅ `ZaaerExpenseController` موجود لكنه يستخدم `HotelId` من DTO
- ✅ `ZaaerExpenseService` موجود لكنه لا يستخدم `ITenantService`
- ✅ `Apartment` model موجود (يمثل Room)

### ❌ ما نريد إضافته:
- ❌ حقل `ApartmentId` في `Expense` model
- ❌ CRUD Controller جديد يستخدم `X-Hotel-Code` header
- ❌ CRUD Service جديد يستخدم `ITenantService`
- ❌ GetById و Delete operations

---

## 🏗️ خطة العمل التفصيلية:

### ✅ Step 1: تحديث Expense Model
**الملف:** `zaaerIntegration/Models/Expense.cs`
- ✅ إضافة `ApartmentId` property (nullable int)
- ✅ إضافة Navigation Property لـ `Apartment`

---

### ✅ Step 2: تحديث Database Schema
**ملف SQL جديد:** `zaaerIntegration/Database/AddApartmentIdToExpenses.sql`
- ✅ إضافة حقل `apartment_id` في جدول `expenses`
- ✅ إضافة Foreign Key لـ `apartments` table

---

### ✅ Step 3: تحديث ApplicationDbContext
**الملف:** `zaaerIntegration/Data/ApplicationDbContext.cs`
- ✅ إضافة Relationship بين `Expense` و `Apartment` في `OnModelCreating`

---

### ✅ Step 4: إنشاء DTOs جديدة
**ملفات جديدة:**
- `zaaerIntegration/DTOs/Expense/CreateExpenseDto.cs` - بدون HotelId (سيُقرأ من header)
- `zaaerIntegration/DTOs/Expense/UpdateExpenseDto.cs` - بدون HotelId
- `zaaerIntegration/DTOs/Expense/ExpenseResponseDto.cs` - مع ApartmentId و ApartmentName

---

### ✅ Step 5: إنشاء Expense Service جديد
**ملف جديد:** `zaaerIntegration/Services/Expense/ExpenseService.cs`
- ✅ يستخدم `ITenantService` للحصول على HotelId من `X-Hotel-Code`
- ✅ CRUD Operations:
  - `GetAllAsync()` - جميع expenses للفندق الحالي
  - `GetByIdAsync(int id)` - expense محدد
  - `CreateAsync(CreateExpenseDto dto)` - إنشاء expense جديد
  - `UpdateAsync(int id, UpdateExpenseDto dto)` - تحديث expense
  - `DeleteAsync(int id)` - حذف expense

---

### ✅ Step 6: إنشاء Expense Controller جديد
**ملف جديد:** `zaaerIntegration/Controllers/ExpenseController.cs`
- ✅ Route: `/api/expenses`
- ✅ جميع Endpoints تستخدم `X-Hotel-Code` header (لا تحتاج HotelId في DTO)
- ✅ CRUD Operations:
  - `GET /api/expenses` - Get all expenses
  - `GET /api/expenses/{id}` - Get expense by id
  - `POST /api/expenses` - Create expense
  - `PUT /api/expenses/{id}` - Update expense
  - `DELETE /api/expenses/{id}` - Delete expense

---

### ✅ Step 7: تحديث DTOs الحالية (Zaaer)
**ملفات موجودة:**
- `zaaerIntegration/DTOs/Zaaer/ZaaerCreateExpenseDto.cs` - إضافة `ApartmentId?`
- `zaaerIntegration/DTOs/Zaaer/ZaaerUpdateExpenseDto.cs` - إضافة `ApartmentId?`
- `zaaerIntegration/DTOs/Zaaer/ZaaerExpenseResponseDto.cs` - إضافة `ApartmentId?` و `ApartmentCode`

---

### ✅ Step 8: تحديث ZaaerExpenseService
**الملف:** `zaaerIntegration/Services/Zaaer/ZaaerExpenseService.cs`
- ✅ إضافة `ApartmentId` في Create و Update operations

---

## 📝 ملاحظات مهمة:

1. ✅ **لا نلمس:**
   - `TenantMiddleware.cs`
   - `TenantService.cs`
   - `TenantDbContextResolver.cs`
   - `Program.cs`
   - أي شيء متعلق بـ Multi-Tenant logic

2. ✅ **X-Hotel-Code Header:**
   - سيتم قراءته تلقائياً من `TenantMiddleware`
   - `ITenantService.GetTenant()` سيُرجع Tenant الحالي
   - سنستخدم `HotelSettings.HotelId` للحصول على HotelId

3. ✅ **Expense-Room Relationship:**
   - `ApartmentId` في `Expense` سيكون nullable (اختياري)
   - المستخدم قد يختار Room من dropdown أو يتركه فارغ
   - `Purpose` field موجود بالفعل ويمكن استخدامه لكتابة ملاحظات

4. ✅ **Database Schema:**
   - نحتاج إلى SQL Migration لإضافة `apartment_id` column
   - نحتاج إلى Foreign Key constraint

---

## 🎯 API Endpoints الجديدة:

```
GET    /api/expenses                    → Get all expenses for current hotel
GET    /api/expenses/{id}               → Get expense by id
POST   /api/expenses                    → Create new expense
PUT    /api/expenses/{id}               → Update expense
DELETE /api/expenses/{id}               → Delete expense
```

**Headers Required:**
```
X-Hotel-Code: Dammam1
Content-Type: application/json
```

---

## 📊 Database Schema Changes:

```sql
-- إضافة apartment_id في جدول expenses
ALTER TABLE expenses
ADD apartment_id INT NULL;

-- إضافة Foreign Key
ALTER TABLE expenses
ADD CONSTRAINT FK_Expenses_Apartments 
FOREIGN KEY (apartment_id) REFERENCES apartments(apartment_id);
```

---

## ✅ Checklist:

- [ ] Step 1: تحديث Expense Model
- [ ] Step 2: تحديث Database Schema (SQL)
- [ ] Step 3: تحديث ApplicationDbContext
- [ ] Step 4: إنشاء DTOs جديدة
- [ ] Step 5: إنشاء Expense Service جديد
- [ ] Step 6: إنشاء Expense Controller جديد
- [ ] Step 7: تحديث Zaaer DTOs
- [ ] Step 8: تحديث ZaaerExpenseService
- [ ] Step 9: اختبار جميع CRUD Operations

---

## 🚀 البدء بالتنفيذ:

هل تريدني أن أبدأ بالتنفيذ الآن؟

