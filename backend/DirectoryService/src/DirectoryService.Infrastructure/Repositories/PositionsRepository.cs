using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedKernel;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionsRepository : IPositionsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    private readonly ILogger<PositionsRepository> _logger;

    public PositionsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Positions.AddAsync(position, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return position.Id.Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                && pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
            {
                return GeneralErrors.AlreadyExists(nameof(Position), nameof(Position.Name), position.Name.Value);
            }

            _logger.LogError(ex, "Database update error while creating Position with name: {name}", position.Name.Value);

            return GeneralErrors.Failure("Database update error while creating Position");
        }
        catch (Exception e)
        {
            string message = "Failed to insert Position";

            _logger.LogError(e, message);

            return Error.Failure("position.insert", message);
        }
    }
}