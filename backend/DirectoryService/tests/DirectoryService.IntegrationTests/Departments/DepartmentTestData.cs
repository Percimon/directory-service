using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedService.SharedKernel;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.IntegrationTests.Departments;

internal static class DepartmentTestData
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

    public static async Task<Department> FindAsync(IServiceProvider services, DepartmentId id)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.Departments
            .Include(item => item.DepartmentLocations)
            .Include(item => item.DepartmentPositions)
            .Include(item => item.Parent)
            .SingleAsync(item => item.Id == id);
    }

    public static Location Location(LocationId id, string name = "Location") => new(
        id,
        Name.Create(name).Value,
        Address.Create($"city-{id.Value:N}", "district", "street", "structure").Value,
        TimeZone.Create("Europe/Moscow").Value,
        DateTime.UtcNow);

    public static Position Position(PositionId id) => new(
        id,
        Name.Create("Position").Value,
        Description.Create("Position description").Value,
        DateTime.UtcNow);

    public static Department Department(
        DepartmentId id,
        LocationId locationId,
        string name = "Department",
        string slug = "department") =>
        Domain.Entities.Department.CreateParent(
            Name.Create(name).Value,
            Slug.Create(slug).Value,
            [DepartmentLocation.Create(id, locationId).Value],
            id).Value;
}
