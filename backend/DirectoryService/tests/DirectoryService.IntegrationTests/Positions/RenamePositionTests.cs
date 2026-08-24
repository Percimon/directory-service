using DirectoryService.Application.Features.Positions.Rename;
using DirectoryService.Domain.Identifiers;

namespace DirectoryService.IntegrationTests.Positions;

public class RenamePositionTests : DirectoryServiceBaseTests
{
    public RenamePositionTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task RenamePosition_should_update_position_name()
    {
        var positionId = PositionId.New();
        await PositionTestData.SeedAsync(Services, dbContext =>
            dbContext.Positions.Add(PositionTestData.CreatePosition(positionId)));

        var result = await PositionTestData.ExecuteAsync<RenamePositionHandler, Guid>(Services, sut => sut.Handle(
            new RenamePositionCommand(positionId.Value, "Renamed position"), CancellationToken.None));
        var position = await PositionTestData.FindAsync(Services, positionId);

        Assert.True(result.IsSuccess);
        Assert.Equal(positionId.Value, result.Value);
        Assert.Equal("Renamed position", position.Name.Value);
    }
}
