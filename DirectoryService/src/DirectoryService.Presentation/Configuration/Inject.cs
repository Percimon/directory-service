using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectoryService.Application;

namespace DirectoryService.Presentation.Configuration
{
    public static class Inject
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddOpenApiSpec()
                .InjectApplication()
                .InjectInfrastructure(configuration);
        }

        private static IServiceCollection AddOpenApiSpec(this IServiceCollection services)
        {
            services.AddControllers();

            services.AddOpenApi();

            return services;
        }
    }
}