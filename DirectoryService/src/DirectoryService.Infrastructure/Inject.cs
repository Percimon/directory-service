using DirectoryService.Infrastructure;
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

        return services;
    }
}