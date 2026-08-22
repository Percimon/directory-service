using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Contracts.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Departments.Get
{
    public sealed record GetDepartmentsQuery(
        int Page,
        int PageSize,
        string SortBy,
        string SortDirection,
        string? Search) : IQuery;

    public sealed class GetDepartmentsQueryValidator : AbstractValidator<GetDepartmentsQuery>
    {
        public GetDepartmentsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithError(
                    Error.Validation("department.get", "Page must be greater than 0."));

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithError(
                    Error.Validation("department.get", "Page must be greater than 0."))
                .LessThanOrEqualTo(100).WithError(
                    Error.Validation("department.get", "PageSize must be at most 100."));

            RuleFor(x => x.SortBy)
                .Custom((sortBy, context) =>
                {
                    var validSortByFields = new[] { "name", "createdAt" };
                    if (!validSortByFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
                    {
                        context.AddFailure(
                            JsonSerializer.Serialize(
                                 Error.Validation(
                                    "department.get",
                                    $"SortBy must be one of the following: {string.Join(", ", validSortByFields)}.")));
                    }
                });

            RuleFor(x => x.SortDirection)
                .NotEmpty().WithMessage("SortDirection is required.")
                .Must(direction => direction.Equals("asc", StringComparison.OrdinalIgnoreCase) || direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
                .WithError(Error.Validation(
                    "department.get",
                    "SortDirection must be either 'asc' or 'desc'."));

            RuleFor(x => x.Search)
                .Must(x => string.IsNullOrWhiteSpace(x) ? true : x.Length <= Constants.TextLength.LENGTH_50)
                .WithError(Error.Validation(
                    "department.get",
                    "Search must be empty or less than 50 characters"));
        }
    }

    public sealed class GetDepartmentsHandler : IQueryHandler<PagedList<DepartmentListItemDto>, GetDepartmentsQuery>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly IValidator<GetDepartmentsQuery> _validator;
        private readonly ILogger<GetDepartmentsHandler> _logger;

        public GetDepartmentsHandler(
            IReadDbContext readDbContext,
            IValidator<GetDepartmentsQuery> validator,
            ILogger<GetDepartmentsHandler> logger)
        {
            _readDbContext = readDbContext;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<PagedList<DepartmentListItemDto>, Error>> Handle(
            GetDepartmentsQuery request,
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToError();
            }

            var query = _readDbContext.DepartmentsRead;

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(d => d.Name.Value.ToLower().Contains(request.Search.ToLower()));
            }

            switch (request.SortBy.ToLower())
            {
                case "name":
                    query = request.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                        ? query.OrderBy(d => d.Name.Value)
                        : query.OrderByDescending(d => d.Name.Value);
                    break;
                case "createdat":
                    query = request.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                        ? query.OrderBy(d => d.CreatedAt)
                        : query.OrderByDescending(d => d.CreatedAt);
                    break;
                default:
                    _logger.LogWarning("Invalid SortBy value: {SortBy}. Defaulting to sorting by Name.", request.SortBy);
                    query = query.OrderBy(d => d.Name.Value);
                    break;
            }

            var departments = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(d => new DepartmentListItemDto(
                    d.Id.Value,
                    d.Name.Value,
                    d.Slug.Value,
                    d.Path.Value,
                    d.CreatedAt))
                .ToListAsync(cancellationToken);

            var totalCount = await query.CountAsync(cancellationToken);

            return new PagedList<DepartmentListItemDto>
            {
                Items = departments,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
            };
        }
    }
}