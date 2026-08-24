using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateLocationsTests : DirectoryServiceBaseTests
{
    public UpdateLocationsTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateLocations_should_replace_department_locations()
    {
        var departmentId = DepartmentId.New();
        var firstLocationId = LocationId.New();
        var secondLocationId = LocationId.New();

        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.AddRange(
                DepartmentTestData.Location(firstLocationId, "Location_1"),
                DepartmentTestData.Location(secondLocationId, "Location_2"));
            dbContext.Departments.Add(DepartmentTestData.Department(departmentId, firstLocationId));
        });

        var result = await DepartmentTestData.ExecuteAsync<UpdateLocationsHandler, Guid>(Services, sut => sut.Handle(
            new UpdateLocationsCommand(departmentId.Value, [secondLocationId.Value]), CancellationToken.None));
        var department = await DepartmentTestData.FindAsync(Services, departmentId);

        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId.Value, result.Value);
        Assert.Equal(secondLocationId, Assert.Single(department.DepartmentLocations).LocationId);
    }
}
