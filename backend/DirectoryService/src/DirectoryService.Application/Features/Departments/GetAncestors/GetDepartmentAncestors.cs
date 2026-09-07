using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Dtos;
using SharedService.Core.Abstractions;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Departments.GetAncestors;

public sealed record GetDepartmentAncestorsQuery(Guid DepartmentId) : IQuery;

public sealed class GetDepartmentAncestorsHandler
    : IQueryHandler<IReadOnlyList<DepartmentTreeDto>, GetDepartmentAncestorsQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetDepartmentAncestorsHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<DepartmentTreeDto>, Error>> Handle(
        GetDepartmentAncestorsQuery request,
        CancellationToken cancellationToken)
    {
        const string pathSql =
            """
            SELECT path::text
            FROM departments
            WHERE id = @DepartmentId AND is_active = true;
            """;

        const string ancestorsSql =
            """
            SELECT
                ancestor.id AS Id,
                ancestor.fk_parent_id AS ParentId,
                ancestor.name AS Name,
                ancestor.slug AS Slug,
                ancestor.path::text AS Path,
                ancestor.depth AS Depth,
                EXISTS (
                    SELECT 1
                    FROM departments child
                    WHERE child.fk_parent_id = ancestor.id
                      AND child.is_active = true) AS HasChildren,
                (
                    SELECT COUNT(*)::int
                    FROM departments child
                    WHERE child.fk_parent_id = ancestor.id
                      AND child.is_active = true) AS ChildrenCount
            FROM departments ancestor
            WHERE ancestor.is_active = true
              AND ancestor.path @> @DepartmentPath::ltree
              AND ancestor.path <> @DepartmentPath::ltree
            ORDER BY ancestor.depth;
            """;

        using var connection = _sqlConnectionFactory.Create();
        var parameters = new
        {
            DepartmentId = request.DepartmentId,
        };
        var departmentPath = await connection.QuerySingleOrDefaultAsync<string>(pathSql, parameters);

        if (departmentPath is null)
        {
            return GeneralErrors.NotFound(request.DepartmentId);
        }

        var ancestors = await connection.QueryAsync<DepartmentTreeDto>(
            ancestorsSql,
            new { DepartmentPath = departmentPath });

        return ancestors.ToList();
    }
}