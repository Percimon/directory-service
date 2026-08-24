using DirectoryService.Application.Features.Locations.GetTop;
using DirectoryService.Contracts.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.IntegrationTests.Locations;

public class GetTopLocationsTests : DirectoryServiceBaseTests
{
    public GetTopLocationsTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetTop_should_return_location_with_department_count()
    {
        var locationId = LocationId.New();
        var departmentId = DepartmentId.New();
        await LocationTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(LocationTestData.CreateLocation(locationId));
            dbContext.Departments.Add(LocationTestData.CreateDepartment(departmentId, locationId));
        });

        var result = await LocationTestData.ExecuteAsync<GetLocationTopHandler, IReadOnlyList<GetLocationTopResponse>>(Services, sut => sut.Handle(
            new GetLocationTopQuery(), CancellationToken.None));

        Assert.True(result.IsSuccess);
        var location = Assert.Single(result.Value);
        Assert.Equal(locationId.Value, location.LocationId);
        Assert.Equal(1, location.DepartmentsCount);
    }

    [Fact]
    public async Task GetTop_should_return_empty_result_when_locations_are_absent()
    {
        var result = await LocationTestData.ExecuteAsync<GetLocationTopHandler, IReadOnlyList<GetLocationTopResponse>>(Services, sut => sut.Handle(
            new GetLocationTopQuery(), CancellationToken.None));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
