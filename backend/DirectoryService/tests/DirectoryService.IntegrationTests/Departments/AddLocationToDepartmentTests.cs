using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Features.Departments.AddLocation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedService.SharedKernel;
using Path = DirectoryService.Domain.ValueObjects.Path;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.IntegrationTests.Departments;

public class AddLocationToDepartmentTests : DirectoryServiceBaseTests
{
    public AddLocationToDepartmentTests(DirectoryTestWebFactory factory)
           : base(factory)
    {
    }

    [Fact]
    public async Task AddLocation_should_succeed()
    {
        // arrange
        AppDbContext dbContext = null;

        var locationId = LocationId.New();

        var departmentId = DepartmentId.New();

        string nameForTestLocation = "TestLocation_2";

        await using (var dbScope = Services.CreateAsyncScope())
        {
            dbContext = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var location = new Location(
                locationId,
                Name.Create("TestLocation_1").Value,
                Address.Create("city_1", "district", "street", "structure").Value,
                TimeZone.Create("Europe/Moscow").Value,
                DateTime.UtcNow);

            dbContext.Locations.Add(location);

            var department = Department.CreateParent(
                Name.Create("TestDepartment").Value,
                Slug.Create("dep").Value,
                new List<DepartmentLocation> { DepartmentLocation.Create(departmentId, locationId).Value },
                departmentId).Value;

            dbContext.Departments.Add(department);

            locationId = LocationId.New();

            location = new Location(
                locationId,
                Name.Create(nameForTestLocation).Value,
                Address.Create("city_2", "district", "street", "structure").Value,
                TimeZone.Create("Europe/Moscow").Value,
                DateTime.UtcNow);

            dbContext.Locations.Add(location);

            await dbContext.SaveChangesAsync();
        }

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler(sut =>
        {
            AddLocationCommand command = new AddLocationCommand(
                departmentId.Value,
                locationId.Value);

            return sut.Handle(command, cancellationToken);
        });

        // asserts
        await using var assertScope = Services.CreateAsyncScope();

        dbContext = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var departmentResult = await dbContext
            .Departments
            .Include(d => d.DepartmentLocations)
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(locationId.Value, result.Value);
        Assert.NotNull(departmentResult);
        Assert.Equal(2, departmentResult.DepartmentLocations.Count);
        Assert.Contains(departmentResult.DepartmentLocations, l => l.LocationId == locationId);
    }

    [Fact]
    public async Task AddLocation_should_fail_when_department_not_found()
    {
        var result = await ExecuteHandler(sut => sut.Handle(
            new AddLocationCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<AddLocationHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<AddLocationHandler>();

        return await action(sut);
    }
}
