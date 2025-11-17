namespace FinanceLedgerAPI.Models
{
    /// <summary>
    /// Tenant Model - يمثل الفندق في النظام متعدد المستأجرين
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// معرف الفندق
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// كود الفندق (Dammam1, Dammam2, etc.)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// اسم الفندق
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Connection String الخاص بقاعدة بيانات الفندق (اختياري - النظام يستخدم DatabaseName بدلاً منه)
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// اسم قاعدة البيانات الخاصة بالفندق
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Base URL للفندق (اختياري)
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// تمكين وضع الطابور لهذا الفندق (يتجاوز القيمة العامة إن وُجدت)
        /// </summary>
        public bool? EnableQueueMode { get; set; }

        /// <summary>
        /// تمكين معالج الخلفية للطابور لهذا الفندق
        /// </summary>
        public bool? EnableQueueWorker { get; set; }

        /// <summary>
        /// الفترة الزمنية بين كل دفعة معالجة للطابور (بالثواني)
        /// </summary>
        public int? QueueWorkerIntervalSeconds { get; set; }

        /// <summary>
        /// حجم الدفعة لكل تشغيل للطابور
        /// </summary>
        public int? QueueWorkerBatchSize { get; set; }

        /// <summary>
        /// تفعيل الـ Middleware الخاص بالطابور (إن وجد)
        /// </summary>
        public bool? UseQueueMiddleware { get; set; }

        /// <summary>
        /// اسم الشريك الافتراضي المستخدم عند الاستدعاء (مثلاً Zaaer)
        /// </summary>
        public string? DefaultPartner { get; set; }
    }
}

