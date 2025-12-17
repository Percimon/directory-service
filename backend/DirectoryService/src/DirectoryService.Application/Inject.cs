using DirectoryService.Application.Locations.Create;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application
{
    public static class Inject
    {
        public static IServiceCollection InjectApplication(this IServiceCollection services)
        {
            services.AddScoped<CreateLocationHandler>();

            services.AddValidatorsFromAssembly(typeof(Inject).Assembly);

            return services;
        }
    }
}