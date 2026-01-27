using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Exceptions;

namespace SharedService.Framework.Logging
{
    public static class LoggingExtensions
    {
        private static IServiceCollection AddSerilogLogging(
            this IServiceCollection services,
            IConfiguration configuration,
            string serviceName)
        {
            services.AddSerilog((sp, lc) => lc
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("ServiceName", serviceName));

            return services;
        }
    }
}