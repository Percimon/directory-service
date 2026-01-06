using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedKernel;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    private readonly ILogger<DepartmentsRepository> _logger;

    public DepartmentsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<DepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Department, Error>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = _dbContext.Departments.FirstOrDefault(d => d.Id.Value == id && d.IsActive);

        if (result is null)
        {
            return GeneralErrors.NotFound(id);
        }

        return result;
    }

    public async Task<Result<Guid, Error>> Add(Department department, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Departments.AddAsync(department, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return department.Id.Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                && pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
            {
                return GeneralErrors.AlreadyExists(nameof(Department), nameof(Department.Name), department.Name.Value);
            }

            _logger.LogError(ex, "Database update error while creating Department with name: {name}", department.Name.Value);

            return GeneralErrors.Failure("Database update error while creating Department");
        }
        catch (Exception e)
        {
            string message = "Failed to insert Department";

            _logger.LogError(e, message);

            return Error.Failure("department.insert", message);
        }
    }
}