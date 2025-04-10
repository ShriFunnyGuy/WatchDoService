using Microsoft.EntityFrameworkCore;
using WatchDogServiceApi.Entities;

public class ApplicationDbContext : DbContext
{
    public DbSet<AdminWatchDogWinServices> admin_watchdogwinservices { get; set; }
    public DbSet<AdminWatchDogSQLJobs> admin_watchdogsqljobs { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminWatchDogWinServices>()
        .HasKey(m => m.Pid);
        modelBuilder.Entity<AdminWatchDogWinServices>()
            .ToTable("admin_watchdogwinservices"); // Explicit Table Name

        modelBuilder.Entity<AdminWatchDogSQLJobs>()
            .HasKey(m => m.Pid);
        modelBuilder.Entity<AdminWatchDogSQLJobs>()
            .ToTable("admin_watchdogsqljobs"); // Explicit Table Name
    }
}
