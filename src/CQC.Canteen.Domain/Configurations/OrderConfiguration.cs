using CQC.Canteen.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQC.Canteen.Domain.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        // العلاقة 1: الفاتورة عملها كاشير واحد (User)
        builder.HasOne(o => o.CreatedByUser)
            .WithMany(u => u.CreatedOrders)
            .HasForeignKey(o => o.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict); // امنع حذف كاشير لو عامل فواتير

        // العلاقة 2: الفاتورة تخص عميل واحد (Customer) (اختياري)
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .IsRequired(false) // عشان البيع النقدي
            .OnDelete(DeleteBehavior.SetNull); // لو العميل اتمسح، خلي الفاتورة موجودة بس CustomerId = NULL

        // العلاقة 3: الفاتورة لها تفاصيل كتير (OrderDetails)
        builder.HasMany(o => o.OrderDetails)
            .WithOne(od => od.Order)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // أهم واحدة: لو الفاتورة اتمسحت، امسح كل تفاصيلها
    }
}
