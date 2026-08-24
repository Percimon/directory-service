using DirectoryService.Application.Departments.Update;
using DirectoryService.Domain.Identifiers;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentTests : DirectoryServiceBaseTests
{
    public UpdateDepartmentTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateDepartment_should_update_name_and_slug()
    {
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            dbContext.Departments.Add(DepartmentTestData.Department(departmentId, locationId));
        });

        var result = await DepartmentTestData.ExecuteAsync<UpdateDepartmentHandler, Guid>(Services, sut => sut.Handle(
            new UpdateDepartmentCommand(departmentId.Value, "Updated department", "updateddepartment"), CancellationToken.None));
        var department = await DepartmentTestData.FindAsync(Services, departmentId);

        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId.Value, result.Value);
        Assert.Equal("Updated department", department.Name.Value);
        Assert.Equal("updateddepartment", department.Slug.Value);
    }

    [Fact]
    public async Task UpdateDepartment_should_fail_when_department_not_found()
    {
        var result = await DepartmentTestData.ExecuteAsync<UpdateDepartmentHandler, Guid>(Services, sut => sut.Handle(
            new UpdateDepartmentCommand(Guid.NewGuid(), "Updated department", "updateddepartment"), CancellationToken.None));

        Assert.True(result.IsFailure);
    }
}
