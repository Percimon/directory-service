using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectoryService.Application;
using Serilog;
using Serilog.Exceptions;

namespace DirectoryService.Presentation.Configuration
{
    public static class Inject
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddOpenApiSpec()
                .AddSerilogLogging(configuration)
                .InjectApplication()
                .InjectInfrastructure(configuration);
        }

        private static IServiceCollection AddOpenApiSpec(this IServiceCollection services)
        {
            services.AddControllers();

            services.AddOpenApi();

            return services;
        }

        private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSerilog((sp, lc) => lc
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("ServiceName", "DirectoryService"));

            return services;
        }
    }
}