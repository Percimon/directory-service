using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/data-seed")]
public class DataSeedController : ControllerBase
{
    [HttpPost("departments")]
    public async Task<ActionResult<int>> SeedDepartments(
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        const int totalDepartments = 10_000;
        const int maxDepth = 5;
        const int rootCount = 10;

        static string Letters(int value)
        {
            var result = string.Empty;
            do
            {
                result = (char)('a' + value % 26) + result;
                value = value / 26 - 1;
            }
            while (value >= 0);

            return result;
        }

        var runToken = Letters(Guid.NewGuid().GetHashCode() & int.MaxValue);
        var locationId = LocationId.New();
        var location = new Location(
            locationId,
            Name.Create($"Seed Location {runToken}").Value,
            Address.Create($"Seed City {runToken}", "Seed District", "Seed Street", $"Seed {runToken}").Value,
            TimeZone.Create("Europe/Moscow").Value,
            DateTime.UtcNow);

        var departmentsByDepth = new List<List<Department>>(maxDepth + 1);
        var departments = new List<Department>(totalDepartments);
        dbContext.Locations.Add(location);

        var departmentsAtDepth = rootCount;
        for (var depth = 0; depth <= maxDepth; depth++)
        {
            var currentLevel = new List<Department>(departmentsAtDepth);
            var parentLevel = depth == 0 ? null : departmentsByDepth[depth - 1];

            for (var index = 0; index < departmentsAtDepth; index++)
            {
                var departmentNumber = departments.Count;
                var departmentId = DepartmentId.New();
                var slug = Slug.Create($"seed{runToken}{Letters(departmentNumber)}").Value;
                var name = Name.Create($"Seed Department {runToken} {departmentNumber}").Value;
                var departmentLocation = DepartmentLocation.Create(
                    departmentId,
                    locationId).Value;

                var department = depth == 0
                    ? Department.CreateParent(name, slug, [departmentLocation], departmentId).Value
                    : Department.CreateChild(name, slug, parentLevel![index % parentLevel.Count], [departmentLocation], departmentId).Value;

                currentLevel.Add(department);
                departments.Add(department);
            }

            departmentsByDepth.Add(currentLevel);
            if (depth < maxDepth)
            {
                departmentsAtDepth = (totalDepartments - departments.Count) / (maxDepth - depth);
            }
        }

        dbContext.Departments.AddRange(departments);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(departments.Count);
    }
}
