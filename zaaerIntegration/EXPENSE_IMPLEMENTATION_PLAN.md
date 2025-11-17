# 📋 خطة التنفيذ النهائية - Expense CRUD Operations

## 🎯 المتطلبات:

1. ✅ إضافة `purpose` field في `expense_rooms` table (ليس expenses)
2. ✅ إنشاء Models لـ `expense_rooms` و `expense_categories`
3. ✅ إنشاء CRUD Operations جديدة تستخدم `X-Hotel-Code` header
4. ✅ الحصول على HotelId من HotelSettings في Tenant DB
5. ✅ لا نلمس Tenant/Middleware/Service/Program.cs logic

---

## 📊 البنية الحالية (من الصور):

### ✅ جداول موجودة في Database:

#### 1. `expenses` table:
- expense_id (PK)
- date_time
- hotel_id
- comment
- created_at
- updated_at
- expense_category_id (FK)
- tax_rate
- tax_amount
- total_amount
- **purpose** (موجود بالفعل في الكود)

#### 2. `expense_rooms` table:
- expense_room_id (PK)
- expense_id (FK)
- apartment_id (FK)
- created_at
- **❌ purpose** (نحتاج إضافته!)

#### 3. `expense_categories` table:
- expense_category_id (PK)
- hotel_id
- category_name
- description
- is_active
- created_at
- updated_at

---

## 🏗️ خطة العمل التفصيلية:

### ✅ Step 1: إنشاء Models جديدة

**ملف جديد:** `zaaerIntegration/Models/ExpenseRoom.cs`
- ✅ يمثل `expense_rooms` table
- ✅ إضافة `purpose` property

**ملف جديد:** `zaaerIntegration/Models/ExpenseCategory.cs`
- ✅ يمثل `expense_categories` table

**تحديث:** `zaaerIntegration/Models/Expense.cs`
- ✅ إضافة Navigation Property لـ `ExpenseRooms` (List)
- ✅ إضافة Navigation Property لـ `ExpenseCategory`
- ✅ إضافة Navigation Property لـ `Apartment` (optional)

---

### ✅ Step 2: تحديث Database Schema

**ملف SQL جديد:** `zaaerIntegration/Database/AddPurposeToExpenseRooms.sql`
```sql
-- إضافة purpose field في expense_rooms
ALTER TABLE expense_rooms
ADD purpose NVARCHAR(500) NULL;
```

---

### ✅ Step 3: تحديث ApplicationDbContext

**الملف:** `zaaerIntegration/Data/ApplicationDbContext.cs`
- ✅ إضافة DbSet<ExpenseRoom>
- ✅ إضافة DbSet<ExpenseCategory>
- ✅ إضافة Relationships في OnModelCreating

---

### ✅ Step 4: إنشاء DTOs جديدة

**ملفات جديدة:**
- `zaaerIntegration/DTOs/Expense/CreateExpenseDto.cs` - بدون HotelId
- `zaaerIntegration/DTOs/Expense/UpdateExpenseDto.cs`
- `zaaerIntegration/DTOs/Expense/ExpenseResponseDto.cs`
- `zaaerIntegration/DTOs/Expense/CreateExpenseRoomDto.cs` - مع purpose
- `zaaerIntegration/DTOs/Expense/UpdateExpenseRoomDto.cs`
- `zaaerIntegration/DTOs/Expense/ExpenseRoomResponseDto.cs`

---

### ✅ Step 5: إنشاء Expense Service جديد

**ملف جديد:** `zaaerIntegration/Services/Expense/IExpenseService.cs`
**ملف جديد:** `zaaerIntegration/Services/Expense/ExpenseService.cs`

**Methods:**
- `GetAllAsync()` - جميع expenses للفندق الحالي
- `GetByIdAsync(int id)` - expense محدد مع expense_rooms
- `CreateAsync(CreateExpenseDto dto)` - إنشاء expense مع expense_rooms
- `UpdateAsync(int id, UpdateExpenseDto dto)` - تحديث expense
- `DeleteAsync(int id)` - حذف expense
- `GetExpenseRoomsAsync(int expenseId)` - Get expense_rooms for expense
- `AddExpenseRoomAsync(int expenseId, CreateExpenseRoomDto dto)` - Add room to expense
- `UpdateExpenseRoomAsync(int expenseRoomId, UpdateExpenseRoomDto dto)` - Update expense_room
- `DeleteExpenseRoomAsync(int expenseRoomId)` - Delete expense_room

**Logic:**
- ✅ يستخدم `ITenantService.GetTenant()` للحصول على Tenant
- ✅ يحصل على HotelId من `HotelSettings` في Tenant DB (بحث بـ HotelCode)
- ✅ جميع العمليات مرتبطة بـ HotelId من Tenant

---

### ✅ Step 6: إنشاء Expense Controller جديد

**ملف جديد:** `zaaerIntegration/Controllers/ExpenseController.cs`

**Endpoints:**
```
GET    /api/expenses                    → Get all expenses
GET    /api/expenses/{id}               → Get expense by id
POST   /api/expenses                    → Create expense (with rooms)
PUT    /api/expenses/{id}               → Update expense
DELETE /api/expenses/{id}               → Delete expense

GET    /api/expenses/{id}/rooms         → Get expense rooms
POST   /api/expenses/{id}/rooms         → Add room to expense
PUT    /api/expenses/{id}/rooms/{roomId} → Update expense room
DELETE /api/expenses/{id}/rooms/{roomId} → Delete expense room
```

**Headers Required:**
```
X-Hotel-Code: Dammam1
Content-Type: application/json
```

---

### ✅ Step 7: تحديث Zaaer DTOs (اختياري)

**ملفات موجودة:**
- إضافة `ExpenseRooms` في `ZaaerExpenseResponseDto` إذا لزم الأمر

---

## 🔑 كيفية الحصول على HotelId:

```csharp
// في ExpenseService
var tenant = _tenantService.GetTenant(); // يحصل على Tenant من X-Hotel-Code header
var hotelCode = tenant.Code; // مثلاً "Dammam1"

// البحث عن HotelSettings في Tenant DB
var hotelSettings = await _context.HotelSettings
    .FirstOrDefaultAsync(h => h.HotelCode == hotelCode);

if (hotelSettings == null)
{
    throw new InvalidOperationException($"HotelSettings not found for code: {hotelCode}");
}

var hotelId = hotelSettings.HotelId; // هذا هو HotelId المطلوب
```

---

## 📊 Database Schema Updates:

### SQL Script 1: إضافة purpose في expense_rooms
```sql
-- إضافة purpose field في expense_rooms
IF COL_LENGTH('dbo.expense_rooms', 'purpose') IS NULL
BEGIN
    ALTER TABLE dbo.expense_rooms
    ADD purpose NVARCHAR(500) NULL;
    PRINT '✅ Added purpose column to expense_rooms table';
END
ELSE
BEGIN
    PRINT '⚠️ purpose column already exists in expense_rooms table';
END
```

---

## ✅ Checklist:

- [ ] Step 1: إنشاء ExpenseRoom و ExpenseCategory Models
- [ ] Step 2: تحديث Expense Model (Navigation Properties)
- [ ] Step 3: تحديث Database Schema (SQL)
- [ ] Step 4: تحديث ApplicationDbContext
- [ ] Step 5: إنشاء DTOs جديدة
- [ ] Step 6: إنشاء Expense Service جديد
- [ ] Step 7: إنشاء Expense Controller جديد
- [ ] Step 8: Register Service في Program.cs
- [ ] Step 9: اختبار جميع CRUD Operations

---

## 🚀 البدء بالتنفيذ:

جاهز للبدء! 🎯

