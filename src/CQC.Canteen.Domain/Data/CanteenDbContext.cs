using CQC.Canteen.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CQC.Canteen.Domain.Data;

public class CanteenDbContext : DbContext
{
    public CanteenDbContext(DbContextOptions<CanteenDbContext> options)
            : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
