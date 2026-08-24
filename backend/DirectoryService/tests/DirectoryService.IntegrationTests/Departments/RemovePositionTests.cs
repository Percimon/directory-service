using DirectoryService.Application.Features.Departments.RemovePosition;
using DirectoryService.Domain.Identifiers;

namespace DirectoryService.IntegrationTests.Departments;

public class RemovePositionTests : DirectoryServiceBaseTests
{
    public RemovePositionTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task RemovePosition_should_remove_position_from_department()
    {
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        var positionId = PositionId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            dbContext.Positions.Add(DepartmentTestData.Position(positionId));
            var department = DepartmentTestData.Department(departmentId, locationId);
            department.AddPosition(positionId.Value);
            dbContext.Departments.Add(department);
        });

        var result = await DepartmentTestData.ExecuteAsync<RemovePositionHandler, Guid>(Services, sut => sut.Handle(
            new RemovePositionCommand(departmentId.Value, positionId.Value), CancellationToken.None));
        var department = await DepartmentTestData.FindAsync(Services, departmentId);

        Assert.True(result.IsSuccess);
        Assert.Equal(positionId.Value, result.Value);
        Assert.Empty(department.DepartmentPositions);
    }
}
