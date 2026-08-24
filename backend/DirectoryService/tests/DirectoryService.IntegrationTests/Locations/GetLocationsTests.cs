using DirectoryService.Application.Features.Locations.Get;
using DirectoryService.Contracts.Responses;
using DirectoryService.Domain.Identifiers;

namespace DirectoryService.IntegrationTests.Locations;

public class GetLocationsTests : DirectoryServiceBaseTests
{
    public GetLocationsTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetLocations_should_filter_by_search()
    {
        var firstId = LocationId.New();
        var secondId = LocationId.New();
        await LocationTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(LocationTestData.CreateLocation(firstId, "Accounting office"));
            dbContext.Locations.Add(LocationTestData.CreateLocation(secondId, "Engineering office"));
        });

        var result = await LocationTestData.ExecuteAsync<GetLocationsHandler, PagedList<LocationListItemDto>>(Services, sut => sut.Handle(
            new GetLocationsQuery(1, 10, "name", "asc", 0, "Accounting"), CancellationToken.None));

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(firstId.Value, item.Id);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetLocations_should_fail_when_page_is_invalid()
    {
        var result = await LocationTestData.ExecuteAsync<GetLocationsHandler, PagedList<LocationListItemDto>>(Services, sut => sut.Handle(
            new GetLocationsQuery(0, 10, "name", "asc", 0, null), CancellationToken.None));

        Assert.True(result.IsFailure);
    }
}
