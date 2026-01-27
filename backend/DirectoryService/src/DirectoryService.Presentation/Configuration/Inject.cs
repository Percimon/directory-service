using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectoryService.Application;
using Serilog;
using Serilog.Exceptions;

namespace DirectoryService.Presentation.Configuration;

public static class Inject
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddControllers()
            .AddOpenApiSpec()
            .AddSerilogLogging(configuration)
            .InjectApplication()
            .InjectInfrastructure(configuration);
    }
}