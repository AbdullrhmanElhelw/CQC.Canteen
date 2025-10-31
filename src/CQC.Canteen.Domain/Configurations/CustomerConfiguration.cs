using CQC.Canteen.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQC.Canteen.Domain.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.Property(c => c.CurrentBalance)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0.0m); // يبدأ برصيد صفر
    }
}
