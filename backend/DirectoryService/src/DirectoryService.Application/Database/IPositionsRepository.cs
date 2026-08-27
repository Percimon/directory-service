using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Database;

public interface IPositionsRepository
{
    Task<Result<Position, Error>> GetById(PositionId id, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken);
}