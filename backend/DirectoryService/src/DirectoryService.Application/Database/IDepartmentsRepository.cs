using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Database;

public interface IDepartmentsRepository
{
    Task<Result<Department, Error>> GetById(DepartmentId id, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithLock(DepartmentId id, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithLocations(DepartmentId id, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithPositions(DepartmentId id, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> Add(Department department, CancellationToken cancellationToken);

    Task<UnitResult<Error>> Save(CancellationToken cancellationToken);

    Task<UnitResult<Error>> DepartmentsExist(IEnumerable<DepartmentId> ids, CancellationToken cancellationToken);

    Task<UnitResult<Error>> ChangeParent(
        string rootPath,
        string newParentPath,
        Guid departmentId,
        Guid? newParentId,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> LockDescendants(string rootPath, CancellationToken cancellationToken);
}