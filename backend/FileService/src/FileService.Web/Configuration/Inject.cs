using DirectoryService.Application;
using SharedService.Framework.Logging;
using SharedService.Framework.Swagger;

namespace FileService.Web.Configuration;

public static class Inject
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        return services
            .AddOpenApiSpec("FileService", "v1")
            .AddSerilogLogging(configuration, "FileService")
            .InjectApplication()
            .InjectInfrastructure(configuration);
    }
}