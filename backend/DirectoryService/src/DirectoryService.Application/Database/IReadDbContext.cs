using DirectoryService.Contracts.Dtos;
using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Database;

public interface IReadDbContext
{
    IQueryable<Location> LocationsRead { get; }

    IQueryable<Department> DepartmentsRead { get; }
}