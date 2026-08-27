using DirectoryService.Application.Database;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Domain.Abstractions;
using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Database;

public class AppDbContext : DbContext, IReadDbContext
{
    private readonly string _connectionString;

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<DepartmentPosition> DepartmentPositions => Set<DepartmentPosition>();

    public IQueryable<Location> LocationsRead => Set<Location>().AsNoTracking();

    public IQueryable<Department> DepartmentsRead =>
        Set<Department>()
            .Include(x => x.Parent)
            .AsNoTracking();

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
    }

    private ILoggerFactory CreateLoggerFactory() =>
          LoggerFactory.Create(builder => builder.AddConsole());

    public async Task PurgeAllDeletedRecordsAsync(
        int retentionDays,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-retentionDays);

        int deletedDepartments = await PurgeEntityAsync<Department>(thresholdDate, batchSize, cancellationToken);
        int deletedLocations = await PurgeEntityAsync<Location>(thresholdDate, batchSize, cancellationToken);
        int deletedPositions = await PurgeEntityAsync<Position>(thresholdDate, batchSize, cancellationToken);
    }

    private async Task<int> PurgeEntityAsync<TEntity>(
       DateTime thresholdDate,
       int batchSize,
       CancellationToken cancellationToken)
       where TEntity : class, ISoftDeletable
    {
        int totalDeleted = 0;
        bool hasMore = true;

        while (hasMore && !cancellationToken.IsCancellationRequested)
        {
            int deletedInBatch = await Set<TEntity>()
                .Where(x => !x.IsActive && x.DeletedAt < thresholdDate)
                .Take(batchSize)
                .ExecuteDeleteAsync(cancellationToken);

            totalDeleted += deletedInBatch;
            hasMore = deletedInBatch == batchSize;

            if (deletedInBatch > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
        }

        return totalDeleted;
    }
}