using DirectoryService.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class SoftDeleteCleanerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SoftDeleteCleanerOptions _options;
    private readonly ILogger<SoftDeleteCleanerService> _logger;

    public SoftDeleteCleanerService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<SoftDeleteCleanerOptions> options,
        ILogger<SoftDeleteCleanerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Background SoftDeleteCleanerService started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var deleteResult = await dbContext.PurgeAllDeletedRecordsAsync(
                    _options.RetentionDays,
                    _options.BatchSize,
                    cancellationToken);

                if (deleteResult.IsFailure)
                {
                    _logger.LogError(deleteResult.Error.GetMessage());
                }
            }
        }

        await Task.CompletedTask;
    }
}