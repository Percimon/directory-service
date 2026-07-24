namespace DirectoryService.IntegrationTests;

public class DirectoryServiceBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    protected IServiceProvider Services { get; }

    private readonly Func<Task> _resetDatabase;

    public DirectoryServiceBaseTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}
