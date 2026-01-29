using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Database;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken);
}