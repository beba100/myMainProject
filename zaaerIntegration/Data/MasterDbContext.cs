using Microsoft.EntityFrameworkCore;
using FinanceLedgerAPI.Models;

namespace zaaerIntegration.Data
{
    /// <summary>
    /// Master Database Context - للتعامل مع قاعدة البيانات المركزية التي تحتوي على معلومات الفنادق
    /// </summary>
    public class MasterDbContext : DbContext
    {
        /// <summary>
        /// Constructor for MasterDbContext
        /// </summary>
        /// <param name="options">DbContext options</param>
        public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// جدول الفنادق (Tenants)
        /// </summary>
        public DbSet<Tenant> Tenants { get; set; }

        /// <summary>
        /// جدول المستخدمين الرئيسيين (MasterUsers)
        /// </summary>
        public DbSet<MasterUser> MasterUsers { get; set; }

        /// <summary>
        /// جدول الأدوار (Roles)
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// جدول ربط المستخدمين بالأدوار (UserRoles)
        /// </summary>
        public DbSet<UserRole> UserRoles { get; set; }

        /// <summary>
        /// تكوين نموذج البيانات
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تكوين جدول Tenants
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable("Tenants");
                entity.HasKey(t => t.Id);
            entity.Property(t => t.Code).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.ConnectionString).HasMaxLength(500); // Optional - system uses DatabaseName instead
            entity.Property(t => t.DatabaseName).IsRequired().HasMaxLength(100);
                entity.Property(t => t.BaseUrl).HasMaxLength(200);
                entity.Property(t => t.EnableQueueMode).HasColumnName("EnableQueueMode");
                entity.Property(t => t.EnableQueueWorker).HasColumnName("EnableQueueWorker");
                entity.Property(t => t.QueueWorkerIntervalSeconds).HasColumnName("QueueWorkerIntervalSeconds");
                entity.Property(t => t.QueueWorkerBatchSize).HasColumnName("QueueWorkerBatchSize");
                entity.Property(t => t.UseQueueMiddleware).HasColumnName("UseQueueMiddleware");
                entity.Property(t => t.DefaultPartner).HasColumnName("DefaultPartner").HasMaxLength(100);

                // إنشاء Index على Code للبحث السريع
                entity.HasIndex(t => t.Code).IsUnique();
            });

            // تكوين جدول MasterUsers
            modelBuilder.Entity<MasterUser>(entity =>
            {
                entity.ToTable("MasterUsers");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
                entity.Property(u => u.TenantId).IsRequired();
                entity.Property(u => u.IsActive).IsRequired();
                entity.Property(u => u.CreatedAt).IsRequired();

                // Foreign Key إلى Tenants
                entity.HasOne(u => u.Tenant)
                    .WithMany()
                    .HasForeignKey(u => u.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index على Username للبحث السريع
                entity.HasIndex(u => u.Username).IsUnique();
            });

            // تكوين جدول Roles
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Code).IsRequired().HasMaxLength(50);

                // Index على Code للبحث السريع
                entity.HasIndex(r => r.Code).IsUnique();
            });

            // تكوين جدول UserRoles
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(ur => ur.Id);
                entity.Property(ur => ur.UserId).IsRequired();
                entity.Property(ur => ur.RoleId).IsRequired();

                // Foreign Keys
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint لمنع تكرار نفس الدور لنفس المستخدم
                entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
            });
        }
    }
}

