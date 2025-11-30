using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opt) : base(opt) {}

    public DbSet<IssFetchLogEntity> IssFetchLogs { get; set; } = null!;
    public DbSet<OsdrItemEntity> OsdrItems { get; set; } = null!;
    public DbSet<SpaceCacheEntity> SpaceCaches { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IssFetchLogEntity>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<OsdrItemEntity>()
            .HasIndex(x => x.DatasetId)
            .IsUnique()
            .HasFilter("\"DatasetId\" IS NOT NULL"); // EF Core syntax for filtered index

        modelBuilder.Entity<SpaceCacheEntity>()
            .HasIndex(x => new { x.Source, x.FetchedAt });

        modelBuilder.Entity<IssFetchLogEntity>()
            .Property(x => x.Payload)
            .HasColumnType("jsonb");

        modelBuilder.Entity<OsdrItemEntity>()
            .Property(x => x.Raw)
            .HasColumnType("jsonb");

        modelBuilder.Entity<SpaceCacheEntity>()
            .Property(x => x.Payload)
            .HasColumnType("jsonb");
    }
}
