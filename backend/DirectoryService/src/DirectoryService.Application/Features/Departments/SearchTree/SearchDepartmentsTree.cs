using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Dtos;
using FluentValidation;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Departments.SearchTree;

public sealed record SearchDepartmentsTreeQuery(string Query) : IQuery;

public sealed class SearchDepartmentsTreeQueryValidator : AbstractValidator<SearchDepartmentsTreeQuery>
{
    public SearchDepartmentsTreeQueryValidator()
    {
        RuleFor(query => query.Query)
            .Must(query => !string.IsNullOrWhiteSpace(query) && query.Trim().Length >= 2)
            .WithError(Error.Validation("department.tree.search", "Search query must contain at least 2 characters."));
    }
}

public sealed class SearchDepartmentsTreeHandler
    : IQueryHandler<IReadOnlyList<DepartmentTreeDto>, SearchDepartmentsTreeQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<SearchDepartmentsTreeQuery> _validator;

    public SearchDepartmentsTreeHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IValidator<SearchDepartmentsTreeQuery> validator)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _validator = validator;
    }

    public async Task<Result<IReadOnlyList<DepartmentTreeDto>, Error>> Handle(
        SearchDepartmentsTreeQuery request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        const string sql =
            """
            SELECT
                department.id AS Id,
                department.fk_parent_id AS ParentId,
                department.name AS Name,
                department.slug AS Slug,
                department.path::text AS Path,
                department.depth AS Depth,
                EXISTS (
                    SELECT 1
                    FROM departments child
                    WHERE child.fk_parent_id = department.id
                      AND child.is_active = true) AS HasChildren,
                (
                    SELECT COUNT(*)::int
                    FROM departments child
                    WHERE child.fk_parent_id = department.id
                      AND child.is_active = true) AS ChildrenCount
            FROM departments department
            WHERE department.is_active = true
              AND department.name ILIKE '%' || @Query || '%'
            ORDER BY department.path;
            """;

        using var connection = _sqlConnectionFactory.Create();
        var departments = await connection.QueryAsync<DepartmentTreeDto>(
            sql,
            new { Query = request.Query.Trim() });

        return departments.ToList();
    }
}