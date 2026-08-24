using DirectoryService.Application.Features.Positions.Delete;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Positions;

public class DeletePositionTests : DirectoryServiceBaseTests
{
    public DeletePositionTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task DeletePosition_should_delete_position()
    {
        var positionId = PositionId.New();
        await PositionTestData.SeedAsync(Services, dbContext =>
            dbContext.Positions.Add(PositionTestData.CreatePosition(positionId)));

        var result = await PositionTestData.ExecuteAsync<DeletePositionHandler, Guid>(Services, sut => sut.Handle(
            new DeletePositionCommand(positionId.Value), CancellationToken.None));

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var position = await dbContext.Positions.FirstOrDefaultAsync(item => item.Id == positionId);

        Assert.True(result.IsSuccess);
        Assert.Equal(positionId.Value, result.Value);
        Assert.Null(position);
    }
}
