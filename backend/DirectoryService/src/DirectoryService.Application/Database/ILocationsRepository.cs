using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Database;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellationToken);

    Task<UnitResult<Error>> LocationNameExists(Name name, CancellationToken cancellationToken);

    Task<UnitResult<Error>> LocationExists(LocationId id, CancellationToken cancellationToken);

    Task<UnitResult<Error>> LocationsExist(IEnumerable<LocationId> ids, CancellationToken cancellationToken);
}