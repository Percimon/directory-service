using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class Inject
{
    public static IServiceCollection InjectInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DirectoryServiceDbContext>(_ =>
            new DirectoryServiceDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<ILocationsRepository, LocationsRepository>();

        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();

        services.AddScoped<IPositionsRepository, PositionsRepository>();

        return services;
    }
}