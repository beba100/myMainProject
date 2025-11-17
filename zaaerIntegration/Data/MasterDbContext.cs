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
        }
    }
}

