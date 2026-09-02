using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;
using SharedService.Core.Abstractions;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Departments.GetChildren;

public sealed record GetDepartmentChildrenQuery(Guid DepartmentId) : IQuery;

public sealed class GetDepartmentChildrenHandler
    : IQueryHandler<IReadOnlyList<DepartmentTreeDto>, GetDepartmentChildrenQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentChildrenHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<IReadOnlyList<DepartmentTreeDto>, Error>> Handle(
        GetDepartmentChildrenQuery request,
        CancellationToken cancellationToken)
    {
        var departmentExists = await _readDbContext.DepartmentsRead
            .AnyAsync(department => department.Id == request.DepartmentId, cancellationToken);

        if (!departmentExists)
        {
            return GeneralErrors.NotFound(request.DepartmentId);
        }

        return await _readDbContext.DepartmentsRead
            .Where(department => department.Parent != null && department.Parent.Id == request.DepartmentId)
            .OrderBy(department => department.Name.Value)
            .Select(department => new DepartmentTreeDto(
                department.Id.Value,
                department.Parent!.Id.Value,
                department.Name.Value,
                department.Slug.Value,
                department.Path.Value,
                department.Depth.Value,
                department.Children.Any(),
                department.Children.Count()))
            .ToListAsync(cancellationToken);
    }
}