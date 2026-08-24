using DirectoryService.Application.Departments.ChangeParent;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.IntegrationTests.Departments;

public class ChangeParentTests : DirectoryServiceBaseTests
{
    public ChangeParentTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task ChangeParent_should_update_department_parent_and_path()
    {
        var parentId = DepartmentId.New();
        var newParentId = DepartmentId.New();
        var childId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            var parent = DepartmentTestData.Department(parentId, locationId, "Parent", "parent");
            var newParent = DepartmentTestData.Department(newParentId, locationId, "New parent", "newparent");
            var child = Department.CreateChild(Name.Create("Child").Value, Slug.Create("child").Value, parent,
                [DepartmentLocation.Create(childId, locationId).Value], childId).Value;
            dbContext.Departments.AddRange(parent, newParent, child);
        });

        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, Guid>(Services, sut => sut.Handle(
            new ChangeParentCommand(childId.Value, newParentId.Value), CancellationToken.None));
        var child = await DepartmentTestData.FindAsync(Services, childId);

        Assert.True(result.IsSuccess);
        Assert.Equal(childId.Value, result.Value);
        Assert.Equal(newParentId, child.Parent.Id);
        Assert.Equal("newparent.child", child.Path.Value);
    }

    [Fact]
    public async Task ChangeParent_should_fail_when_department_not_found()
    {
        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, Guid>(Services, sut => sut.Handle(
            new ChangeParentCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }
}
