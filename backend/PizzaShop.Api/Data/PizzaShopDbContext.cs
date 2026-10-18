using Microsoft.EntityFrameworkCore;
using PizzaShop.Api.Models;

namespace PizzaShop.Api.Data;

public class PizzaShopDbContext(DbContextOptions<PizzaShopDbContext> options)
    : DbContext(options)
{
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Emoji).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Price).HasPrecision(10, 2);
        });

        modelBuilder.Entity<RestaurantTable>(entity =>
            entity.HasIndex(x => x.TableNumber).IsUnique());

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(x => x.Total).HasPrecision(10, 2);
            entity.Property(x => x.PaymentMethod).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.OrderStatus).HasMaxLength(40).IsRequired();
            entity.HasMany(x => x.Items).WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Payments).WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(x => x.ItemName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(10, 2);
 
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.Provider).HasMaxLength(40).IsRequired();
            entity.Property(x => x.ProviderReference).HasMaxLength(200);
            entity.Property(x => x.Amount).HasPrecision(10, 2);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        });
    }
}
