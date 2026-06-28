using JetTest.Models;
using JetTest.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace JetTest.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<DeliveryDriver> DeliveryDrivers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
    public DbSet<DriverStatusHistory> DriverStatusHistories { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<int>();

        modelBuilder.Entity<DeliveryDriver>()
            .Property(d => d.Status)
            .HasConversion<int>();

        modelBuilder.Entity<OrderStatusHistory>()
            .Property(h => h.OldStatus)
            .HasConversion<int>();

        modelBuilder.Entity<OrderStatusHistory>()
            .Property(h => h.NewStatus)
            .HasConversion<int>();

        modelBuilder.Entity<DriverStatusHistory>()
            .Property(h => h.OldStatus)
            .HasConversion<int>();

        modelBuilder.Entity<DriverStatusHistory>()
            .Property(h => h.NewStatus)
            .HasConversion<int>();

        // Optional FK: Order → DeliveryDriver — no cascade
        modelBuilder.Entity<Order>()
            .HasOne(o => o.DeliveryDriver)
            .WithMany(d => d.Orders)
            .HasForeignKey(o => o.DeliveryDriverId)
            .OnDelete(DeleteBehavior.Restrict);

        // Keep history rows even if order is deleted
        modelBuilder.Entity<OrderStatusHistory>()
            .HasOne(h => h.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Keep history rows even if driver is deleted
        modelBuilder.Entity<DriverStatusHistory>()
            .HasOne(h => h.Driver)
            .WithMany(d => d.StatusHistory)
            .HasForeignKey(h => h.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<Dish>()
            .Property(d => d.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        // Dish → Category / Restaurant — no cascade
        modelBuilder.Entity<Dish>()
            .HasOne(d => d.Category)
            .WithMany(c => c.Dishes)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Dish>()
            .HasOne(d => d.Restaurant)
            .WithMany(r => r.Dishes)
            .HasForeignKey(d => d.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrderItem → Order: cascade (items belong to order); → Dish: restrict
        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Dish)
            .WithMany(d => d.OrderItems)
            .HasForeignKey(i => i.DishId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
