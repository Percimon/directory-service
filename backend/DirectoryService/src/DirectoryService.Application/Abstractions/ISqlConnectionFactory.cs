using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DirectoryService.Application.Abstractions;

public interface ISqlConnectionFactory
{
    IDbConnection Create();
}