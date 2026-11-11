using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace A11yReport.Storage;

public sealed class ReportDbContext(DbContextOptions<ReportDbContext> options) : DbContext(options)
{
    public DbSet<ScanRunEntity> Runs => Set<ScanRunEntity>();
    public DbSet<RunSessionEntity> Sessions => Set<RunSessionEntity>();
    public DbSet<SiteScanEntity> Sites => Set<SiteScanEntity>();
    public DbSet<ViolationEntity> Violations => Set<ViolationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScanRunEntity>(run =>
        {
            run.HasIndex(r => r.Name).IsUnique();
            run.Property(r => r.State).HasConversion<string>();
        });

        modelBuilder.Entity<RunSessionEntity>().Property(s => s.State).HasConversion<string>();

        modelBuilder.Entity<SiteScanEntity>(site =>
        {
            site.HasIndex(s => new { s.RunId, s.Domain }).IsUnique();
            site.Property(s => s.Status).HasConversion<string>();
        });

        modelBuilder.Entity<ViolationEntity>().HasIndex(v => v.RuleId);
    }
}

/// <summary>Used only by the dotnet-ef tool when creating migrations.</summary>
public sealed class ReportDbContextDesignFactory : IDesignTimeDbContextFactory<ReportDbContext>
{
    public ReportDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ReportDbContext>().UseSqlite("Data Source=design-time.db").Options);
}
