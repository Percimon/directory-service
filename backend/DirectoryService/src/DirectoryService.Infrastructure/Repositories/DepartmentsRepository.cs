using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Infrastructure.Database;
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

    public async Task<Result<Department, Error>> GetById(
        DepartmentId id,
        CancellationToken cancellationToken = default)
    {
        var result = _dbContext.Departments
            .FirstOrDefault(d => d.Id == id && d.IsActive);

        if (result is null)
        {
            return GeneralErrors.NotFound(id.Value);
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
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null })
            {
                if (pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GeneralErrors.AlreadyExists(nameof(Department), nameof(Department.Name), department.Name.Value);
                }

                if (pgEx.ConstraintName.Contains("identifier", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GeneralErrors.AlreadyExists(nameof(Department), nameof(Department.Identifier), department.Identifier.Value);
                }
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

    public async Task<UnitResult<Error>> Save(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            string message = "Failed to save changes";

            _logger.LogError(e, message);

            return Error.Failure("database", message);
        }
    }

    public async Task<Result<Department, Error>> GetByIdWithLocations(DepartmentId id, CancellationToken cancellationToken)
    {
        var result = _dbContext.Departments
            .Include(d => d.DepartmentLocations)
            .FirstOrDefault(d => d.Id == id && d.IsActive);

        if (result is null)
        {
            return GeneralErrors.NotFound(id.Value);
        }

        return result;
    }
}