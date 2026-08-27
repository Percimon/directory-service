using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Domain.Abstractions;
using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedService.SharedKernel;

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

    public async Task<UnitResult<Error>> PurgeAllDeletedRecordsAsync(
        int retentionDays,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-retentionDays);

        var deletedDepartmentsResult = await PurgeEntityAsync<Department>(
            thresholdDate,
            batchSize,
            cancellationToken);

        if (deletedDepartmentsResult.IsFailure)
            return deletedDepartmentsResult.Error;

        var deletedLocationsResult = await PurgeEntityAsync<Location>(
            thresholdDate,
            batchSize,
            cancellationToken);

        if (deletedLocationsResult.IsFailure)
            return deletedDepartmentsResult.Error;

        var deletedPositions = await PurgeEntityAsync<Position>(
            thresholdDate,
            batchSize,
            cancellationToken);

        if (deletedPositions.IsFailure)
            return deletedDepartmentsResult.Error;

        return Result.Success<Error>();
    }

    private async Task<Result<int, Error>> PurgeEntityAsync<TEntity>(
       DateTime thresholdDate,
       int batchSize,
       CancellationToken cancellationToken)
       where TEntity : class, ISoftDeletable
    {
        int totalDeleted = 0;
        bool hasMore = true;

        try
        {
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
        catch
        {
            return Error.Failure("databe.delete", $"Failed to delete {typeof(TEntity)}");
        }
    }
}