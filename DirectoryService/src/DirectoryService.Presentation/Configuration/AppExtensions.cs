namespace DirectoryService.Presentation.Configuration
{
    public static class AppExtensions
    {
        public static IApplicationBuilder Configure(this WebApplication app)
        {
            app.AddSwagger();

            app.MapControllers();

            return app;
        }

        private static WebApplication AddSwagger(this WebApplication app)
        {
            app.MapOpenApi();

            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Directory Service"));

            return app;
        }
    }
}