using DirectoryService.Application.Features.Locations.GetById;
using DirectoryService.Contracts.Responses;
using DirectoryService.Domain.Identifiers;

namespace DirectoryService.IntegrationTests.Locations;

public class GetLocationByIdTests : DirectoryServiceBaseTests
{
    public GetLocationByIdTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetById_should_return_location()
    {
        var locationId = LocationId.New();
        await LocationTestData.SeedAsync(Services, dbContext =>
            dbContext.Locations.Add(LocationTestData.CreateLocation(locationId, "Read location")));

        var result = await LocationTestData.ExecuteAsync<GetLocationByIdHandler, GetLocationResponse>(Services, sut => sut.Handle(
            new GetLocationByIdQuery(locationId.Value), CancellationToken.None));

        Assert.True(result.IsSuccess);
        Assert.Equal(locationId.Value, result.Value.Id);
        Assert.Equal("Read location", result.Value.Name);
        Assert.Equal("Europe/Moscow", result.Value.TimeZone);
    }

    [Fact]
    public async Task GetById_should_fail_when_location_not_found()
    {
        var result = await LocationTestData.ExecuteAsync<GetLocationByIdHandler, GetLocationResponse>(Services, sut => sut.Handle(
            new GetLocationByIdQuery(Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }
}
