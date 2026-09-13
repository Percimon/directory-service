using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedService.Core.Extensions;

namespace DirectoryService.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection InjectApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjectionExtensions).Assembly);

        services.AddCommands(typeof(DependencyInjectionExtensions).Assembly);

        services.AddQueries(typeof(DependencyInjectionExtensions).Assembly);

        return services;
    }
}