using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Presentation;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DirectoryService.IntegrationTests.BackgroundServices;

public class SoftDeleteCleanerBackgroundServiceTests : DirectoryServiceBaseTests
{
    private readonly WebApplicationFactory<Program> _factory;

    public SoftDeleteCleanerBackgroundServiceTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                var testSettings = new Dictionary<string, string?>
                {
                    ["SoftDeleteCleanerOptions:Interval"] = "00:00:01",
                    ["SoftDeleteCleanerOptions:RetentionDays"] = "30",
                    ["SoftDeleteCleanerOptions:BatchSize"] = "10",
                };

                configBuilder.AddInMemoryCollection(testSettings);
            });
        });
    }

    [Fact]
    public async Task Cleaner_should_delete_expired_soft_deleted_locations()
    {

        var locationId = LocationId.New();

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var location = Locations.LocationTestData.CreateLocation(locationId);

            location.SoftDelete();

            dbContext.Locations.Add(location);

            await dbContext.SaveChangesAsync();

            await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE locations
                SET deleted_at = {DateTime.UtcNow.AddDays(-31)}
                WHERE id = {locationId.Value}
                """);
        }

        var cleaner = _factory.Services.GetServices<IHostedService>()
            .OfType<SoftDeleteCleanerBackgroundService>()
            .Single();

        Location? deletedLocation = null;
        for (var attempt = 0; attempt < 100; attempt++)
        {
            await using var assertScope = _factory.Services.CreateAsyncScope();
            var dbContext = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
            deletedLocation = await dbContext.Locations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(location => location.Id == locationId);

            if (deletedLocation is null)
            {
                break;
            }

            await Task.Delay(50);
        }

        await cleaner.StopAsync(CancellationToken.None);

        Assert.Null(deletedLocation);
    }
}
