using System.Threading.Tasks;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.IntegrationTests;

public class CreateDepartmentTests : DirectoryServiceBaseTests
{
    public CreateDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        // arrange
        AppDbContext dbContext = null;

        LocationId id = LocationId.New();

        await using (var dbScope = Services.CreateAsyncScope())
        {
            dbContext = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var location = new Location(
                id,
                Name.Create("TestLocation").Value,
                Address.Create("city", "district", "street", "structure").Value,
                TimeZone.Create("Europe/Moscow").Value,
                DateTime.UtcNow);

            dbContext.Locations.Add(location);

            await dbContext.SaveChangesAsync();
        }

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler(sut =>
        {
            CreateDepartmentCommand command = new CreateDepartmentCommand(
                "DepartmentName",
                "DepName",
                null,
                [id.Value]);

            return sut.Handle(command, cancellationToken);
        });

        // asserts
        await using var assertScope = Services.CreateAsyncScope();

        dbContext = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var department = await dbContext.Departments
            .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(result.Value), cancellationToken);

        Assert.NotNull(department);

        Assert.Equal(department.Id.Value, result.Value);

        Assert.True(result.IsSuccess);

        Assert.NotEqual(Guid.Empty, result.Value);
    }

    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        return await action(sut);
    }
}
