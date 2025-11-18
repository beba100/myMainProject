"""
Script: إضافة 70 مستخدم في Master DB
Database: Master DB (db32357)
Server: s800.public.eu.machineasp.net
"""

import pyodbc
import bcrypt
import random
from datetime import datetime

# Connection String
CONNECTION_STRING = (
    "Driver={ODBC Driver 17 for SQL Server};"
    "Server=s800.public.eu.machineasp.net;"
    "Database=db32357;"
    "UID=admin;"
    "PWD=vS6FWjgGoHcjwbcQaFby1pcx;"
    "Encrypt=yes;"
    "TrustServerCertificate=yes;"
)

DEFAULT_PASSWORD = "123"

def hash_password(password: str) -> str:
    """تشفير كلمة المرور باستخدام BCrypt"""
    salt = bcrypt.gensalt(rounds=12)
    return bcrypt.hashpw(password.encode('utf-8'), salt).decode('utf-8')

def main():
    print("=" * 50)
    print("إضافة 70 مستخدم في Master DB")
    print("=" * 50)
    print()

    try:
        # الاتصال بقاعدة البيانات
        conn = pyodbc.connect(CONNECTION_STRING)
        cursor = conn.cursor()
        print("✅ تم الاتصال بقاعدة البيانات بنجاح")
    except Exception as e:
        print(f"❌ فشل الاتصال بقاعدة البيانات: {e}")
        return

    # الحصول على قائمة Tenants
    cursor.execute("SELECT Id, Code, Name FROM Tenants")
    tenants = cursor.fetchall()
    
    if not tenants:
        print("❌ لا توجد فنادق في قاعدة البيانات. يرجى إضافة فنادق أولاً.")
        conn.close()
        return

    print(f"✅ تم العثور على {len(tenants)} فندق")
    print()

    # الحصول على قائمة Roles
    cursor.execute("SELECT Id, Code FROM Roles")
    roles = cursor.fetchall()
    
    if not roles:
        print("❌ لا توجد أدوار في قاعدة البيانات. يرجى تشغيل CreateMasterUsersTables.sql أولاً.")
        conn.close()
        return

    print(f"✅ تم العثور على {len(roles)} دور")
    print()

    # تشفير كلمة المرور
    password_hash = hash_password(DEFAULT_PASSWORD)

    success_count = 0
    error_count = 0

    # توزيع المستخدمين على الفنادق والأدوار
    tenant_index = 0
    role_index = 0

    for i in range(1, 71):
        try:
            # اختيار Tenant (توزيع دوري)
            tenant = tenants[tenant_index]
            tenant_id = tenant[0]
            tenant_code = tenant[1]
            
            # اختيار Role (توزيع دوري)
            role = roles[role_index]
            role_id = role[0]
            
            # إنشاء اسم مستخدم فريد
            username = f"user{i}"
            
            # التحقق من عدم وجود مستخدم بنفس الاسم
            cursor.execute("SELECT COUNT(*) FROM MasterUsers WHERE Username = ?", username)
            exists = cursor.fetchone()[0]
            
            if exists > 0:
                print(f"⚠️  المستخدم {username} موجود بالفعل، يتم التخطي...")
                error_count += 1
                continue
            
            # إدراج المستخدم
            cursor.execute("""
                INSERT INTO MasterUsers (Username, PasswordHash, TenantId, IsActive, CreatedAt)
                VALUES (?, ?, ?, 1, GETUTCDATE())
            """, username, password_hash, tenant_id)
            
            # الحصول على UserId
            cursor.execute("SELECT SCOPE_IDENTITY()")
            user_id = cursor.fetchone()[0]
            
            # إضافة الدور للمستخدم
            cursor.execute("""
                INSERT INTO UserRoles (UserId, RoleId)
                VALUES (?, ?)
            """, user_id, role_id)
            
            conn.commit()
            
            print(f"✅ تم إضافة المستخدم: {username} (Tenant: {tenant_code}, Role: {role[1]})")
            success_count += 1
            
            # تحديث المؤشرات
            tenant_index = (tenant_index + 1) % len(tenants)
            role_index = (role_index + 1) % len(roles)
            
        except Exception as e:
            print(f"❌ خطأ في إضافة المستخدم {i}: {e}")
            error_count += 1
            conn.rollback()

    conn.close()

    print()
    print("=" * 50)
    print("تم الانتهاء!")
    print(f"✅ نجح: {success_count}")
    print(f"❌ فشل: {error_count}")
    print("=" * 50)
    print()
    print(f"كلمة المرور الافتراضية لجميع المستخدمين: {DEFAULT_PASSWORD}")
    print("يُنصح بتغيير كلمات المرور بعد أول تسجيل دخول")

if __name__ == "__main__":
    main()

