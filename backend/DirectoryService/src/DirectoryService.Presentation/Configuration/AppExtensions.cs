using Scalar.AspNetCore;
using Serilog;
using SharedService.Framework.Middlewares;

namespace DirectoryService.Presentation.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseCors(builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });

        app.UseExceptionMiddleware();
        app.UseRequestCorrelationId();
        app.UseSerilogRequestLogging();

        app.MapControllers();

        app.AddScalar();

        app.MapHealthChecks("/health");

        return app;
    }

    private static WebApplication AddScalar(this WebApplication app)
    {
        app.MapOpenApi();

        app.MapScalarApiReference();

        return app;
    }
}