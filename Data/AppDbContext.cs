using Microsoft.EntityFrameworkCore;
using BizSecureDemo22180092.Models;
using Microsoft.AspNetCore.Identity;

namespace BizSecureDemo22180092.Data;

public class AppDbContext : DbContext
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Order> Orders => Set<Order>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed test users
        var hasher = new PasswordHasher<AppUser>();
        
        var user1 = new AppUser 
        { 
            Id = 1, 
            Email = "alice@test.com"
        };
        user1.PasswordHash = hasher.HashPassword(user1, "Password123!");

        var user2 = new AppUser 
        { 
            Id = 2, 
            Email = "bob@test.com"
        };
        user2.PasswordHash = hasher.HashPassword(user2, "Password456!");

        modelBuilder.Entity<AppUser>().HasData(user1, user2);

        // Seed test orders
        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 1, UserId = 1, Title = "Premium Package", Amount = 999.99m },
            new Order { Id = 2, UserId = 1, Title = "Standard Package", Amount = 499.99m },
            new Order { Id = 3, UserId = 2, Title = "Basic Package", Amount = 99.99m }
        );
    }
}
