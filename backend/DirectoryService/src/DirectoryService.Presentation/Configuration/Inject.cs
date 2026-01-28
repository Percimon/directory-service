using DirectoryService.Application;
using SharedService.Framework.Logging;
using SharedService.Framework.Swagger;

namespace DirectoryService.Presentation.Configuration;

public static class Inject
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        return services
            .AddOpenApiSpec("DirectoryService", "v1")
            .AddSerilogLogging(configuration, "DirectoryService")
            .InjectApplication()
            .InjectInfrastructure(configuration);
    }
}