using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Dtos;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Departments.GetById;

public sealed record GetByIdDepartmentQuery(Guid Id) : IQuery;

public sealed class GetByIdDepartmentQueryValidator : AbstractValidator<GetByIdDepartmentQuery>
{
    public GetByIdDepartmentQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Department id is required.");
    }
}

public sealed class GetDepartmentByIdHandler : IQueryHandler<DepartmentDto, GetByIdDepartmentQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetByIdDepartmentQuery> _validator;
    private readonly ILogger<GetDepartmentByIdHandler> _logger;

    public GetDepartmentByIdHandler(
        IReadDbContext readDbContext,
        IValidator<GetByIdDepartmentQuery> validator,
        ILogger<GetDepartmentByIdHandler> logger)
    {
        _readDbContext = readDbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<DepartmentDto, Error>> Handle(GetByIdDepartmentQuery query, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var department = _readDbContext.DepartmentsRead
            .FirstOrDefault(d => d.Id == query.Id);

        if (department is null)
        {
            return GeneralErrors.NotFound(query.Id);
        }

        _logger.LogInformation("Department with id {DepartmentId} found.", query.Id);

        return new DepartmentDto(
            department.Id.Value,
            department.Name.Value,
            department.Slug.Value,
            department.Path.Value,
            department.Parent is null ? null : department.Parent.Id.Value,
            department.CreatedAt,
            department.UpdatedAt);
    }
}