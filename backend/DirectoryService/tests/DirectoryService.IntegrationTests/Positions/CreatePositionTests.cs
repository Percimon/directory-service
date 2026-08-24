using DirectoryService.Application.Positions.Create;
using DirectoryService.Domain.Identifiers;
using DirectoryService.IntegrationTests.Departments;
using DirectoryService.IntegrationTests.Locations;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Positions;

public class CreatePositionTests : DirectoryServiceBaseTests
{
    public CreatePositionTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CreatePosition_with_valid_data_should_succeed()
    {
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        await PositionTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(LocationTestData.CreateLocation(locationId));
            dbContext.Departments.Add(DepartmentTestData.Department(departmentId, locationId));
        });

        var result = await PositionTestData.ExecuteAsync<CreatePositionHandler, Guid>(Services, sut => sut.Handle(
            new CreatePositionCommand("Created position", "Position description", [departmentId.Value]),
            CancellationToken.None));
        var position = await PositionTestData.FindAsync(Services, PositionId.Create(result.Value));

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Equal("Created position", position.Name.Value);
        Assert.Contains(position.Id.Value, await GetDepartmentPositionIdsAsync(departmentId));
    }

    [Fact]
    public async Task CreatePosition_should_fail_when_department_not_found()
    {
        var result = await PositionTestData.ExecuteAsync<CreatePositionHandler, Guid>(Services, sut => sut.Handle(
            new CreatePositionCommand("Created position", "Position description", [Guid.NewGuid()]),
            CancellationToken.None));

        Assert.True(result.IsFailure);
    }

    private async Task<IReadOnlyList<Guid>> GetDepartmentPositionIdsAsync(DepartmentId departmentId)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryService.Infrastructure.Database.AppDbContext>();
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .ToListAsync(dbContext.DepartmentPositions
                .Where(item => item.DepartmentId == departmentId)
                .Select(item => item.PositionId.Value));
    }
}
