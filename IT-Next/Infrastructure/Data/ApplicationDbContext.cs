using IT_Next.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace IT_Next.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }

    public DbSet<SubCategory> SubCategories { get; set; }

    public DbSet<Brand> Brands { get; set; }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        modelBuilder.Entity<SubCategory>().HasIndex(sc => sc.Name).IsUnique();

        modelBuilder.Entity<Brand>().HasIndex(b => b.Name).IsUnique();

        modelBuilder.Entity<Product>().HasIndex(p => p.Name).IsUnique();
    }
}