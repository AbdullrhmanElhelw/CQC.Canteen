using CQC.Canteen.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQC.Canteen.Domain.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);

        builder.Property(u => u.UserName).IsRequired().HasMaxLength(50);

        // عشان نضمن مفيش 2 يوزر بنفس الاسم
        builder.HasIndex(u => u.UserName).IsUnique();

        builder.Property(u => u.Password).IsRequired(); // هتكون Hashed
        builder.Property(u => u.Role).IsRequired();
    }
}
