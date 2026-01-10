using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Positions.Create;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class Inject
{
    public static IServiceCollection InjectApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateLocationHandler>();
        services.AddScoped<CreatePositionHandler>();
        services.AddScoped<CreateDepartmentHandler>();

        services.AddValidatorsFromAssembly(typeof(Inject).Assembly);

        return services;
    }
}