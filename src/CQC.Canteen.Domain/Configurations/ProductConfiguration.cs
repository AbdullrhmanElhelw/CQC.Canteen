using CQC.Canteen.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQC.Canteen.Domain.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // اسم الجدول
        builder.ToTable("Products");

        // المفتاح الأساسي (بافتراض أن Id موجود في AuditableEntity)
        builder.HasKey(p => p.Id);

        // تحديد خصائص الأعمدة
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PurchasePrice)
            .HasColumnType("decimal(18,2)"); // مهم جداً للفلوس

        builder.Property(p => p.SalePrice)
            .HasColumnType("decimal(18,2)"); // مهم جداً للفلوس

        // العلاقة: الصنف له فئة واحدة
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products) // الفئة لها أصناف كتير
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // امنع حذف فئة لو فيها أصناف
    }
}
