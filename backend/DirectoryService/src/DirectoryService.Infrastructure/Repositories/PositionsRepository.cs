using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedService.SharedKernel;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionsRepository : IPositionsRepository
{
    private readonly AppDbContext _dbContext;

    private readonly ILogger<PositionsRepository> _logger;

    public PositionsRepository(
        AppDbContext dbContext,
        ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Position, Error>> GetById(PositionId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var position = await _dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (position is null)
            {
                return Result.Failure<Position, Error>(GeneralErrors.NotFound(id.Value));
            }

            return position;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to retrieve Position with ID: {Id}", id.Value);

            return Error.Failure("position.get", "Failed to retrieve Position");
        }
    }

    public async Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Positions.AddAsync(position, cancellationToken);

            return position.Id.Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                && pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
            {
                return GeneralErrors.AlreadyExists(nameof(Position), nameof(Position.Name), position.Name.Value);
            }

            string message = $"Failed to insert Position with Name: {position.Name.Value}";

            _logger.LogError(pgEx, message);

            return Error.Failure("position.add", message);
        }
        catch (Exception e)
        {
            string message = "Failed to insert Position";

            _logger.LogError(e, message);

            return Error.Failure("position.add", message);
        }
    }
}