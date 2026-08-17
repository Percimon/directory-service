// using System.Threading.Tasks;
// using CSharpFunctionalExtensions;
// using Dapper;
// using DirectoryService.Application.Abstractions;
// using DirectoryService.Application.Database;
// using DirectoryService.Contracts.Requests;
// using DirectoryService.Domain.Entities;
// using DirectoryService.Domain.Identifiers;
// using DirectoryService.Domain.ValueObjects;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Npgsql;
// using SharedService.SharedKernel;
// using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

// namespace DirectoryService.Infrastructure.Repositories;

// public class LocationsNpgSqlRepository : ILocationsRepository
// {
//     private readonly ISqlConnectionFactory _sqlConnectionFactory;
//     private readonly ILogger<LocationsNpgSqlRepository> _logger;

//     public LocationsNpgSqlRepository(
//         ISqlConnectionFactory sqlConnectionFactory,
//         ILogger<LocationsNpgSqlRepository> logger)
//     {
//         _sqlConnectionFactory = sqlConnectionFactory;
//         _logger = logger;
//     }

//     public async Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellationToken)
//     {
//         try
//         {
//             using (var connection = _sqlConnectionFactory.Create())
//             {
//                 string sql =
//                     """
//                     INSERT INTO locations (id, name, city, district, street, structure, timezone, is_active, created_at, updated_at)
//                     values (@Id, @Name, @City, @District, @Street, @Structure, @Timezone, @IsActive, @CreatedAt, @UpdatedAt)
//                     """;

//                 var addLocationsParams = new
//                 {
//                     Id = location.Id.Value,
//                     Name = location.Name.Value,
//                     City = location.Address.City,
//                     District = location.Address.District,
//                     Street = location.Address.Street,
//                     Structure = location.Address.Structure,
//                     IsActive = true,
//                     Timezone = location.TimeZone.Value,
//                     CreatedAt = location.CreatedAt,
//                     UpdatedAt = location.UpdatedAt,
//                 };

//                 await connection.ExecuteAsync(sql, addLocationsParams);
//             }

//             return location.Id.Value;
//         }
//         catch (PostgresException pgEx)
//         {
//             if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null })
//             {
//                 if (pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
//                 {
//                     return GeneralErrors.AlreadyExists(nameof(Location), nameof(Location.Name), location.Name.Value);
//                 }
//                 else if (pgEx.ConstraintName.Contains("address", StringComparison.InvariantCultureIgnoreCase))
//                 {
//                     return GeneralErrors.AlreadyExists(nameof(Location), nameof(Location.Address), location.Address.ToString());
//                 }
//             }

//             _logger.LogError(pgEx, "Database update error while creating location with name: {name}", location.Name.Value);

//             return GeneralErrors.Failure("Database update error while creating location");
//         }
//         catch (Exception e)
//         {
//             string message = "Failed to insert location";

//             _logger.LogError(e, message);

//             return Error.Failure("location.insert", message);
//         }
//     }

//     public async Task<Result<Location, Error>> GetById(LocationId id, CancellationToken cancellationToken)
//     {
//         try
//         {
//             using (var connection = _sqlConnectionFactory.Create())
//             {
//                 string sql =
//                     """
//                     SELECT 
//                         id,
//                         name,
//                         city,
//                         district,
//                         street,
//                         structure,
//                         timeZone,
//                         is_active,
//                         created_at,
//                         updated_at
//                     FROM locations
//                     WHERE id = @Id AND is_active = true;
//                     """;

//                 var nameParams = new
//                 {
//                     Id = id.Value,
//                 };

//                 var result = await connection.ExecuteScalarAsync<LocationDto>(sql, nameParams);

//                 var name = Name.Create(result.Name).Value;

//                 var address = Address.Create(result.City, result.District, result.Street, result.Structure).Value;

//                 var timeZone = TimeZone.Create(result.TimeZone).Value;

//                 var location = new Location(id, name, address, timeZone, result.CreatedAt);

//                 return location;
//             }
//         }
//         catch (Exception e)
//         {
//             _logger.LogError(e, "Database error quering location with Id={Id}", id.Value);

//             return GeneralErrors.Failure("Database error quering location with Id=" + id.Value);
//         }
//     }

//     public async Task<UnitResult<Error>> LocationExists(LocationId id, CancellationToken cancellationToken = default)
//     {
//         using (var connection = _sqlConnectionFactory.Create())
//         {
//             string sql =
//                 """
//                 SELECT COUNT(*) AS total_count
//                 FROM locations
//                 WHERE id = @Id AND is_active = true;
//                 """;

//             var nameParams = new
//             {
//                 Id = id.Value,
//             };

//             int result = await connection.ExecuteScalarAsync<int>(sql, nameParams);

//             return result > 0
//                 ? UnitResult.Success<Error>()
//                 : GeneralErrors.NotFound(id.Value);
//         }
//     }

//     public async Task<UnitResult<Error>> LocationNameExists(Name name, CancellationToken cancellationToken = default)
//     {
//         using (var connection = _sqlConnectionFactory.Create())
//         {
//             string sql =
//                 """
//                 SELECT COUNT(*) AS total_count
//                 FROM locations
//                 WHERE name = @Name AND is_active = true;
//                 """;

//             var nameParams = new
//             {
//                 Name = name.Value,
//             };

//             int result = await connection.ExecuteScalarAsync<int>(sql, nameParams);

//             return result > 0
//                 ? GeneralErrors.AlreadyExists(name.Value)
//                 : UnitResult.Success<Error>();
//         }
//     }

//     public async Task<UnitResult<Error>> LocationsExist(IEnumerable<LocationId> ids, CancellationToken cancellationToken = default)
//     {
//         var idList = ids
//             .Select(id => id.Value)
//             .ToArray();

//         if (idList.Length == 0)
//             return Result.Success<Error>(); // Пустой список — формально все есть

//         using (var connection = _sqlConnectionFactory.Create())
//         {
//             string sql =
//                 """
//                 SELECT (COUNT(DISTINCT id) = @ExpectedCount) 
//                 FROM locations 
//                 WHERE id = ANY(@Ids) AND is_active = true;
//                 """;

//             bool result = await connection.ExecuteScalarAsync<bool>(sql, new { Ids = idList, ExpectedCount = idList.Length });

//             return result
//                 ? UnitResult.Success<Error>()
//                 : Error.NotFound("location.id", "One of location ids were not found");
//         }
//     }
// }
