using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;
using SharedService.Core.Abstractions;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Departments.GetTree;

public sealed record GetDepartmentsTreeQuery : IQuery;

public sealed class GetDepartmentsTreeHandler
    : IQueryHandler<IReadOnlyList<DepartmentTreeDto>, GetDepartmentsTreeQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentsTreeHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<IReadOnlyList<DepartmentTreeDto>, Error>> Handle(
        GetDepartmentsTreeQuery request,
        CancellationToken cancellationToken)
    {
        return await _readDbContext.DepartmentsRead
            .Where(department => department.Parent == null)
            .OrderBy(department => department.Name.Value)
            .Select(department => new DepartmentTreeDto(
                department.Id.Value,
                department.Name.Value,
                department.Slug.Value,
                department.Path.Value,
                department.Depth.Value,
                department.Children.Any(),
                department.Children.Count()))
            .ToListAsync(cancellationToken);
    }
}