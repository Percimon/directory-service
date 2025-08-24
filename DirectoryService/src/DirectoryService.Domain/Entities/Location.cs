using DirectoryService.Domain.Identifiers;
using SharedKernel;

namespace DirectoryService.Domain.Entities;

public class Location : Entity<LocationId>
{
    public Location(LocationId id) : base(id)
    {
    }
}