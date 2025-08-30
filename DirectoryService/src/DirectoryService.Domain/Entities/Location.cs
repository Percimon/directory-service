using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using SharedKernel;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.Domain.Entities;

public class Location : Entity<LocationId>
{
    private bool _isActive = true;
    
    //ef core
    private Location(LocationId id) : base(id)
    {
    }
    
    public Location(
        LocationId id, 
        Name name,
        Address address,
        TimeZone timeZone,
        DateTime createdAt) : base(id)
    {
        Name = name;
        Address = address;
          TimeZone = timeZone;
        CreatedAt = createdAt;
        
        _isActive = true;
    }
    
    public Name Name { get; private set; }
    
    public Address Address { get; private set; }
    
    public TimeZone TimeZone { get; private set; }
    
    public bool IsActive => _isActive;
    
    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }
}