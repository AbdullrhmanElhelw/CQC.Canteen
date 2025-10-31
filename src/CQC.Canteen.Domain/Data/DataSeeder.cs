using CQC.Canteen.Domain.Data;
using CQC.Canteen.Domain.Entities;
using CQC.Canteen.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CQC.Canteen.Data
{
    /// <summary>
    /// كلاس مسئول فقط عن إضافة البيانات الأولية (Seeding)
    /// يتلقى الـ DbContext جاهزاً ولا ينشئه بنفسه
    /// </summary>
    public static class DataSeeder
    {
        /// <summary>
        /// يستدعي كل دوال الـ Seeding لو كانت الجداول فارغة
        /// </summary>
        public static async Task SeedAsync(CanteenDbContext context)
        {
            // 2. هيبدأ يضيف البيانات الأولية
            await SeedCategoriesAndProductsAsync(context);
            await SeedUsersAsync(context);
            await SeedCustomersAsync(context);
        }

        private static async Task SeedCategoriesAndProductsAsync(CanteenDbContext context)
        {
            // لو لقينا أي فئة، يبقى البيانات موجودة ومش هنضيف حاجة
            if (await context.Categories.AnyAsync())
            {
                return;
            }

            // --- إضافة الفئات ---
            var catHotDrinks = new Category { Name = "مشروبات ساخنة" };
            var catColdDrinks = new Category { Name = "مشروبات باردة" };
            var catSnacks = new Category { Name = "سناكس" };

            await context.Categories.AddRangeAsync(catHotDrinks, catColdDrinks, catSnacks);

            // --- إضافة الأصناف ---
            await context.Products.AddRangeAsync(
                new Product
                {
                    Name = "شاي",
                    PurchasePrice = 2.0m,
                    SalePrice = 5.0m,
                    StockQuantity = 100,
                    IsActive = true,
                    Category = catHotDrinks
                },
                new Product
                {
                    Name = "قهوة",
                    PurchasePrice = 4.0m,
                    SalePrice = 7.0m,
                    StockQuantity = 100,
                    IsActive = true,
                    Category = catHotDrinks
                },
                new Product
                {
                    Name = "بيبسي كانز",
                    PurchasePrice = 7.5m,
                    SalePrice = 10.0m,
                    StockQuantity = 200,
                    IsActive = true,
                    Category = catColdDrinks
                },
                new Product
                {
                    Name = "شيبسي",
                    PurchasePrice = 4.0m,
                    SalePrice = 5.0m,
                    StockQuantity = 150,
                    IsActive = true,
                    Category = catSnacks
                }
            );

            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(CanteenDbContext context)
        {
            // لو لقينا أي مستخدم، يبقى البيانات موجودة
            if (await context.Users.AnyAsync())
            {
                return;
            }

            // !! تنبيه هام: في التطبيق الحقيقي، يجب عمل Hashing لكلمة السر
            // !! Do NOT store plain text passwords in a real application.
            // !! You should use a password hasher.

            var adminUser = new User
            {
                Name = "Admin User",
                UserName = "admin",
                Password = "123456", // لازم تعملها HASH
                Role = Role.Admin
            };

            var casherUser = new User
            {
                Name = "Casher 1",
                UserName = "casher1",
                Password = "123456", // لازم تعملها HASH
                Role = Role.Casher
            };

            await context.Users.AddRangeAsync(adminUser, casherUser);
            await context.SaveChangesAsync();
        }

        private static async Task SeedCustomersAsync(CanteenDbContext context)
        {
            // لو لقينا أي عميل، يبقى البيانات موجودة
            if (await context.Customers.AnyAsync())
            {
                return;
            }

            await context.Customers.AddRangeAsync(
                new Customer
                {
                    Name = "السيد العميد / مدير الوحدة",
                    CurrentBalance = 0,
                    IsActive = true
                },
                new Customer
                {
                    Name = "العقيد صالح",
                    CurrentBalance = 0,
                    IsActive = true
                }
            );

            await context.SaveChangesAsync();
        }
    }
}

