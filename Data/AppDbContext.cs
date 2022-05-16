using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RecruitmentApp.Core.Models;
using System.Reflection;

namespace RecruitmentApp.Data;

public class AppDbContext : DbContext
{
    private readonly string _connection;
    private readonly IHostEnvironment _environment;

    public DbSet<ExchangeRate>? ExchangeRates { get; set; }

    public AppDbContext(
        DbContextOptions dbContextOptions,
        IConfiguration configuration,
        IHostEnvironment environment) : base(dbContextOptions)
    {
        _connection = configuration.GetConnectionString("SQLServer");
        _environment = environment;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSqlServer(_connection);

        if (_environment.IsDevelopment())
        {
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();

        }
    }

    // TODO check if can delete
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override int SaveChanges()
    {
        SetDatesOnAuditables();

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetDatesOnAuditables();

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetDatesOnAuditables()
    {
        var addedAuditables = (IEnumerable<AuditableEntity>)ChangeTracker.Entries()
            .Where(entry =>
                 entry.State == EntityState.Added
                 && entry.Entity is AuditableEntity)
            .Select(entry => entry.Entity as AuditableEntity);

        var modifiedAuditables = (IEnumerable<AuditableEntity>)ChangeTracker.Entries()
            .Where(entry =>
                entry.State == EntityState.Modified
                && entry.Entity is AuditableEntity)
            .Select(entry => entry.Entity as AuditableEntity);

        foreach (var entity in addedAuditables)
        {
            var date = DateTime.UtcNow;
            entity.CreatedOn = date;
            entity.ModifiedOn = date;
        }

        foreach (var entity in modifiedAuditables)
        {
            entity.ModifiedOn = DateTime.UtcNow;
        }
    }
}
