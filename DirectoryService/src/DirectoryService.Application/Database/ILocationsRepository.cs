using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using SharedKernel;

namespace DirectoryService.Application.Database
{
    public interface ILocationsRepository
    {
        Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellationToken = default);
    }
}