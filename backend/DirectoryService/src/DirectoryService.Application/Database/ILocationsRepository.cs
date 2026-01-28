using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Database;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellationToken);

    UnitResult<Error> LocationExists(LocationId id);

    Task<UnitResult<Error>> LocationsExist(IEnumerable<LocationId> ids, CancellationToken cancellationToken);
}