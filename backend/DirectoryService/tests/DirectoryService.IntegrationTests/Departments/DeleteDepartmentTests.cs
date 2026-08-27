using DirectoryService.Application.Features.Departments.Delete;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class DeleteDepartmentTests : DirectoryServiceBaseTests
{
    public DeleteDepartmentTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task DeleteDepartment_should_delete_department()
    {
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            dbContext.Departments.Add(DepartmentTestData.Department(departmentId, locationId));
        });

        var result = await DepartmentTestData.ExecuteAsync<DeleteDepartmentHandler, Guid>(Services, sut => sut.Handle(
            new DeleteDepartmentCommand(departmentId.Value), CancellationToken.None));
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var department = await dbContext.Departments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(item => item.Id == departmentId);

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.GetMessage() : string.Empty);
        Assert.Equal(departmentId.Value, result.Value);
        Assert.NotNull(department);
        Assert.False(department.IsActive);
        Assert.NotNull(department.DeletedAt);
    }

    [Fact]
    public async Task DeleteDepartment_should_fail_when_department_not_found()
    {
        var result = await DepartmentTestData.ExecuteAsync<DeleteDepartmentHandler, Guid>(Services, sut => sut.Handle(
            new DeleteDepartmentCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task DeleteDepartment_should_soft_delete_children_cascadingly()
    {
        var parentId = DepartmentId.New();
        var childId = DepartmentId.New();
        var locationId = LocationId.New();

        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));

            var parent = DepartmentTestData.Department(parentId, locationId, "Parent", "parent");
            var child = Department.CreateChild(
                Name.Create("Child").Value,
                Slug.Create("child").Value,
                parent,
                [DepartmentLocation.Create(childId, locationId).Value],
                childId).Value;

            dbContext.Departments.AddRange(parent, child);
        });

        var result = await DepartmentTestData.ExecuteAsync<DeleteDepartmentHandler, Guid>(Services, sut => sut.Handle(
            new DeleteDepartmentCommand(parentId.Value), CancellationToken.None));

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var departments = await dbContext.Departments
            .IgnoreQueryFilters()
            .Where(item => item.Id == parentId || item.Id == childId)
            .ToListAsync();

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.GetMessage() : string.Empty);
        Assert.Equal(2, departments.Count);
        Assert.All(departments, department =>
        {
            Assert.False(department.IsActive);
            Assert.NotNull(department.DeletedAt);
        });
    }
}
