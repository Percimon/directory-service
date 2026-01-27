using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace SharedService.Framework.Swagger
{
    public static class OpenApiExtensions
    {
        public static IServiceCollection AddOpenApiSpec(
            this IServiceCollection services,
            string title,
            string version)
        {
            services.AddOpenApi();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(version, new OpenApiInfo
                {
                    Title = title,
                    Version = version,
                    Contact = new OpenApiContact
                    {
                        Name = "Name",
                        Email = "email@gmail.com",
                    },
                });
            });

            return services;
        }
    }
}