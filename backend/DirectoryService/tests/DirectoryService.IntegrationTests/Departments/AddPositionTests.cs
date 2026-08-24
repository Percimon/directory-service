using DirectoryService.Application.Features.Departments.AddPosition;
using DirectoryService.Domain.Identifiers;

namespace DirectoryService.IntegrationTests.Departments;

public class AddPositionTests : DirectoryServiceBaseTests
{
    public AddPositionTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task AddPosition_should_add_position_to_department()
    {
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        var positionId = PositionId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            dbContext.Positions.Add(DepartmentTestData.Position(positionId));
            dbContext.Departments.Add(DepartmentTestData.Department(departmentId, locationId));
        });

        var result = await DepartmentTestData.ExecuteAsync<AddPositionHandler, Guid>(Services, sut => sut.Handle(
            new AddPositionCommand(departmentId.Value, positionId.Value), CancellationToken.None));
        var department = await DepartmentTestData.FindAsync(Services, departmentId);

        Assert.True(result.IsSuccess);
        Assert.Equal(positionId.Value, result.Value);
        Assert.Contains(department.DepartmentPositions, item => item.PositionId == positionId);
    }
}
