using System.Globalization;
using DirectoryService.Presentation.Configuration;
using Serilog;

var isTesting = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing"
    || AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.Contains("test"));

if (!isTesting)
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        .CreateBootstrapLogger();
}
else
{
    // Для тестов ставим заглушку или простую конфигурацию без заморозки
    Log.Logger = new LoggerConfiguration().CreateLogger();
}

try
{
    Log.Information("Starting web application..");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    string environment = builder.Environment.EnvironmentName;

    builder.Configuration.AddJsonFile($"appsetiings.{environment}", true, true);

    builder.Services.AddConfiguration(builder.Configuration);

    var app = builder.Build();

    app.Configure();

    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

namespace DirectoryService.Presentation
{
    public partial class Program;
}
