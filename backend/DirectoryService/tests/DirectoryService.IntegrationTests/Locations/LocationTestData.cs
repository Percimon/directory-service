using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedService.SharedKernel;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.IntegrationTests.Locations;

internal static class LocationTestData
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

    public static async Task<Location> FindAsync(IServiceProvider services, LocationId id)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.Locations.SingleAsync(item => item.Id == id);
    }

    public static Location CreateLocation(LocationId id, string name = "Location") => new(
        id,
        Name.Create(name).Value,
        Address.Create($"city-{id.Value:N}", "district", "street", "structure").Value,
        TimeZone.Create("Europe/Moscow").Value,
        DateTime.UtcNow);

    public static Department CreateDepartment(DepartmentId id, LocationId locationId)
    {
        return Department.CreateParent(
            Name.Create("Department").Value,
            Slug.Create("department").Value,
            [DepartmentLocation.Create(id, locationId).Value],
            id).Value;
    }
}
