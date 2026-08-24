using DirectoryService.Domain.Identifiers;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Locations;

public class UpdateLocationTests : DirectoryServiceBaseTests
{
    public UpdateLocationTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateLocation_should_update_location_data()
    {
        var locationId = LocationId.New();
        await LocationTestData.SeedAsync(Services, dbContext =>
            dbContext.Locations.Add(LocationTestData.CreateLocation(locationId)));

        var result = await LocationTestData.ExecuteAsync<UpdateLocationHandler, Guid>(Services, sut => sut.Handle(
            new UpdateLocationCommand(locationId.Value, "Updated location", "new city", "new district", "new street", "new structure", "UTC"),
            CancellationToken.None));
        var location = await LocationTestData.FindAsync(Services, locationId);

        Assert.True(result.IsSuccess);
        Assert.Equal(locationId.Value, result.Value);
        Assert.Equal("Updated location", location.Name.Value);
        Assert.Equal("new city", location.Address.City);
        Assert.Equal("UTC", location.TimeZone.Value);
    }
}
