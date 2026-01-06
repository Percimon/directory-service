using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using SharedKernel;

namespace DirectoryService.Application.Database;

public interface IDepartmentsRepository
{
    Task<Result<Department, Error>> GetById(Guid id, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> Add(Department department, CancellationToken cancellationToken);
}