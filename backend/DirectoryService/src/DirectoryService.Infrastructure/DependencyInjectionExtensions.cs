using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection InjectInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AppDbContext>(_ =>
            new AppDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

        services.AddScoped<IReadDbContext, AppDbContext>(_ =>
            new AppDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

        services.Configure<SoftDeleteCleanerOptions>(configuration.GetSection("SoftDeleteCleanerOptions"));

        services.AddHostedService<SoftDeleteCleanerService>();

        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<ITransactionManager, TransactionManager>();

        services.AddScoped<ILocationsRepository, LocationsRepository>();

        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();

        services.AddScoped<IPositionsRepository, PositionsRepository>();

        return services;
    }
}