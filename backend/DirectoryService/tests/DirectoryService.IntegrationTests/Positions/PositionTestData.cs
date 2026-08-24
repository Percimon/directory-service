using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Positions;

internal static class PositionTestData
{
    public static async Task SeedAsync(IServiceProvider services, Action<AppDbContext> seed)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        seed(dbContext);
        await dbContext.SaveChangesAsync();
    }

    public static async Task<Result<TResult, Error>> ExecuteAsync<T, TResult>(
        IServiceProvider services,
        Func<T, Task<Result<TResult, Error>>> action)
        where T : notnull
    {
        await using var scope = services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<T>());
    }

    public static async Task<Position> FindAsync(IServiceProvider services, PositionId id)
    {
        await using var scope = ServicesScope(services);
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.Positions.SingleAsync(item => item.Id == id);
    }

    public static Position CreatePosition(PositionId id, string name = "Position") => new(
        id,
        Name.Create(name).Value,
        Description.Create("Position description").Value,
        DateTime.UtcNow);

    private static AsyncServiceScope ServicesScope(IServiceProvider services) => services.CreateAsyncScope();
}
