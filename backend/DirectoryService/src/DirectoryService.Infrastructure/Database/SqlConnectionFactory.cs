using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DirectoryService.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DirectoryService.Infrastructure.Database;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly IConfiguration _configuration;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection Create() =>
        new NpgsqlConnection(_configuration.GetConnectionString("DirectoryServiceDb"));
}