using System.Data;

namespace DirectoryService.Application.Abstractions;

public interface ISqlConnectionFactory
{
    IDbConnection Create();
}