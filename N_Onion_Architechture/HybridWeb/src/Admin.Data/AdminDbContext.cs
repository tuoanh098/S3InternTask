// src/Admin.Data/AdminDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace Admin.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

    public DbSet<OrderReadModel> Orders => Set<OrderReadModel>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasCharSet("utf8mb4").UseCollation("utf8mb4_unicode_ci");

        b.Entity<OrderReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Customer).IsRequired().HasMaxLength(200);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
        });
    }
}

public class OrderReadModel
{
    public Guid Id { get; set; }
    public string Customer { get; set; } = default!;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
