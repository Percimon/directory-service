using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using SharedKernel;

namespace DirectoryService.Application.Database;

public interface IDepartmentsRepository
{
    Task<Result<Department, Error>> GetById(DepartmentId id, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> Add(Department department, CancellationToken cancellationToken);

    Task<UnitResult<Error>> Save(CancellationToken cancellationToken);
}