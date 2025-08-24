using DirectoryService.Domain.Identifiers;
using SharedKernel;

namespace DirectoryService.Domain.Entities;

public class Position : Entity<PositionId>
{
    public Position(PositionId id) : base(id)
    {
    }
}