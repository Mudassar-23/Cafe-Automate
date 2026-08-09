using CafeAutomate.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeAutomate.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<AllMenuItem> AllMenuItems => Set<AllMenuItem>();
    public DbSet<DailyMenuItem> DailyMenuItems => Set<DailyMenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CafePaymentDetails> CafePaymentDetails => Set<CafePaymentDetails>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AllMenuItem>()
            .Property(m => m.Price).HasColumnType("decimal(10,2)");

        modelBuilder.Entity<DailyMenuItem>()
            .Property(m => m.Price).HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPriceSnapshot).HasColumnType("decimal(10,2)");
    }
}
