# ⚡ Quick Fix - إصلاح جميع الـ Endpoints في دقائق!
## حل سريع باستخدام Find & Replace

---

## 🎯 **الهدف:**

إصلاح **~36 دالة** في `index.html` لإضافة `X-Hotel-Code` header.

---

## ⚡ **الطريقة الأسرع - Find & Replace في VS Code:**

### **الخطوة 1: افتح Find & Replace**
```
Ctrl + H (Windows/Linux)
Cmd + H (Mac)
```

---

### **التعديل #1: POST/PUT Requests**

**Find:**
```
headers: {
                        'Content-Type': 'application/json',
                    },
```

**Replace:**
```
headers: getApiHeaders(),
```

**ملاحظة:** انسخ **بالضبط** مع المسافات!

**عدد التعديلات المتوقعة:** ~15-20

---

###  **التعديل #2: GET Requests (النمط 1)**

**Find:**
```
const response = await fetch(`${API_BASE_URL}/api/zaaer/Reservation/hotel/${hotelId}`);
```

**Replace:**
```
const response = await fetch(`${API_BASE_URL}/api/zaaer/Reservation/hotel/${hotelId}`, {
                    headers: { 'X-Hotel-Code': getCurrentHotelCode() }
                });
```

**كرر لكل endpoint:**
- `/api/zaaer/Reservation/hotel/`
- `/api/zaaer/PaymentReceipt/hotel/`
- `/api/zaaer/Invoice/hotel/`
- `/api/zaaer/Refund/hotel/`
- `/api/zaaer/CreditNote/hotel/`
- `/api/zaaer/RoomType/hotel/`
- `/api/zaaer/Floor/hotel/`
- `/api/zaaer/Apartment/hotel/`

---

### **التعديل #3: GET by ID (النمط 2)**

**Find:**
```
const response = await fetch(`${API_BASE_URL}/api/zaaer/Invoice/${invoiceId}`);
```

**Replace:**
```
const response = await fetch(`${API_BASE_URL}/api/zaaer/Invoice/${invoiceId}`, {
                    headers: { 'X-Hotel-Code': getCurrentHotelCode() }
                });
```

---

### **التعديل #4: DELETE Requests**

**Find:**
```
const response = await fetch(`${API_BASE_URL}/api/zaaer/Reservation/${reservationId}`, {
                    method: 'DELETE'
                });
```

**Replace:**
```
const response = await fetch(`${API_BASE_URL}/api/zaaer/Reservation/${reservationId}`, {
                    method: 'DELETE',
                    headers: { 'X-Hotel-Code': getCurrentHotelCode() }
                });
```

---

## 📋 **قائمة التحقق - Checklist:**

بعد كل تعديل، تأكد من اختبار:

### **Reservation API:**
- [ ] `createZaaerReservation()` - ✅ تم
- [ ] `updateZaaerReservation()`
- [ ] `updateZaaerReservationByNumber()`
- [ ] `getAllZaaerReservations()`
- [ ] `deleteZaaerReservation()`

### **Payment Receipt API:**
- [ ] `createZaaerPaymentReceipt()`
- [ ] `updateZaaerPaymentReceipt()`
- [ ] `updateZaaerPaymentReceiptByNo()`
- [ ] `getAllZaaerPaymentReceipts()`
- [ ] `deleteZaaerPaymentReceipt()`

### **Invoice API:**
- [ ] `createZaaerInvoice()`
- [ ] `getAllZaaerInvoices()`
- [ ] `getZaaerInvoiceById()`
- [ ] `linkReceiptsToInvoiceFromInvoice()`
- [ ] `unlinkReceiptsFromInvoiceFromInvoice()`

### **Refund API:**
- [ ] `createZaaerRefund()`
- [ ] `updateZaaerRefund()`
- [ ] `updateZaaerRefundByNo()`
- [ ] `getAllZaaerRefunds()`

### **Credit Note API:**
- [ ] `createZaaerCreditNote()`
- [ ] `getAllZaaerCreditNotes()`

### **Room Type API:**
- [ ] `createZaaerRoomType()`
- [ ] `updateZaaerRoomType()`
- [ ] `getAllZaaerRoomTypes()`
- [ ] `getZaaerRoomTypeById()`
- [ ] `deleteZaaerRoomType()`

### **Floor API:**
- [ ] `createZaaerFloor()`
- [ ] `updateZaaerFloor()`
- [ ] `getAllZaaerFloors()`
- [ ] `getZaaerFloorById()`
- [ ] `deleteZaaerFloor()`

### **Apartment API:**
- [ ] `createZaaerApartment()`
- [ ] `updateZaaerApartment()`
- [ ] `updateZaaerApartmentByCode()`
- [ ] `getAllZaaerApartments()`
- [ ] `getZaaerApartmentById()`
- [ ] `deleteZaaerApartment()`

### **Hotel Settings API:**
- [ ] `createZaaerHotelSettings()`
- [ ] `updateZaaerHotelSettings()`
- [ ] `getAllZaaerHotelSettings()`
- [ ] `getZaaerHotelSettingsById()`
- [ ] `deleteZaaerHotelSettings()`

---

## 🧪 **الاختبار السريع:**

بعد كل تعديل:

1. ✅ احفظ الملف (Ctrl+S)
2. ✅ حدّث المتصفح (F5)
3. ✅ اختر فندق من Hotel Selector
4. ✅ جرّب الدالة
5. ✅ افتح F12 → Network → تحقق من وجود `X-Hotel-Code` header

---

## ⚙️ **نصائح مهمة:**

### **1. استخدم Multi-Cursor:**
```
Ctrl + D (Select next occurrence)
Alt + Click (Add cursor)
```

### **2. Preview قبل Replace:**
```
استخدم "Replace" (مرة واحدة) بدلاً من "Replace All"
تحقق من النتيجة
ثم اضغط "Replace All"
```

### **3. Undo إذا حدث خطأ:**
```
Ctrl + Z (Undo)
```

---

## 📊 **الوقت المتوقع:**

- **Find & Replace:** ~10-15 دقيقة
- **Testing:** ~10 دقيقة
- **Total:** ~20-25 دقيقة

---

## 🎁 **Bonus Tip:**

إذا كنت تستخدم **Git**:

```bash
# Before starting:
git commit -am "Before fixing all endpoints"

# After fixing:
git diff index.html  # Review changes
git commit -am "Fixed all endpoints - added X-Hotel-Code header"
```

---

## 🎯 **النتيجة المتوقعة:**

بعد الانتهاء:
- ✅ **40/40 دالة** تعمل بشكل صحيح
- ✅ كل الـ endpoints ترسل `X-Hotel-Code` تلقائياً
- ✅ لا مزيد من "Unauthorized" errors
- ✅ Hotel Selector يعمل مع كل API!

---

**🚀 ابدأ الآن! 20 دقيقة فقط وتنتهي! 🚀**

