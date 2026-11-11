using A11yReport.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace A11yReport.Tests.Integration;

/// <summary>A fresh SQLite database file in a temp folder, deleted afterwards.</summary>
public sealed class TempStore : IDisposable
{
    public TempStore()
    {
        Folder = Directory.CreateTempSubdirectory("a11yreport-tests-").FullName;
        var path = Path.Combine(Folder, "test.db");
        var options = new DbContextOptionsBuilder<ReportDbContext>()
            .UseSqlite(new SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ToString())
            .Options;
        Store = new RunStore(new PooledDbContextFactory<ReportDbContext>(options), Options.Create(TestSettings.Storage(path)));
    }

    public string Folder { get; }

    public RunStore Store { get; }

    public void Dispose()
    {
        Store.Dispose();
        Directory.Delete(Folder, recursive: true);
    }
}
