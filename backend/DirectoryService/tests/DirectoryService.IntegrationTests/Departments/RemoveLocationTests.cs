using DirectoryService.Application.Departments.RemoveLocation;
using DirectoryService.Domain.Identifiers;

namespace DirectoryService.IntegrationTests.Departments;

public class RemoveLocationTests : DirectoryServiceBaseTests
{
    public RemoveLocationTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task RemoveLocation_should_remove_department_location()
    {
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            dbContext.Departments.Add(DepartmentTestData.Department(departmentId, locationId));
        });

        var result = await DepartmentTestData.ExecuteAsync<RemoveLocationHandler, Guid>(Services, sut => sut.Handle(
            new RemoveLocationCommand(departmentId.Value, locationId.Value), CancellationToken.None));
        var department = await DepartmentTestData.FindAsync(Services, departmentId);

        Assert.True(result.IsSuccess);
        Assert.Equal(locationId.Value, result.Value);
        Assert.Empty(department.DepartmentLocations);
    }

    [Fact]
    public async Task RemoveLocation_should_fail_when_department_not_found()
    {
        var result = await DepartmentTestData.ExecuteAsync<RemoveLocationHandler, Guid>(Services, sut => sut.Handle(
            new RemoveLocationCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }
}
