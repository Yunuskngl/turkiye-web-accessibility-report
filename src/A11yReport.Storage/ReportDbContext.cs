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
        // Foreign keys are named explicitly: EF's naming convention would otherwise add hidden
        // "<Type>EntityId" columns next to our RunId / SiteScanId / ViolationId columns.
        modelBuilder.Entity<ScanRunEntity>(run =>
        {
            run.HasIndex(r => r.Name).IsUnique();
            run.Property(r => r.State).HasConversion<string>();
            run.HasMany(r => r.Sessions).WithOne().HasForeignKey(s => s.RunId).OnDelete(DeleteBehavior.Cascade);
            run.HasMany(r => r.Sites).WithOne().HasForeignKey(s => s.RunId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RunSessionEntity>().Property(s => s.State).HasConversion<string>();

        modelBuilder.Entity<SiteScanEntity>(site =>
        {
            site.HasIndex(s => new { s.RunId, s.Domain }).IsUnique();
            site.Property(s => s.Status).HasConversion<string>();
            site.HasMany(s => s.Violations).WithOne().HasForeignKey(v => v.SiteScanId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ViolationEntity>(violation =>
        {
            violation.HasIndex(v => v.RuleId);
            violation.HasMany(v => v.Nodes).WithOne().HasForeignKey(n => n.ViolationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ViolationNodeEntity>().ToTable("ViolationNodes");
    }
}

/// <summary>Used only by the dotnet-ef tool when creating migrations.</summary>
public sealed class ReportDbContextDesignFactory : IDesignTimeDbContextFactory<ReportDbContext>
{
    public ReportDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ReportDbContext>().UseSqlite("Data Source=design-time.db").Options);
}
