using DirectoryService.Application.Features.Locations.Delete;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Locations;

public class DeleteLocationTests : DirectoryServiceBaseTests
{
    public DeleteLocationTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task DeleteLocation_should_delete_location()
    {
        var locationId = LocationId.New();
        await LocationTestData.SeedAsync(Services, dbContext =>
            dbContext.Locations.Add(LocationTestData.CreateLocation(locationId)));

        var result = await LocationTestData.ExecuteAsync<DeleteLocationHandler, Guid>(Services, sut => sut.Handle(
            new DeleteLocationCommand(locationId.Value), CancellationToken.None));

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var location = await dbContext.Locations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(item => item.Id == locationId);

        Assert.True(result.IsSuccess);
        Assert.Equal(locationId.Value, result.Value);
        Assert.NotNull(location);
        Assert.False(location.IsActive);
        Assert.NotNull(location.DeletedAt);
    }

    [Fact]
    public async Task DeleteLocation_should_fail_when_location_not_found()
    {
        var result = await LocationTestData.ExecuteAsync<DeleteLocationHandler, Guid>(Services, sut => sut.Handle(
            new DeleteLocationCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }
}
