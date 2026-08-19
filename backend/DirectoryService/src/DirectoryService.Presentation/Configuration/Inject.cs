using DirectoryService.Application;
using Serilog;
using Serilog.Exceptions;
using SharedService.Framework.Logging;
using SharedService.Framework.Swagger;

namespace DirectoryService.Presentation.Configuration;

public static class Inject
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddHealthChecks();

        return services
            .AddOpenApi()
            .AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "DirectoryService"))
            .InjectApplication()
            .InjectInfrastructure(configuration);
    }
}