using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Dtos;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedService.SharedKernel;

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

    public async Task<Result<Department, Error>> GetByIdWithPositions(DepartmentId id, CancellationToken cancellationToken)
    {
        var result = _dbContext.Departments
           .Include(d => d.DepartmentPositions)
           .FirstOrDefault(d => d.Id == id && d.IsActive);

        if (result is null)
        {
            return GeneralErrors.NotFound(id.Value);
        }

        return result;
    }

    public async Task<UnitResult<Error>> DepartmentsExist(IEnumerable<DepartmentId> ids, CancellationToken cancellationToken)
    {
        try
        {
            if (ids is null)
                return Error.NotFound("department.id", "Departments id list is null");

            DepartmentId[] departmentIds = ids.ToArray();

            int expectedCount = departmentIds.Length;

            if (expectedCount == 0)
                return Error.NotFound("department.id", "Departments id list is empty");

            int count = await _dbContext.Departments
                .Where(l => Array.IndexOf(departmentIds, l.Id) > -1 && l.IsActive)
                .CountAsync(cancellationToken);

            return expectedCount == count
                ? UnitResult.Success<Error>()
                : Error.NotFound("department.id", "One of department ids were not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("database", "Department id count failed");
        }
    }

    public async Task<Result<Department, Error>> GetByIdWithLock(DepartmentId id, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .FromSql($"SELECT * FROM departments WHERE id = {id.Value} AND is_active = true FOR UPDATE NOWAIT")
            .Include(d => d.Children)
            .FirstOrDefaultAsync(cancellationToken);

        if (department is null)
        {
            return GeneralErrors.NotFound(id.Value);
        }

        return department;
    }

    public async Task<UnitResult<Error>> ChangeParent(
        string rootPath,
        string newParentPath,
        Guid departmentId,
        Guid? newParentId,
        CancellationToken cancellationToken)
    {
        var sql =
            """
            UPDATE departments
            SET 
                path = (@newParentPath::ltree || subpath(path, nlevel(@rootPath::ltree) - 1)), 
                depth = nlevel(@newParentPath::ltree || subpath(path, nlevel(@rootPath::ltree) - 1)) - 1
            WHERE path <@ @rootPath::ltree;

            UPDATE departments
            SET fk_parent_id = @newParentId
            WHERE id = @departmentId;
            """;

        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                sql,
                [new NpgsqlParameter("@rootPath", rootPath),
                new NpgsqlParameter("@newParentPath", newParentPath),
                new NpgsqlParameter("@newParentId", newParentId.HasValue ? newParentId : DBNull.Value),
                new NpgsqlParameter("@departmentId", departmentId)],
                cancellationToken);
        }
        catch (Exception ex)
        {
            return Error.Failure("database", ex.Message);
        }

        return Result.Success<Error>();
    }

    public async Task<UnitResult<Error>> LockDescendants(string rootPath, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT id
            FROM departments 
            WHERE path <@ @rootPath::ltree
            AND path != @rootPath::ltree FOR UPDATE NOWAIT;
            """;
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                sql,
                [new NpgsqlParameter("@rootPath", rootPath)],
                cancellationToken);
        }
        catch (Exception ex)
        {
            return Error.Failure("database.lock-descendants", ex.Message);
        }

        return Result.Success<Error>();
    }

    public async Task<List<DepartmenDto>> GetHierarchyLtree(string rootPath)
    {
        const string sql =
            """
            SELECT id, 
                parent_id, 
                path, 
                depth,
                is_active 
            FROM departments 
            WHERE path <@ @rootPath::ltree
            ORDER BY depth;
            """;

        var connection = _dbContext.Database.GetDbConnection();

        var parameters = new { rootPath };

        var departmentRows = (await connection.QueryAsync<DepartmenDto>(sql, parameters))
            .ToList();

        var departmentDictionary = departmentRows.ToDictionary(x => x.Id);

        var roots = new List<DepartmenDto>();

        foreach (var row in departmentRows)
        {
            if (row.Parent.HasValue && departmentDictionary.TryGetValue(row.Parent.Value, out var parent))
            {
                parent.Children.Add(departmentDictionary[row.Id]);
            }
            else
            {
                roots.Add(departmentDictionary[row.Id]);
            }
        }

        return roots;
    }

}