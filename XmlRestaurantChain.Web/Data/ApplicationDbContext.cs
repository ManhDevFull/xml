using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<DiningTable> DiningTables => Set<DiningTable>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<StaffProfile> StaffProfiles => Set<StaffProfile>();
    public DbSet<LoyaltyMember> LoyaltyMembers => Set<LoyaltyMember>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MenuItem>()
            .Property(m => m.Price)
            .HasPrecision(12, 2);

        builder.Entity<Order>()
            .Property(o => o.Total)
            .HasPrecision(14, 2);

        builder.Entity<OrderItem>()
            .Property(o => o.UnitPrice)
            .HasPrecision(12, 2);

        builder.Entity<OrderItem>()
            .Property(o => o.LineTotal)
            .HasPrecision(14, 2);

        builder.Entity<InventoryItem>()
            .Property(i => i.Quantity)
            .HasPrecision(12, 2);

        builder.Entity<InventoryItem>()
            .Property(i => i.ReorderLevel)
            .HasPrecision(12, 2);
    }
}
