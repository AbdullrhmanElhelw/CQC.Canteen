using CQC.Canteen.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQC.Canteen.Domain.Configurations;

public class OrderDetailsConfiguration : IEntityTypeConfiguration<OrderDetails>
{
    public void Configure(EntityTypeBuilder<OrderDetails> builder)
    {
        builder.ToTable("OrderDetails");
        builder.HasKey(od => od.Id);

        builder.Property(od => od.Quantity).IsRequired();

        builder.Property(od => od.UnitPrice)
            .HasColumnType("decimal(18,2)"); // سعر الصنف وقت البيع

        // العلاقة 1: (تم تعريفها في OrderConfiguration)
        // .HasOne(od => od.Order) ...

        // العلاقة 2: تفصيل الفاتورة يخص صنف واحد (Product)
        builder.HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // امنع حذف صنف لو اتباع قبل كده
    }
}