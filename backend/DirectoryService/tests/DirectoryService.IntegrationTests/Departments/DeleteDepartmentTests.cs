using DirectoryService.Application.Features.Departments.Delete;
using DirectoryService.Domain.Identifiers;
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
        var department = await dbContext.Departments.FirstOrDefaultAsync(item => item.Id == departmentId);

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : string.Empty);
        Assert.Equal(departmentId.Value, result.Value);
        Assert.Null(department);
    }
}
