using DirectoryService.Application.Locations.Create;
using DirectoryService.Domain.Identifiers;

namespace DirectoryService.IntegrationTests.Locations;

public class CreateLocationTests : DirectoryServiceBaseTests
{
    public CreateLocationTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateLocation_with_valid_data_should_succeed()
    {
        var result = await LocationTestData.ExecuteAsync<CreateLocationHandler, Guid>(Services, sut => sut.Handle(
            new CreateLocationCommand("Created location", "city", "district", "street", "structure", "Europe/Moscow"),
            CancellationToken.None));

        var location = await LocationTestData.FindAsync(Services, LocationId.Create(result.Value));

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Equal("Created location", location.Name.Value);
    }
}
