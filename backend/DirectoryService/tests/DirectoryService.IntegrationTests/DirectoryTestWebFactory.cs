using System;
using System.Data.Common;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Presentation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests;

public class DirectoryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("directory_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner;

    private DbConnection _dbConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<IReadDbContext>();

            services.AddScoped<AppDbContext>(_ =>
                new AppDbContext(_dbContainer.GetConnectionString()));

            services.AddScoped<IReadDbContext>(_ =>
                new AppDbContext(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureDeletedAsync();

        await dbContext.Database.EnsureCreatedAsync();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());

        await _dbConnection.OpenAsync();

        await InitializeRespawner();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();

        await _dbContainer.DisposeAsync();

        await _dbConnection.CloseAsync();

        await _dbConnection.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    private async Task InitializeRespawner()
    {
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions()
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
            });
    }
}
