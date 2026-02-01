using System.Globalization;
using FileService.Web.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

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