using System.Threading.Tasks;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.IntegrationTests;

public class CreateDepartmentTests : DirectoryTestWebFactory
{
    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        LocationId id = LocationId.New();

        await using (var dbScope = Services.CreateAsyncScope())
        {
            var dbContext = dbScope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

            var location = new Location(
                id,
                Name.Create("TestLocation").Value,
                Address.Create("city", "district", "street", "structure").Value,
                TimeZone.Create("Europe/Moscow").Value,
                DateTime.UtcNow);

            dbContext.Locations.Add(location);

            await dbContext.SaveChangesAsync();
        }

        // arrange
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        var cancellationToken = CancellationToken.None;

        CreateDepartmentCommand command = new CreateDepartmentCommand(
            "DepartmentName",
            "DepName",
            null,
            [id.Value]);

        // act
        var result = await sut.Handle(command, cancellationToken);

        // asserts
        Assert.True(result.IsSuccess);

        Assert.NotEqual(Guid.Empty, result.Value);
    }
}
