using CQC.Canteen.Domain.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQC.Canteen.Data
{
    /// <summary>
    /// "المنسق" المسئول عن تجهيز قاعدة البيانات عند بدء التشغيل
    /// </summary>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// 1. ينشئ سكوب للخدمات
        /// 2. يطبق المايجريشن (Migrate)
        /// 3. يستدعي الـ DataSeeder لإضافة البيانات
        /// </summary>
        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            // بنعمل Scope جديد عشان الـ DbContext (عشان هو Scoped service)
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CanteenDbContext>();

            // --- الخطوة الأولى: تطبيق المايجريشن ---
            // (هذه هي المسئولية الأولى)
            await context.Database.MigrateAsync();

            // --- الخطوة الثانية: إضافة البيانات الأولية ---
            // (نمرر الـ context الجاهز إلى الـ Seeder)
            await DataSeeder.SeedAsync(context);
        }
    }
}

