namespace LuckyReport.Server.Models;

public class LuckyReportContext : DbContext
{
    public DbSet<Report>? Reports { get; set; }

    public DbSet<DataSource>? DataSources { get; set; }
    public string DbPath { get; }

    public LuckyReportContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        Environment.GetFolderPath(folder);
        DbPath = Path.Join( "luckyreport.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

